// -------------------------------------------------------------------------------------------------
// <copyright file="UvProvisioner.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Sage.Knowledge.Toolchain
{
    using System;
    using System.Formats.Tar;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Fetches, checksum-verifies and caches the <c>uv</c> binary from its GitHub releases the first
    /// time it is needed.
    /// </summary>
    /// <remarks>
    /// Mirrors <c>Uml4Net.Sage.Tools.Hook</c>'s <c>CliProvisioner</c> architecturally (cache-check,
    /// download, checksum-verify, extract, mark-verified, never throw) but is independent code: the
    /// hook project is deliberately kept dependency-free and NativeAOT-publishable, so neither project
    /// references the other.
    /// <para>
    /// Never throws past <see cref="EnsureAsync"/>: any failure here (offline, rate-limited, a corrupt
    /// download, an unsupported platform) means the caller degrades to a
    /// <see cref="SpecExtractionOutcome.Skipped"/> rather than aborting the rest of a
    /// <c>generate</c> run.
    /// </para>
    /// </remarks>
    public sealed class UvProvisioner : IUvProvisioner
    {
        private const string Repository = "astral-sh/uv";

        /// <summary>The pinned uv release. Bumped by PR, the same way the CLI's own release is.</summary>
        /// <remarks>Public so tests can address the cache path this provisions without duplicating the value.</remarks>
        public const string PinnedVersion = "0.12.10";

        private const string CacheFolderName = "uml4net-sage";

        private readonly HttpClient client;
        private readonly DirectoryInfo cacheRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="UvProvisioner"/> class that caches under
        /// <see cref="ResolveCacheRoot"/>.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> to query GitHub and download uv with.</param>
        /// <exception cref="ArgumentNullException"><paramref name="client"/> is <c>null</c>.</exception>
        public UvProvisioner(HttpClient client)
            : this(client, ResolveCacheRoot())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UvProvisioner"/> class with an explicit cache
        /// root, so a test never touches the real <c>%LOCALAPPDATA%</c>.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> to query GitHub and download uv with.</param>
        /// <param name="cacheRoot">The directory under which the <c>uv/</c> cache lives.</param>
        /// <exception cref="ArgumentNullException"><paramref name="client"/> or <paramref name="cacheRoot"/> is <c>null</c>.</exception>
        public UvProvisioner(HttpClient client, DirectoryInfo cacheRoot)
        {
            ArgumentNullException.ThrowIfNull(client);
            ArgumentNullException.ThrowIfNull(cacheRoot);

            this.client = client;
            this.cacheRoot = cacheRoot;
        }

        /// <summary>
        /// Resolves the cache root via <see cref="Environment.SpecialFolder.LocalApplicationData"/> -
        /// the same folder the CLI's own self-fetch hook (<c>CacheLayout.CacheRoot</c>) already caches
        /// into, so this is a sibling, not a new location to document.
        /// </summary>
        /// <param name="getFolderPath">
        /// Resolves a special folder; <c>null</c> uses <see cref="Environment.GetFolderPath(Environment.SpecialFolder)"/>.
        /// Injectable so tests can supply their own folder.
        /// </param>
        /// <returns>The <c>uml4net-sage</c> folder under the local application data folder.</returns>
        public static DirectoryInfo ResolveCacheRoot(Func<Environment.SpecialFolder, string>? getFolderPath = null)
        {
            getFolderPath ??= Environment.GetFolderPath;

            return new DirectoryInfo(
                Path.Combine(getFolderPath(Environment.SpecialFolder.LocalApplicationData), CacheFolderName));
        }

        /// <summary><c>uv/&lt;version&gt;/&lt;target&gt;/</c> - where one uv build is cached.</summary>
        /// <param name="version">The uv release version.</param>
        /// <param name="target">The uv release target triple.</param>
        /// <returns>The cache directory for that build.</returns>
        public DirectoryInfo BinDirectory(string version, string target) =>
            new(Path.Combine(this.cacheRoot.FullName, "uv", version, target));

        /// <summary>The cached <c>uv</c> executable itself.</summary>
        /// <param name="version">The uv release version.</param>
        /// <param name="target">The uv release target triple.</param>
        /// <returns>The path of the executable in that build's cache directory, whether or not it exists.</returns>
        public FileInfo Executable(string version, string target) =>
            new(Path.Combine(this.BinDirectory(version, target).FullName, UvPlatform.ExecutableName(target)));

        /// <summary>
        /// Written only once the cached download's checksum has been verified - its presence is what
        /// lets a later run skip the network entirely.
        /// </summary>
        /// <param name="version">The uv release version.</param>
        /// <param name="target">The uv release target triple.</param>
        /// <returns>The path of the <c>.ok</c> marker file, whether or not it exists.</returns>
        public FileInfo OkMarker(string version, string target) =>
            new(Path.Combine(this.BinDirectory(version, target).FullName, ".ok"));

        /// <summary>
        /// Ensures <c>uv</c> is cached, downloading and verifying it if necessary.
        /// </summary>
        /// <param name="cancellationToken">Cancels the download.</param>
        /// <returns>The cached executable, or <c>null</c> when it could not be provisioned.</returns>
        public async Task<FileInfo?> EnsureAsync(CancellationToken cancellationToken = default)
        {
            var target = UvPlatform.CurrentTarget;
            if (target is null)
            {
                return null;
            }

            var executable = this.Executable(PinnedVersion, target);

            if (this.IsCached(PinnedVersion, target))
            {
                return executable;
            }

            try
            {
                var assetName = UvPlatform.AssetName(target);

                using var releaseRequest = new HttpRequestMessage(
                    HttpMethod.Get, $"https://api.github.com/repos/{Repository}/releases/tags/{PinnedVersion}");
                releaseRequest.Headers.UserAgent.ParseAdd("uml4net-sage");

                using var releaseResponse = await this.client.SendAsync(releaseRequest, cancellationToken);
                if (!releaseResponse.IsSuccessStatusCode)
                {
                    return null;
                }

                var releasePayload = await releaseResponse.Content.ReadAsStringAsync(cancellationToken);
                using var release = JsonDocument.Parse(releasePayload);

                var (downloadUrl, checksumUrl) = FindAssetUrls(release.RootElement, assetName);
                if (downloadUrl is null || checksumUrl is null)
                {
                    return null;
                }

                var expectedSha256 = await this.ReadChecksumAsync(checksumUrl, cancellationToken);
                if (expectedSha256 is null)
                {
                    return null;
                }

                var binDirectory = this.BinDirectory(PinnedVersion, target);
                binDirectory.Create();

                var archivePath = Path.Combine(binDirectory.FullName, assetName + ".download");

                await using (var archiveStream = File.Create(archivePath))
                await using (var downloadStream = await this.client.GetStreamAsync(downloadUrl, cancellationToken))
                {
                    await downloadStream.CopyToAsync(archiveStream, cancellationToken);
                }

                if (!VerifySha256(archivePath, expectedSha256))
                {
                    File.Delete(archivePath);

                    return null;
                }

                Extract(archivePath, binDirectory.FullName, target);
                File.Delete(archivePath);

                if (!OperatingSystem.IsWindows())
                {
                    executable.Refresh();
                    if (executable.Exists)
                    {
                        File.SetUnixFileMode(
                            executable.FullName,
                            UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute
                            | UnixFileMode.GroupRead | UnixFileMode.GroupExecute
                            | UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
                    }
                }

                // Written last, only once extraction succeeded: its presence is what IsCached trusts to
                // skip the network on every later run.
                await File.WriteAllTextAsync(
                    this.OkMarker(PinnedVersion, target).FullName, expectedSha256, cancellationToken);

                executable.Refresh();

                return executable.Exists ? executable : null;
            }
            catch (Exception exception) when (
                exception is HttpRequestException or IOException or JsonException
                or TaskCanceledException or InvalidOperationException)
            {
                return null;
            }
        }

        /// <summary>
        /// Checks whether a build is already cached: both its executable and its <c>.ok</c> marker exist.
        /// </summary>
        /// <param name="version">The uv release version.</param>
        /// <param name="target">The uv release target triple.</param>
        /// <returns><c>true</c> when the executable and the marker both exist; otherwise <c>false</c>.</returns>
        private bool IsCached(string version, string target)
        {
            var executable = this.Executable(version, target);
            var marker = this.OkMarker(version, target);

            executable.Refresh();
            marker.Refresh();

            return executable.Exists && marker.Exists;
        }

        /// <summary>uv publishes each asset's checksum as a separate, same-named ``.sha256`` asset.</summary>
        /// <param name="release">The GitHub release JSON, with its <c>assets</c> array.</param>
        /// <param name="assetName">The archive asset name to look for.</param>
        /// <returns>
        /// The download URLs of the archive and of its <c>.sha256</c> asset; either is <c>null</c> when
        /// the release does not list it.
        /// </returns>
        private static (string? DownloadUrl, string? ChecksumUrl) FindAssetUrls(
            JsonElement release, string assetName)
        {
            string? downloadUrl = null;
            string? checksumUrl = null;

            foreach (var asset in release.GetProperty("assets").EnumerateArray())
            {
                var name = asset.GetProperty("name").GetString();

                if (string.Equals(name, assetName, StringComparison.Ordinal))
                {
                    downloadUrl = asset.GetProperty("browser_download_url").GetString();
                }
                else if (string.Equals(name, assetName + ".sha256", StringComparison.Ordinal))
                {
                    checksumUrl = asset.GetProperty("browser_download_url").GetString();
                }
            }

            return (downloadUrl, checksumUrl);
        }

        /// <summary>Reads a `sha256sum`-format checksum file: "&lt;hex&gt; *&lt;filename&gt;".</summary>
        /// <param name="checksumUrl">The URL of the <c>.sha256</c> asset.</param>
        /// <param name="cancellationToken">Cancels the download.</param>
        /// <returns>The hexadecimal hash (the first whitespace-separated token), or <c>null</c> when the file is empty.</returns>
        private async Task<string?> ReadChecksumAsync(string checksumUrl, CancellationToken cancellationToken)
        {
            var content = await this.client.GetStringAsync(checksumUrl, cancellationToken);
            var hex = content.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

            return string.IsNullOrWhiteSpace(hex) ? null : hex;
        }

        /// <summary>
        /// Checks whether the SHA-256 hash of the file at <paramref name="path"/> matches
        /// <paramref name="expectedHex"/>, ignoring case.
        /// </summary>
        /// <param name="path">The downloaded archive.</param>
        /// <param name="expectedHex">The expected hexadecimal hash.</param>
        /// <returns><c>true</c> when the hashes match; otherwise <c>false</c>.</returns>
        private static bool VerifySha256(string path, string expectedHex)
        {
            using var stream = File.OpenRead(path);
            var actual = Convert.ToHexStringLower(SHA256.HashData(stream));

            return string.Equals(actual, expectedHex, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Extracts the zip or tar.gz archive into <paramref name="destination"/>, overwriting existing
        /// files. Every uv release target's archive is flat (no wrapping folder) - verified live.
        /// </summary>
        /// <param name="archivePath">The downloaded archive.</param>
        /// <param name="destination">The directory to extract into.</param>
        /// <param name="target">The uv release target triple, which decides the archive format.</param>
        private static void Extract(string archivePath, string destination, string target)
        {
            if (UvPlatform.IsZip(target))
            {
                ZipFile.ExtractToDirectory(archivePath, destination, overwriteFiles: true);

                return;
            }

            using var fileStream = File.OpenRead(archivePath);
            using var gzip = new GZipStream(fileStream, CompressionMode.Decompress);
            TarFile.ExtractToDirectory(gzip, destination, overwriteFiles: true);
        }
    }
}
