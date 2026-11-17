// -------------------------------------------------------------------------------------------------
// <copyright file="UvProvisionerTests.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Sage.Knowledge.Tests
{
    using System;
    using System.Formats.Tar;
    using System.IO;
    using System.IO.Compression;
    using System.Net;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Uml4Net.Sage.Knowledge.Toolchain;

    [TestFixture]
    public class UvProvisionerTests
    {
        private DirectoryInfo cacheRoot = null!;
        private string target = null!;

        [SetUp]
        public void SetUp()
        {
            this.cacheRoot = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "uml4net-sage-knowledge-tests", Guid.NewGuid().ToString("N")));
            this.target = UvPlatform.CurrentTarget ?? throw new InvalidOperationException("This test machine's platform is not one of uv's published targets - cannot run.");
        }

        [TearDown]
        public void TearDown()
        {
            if (this.cacheRoot.Exists)
            {
                this.cacheRoot.Delete(recursive: true);
            }
        }

        /// <summary>
        /// Builds an in-memory archive laid out as uv publishes one for <paramref name="target"/>: a
        /// zip with the entry at its root on Windows, a tar.gz with the entry in a
        /// <c>uv-&lt;target&gt;</c> folder elsewhere.
        /// </summary>
        /// <param name="target">The uv release target triple, which decides the format and layout.</param>
        /// <param name="entryName">The name of the single entry.</param>
        /// <param name="entryContent">The content of the single entry.</param>
        /// <returns>The archive's bytes.</returns>
        private static byte[] BuildArchiveContaining(string target, string entryName, byte[] entryContent)
        {
            using var memoryStream = new MemoryStream();

            if (UvPlatform.IsZip(target))
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
                {
                    var entry = archive.CreateEntry(entryName);
                    using var entryStream = entry.Open();
                    entryStream.Write(entryContent);
                }

                return memoryStream.ToArray();
            }

            using (var gzip = new GZipStream(memoryStream, CompressionLevel.Fastest, leaveOpen: true))
            using (var tar = new TarWriter(gzip, TarEntryFormat.Pax, leaveOpen: true))
            {
                var entry = new PaxTarEntry(TarEntryType.RegularFile, $"uv-{target}/{entryName}")
                {
                    DataStream = new MemoryStream(entryContent),
                };
                tar.WriteEntry(entry);
            }

            return memoryStream.ToArray();
        }

        /// <summary>
        /// Builds a handler that serves one valid release of uv for <paramref name="target"/>: the
        /// release document, the archive's checksum and the archive, in the order the provisioner asks.
        /// </summary>
        /// <param name="target">The uv release target triple the archive is built for.</param>
        /// <returns>The handler, with the three responses queued.</returns>
        private static StubHttpMessageHandler HandlerServingAValidRelease(string target)
        {
            var archiveBytes = BuildArchiveContaining(target, UvPlatform.ExecutableName(target), Encoding.UTF8.GetBytes("fake uv"));
            var sha256 = Convert.ToHexStringLower(SHA256.HashData(archiveBytes));
            var assetName = UvPlatform.AssetName(target);

            var releaseJson = JsonSerializer.Serialize(new
            {
                assets = new object[]
                {
                    new { name = assetName, browser_download_url = "https://example.com/download/" + assetName },
                    new { name = assetName + ".sha256", browser_download_url = "https://example.com/download/" + assetName + ".sha256" },
                },
            });

            var handler = new StubHttpMessageHandler();
            handler.Enqueue(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(releaseJson) });
            handler.Enqueue(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent($"{sha256} *{assetName}") });
            handler.Enqueue(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(archiveBytes) });

            return handler;
        }

        [Test]
        public async Task EnsureAsync_downloads_verifies_and_caches_a_matching_release()
        {
            var provisioner = new UvProvisioner(new HttpClient(HandlerServingAValidRelease(this.target)), this.cacheRoot);

            var executable = await provisioner.EnsureAsync();

            Assert.That(executable, Is.Not.Null);
            Assert.That(File.Exists(executable!.FullName));
            Assert.That(File.Exists(provisioner.OkMarker(UvProvisioner.PinnedVersion, this.target).FullName));
        }

        [Test]
        public async Task EnsureAsync_makes_the_extracted_executable_runnable_on_unix()
        {
            // The archive's entry carries no execute bit, so only the provisioner's own chmod, applied
            // to the executable in the uv-<target> folder the tar.gz holds it in, makes it runnable.
            // Windows has no execute bit, and its archive is a flat zip.
            if (OperatingSystem.IsWindows())
            {
                Assert.Ignore("Unix file modes do not exist on Windows.");
                return;
            }

            var provisioner = new UvProvisioner(new HttpClient(HandlerServingAValidRelease(this.target)), this.cacheRoot);

            var executable = await provisioner.EnsureAsync();

            Assert.That(executable, Is.Not.Null);
            var expected = Path.Combine(
                provisioner.BinDirectory(UvProvisioner.PinnedVersion, this.target).FullName,
                $"uv-{this.target}",
                UvPlatform.ExecutableName(this.target));
            Assert.That(executable!.FullName, Is.EqualTo(expected));
            Assert.That(File.GetUnixFileMode(executable.FullName).HasFlag(UnixFileMode.UserExecute), Is.True);
        }

        [TestCase(false, TestName = "Executable_is_at_the_root_of_a_flat_archive")]
        [TestCase(true, TestName = "Executable_is_in_the_target_folder_of_a_wrapped_archive")]
        public void Executable_finds_the_executable_of_a_flat_or_a_wrapped_archive(bool wrapped)
        {
            // uv's zip archives are flat and its tar.gz archives wrap the executable in uv-<target>/;
            // this checks both on whichever platform the tests run, not only the one it downloads for.
            var provisioner = new UvProvisioner(new HttpClient(new StubHttpMessageHandler()), this.cacheRoot);
            var binDirectory = provisioner.BinDirectory(UvProvisioner.PinnedVersion, this.target).FullName;
            var executableName = UvPlatform.ExecutableName(this.target);
            var expected = wrapped
                ? Path.Combine(binDirectory, $"uv-{this.target}", executableName)
                : Path.Combine(binDirectory, executableName);
            Directory.CreateDirectory(Path.GetDirectoryName(expected)!);
            File.WriteAllText(expected, "fake uv");

            var executable = provisioner.Executable(UvProvisioner.PinnedVersion, this.target);

            Assert.That(executable.FullName, Is.EqualTo(expected));
        }

        [Test]
        public async Task EnsureAsync_returns_null_and_does_not_cache_on_a_checksum_mismatch()
        {
            var executableName = UvPlatform.ExecutableName(this.target);
            var archiveBytes = BuildArchiveContaining(this.target, executableName, Encoding.UTF8.GetBytes("fake uv"));
            var assetName = UvPlatform.AssetName(this.target);

            var releaseJson = JsonSerializer.Serialize(new
            {
                assets = new object[]
                {
                    new { name = assetName, browser_download_url = "https://example.com/download/" + assetName },
                    new { name = assetName + ".sha256", browser_download_url = "https://example.com/download/" + assetName + ".sha256" },
                },
            });

            var handler = new StubHttpMessageHandler();
            handler.Enqueue(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(releaseJson) });
            handler.Enqueue(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent($"{new string('0', 64)} *{assetName}") }); // deliberately wrong
            handler.Enqueue(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(archiveBytes) });

            var provisioner = new UvProvisioner(new HttpClient(handler), this.cacheRoot);

            var executable = await provisioner.EnsureAsync();

            Assert.That(executable, Is.Null);
            Assert.That(provisioner.OkMarker(UvProvisioner.PinnedVersion, this.target).Exists, Is.False);
        }

        [Test]
        public async Task EnsureAsync_returns_null_when_the_release_is_not_found()
        {
            var handler = new StubHttpMessageHandler();
            handler.Enqueue(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

            var provisioner = new UvProvisioner(new HttpClient(handler), this.cacheRoot);

            var executable = await provisioner.EnsureAsync();

            Assert.That(executable, Is.Null);
        }

        [Test]
        public async Task EnsureAsync_returns_null_when_no_asset_matches_this_platform()
        {
            var releaseJson = JsonSerializer.Serialize(new
            {
                assets = new object[]
                {
                    new { name = "uv-some-other-target.zip", browser_download_url = "https://example.com/x.zip" },
                },
            });

            var handler = new StubHttpMessageHandler();
            handler.Enqueue(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(releaseJson) });

            var provisioner = new UvProvisioner(new HttpClient(handler), this.cacheRoot);

            var executable = await provisioner.EnsureAsync();

            Assert.That(executable, Is.Null);
        }

        [Test]
        public async Task EnsureAsync_never_throws_when_the_http_client_itself_throws()
        {
            var handler = new StubHttpMessageHandler();
            handler.Enqueue(_ => throw new HttpRequestException("network is down"));

            var provisioner = new UvProvisioner(new HttpClient(handler), this.cacheRoot);

            FileInfo? executable = null;
            await Assert.DoesNotThrowAsync(async () => executable = await provisioner.EnsureAsync());
            Assert.That(executable, Is.Null);
        }

        [Test]
        public async Task EnsureAsync_reuses_the_cache_without_a_network_call_once_verified()
        {
            var binDirectory = new UvProvisioner(new HttpClient(new StubHttpMessageHandler()), this.cacheRoot)
                .BinDirectory(UvProvisioner.PinnedVersion, this.target);
            binDirectory.Create();
            var executableName = UvPlatform.ExecutableName(this.target);
            await File.WriteAllTextAsync(Path.Combine(binDirectory.FullName, executableName), "already here");
            await File.WriteAllTextAsync(Path.Combine(binDirectory.FullName, ".ok"), "verified");

            var handler = new StubHttpMessageHandler(); // no responses enqueued - any HTTP call would throw
            var provisioner = new UvProvisioner(new HttpClient(handler), this.cacheRoot);

            var executable = await provisioner.EnsureAsync();

            Assert.That(executable, Is.Not.Null);
            Assert.That(executable!.FullName, Is.EqualTo(Path.Combine(binDirectory.FullName, executableName)));
        }

        [Test]
        public async Task EnsureAsync_reuses_a_wrapped_cache_without_a_network_call_once_verified()
        {
            // The cache a tar.gz leaves on Linux and macOS: the executable in uv-<target>/, the marker
            // at the build's root. A later run must find it there rather than download again.
            var binDirectory = new UvProvisioner(new HttpClient(new StubHttpMessageHandler()), this.cacheRoot)
                .BinDirectory(UvProvisioner.PinnedVersion, this.target);
            var wrappedDirectory = Directory.CreateDirectory(Path.Combine(binDirectory.FullName, $"uv-{this.target}"));
            var executableName = UvPlatform.ExecutableName(this.target);
            var expected = Path.Combine(wrappedDirectory.FullName, executableName);
            await File.WriteAllTextAsync(expected, "already here");
            await File.WriteAllTextAsync(Path.Combine(binDirectory.FullName, ".ok"), "verified");

            var handler = new StubHttpMessageHandler(); // no responses enqueued - any HTTP call would throw
            var provisioner = new UvProvisioner(new HttpClient(handler), this.cacheRoot);

            var executable = await provisioner.EnsureAsync();

            Assert.That(executable, Is.Not.Null);
            Assert.That(executable!.FullName, Is.EqualTo(expected));
        }
    }
}
