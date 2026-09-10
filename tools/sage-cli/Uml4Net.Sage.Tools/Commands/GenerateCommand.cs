// -------------------------------------------------------------------------------------------------
// <copyright file="GenerateCommand.cs" company="Starion Group S.A.">
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
    using System.Net.Http;

    using Spectre.Console;

    using Uml4Net.Sage.Knowledge;
    using Uml4Net.Sage.Knowledge.Toolchain;

    /// <summary>
    /// <c>uml4net-sage generate</c>: regenerates the metamodel, standard-profile, (if the PDF and Python
    /// are available) spec text, and the scoped <c>datapackage.json</c> for an already-fetched UML version.
    /// </summary>
    public static class GenerateCommand
    {
        /// <summary>
        /// Builds the command.
        /// </summary>
        public static Command Build()
        {
            var command = new Command("generate", "Generate the metamodel/standard-profile/spec knowledge base from an already-fetched UML version's sources.");
            command.Add(GlobalOptions.RepositoryRoot);
            command.Add(GlobalOptions.Version);

            command.SetAction(async (parseResult, cancellationToken) =>
            {
                var version = parseResult.GetValue(GlobalOptions.Version)!;
                if (!KnownUmlVersions.TryFind(version, out var descriptor))
                {
                    AnsiConsole.MarkupLineInterpolated($"[red]Unknown UML version '{version}'.[/]");
                    return 1;
                }

                var layout = new RepositoryLayout(RepositoryRootResolver.Resolve(parseResult.GetValue(GlobalOptions.RepositoryRoot)));
                var xmiDirectory = Path.Combine(layout.SourcesDirectoryFor(descriptor!.Version), "xmi");
                if (!File.Exists(Path.Combine(xmiDirectory, "UML.xmi")))
                {
                    AnsiConsole.MarkupLineInterpolated($"[red]UML {descriptor.Version} has not been fetched yet.[/] Run 'uml4net-sage fetch --version {descriptor.Version}' first.");
                    return 1;
                }

                using var httpClient = new HttpClient();
                var specExtractRunner = new PythonSpecExtractRunner(new ProcessRunner(), new UvProvisioner(httpClient));
                var service = new KnowledgeGenerationService(new SourceFetcher(httpClient), specExtractRunner);

                AnsiConsole.MarkupLineInterpolated($"Generating knowledge base for UML {descriptor.Version}...");
                var outcome = await service.GenerateAsync(layout, descriptor, cancellationToken);

                AnsiConsole.MarkupLine("  [green]done[/] metamodel/");
                AnsiConsole.MarkupLine("  [green]done[/] standard-profile/");
                if (outcome.SpecExtraction.Succeeded)
                {
                    AnsiConsole.MarkupLine("  [green]done[/] spec/ (verbatim spec citation available)");
                }
                else
                {
                    AnsiConsole.MarkupLineInterpolated($"  [yellow]skipped[/] spec/ - {outcome.SpecExtraction.SkippedReason}");
                }

                if (outcome.XmiSpecExtraction.Succeeded)
                {
                    AnsiConsole.MarkupLine("  [green]done[/] xmi-spec/ (XMI specification citation available)");
                }
                else
                {
                    AnsiConsole.MarkupLineInterpolated($"  [yellow]skipped[/] xmi-spec/ - {outcome.XmiSpecExtraction.SkippedReason}");
                }

                AnsiConsole.MarkupLine("  [green]done[/] datapackage.json");
                return 0;
            });

            return command;
        }
    }
}
