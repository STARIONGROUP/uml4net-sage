// -------------------------------------------------------------------------------------------------
// <copyright file="RecordedConsole.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Sage.Tools.Tests
{
    using System;
    using System.IO;

    using Spectre.Console;

    /// <summary>
    /// Redirects what the verbs write to <see cref="AnsiConsole"/>, and anything written straight to
    /// <see cref="Console.Out"/> or <see cref="Console.Error"/> (such as System.CommandLine's own parse
    /// errors and help), into strings, and restores all three afterwards.
    /// </summary>
    internal sealed class RecordedConsole : IDisposable
    {
        private readonly IAnsiConsole previous;
        private readonly TextWriter previousOut;
        private readonly TextWriter previousError;
        private readonly StringWriter writer = new();
        private readonly StringWriter standardOutput = new();
        private readonly StringWriter standardError = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordedConsole"/> class.
        /// </summary>
        public RecordedConsole()
        {
            this.previousOut = Console.Out;
            this.previousError = Console.Error;
            Console.SetOut(this.standardOutput);
            Console.SetError(this.standardError);

            this.previous = AnsiConsole.Console;
            AnsiConsole.Console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Ansi = AnsiSupport.No,
                ColorSystem = ColorSystemSupport.NoColors,
                Interactive = InteractionSupport.No,
                Out = new AnsiConsoleOutput(this.writer),
            });

            // Wide enough that a sentence holding a temp path is not wrapped across lines.
            AnsiConsole.Console.Profile.Width = 1000;
        }

        /// <summary>
        /// Gets everything written through <see cref="AnsiConsole"/> since this instance was created.
        /// </summary>
        public string Output => this.writer.ToString();

        /// <summary>
        /// Gets <see cref="Output"/> split into lines, without the final line ending.
        /// </summary>
        public string[] Lines => this.Output.TrimEnd('\r', '\n').Split('\n');

        /// <summary>
        /// Gets what was written straight to <see cref="Console.Out"/>, bypassing <see cref="AnsiConsole"/>.
        /// </summary>
        public string StandardOutput => this.standardOutput.ToString();

        /// <summary>
        /// Gets what was written to <see cref="Console.Error"/>.
        /// </summary>
        public string StandardError => this.standardError.ToString();

        /// <summary>
        /// Restores the console and the standard writers that were active before this instance was created.
        /// </summary>
        public void Dispose()
        {
            AnsiConsole.Console = this.previous;
            Console.SetOut(this.previousOut);
            Console.SetError(this.previousError);
            this.writer.Dispose();
            this.standardOutput.Dispose();
            this.standardError.Dispose();
        }
    }
}
