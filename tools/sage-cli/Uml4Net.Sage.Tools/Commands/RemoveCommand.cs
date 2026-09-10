// -------------------------------------------------------------------------------------------------
// <copyright file="RemoveCommand.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Sage.Tools.Commands
{
    using System.CommandLine;
    using System.IO;

    using Spectre.Console;

    using Uml4Net.Sage.Knowledge;

    /// <summary>
    /// <c>uml4net-sage remove --version &lt;version&gt;</c>: deletes a version's fetched sources and
    /// generated knowledge, refusing on the current default or the last remaining version without <c>--force</c>.
    /// </summary>
    public static class RemoveCommand
    {
        private static readonly Option<bool> ForceOption = new("--force") { Description = "Allow removing the current default version, or the only installed version." };

        /// <summary>
        /// Builds the command.
        /// </summary>
        public static Command Build()
        {
            var command = new Command("remove", "Delete a UML version's fetched sources and generated knowledge base.");
            command.Add(GlobalOptions.RepositoryRoot);
            command.Add(GlobalOptions.Version);
            command.Add(ForceOption);

            command.SetAction(parseResult =>
            {
                var version = parseResult.GetValue(GlobalOptions.Version)!;
                var force = parseResult.GetValue(ForceOption);
                var layout = new RepositoryLayout(RepositoryRootResolver.Resolve(parseResult.GetValue(GlobalOptions.RepositoryRoot)));
                var store = new InstalledVersionsStore(layout.KnowledgeRoot);
                var manifest = store.Load();

                if (!force && manifest.Default == version)
                {
                    AnsiConsole.MarkupLineInterpolated($"[red]UML {version} is the current default.[/] Switch to another version first ('use --version <other>'), or pass --force.");
                    return 1;
                }

                if (!force && manifest.Versions.Count == 1 && manifest.Versions[0].Version == version)
                {
                    AnsiConsole.MarkupLineInterpolated($"[red]UML {version} is the only installed version.[/] Pass --force to remove it anyway.");
                    return 1;
                }

                if (Directory.Exists(layout.SourcesDirectoryFor(version)))
                {
                    Directory.Delete(layout.SourcesDirectoryFor(version), recursive: true);
                }

                if (Directory.Exists(layout.KnowledgeDirectoryFor(version)))
                {
                    Directory.Delete(layout.KnowledgeDirectoryFor(version), recursive: true);
                }

                store.Remove(version);

                AnsiConsole.MarkupLineInterpolated($"[green]Removed UML {version}.[/]");
                AnsiConsole.MarkupLine("(The companion XMI specification knowledge under knowledge/xmi/ is a shared, version-independent corpus and is left in place.)");
                return 0;
            });

            return command;
        }
    }
}
