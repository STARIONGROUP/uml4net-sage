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
    using System;
    using System.CommandLine;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Spectre.Console;

    using Uml4Net.Sage.Knowledge;

    /// <summary>
    /// <c>uml4net-sage remove --version &lt;version&gt;</c>: deletes a version's fetched sources and
    /// generated knowledge, refusing on the current default or the last remaining version without <c>--force</c>.
    /// </summary>
    /// <remarks>
    /// The version becomes part of the folders that are deleted recursively, so it has to be either a
    /// registered UML version or one that <c>knowledge/installed.json</c> lists and that looks like a
    /// version number (so an install whose version was later dropped from the registry can still be
    /// removed). Either way the folders must sit directly under <c>sources/</c> and <c>knowledge/</c>.
    /// A version with nothing installed is refused rather than reported as removed.
    /// </remarks>
    public static partial class RemoveCommand
    {
        private static readonly Option<bool> ForceOption = new("--force") { Description = "Allow removing the current default version, or the only installed version." };

        /// <summary>
        /// Builds the command.
        /// </summary>
        /// <returns>The <c>remove</c> command with its options and action.</returns>
        public static Command Build()
        {
            var command = new Command("remove", "Delete a UML version's fetched sources and generated knowledge base.");
            command.Add(GlobalOptions.RepositoryRoot);
            command.Add(GlobalOptions.Version);
            command.Add(ForceOption);

            command.SetAction(parseResult =>
            {
                var version = parseResult.GetValue(GlobalOptions.Version) ?? string.Empty;
                var force = parseResult.GetValue(ForceOption);
                var layout = new RepositoryLayout(RepositoryRootResolver.Resolve(parseResult.GetValue(GlobalOptions.RepositoryRoot)));
                var store = new InstalledVersionsStore(layout.KnowledgeRoot);
                var manifest = store.Load();
                var listed = manifest.Versions.Any(v => v.Version == version);

                // The version becomes part of the paths that get deleted. A hand-edited manifest may list
                // anything, so a version that is not registered also has to look like a version number.
                if (!KnownUmlVersions.TryFind(version, out _) && !(listed && VersionNumber().IsMatch(version)))
                {
                    AnsiConsole.MarkupLineInterpolated($"[red]Unknown UML version '{version}'.[/] Known versions: {string.Join(", ", KnownUmlVersions.All.Select(v => v.Version))}");
                    return 1;
                }

                var sourcesDirectory = layout.SourcesDirectoryFor(version);
                var knowledgeDirectory = layout.KnowledgeDirectoryFor(version);

                if (!IsVersionDirectory(layout.SourcesRoot, sourcesDirectory) || !IsVersionDirectory(layout.KnowledgeRoot, knowledgeDirectory))
                {
                    AnsiConsole.MarkupLineInterpolated($"[red]Refusing to remove '{version}':[/] it does not resolve to a version folder directly under sources/ and knowledge/.");
                    return 1;
                }

                if (!listed && !Directory.Exists(sourcesDirectory) && !Directory.Exists(knowledgeDirectory))
                {
                    AnsiConsole.MarkupLineInterpolated($"[red]UML {version} is not installed;[/] there is nothing to remove.");
                    return 1;
                }

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

                if (Directory.Exists(sourcesDirectory))
                {
                    Directory.Delete(sourcesDirectory, recursive: true);
                }

                if (Directory.Exists(knowledgeDirectory))
                {
                    Directory.Delete(knowledgeDirectory, recursive: true);
                }

                store.Remove(version);

                AnsiConsole.MarkupLineInterpolated($"[green]Removed UML {version}.[/]");
                AnsiConsole.MarkupLine("(The companion XMI specification knowledge under knowledge/xmi/ is a shared, version-independent corpus and is left in place.)");
                return 0;
            });

            return command;
        }

        /// <summary>
        /// Checks that <paramref name="directory"/> is a direct child of <paramref name="root"/> and is
        /// not the shared <c>xmi</c> folder.
        /// </summary>
        /// <param name="root">The <c>sources/</c> or <c>knowledge/</c> folder.</param>
        /// <param name="directory">The resolved folder that would be deleted.</param>
        /// <returns><c>true</c> when the folder is safe to delete as a version folder.</returns>
        internal static bool IsVersionDirectory(string root, string directory)
        {
            var fullRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
            var fullDirectory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory));
            var parent = Path.GetDirectoryName(fullDirectory);
            var name = Path.GetFileName(fullDirectory);

            return parent is not null
                && string.Equals(parent, fullRoot, StringComparison.OrdinalIgnoreCase)
                && name.Length > 0
                && !string.Equals(name, "xmi", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Matches a version number such as <c>2.5.1</c>: two to four dot-separated numbers.
        /// </summary>
        /// <returns>The compiled expression.</returns>
        [GeneratedRegex(@"^[0-9]+(\.[0-9]+){1,3}\z", RegexOptions.CultureInvariant)]
        private static partial Regex VersionNumber();
    }
}
