// -------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Sage.Tools
{
    using System.CommandLine;
    using System.CommandLine.Help;
    using System.Linq;
    using System.Threading.Tasks;

    using Uml4Net.Sage.Tools.Commands;

    /// <summary>
    /// The <c>uml4net-sage</c> entry point.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Runs the command line.
        /// </summary>
        /// <param name="args">The verb and its options.</param>
        /// <returns>
        /// The verb's exit code; 1 when a command line with <c>--json</c> cannot produce a JSON document (it
        /// does not parse, names a verb without <c>--json</c>, or asks for help), after writing why as one
        /// <c>{"error": "..."}</c> document.
        /// </returns>
        public static async Task<int> Main(string[] args)
        {
            var root = BuildRootCommand();
            var parseResult = root.Parse(args);

            // The raw arguments decide whether the caller wants JSON: a command line that does not parse
            // has no reliable option values. So any spelling of the option counts, whatever its value:
            // 'check --json false --bogus' also gets a JSON error.
            if (args.Any(JsonOutput.IsJsonArgument))
            {
                var error = JsonCommandLineError(root, parseResult);
                if (error is not null)
                {
                    JsonOutput.WriteError(error);
                    return 1;
                }
            }

            return await parseResult.InvokeAsync();
        }

        /// <summary>
        /// Gets why a command line that asks for <c>--json</c> cannot run as a JSON verb, so the caller gets
        /// one error document instead of System.CommandLine's text.
        /// </summary>
        /// <param name="root">The root command, which lists the verbs.</param>
        /// <param name="parseResult">The parsed command line.</param>
        /// <returns>
        /// The error message, or <c>null</c> when the command line can run: a verb that offers <c>--json</c>,
        /// no parse errors, and not a request for help.
        /// </returns>
        internal static string? JsonCommandLineError(RootCommand root, ParseResult parseResult)
        {
            var command = parseResult.CommandResult.Command;

            if (!command.Options.Contains(GlobalOptions.Json))
            {
                var supported = string.Join(", ", root.Subcommands.Where(verb => verb.Options.Contains(GlobalOptions.Json)).Select(verb => verb.Name));
                var target = command == root ? "a command line without a verb" : $"'{command.Name}'";
                return $"--json is only supported by {supported}, not by {target}.";
            }

            if (parseResult.Errors.Count > 0)
            {
                return string.Join(" ", parseResult.Errors.Select(error => error.Message));
            }

            if (parseResult.Action is HelpAction)
            {
                return $"--help prints text and cannot be combined with --json; run 'uml4net-sage {command.Name} --help' to read it.";
            }

            return null;
        }

        /// <summary>
        /// Assembles the root command and its verbs.
        /// </summary>
        /// <returns>The root command the tool exposes.</returns>
        internal static RootCommand BuildRootCommand()
        {
            var root = new RootCommand("uml4net-sage: fetch, generate and query the OMG UML 2.5.1 knowledge base.");
            root.Add(VersionsCommand.Build());
            root.Add(FetchCommand.Build());
            root.Add(GenerateCommand.Build());
            root.Add(UseCommand.Build());
            root.Add(RemoveCommand.Build());
            root.Add(CheckCommand.Build());
            root.Add(InspectCommand.Build());

            return root;
        }
    }
}
