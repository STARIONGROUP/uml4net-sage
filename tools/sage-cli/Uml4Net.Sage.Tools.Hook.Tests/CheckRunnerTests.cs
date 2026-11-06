// -------------------------------------------------------------------------------------------------
// <copyright file="CheckRunnerTests.cs" company="Starion Group S.A.">
//
//   Copyright (C) 2019-2026 Starion Group S.A.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Uml4Net.Sage.Tools.Hook.Tests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Runs <see cref="CheckRunner"/> against small fake CLIs (a <c>.cmd</c> file on Windows, a shell
    /// script elsewhere) to pin how it treats the CLI process itself.
    /// </summary>
    [TestFixture]
    public class CheckRunnerTests
    {
        private const string CheckDocument = """{"Default":"2.5.1","HasAnyVersion":true,"Versions":[{"Version":"2.5.1","Fetched":true,"Generated":true,"SpecGenerated":true}]}""";

        private string workDirectory = null!;

        /// <summary>
        /// Creates an empty work directory for this test's fake CLI.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.workDirectory = Path.Combine(Path.GetTempPath(), "uml4net-sage-hook-tests", Path.GetRandomFileName());
            Directory.CreateDirectory(this.workDirectory);
        }

        /// <summary>
        /// Deletes the work directory.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(this.workDirectory))
            {
                Directory.Delete(this.workDirectory, recursive: true);
            }
        }

        /// <summary>
        /// Verifies that a CLI that writes far more to standard error than a pipe holds still runs to the
        /// end, and that its <c>check --json</c> document is parsed. With standard error unread, the CLI
        /// would block on a full pipe and the run would only end when the token is cancelled.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task RunAsync_reads_standard_error_so_a_chatty_cli_does_not_block()
        {
            var cli = this.WriteFakeCli(
                windows:
                [
                    "@echo off",
                    "for /L %%i in (1,1,3000) do echo xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx 1>&2",
                    $"echo {CheckDocument}",
                ],
                unix:
                [
                    "#!/bin/sh",
                    "i=0; while [ $i -lt 3000 ]; do echo xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx >&2; i=$((i+1)); done",
                    $"echo '{CheckDocument}'",
                ]);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            var result = await new CheckRunner().RunAsync(cli, this.workDirectory, timeout.Token);

            Assert.Multiple(() =>
            {
                Assert.That(timeout.IsCancellationRequested, Is.False, "the run should end because the CLI exited, not because of the timeout");
                Assert.That(result, Is.Not.Null);
                Assert.That(result!.Default, Is.EqualTo("2.5.1"));
            });
        }

        /// <summary>
        /// Verifies that cancelling the run while the CLI is still working returns <c>null</c> promptly
        /// and kills the CLI and the process it started, so the CLI never gets to write the marker file
        /// it would write after a three-second wait.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task RunAsync_kills_the_cli_when_cancelled()
        {
            var marker = Path.Combine(this.workDirectory, "still-running.txt");
            var cli = this.WriteFakeCli(
                windows:
                [
                    "@echo off",
                    "ping -n 4 127.0.0.1 >nul",
                    $"echo done> \"{marker}\"",
                ],
                unix:
                [
                    "#!/bin/sh",
                    "sleep 3",
                    $"echo done > '{marker}'",
                ]);
            using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
            var stopwatch = Stopwatch.StartNew();

            var result = await new CheckRunner().RunAsync(cli, this.workDirectory, cancellation.Token);
            var elapsed = stopwatch.Elapsed;

            // Give a surviving CLI more than enough time to finish its wait and write the marker.
            await Task.Delay(TimeSpan.FromSeconds(5));

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Null);
                Assert.That(elapsed, Is.LessThan(TimeSpan.FromSeconds(2.5)), "the run should end as soon as it is cancelled");
                Assert.That(File.Exists(marker), Is.False, "the CLI should have been killed before it wrote the marker");
            });
        }

        /// <summary>
        /// Writes a fake CLI for the current platform into the work directory and makes it executable.
        /// </summary>
        /// <param name="windows">The lines of the <c>.cmd</c> file used on Windows.</param>
        /// <param name="unix">The lines of the shell script used elsewhere.</param>
        /// <returns>The path of the fake CLI.</returns>
        private string WriteFakeCli(string[] windows, string[] unix)
        {
            if (OperatingSystem.IsWindows())
            {
                var path = Path.Combine(this.workDirectory, "fake-cli.cmd");
                File.WriteAllText(path, string.Join("\r\n", windows) + "\r\n");
                return path;
            }

            var script = Path.Combine(this.workDirectory, "fake-cli.sh");
            File.WriteAllText(script, string.Join("\n", unix) + "\n");
            File.SetUnixFileMode(script, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            return script;
        }
    }
}
