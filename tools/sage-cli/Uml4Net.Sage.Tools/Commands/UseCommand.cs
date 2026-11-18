// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Tools.Commands
{
    using System;
    using System.CommandLine;

    using Spectre.Console;

    using Uml4Net.Sage.Knowledge;

    /// <summary>
    /// <c>uml4net-sage use --version &lt;version&gt;</c>: switches the default version, refusing if it
    /// hasn't been generated yet. Exists so a future multi-version world (2.6, 3.0 alongside 2.5.1) has a
    /// way to switch without re-fetching.
    /// </summary>
    public static class UseCommand
    {
        /// <summary>
        /// Builds the command.
        /// </summary>
        /// <returns>The <c>use</c> command with its options and action.</returns>
        public static Command Build()
        {
            var command = new Command("use", "Set the default UML version.");
            command.Add(GlobalOptions.RepositoryRoot);
            command.Add(GlobalOptions.Version);

            command.SetAction(parseResult =>
            {
                var version = parseResult.GetValue(GlobalOptions.Version)!;
                var layout = new RepositoryLayout(RepositoryRootResolver.Resolve(parseResult.GetValue(GlobalOptions.RepositoryRoot)));

                try
                {
                    new InstalledVersionsStore(layout.KnowledgeRoot).SetDefault(version);
                }
                catch (InvalidOperationException exception)
                {
                    AnsiConsole.MarkupLineInterpolated($"[red]{exception.Message}[/]");
                    return 1;
                }

                AnsiConsole.MarkupLineInterpolated($"[green]UML {version} is now the default.[/]");
                return 0;
            });

            return command;
        }
    }
}
