// -------------------------------------------------------------------------------------------------
// <copyright file="CacheLayout.cs" company="Starion Group S.A.">
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
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Computes where the full <c>uml4net-sage</c> CLI is cached on disk, outside the plugin's own
    /// git-managed tree so a plugin update/reinstall never disturbs it.
    /// </summary>
    public static class CacheLayout
    {
        /// <summary>
        /// The root cache directory: <c>%LOCALAPPDATA%\uml4net-sage</c> on Windows, the XDG/home
        /// equivalent elsewhere.
        /// </summary>
        public static string CacheRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "uml4net-sage");

        /// <summary>
        /// The directory a specific (version, runtime identifier, plugin root) combination's CLI is
        /// extracted into. Keyed by an install-key hash of the plugin root path so a maintainer checkout
        /// and a marketplace install of the same version never collide.
        /// </summary>
        /// <param name="pluginRoot">The plugin root directory, hashed into the install key.</param>
        /// <param name="version">The CLI version.</param>
        /// <param name="runtimeIdentifier">The .NET runtime identifier, for example <c>win-x64</c>.</param>
        /// <returns>The full path of the cache directory for that combination.</returns>
        public static string CliDirectoryFor(string pluginRoot, string version, string runtimeIdentifier)
        {
            return Path.Combine(CacheRoot, "bin", version, runtimeIdentifier, InstallKey(pluginRoot));
        }

        /// <summary>
        /// A short, stable hash of the plugin root path, used to key the cache per-checkout.
        /// </summary>
        /// <param name="pluginRoot">The plugin root directory. It is made absolute and lower-cased before hashing.</param>
        /// <returns>The first 12 lower-case hex characters of the SHA-256 hash of the path.</returns>
        public static string InstallKey(string pluginRoot)
        {
            var bytes = Encoding.UTF8.GetBytes(Path.GetFullPath(pluginRoot).ToLowerInvariant());
            return Convert.ToHexStringLower(SHA256.HashData(bytes))[..12];
        }

        /// <summary>
        /// The marker file written once an extracted CLI has been checksum-verified - its presence means
        /// the cached CLI can be trusted without re-verifying on every session start.
        /// </summary>
        /// <param name="cliDirectory">The directory the CLI is extracted into.</param>
        /// <returns>The path of the <c>.verified</c> file in <paramref name="cliDirectory"/>.</returns>
        public static string VerifiedMarkerPath(string cliDirectory) => Path.Combine(cliDirectory, ".verified");
    }
}
