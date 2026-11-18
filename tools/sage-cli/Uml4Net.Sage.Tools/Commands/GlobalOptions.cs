// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Tools.Commands
{
    using System.CommandLine;

    /// <summary>
    /// Options shared by more than one verb.
    /// </summary>
    public static class GlobalOptions
    {
        /// <summary>
        /// The plugin repository root; see <see cref="RepositoryRootResolver"/> for the default resolution order.
        /// </summary>
        public static readonly Option<string?> RepositoryRoot = new("--repository-root")
        {
            Description = "The uml4net-sage plugin repository root. Defaults to $CLAUDE_PLUGIN_ROOT, then the current directory.",
        };

        /// <summary>
        /// The UML version to operate on.
        /// </summary>
        public static readonly Option<string> Version = new("--version")
        {
            Description = "The UML specification version to operate on.",
            DefaultValueFactory = _ => Uml4Net.Sage.Knowledge.KnownUmlVersions.Current.Version,
        };

        /// <summary>
        /// Requests machine-readable JSON output instead of a human-readable table.
        /// </summary>
        public static readonly Option<bool> Json = new("--json")
        {
            Description = "Emit machine-readable JSON instead of a human-readable table.",
        };
    }
}
