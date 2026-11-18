// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The real, <see cref="Process"/>-backed implementation of <see cref="IProcessRunner"/>.
    /// </summary>
    public sealed class ProcessRunner : IProcessRunner
    {
        /// <summary>
        /// Runs <paramref name="fileName"/> with <paramref name="arguments"/> and returns its result.
        /// </summary>
        /// <param name="fileName">The executable to run.</param>
        /// <param name="arguments">The arguments, passed one by one without shell quoting.</param>
        /// <param name="workingDirectory">The working directory; <c>null</c> keeps the current one.</param>
        /// <param name="cancellationToken">Cancels waiting for the process.</param>
        /// <returns>The exit code and everything the process wrote to standard output and standard error.</returns>
        /// <exception cref="System.ComponentModel.Win32Exception">
        /// Thrown when <paramref name="fileName"/> cannot be found/started.
        /// </exception>
        public async Task<ProcessRunResult> RunAsync(string fileName, IReadOnlyList<string> arguments, string? workingDirectory = null, CancellationToken cancellationToken = default)
        {
            var startInfo = new ProcessStartInfo(fileName)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = workingDirectory ?? string.Empty,
            };

            foreach (var argument in arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            var standardOutputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var standardErrorTask = process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            return new ProcessRunResult(process.ExitCode, await standardOutputTask, await standardErrorTask);
        }
    }
}
