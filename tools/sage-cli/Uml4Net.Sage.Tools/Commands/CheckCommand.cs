// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Tools.Commands
{
    using System.CommandLine;

    using Spectre.Console;

    using Uml4Net.Sage.Knowledge;

    /// <summary>
    /// <c>uml4net-sage check</c>: a fully offline, file-existence-only status check - no network call,
    /// unlike mycelium-hypha's <c>check</c> (which compares against upstream GitHub releases). This is
    /// what the plugin's SessionStart hook runs every session.
    /// </summary>
    public static class CheckCommand
    {
        /// <summary>
        /// Builds the command.
        /// </summary>
        /// <returns>The <c>check</c> command with its options and action.</returns>
        public static Command Build()
        {
            var command = new Command("check", "Report local fetch/generate status - offline, no network call.");
            command.Add(GlobalOptions.RepositoryRoot);
            command.Add(GlobalOptions.Json);

            command.SetAction((parseResult, cancellationToken) => JsonOutput.RunAsync(parseResult, cancellationToken, () =>
            {
                var layout = new RepositoryLayout(RepositoryRootResolver.Resolve(parseResult.GetValue(GlobalOptions.RepositoryRoot)));
                var manifest = new InstalledVersionsStore(layout.KnowledgeRoot).Load();

                var result = new
                {
                    manifest.Default,
                    HasAnyVersion = manifest.Versions.Count > 0,
                    Versions = manifest.Versions,
                    XmiSpecs = manifest.XmiSpecs,
                };

                if (JsonOutput.IsRequested(parseResult))
                {
                    JsonOutput.Write(result);
                    return 0;
                }

                if (manifest.Default is null)
                {
                    AnsiConsole.MarkupLineInterpolated($"No UML version fetched yet. Run 'uml4net-sage fetch --version {KnownUmlVersions.Current.Version}'.");
                    return 0;
                }

                var defaultEntry = manifest.Versions.Find(v => v.Version == manifest.Default);
                if (defaultEntry is { Generated: false })
                {
                    AnsiConsole.MarkupLineInterpolated($"UML {manifest.Default} is fetched but not generated. Run 'uml4net-sage generate --version {manifest.Default}'.");
                    return 0;
                }

                var xmiSpecReady = manifest.XmiSpecs.Find(x => x.Version == KnownXmiVersions.Current.Version) is { Generated: true };

                if (defaultEntry is { SpecGenerated: false })
                {
                    AnsiConsole.MarkupLineInterpolated($"UML {manifest.Default} is ready (spec citation limited to clause numbers - PDFs not fetched or Python unavailable).");
                    if (!xmiSpecReady)
                    {
                        AnsiConsole.MarkupLine("XMI specification citation is not available either.");
                    }

                    return 0;
                }

                AnsiConsole.MarkupLineInterpolated($"UML {manifest.Default} is fully generated, including verbatim spec citation.");
                if (!xmiSpecReady)
                {
                    AnsiConsole.MarkupLine("XMI specification citation is not yet available - see 'uml4net-sage generate' output for why.");
                }

                return 0;
            }));

            return command;
        }
    }
}
