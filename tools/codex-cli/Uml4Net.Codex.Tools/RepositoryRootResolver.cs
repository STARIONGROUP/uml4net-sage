// -------------------------------------------------------------------------------------------------
// <copyright file="RepositoryRootResolver.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Codex.Tools
{
    using System;
    using System.IO;

    /// <summary>
    /// Resolves the <c>uml4net-codex</c> plugin repository root: an explicit <c>--repository-root</c> value,
    /// then the <c>CLAUDE_PLUGIN_ROOT</c> environment variable (set by Claude Code when running the plugin's
    /// SessionStart hook), then the current directory.
    /// </summary>
    public static class RepositoryRootResolver
    {
        /// <summary>
        /// Resolves the repository root to use.
        /// </summary>
        public static string Resolve(string? explicitValue)
        {
            if (!string.IsNullOrWhiteSpace(explicitValue))
            {
                return Path.GetFullPath(explicitValue);
            }

            var pluginRoot = Environment.GetEnvironmentVariable("CLAUDE_PLUGIN_ROOT");
            if (!string.IsNullOrWhiteSpace(pluginRoot))
            {
                return Path.GetFullPath(pluginRoot);
            }

            return Directory.GetCurrentDirectory();
        }
    }
}
