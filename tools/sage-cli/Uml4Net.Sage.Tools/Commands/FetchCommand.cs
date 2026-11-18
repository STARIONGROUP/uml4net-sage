// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Tools.Commands
{
    using System;
    using System.CommandLine;
    using System.Net.Http;

    using Spectre.Console;

    using Uml4Net.Sage.Knowledge;

    /// <summary>
    /// <c>uml4net-sage fetch</c>: downloads a UML version's XMI (and, unless <c>--no-specs</c>, PDF) files
    /// directly from omg.org - plain HTTPS GETs, no GitHub mirror/API involved.
    /// </summary>
    public static class FetchCommand
    {
        private static readonly Option<bool> NoSpecsOption = new("--no-specs") { Description = "Skip downloading the specification PDFs (metamodel/standard-profile knowledge will still be generated; spec citation will degrade to clause numbers only)." };
        private static readonly Option<bool> NoDefaultOption = new("--no-default") { Description = "Don't set this version as the default after fetching." };

        /// <summary>
        /// Builds the command.
        /// </summary>
        /// <returns>The <c>fetch</c> command with its options and action.</returns>
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
                using var gitHubClient = UvProvisioning.CreateGitHubClient();
                var specExtractRunner = new PythonSpecExtractRunner(new ProcessRunner(), UvProvisioning.Create(gitHubClient));
                var service = new KnowledgeGenerationService(new SourceFetcher(httpClient), specExtractRunner);

                AnsiConsole.MarkupLineInterpolated($"Fetching UML {descriptor!.Version}{(includeSpecs ? " (including specification PDFs)" : "")}...");
                var outcome = await service.FetchAsync(layout, descriptor, includeSpecs, setAsDefault, cancellationToken);

                foreach (var entry in outcome.Entries)
                {
                    AnsiConsole.MarkupLineInterpolated($"  [green]fetched[/] {entry.File} (sha256 {entry.Sha256[..12]}...)");
                }

                if (includeSpecs)
                {
                    var xmiOutcome = await service.FetchXmiSpecAsync(layout, KnownXmiVersions.Current, cancellationToken);
                    if (xmiOutcome.Succeeded)
                    {
                        foreach (var entry in xmiOutcome.Entries)
                        {
                            AnsiConsole.MarkupLineInterpolated($"  [green]fetched[/] {entry.File} (sha256 {entry.Sha256[..12]}...)");
                        }
                    }
                    else
                    {
                        AnsiConsole.MarkupLineInterpolated($"  [yellow]warning[/] {xmiOutcome.Warning}");
                    }
                }

                AnsiConsole.MarkupLineInterpolated($"[green]Done.[/] Run 'uml4net-sage generate --version {descriptor.Version}' next.");
                return 0;
            });

            return command;
        }
    }
}
