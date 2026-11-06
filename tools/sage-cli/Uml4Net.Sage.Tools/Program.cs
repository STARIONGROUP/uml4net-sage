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
        /// The verb's exit code; 1 when a command line with <c>--json</c> does not parse, after writing the
        /// parse errors as one <c>{"error": "..."}</c> document.
        /// </returns>
        public static async Task<int> Main(string[] args)
        {
            var parseResult = BuildRootCommand().Parse(args);

            // A command line that does not parse never reaches a verb, and System.CommandLine would print
            // its errors and help as text. A --json caller gets one error document instead. The raw
            // arguments are all there is to go on, so any spelling of the option counts, whatever its
            // value: 'check --json false --bogus' also gets a JSON error.
            if (parseResult.Errors.Count > 0 && args.Any(JsonOutput.IsJsonArgument))
            {
                JsonOutput.WriteError(string.Join(" ", parseResult.Errors.Select(error => error.Message)));
                return 1;
            }

            return await parseResult.InvokeAsync();
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
