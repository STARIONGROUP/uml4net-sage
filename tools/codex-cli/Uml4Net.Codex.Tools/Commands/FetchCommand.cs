// -------------------------------------------------------------------------------------------------
// <copyright file="FetchCommand.cs" company="Starion Group S.A.">
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
    using System;
    using System.CommandLine;
    using System.Net.Http;

    using Spectre.Console;

    using Uml4Net.Codex.Knowledge;

    /// <summary>
    /// <c>uml4net-codex fetch</c>: downloads a UML version's XMI (and, unless <c>--no-specs</c>, PDF) files
    /// directly from omg.org - plain HTTPS GETs, no GitHub mirror/API involved.
    /// </summary>
    public static class FetchCommand
    {
        private static readonly Option<bool> NoSpecsOption = new("--no-specs") { Description = "Skip downloading the specification PDFs (metamodel/standard-profile knowledge will still be generated; spec citation will degrade to clause numbers only)." };
        private static readonly Option<bool> NoDefaultOption = new("--no-default") { Description = "Don't set this version as the default after fetching." };

        /// <summary>
        /// Builds the command.
        /// </summary>
        public static Command Build()
        {
            var command = new Command("fetch", "Download a UML version's XMI and (optionally) specification PDF files from omg.org.");
            command.Add(GlobalOptions.RepositoryRoot);
            command.Add(GlobalOptions.Version);
            command.Add(NoSpecsOption);
            command.Add(NoDefaultOption);

            command.SetAction(async (parseResult, cancellationToken) =>
            {
                var version = parseResult.GetValue(GlobalOptions.Version)!;
                if (!KnownUmlVersions.TryFind(version, out var descriptor))
                {
                    AnsiConsole.MarkupLineInterpolated($"[red]Unknown UML version '{version}'.[/] Known versions: {string.Join(", ", System.Linq.Enumerable.Select(KnownUmlVersions.All, v => v.Version))}");
                    return 1;
                }

                var layout = new RepositoryLayout(RepositoryRootResolver.Resolve(parseResult.GetValue(GlobalOptions.RepositoryRoot)));
                var includeSpecs = !parseResult.GetValue(NoSpecsOption);
                var setAsDefault = !parseResult.GetValue(NoDefaultOption);

                using var httpClient = new HttpClient();
                var service = new KnowledgeGenerationService(new SourceFetcher(httpClient), new PythonSpecExtractRunner(new ProcessRunner()));

                AnsiConsole.MarkupLineInterpolated($"Fetching UML {descriptor!.Version}{(includeSpecs ? " (including specification PDFs)" : "")}...");
                var outcome = await service.FetchAsync(layout, descriptor, includeSpecs, setAsDefault, cancellationToken);

                foreach (var entry in outcome.Entries)
                {
                    AnsiConsole.MarkupLineInterpolated($"  [green]fetched[/] {entry.File} (sha256 {entry.Sha256[..12]}...)");
                }

                AnsiConsole.MarkupLineInterpolated($"[green]Done.[/] Run 'uml4net-codex generate --version {descriptor.Version}' next.");
                return 0;
            });

            return command;
        }
    }
}
