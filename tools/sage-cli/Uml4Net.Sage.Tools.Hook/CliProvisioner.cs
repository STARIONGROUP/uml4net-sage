// -------------------------------------------------------------------------------------------------
// <copyright file="CliProvisioner.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Sage.Tools.Hook
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Downloads, SHA-256-verifies, and caches the full <c>uml4net-sage</c> CLI from a pinned GitHub
    /// Release the first time it's actually needed - the SessionStart hook itself stays a small,
    /// dependency-free binary (see the root CLAUDE.md's "small hook, big CLI" split); the CLI's heavier
    /// dependencies (<c>uml4net.xmi</c>, Spectre.Console, System.CommandLine) are not AOT-trim-friendly,
    /// which is why they live in a separately-published, on-demand-downloaded executable instead.
    /// </summary>
    public sealed class CliProvisioner
    {
        /// <summary>
        /// The GitHub repository the full CLI is released from.
        /// </summary>
        public const string Repository = "STARIONGROUP/uml4net-sage";

        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="CliProvisioner"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client used to call the GitHub Releases API and download the asset.</param>
        public CliProvisioner(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <summary>
        /// Ensures the CLI for <paramref name="cliVersion"/> is present and verified in the local cache,
        /// downloading and extracting it if necessary. Never throws: any failure (offline, rate-limited,
        /// no matching asset, checksum mismatch) results in a <c>null</c> return, meaning the hook simply
        /// has nothing to report this session and will try again next time.
        /// </summary>
        /// <param name="pluginRoot">The plugin root directory, used to key the cache.</param>
        /// <param name="cliVersion">The pinned CLI version to provision.</param>
        /// <param name="cancellationToken">Cancels the download.</param>
        /// <returns>
        /// The path to the verified, ready-to-run CLI executable, or <c>null</c> if it could not be provisioned.
        /// </returns>
        public async Task<string?> EnsureAsync(string pluginRoot, string cliVersion, CancellationToken cancellationToken)
        {
            try
            {
                var runtimeIdentifier = RuntimeIdentifier.Current();
                if (runtimeIdentifier is null)
                {
                    return null;
                }

                var cliDirectory = CacheLayout.CliDirectoryFor(pluginRoot, cliVersion, runtimeIdentifier);
                var executablePath = Path.Combine(cliDirectory, RuntimeIdentifier.ExecutableName(runtimeIdentifier));

                if (File.Exists(CacheLayout.VerifiedMarkerPath(cliDirectory)) && File.Exists(executablePath))
                {
                    return executablePath;
                }

                return await this.DownloadAndExtractAsync(cliVersion, runtimeIdentifier, cliDirectory, executablePath, cancellationToken);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Fetches the release for <paramref name="cliVersion"/>, downloads the zip asset for
        /// <paramref name="runtimeIdentifier"/>, checks its SHA-256 hash against the release body's checksum
        /// table, and extracts it into <paramref name="cliDirectory"/>. On success it marks the executable
        /// as executable on non-Windows platforms and writes the verified marker file.
        /// </summary>
        /// <param name="cliVersion">The CLI version to download.</param>
        /// <param name="runtimeIdentifier">The runtime identifier whose asset is downloaded.</param>
        /// <param name="cliDirectory">The cache directory to extract into.</param>
        /// <param name="executablePath">The path the CLI executable is expected at after extraction.</param>
        /// <param name="cancellationToken">Cancels the HTTP calls and file writes.</param>
        /// <returns>
        /// <paramref name="executablePath"/>, or <c>null</c> if the release, the asset or its checksum is
        /// missing, the hash does not match, or the zip does not contain the executable.
        /// </returns>
        private async Task<string?> DownloadAndExtractAsync(string cliVersion, string runtimeIdentifier, string cliDirectory, string executablePath, CancellationToken cancellationToken)
        {
            var release = await this.FetchReleaseAsync(cliVersion, cancellationToken);
            if (release is null)
            {
                return null;
            }

            var assetName = $"uml4net-sage-{cliVersion}-{runtimeIdentifier}.zip";
            var asset = release.Assets.Find(a => a.Name == assetName);
            if (asset is null)
            {
                return null;
            }

            var checksums = ChecksumTable.Parse(release.Body);
            if (!checksums.TryGetValue(assetName, out var expectedSha256))
            {
                return null;
            }

            var zipBytes = await this.httpClient.GetByteArrayAsync(asset.BrowserDownloadUrl, cancellationToken);
            var actualSha256 = Convert.ToHexStringLower(SHA256.HashData(zipBytes));
            if (!string.Equals(actualSha256, expectedSha256, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            Directory.CreateDirectory(cliDirectory);
            var zipPath = Path.Combine(Path.GetTempPath(), $"{assetName}-{Guid.NewGuid():N}.zip");
            try
            {
                await File.WriteAllBytesAsync(zipPath, zipBytes, cancellationToken);
                ZipFile.ExtractToDirectory(zipPath, cliDirectory, overwriteFiles: true);
            }
            finally
            {
                File.Delete(zipPath);
            }

            if (!File.Exists(executablePath))
            {
                return null;
            }

            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(executablePath, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute
                    | UnixFileMode.GroupRead | UnixFileMode.GroupExecute | UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
            }

            await File.WriteAllTextAsync(CacheLayout.VerifiedMarkerPath(cliDirectory), DateTimeOffset.UtcNow.ToString("o"), cancellationToken);
            return executablePath;
        }

        /// <summary>
        /// Gets the GitHub Release tagged <c>tools-v&lt;cliVersion&gt;</c> from <see cref="Repository"/>.
        /// </summary>
        /// <param name="cliVersion">The CLI version, used to build the release tag.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>The parsed release, or <c>null</c> if the API does not return a success status code.</returns>
        private async Task<GitHubRelease?> FetchReleaseAsync(string cliVersion, CancellationToken cancellationToken)
        {
            var url = $"https://api.github.com/repos/{Repository}/releases/tags/tools-v{cliVersion}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd("uml4net-sage-hook");

            using var response = await this.httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync(stream, HookJsonContext.Default.GitHubRelease, cancellationToken);
        }
    }
}
