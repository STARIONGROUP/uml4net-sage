// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Tools
{
    using System;
    using System.IO;

    /// <summary>
    /// Resolves the <c>uml4net-sage</c> plugin repository root: an explicit <c>--repository-root</c> value,
    /// then the <c>CLAUDE_PLUGIN_ROOT</c> environment variable (set by Claude Code when running the plugin's
    /// SessionStart hook), then the current directory.
    /// </summary>
    public static class RepositoryRootResolver
    {
        /// <summary>
        /// Resolves the repository root to use.
        /// </summary>
        /// <param name="explicitValue">The <c>--repository-root</c> value, or <c>null</c> when it was not given.</param>
        /// <returns>The full path of the repository root.</returns>
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
