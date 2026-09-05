// -------------------------------------------------------------------------------------------------
// <copyright file="GlobalOptions.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Codex.Tools.Commands
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
            Description = "The uml4net-codex plugin repository root. Defaults to $CLAUDE_PLUGIN_ROOT, then the current directory.",
        };

        /// <summary>
        /// The UML version to operate on.
        /// </summary>
        public static readonly Option<string> Version = new("--version")
        {
            Description = "The UML specification version to operate on.",
            DefaultValueFactory = _ => Uml4Net.Codex.Knowledge.KnownUmlVersions.Current.Version,
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
