// -------------------------------------------------------------------------------------------------
// <copyright file="ChecksumTable.cs" company="Starion Group S.A.">
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
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Parses the per-asset SHA-256 checksum table the release workflow writes into each GitHub Release's
    /// body, one line per asset: <c>uml4net-sage-1.0.0-win-x64.zip sha256:&lt;64 hex chars&gt;</c>.
    /// </summary>
    public static partial class ChecksumTable
    {
        /// <summary>
        /// Parses <paramref name="releaseBody"/> into a map of asset file name to its expected SHA-256 hash.
        /// </summary>
        /// <param name="releaseBody">The GitHub Release body text.</param>
        /// <returns>
        /// A map of each <c>.zip</c> asset name to its lower-case SHA-256 hash; empty when the body has no
        /// checksum lines. If a name appears more than once, the last line wins.
        /// </returns>
        public static Dictionary<string, string> Parse(string releaseBody)
        {
            var result = new Dictionary<string, string>();

            foreach (Match match in ChecksumLine().Matches(releaseBody))
            {
                result[match.Groups["file"].Value] = match.Groups["sha256"].Value.ToLowerInvariant();
            }

            return result;
        }

        /// <summary>
        /// The source-generated regular expression that matches one <c>&lt;file&gt;.zip sha256:&lt;hash&gt;</c>
        /// entry, capturing the groups <c>file</c> and <c>sha256</c>.
        /// </summary>
        /// <returns>The compiled <see cref="Regex"/>.</returns>
        [GeneratedRegex(@"(?<file>[\w.\-]+\.zip)\s+sha256:(?<sha256>[0-9a-fA-F]{64})", RegexOptions.None)]
        private static partial Regex ChecksumLine();
    }
}
