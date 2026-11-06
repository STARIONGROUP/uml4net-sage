// -------------------------------------------------------------------------------------------------
// <copyright file="JsonOutput.cs" company="Starion Group S.A.">
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
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;

    using Spectre.Console;

    /// <summary>
    /// The rules every verb's <c>--json</c> output follows, so the SessionStart hook, scripts and other
    /// hosts can rely on them for every verb.
    /// </summary>
    /// <remarks>
    /// Standard output carries exactly one line holding one JSON document: no banner, no progress or
    /// log lines, no wrapping. A failure is a document too, <c>{"error": "..."}</c>, with a non-zero
    /// exit code, whether the verb reports it or <c>Program</c> does before the verb runs. The property
    /// names are the contract: <c>JsonOutputTests</c> pins them, and tools/README.md documents them.
    /// <para>
    /// Ctrl+C is the one exception: System.CommandLine only handles it while an asynchronous action is
    /// still running, and these verbs run synchronously, so the process ends without a document. A
    /// caller treats a missing or unparseable document as a failure.
    /// </para>
    /// </remarks>
    public static class JsonOutput
    {
        /// <summary>
        /// The exit code when the caller's cancellation token is already cancelled as the verb starts
        /// (128 + SIGINT, the shell convention).
        /// </summary>
        public const int CancelledExitCode = 130;

        /// <summary>
        /// The serializer settings of every <c>--json</c> document.
        /// </summary>
        /// <remarks>
        /// Property names are written as declared (PascalCase): the committed hook binary and the
        /// skills read them that way. Absent values are written as <c>null</c> rather than left out, so
        /// every property is always there. The default encoder is kept on purpose: it writes every
        /// non-ASCII character as <c>\uXXXX</c>, so the document is pure ASCII and survives whatever code
        /// page standard output uses. On Windows a redirected standard output uses the console code page,
        /// not UTF-8, so raw non-ASCII text in a path would be corrupted.
        /// </remarks>
        public static readonly JsonSerializerOptions Format = new(JsonSerializerDefaults.General)
        {
            WriteIndented = false,
        };

        /// <summary>
        /// Gets whether the verb that runs was asked for JSON.
        /// </summary>
        /// <param name="parseResult">The parsed command line.</param>
        /// <returns><c>true</c> when the verb declares <see cref="GlobalOptions.Json"/> and it is set.</returns>
        public static bool IsRequested(ParseResult parseResult)
        {
            ArgumentNullException.ThrowIfNull(parseResult);

            return parseResult.CommandResult.Command.Options.Contains(GlobalOptions.Json)
                && parseResult.GetValue(GlobalOptions.Json);
        }

        /// <summary>
        /// Gets whether a raw command-line argument sets the <c>--json</c> option, in any of the
        /// spellings System.CommandLine accepts: <c>--json</c>, <c>--json:value</c> or <c>--json=value</c>.
        /// </summary>
        /// <param name="argument">One argument as the shell passed it.</param>
        /// <returns><c>true</c> when the argument is the <c>--json</c> option.</returns>
        public static bool IsJsonArgument(string argument)
        {
            ArgumentNullException.ThrowIfNull(argument);

            var name = GlobalOptions.Json.Name;
            return string.Equals(argument, name, StringComparison.Ordinal)
                || argument.StartsWith(name + ":", StringComparison.Ordinal)
                || argument.StartsWith(name + "=", StringComparison.Ordinal);
        }

        /// <summary>
        /// Writes one document as one line to standard output.
        /// </summary>
        /// <typeparam name="T">The document type; its property names are the contract with the host.</typeparam>
        /// <param name="document">The document to write.</param>
        /// <remarks>
        /// Straight to the console's writer rather than through a Spectre widget, which would wrap a
        /// long line at the console width.
        /// </remarks>
        public static void Write<T>(T document)
        {
            var writer = AnsiConsole.Profile.Out.Writer;
            writer.WriteLine(JsonSerializer.Serialize(document, Format));
            writer.Flush();
        }

        /// <summary>
        /// Writes a failure as <c>{"error": "..."}</c>.
        /// </summary>
        /// <param name="message">What went wrong and, where there is one, what to do about it.</param>
        public static void WriteError(string message) => Write(new JsonError(message));

        /// <summary>
        /// Reports a failure: as <c>{"error": "..."}</c> when the verb was asked for JSON, and as a red
        /// sentence otherwise.
        /// </summary>
        /// <param name="parseResult">The parsed command line; decides the form.</param>
        /// <param name="message">What went wrong, as a plain sentence (no Spectre markup).</param>
        /// <param name="exitCode">The exit code to return.</param>
        /// <returns><paramref name="exitCode"/>, so a verb can return the call directly.</returns>
        public static int Fail(ParseResult parseResult, string message, int exitCode = 1)
        {
            if (IsRequested(parseResult))
            {
                WriteError(message);
                return exitCode;
            }

            AnsiConsole.MarkupLine($"[red]{Markup.Escape(message)}[/]");
            return exitCode;
        }

        /// <summary>
        /// Runs a verb's action and turns its failures into a reported failure, so a <c>--json</c>
        /// caller always gets one error document instead of a stack trace.
        /// </summary>
        /// <param name="parseResult">The parsed command line of the verb.</param>
        /// <param name="cancellationToken">
        /// The caller's token. Only checked before the action starts; see the remarks on <see cref="JsonOutput"/>
        /// for why Ctrl+C does not reach it.
        /// </param>
        /// <param name="action">The verb's work; returns its exit code.</param>
        /// <returns>
        /// The action's exit code; 130 when cancelled; 1 when a file could not be read, or, with <c>--json</c>,
        /// when anything else went wrong.
        /// </returns>
        /// <remarks>
        /// Without <c>--json</c> an unexpected exception is not caught, so a bug still shows its stack trace.
        /// </remarks>
        public static Task<int> RunAsync(ParseResult parseResult, CancellationToken cancellationToken, Func<int> action)
        {
            ArgumentNullException.ThrowIfNull(action);

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(action());
            }
            catch (OperationCanceledException)
            {
                return Task.FromResult(Fail(parseResult, "Cancelled.", CancelledExitCode));
            }
            catch (Exception exception) when (exception is JsonException or XmlException or IOException or InvalidDataException or UnauthorizedAccessException)
            {
                return Task.FromResult(Fail(parseResult, $"A file could not be read: {exception.Message}"));
            }
            catch (Exception exception) when (IsRequested(parseResult))
            {
                return Task.FromResult(Fail(parseResult, $"Unexpected error ({exception.GetType().Name}): {exception.Message}"));
            }
        }

        /// <summary>
        /// The document a failed <c>--json</c> verb writes.
        /// </summary>
        /// <param name="Error">What went wrong.</param>
        internal sealed record JsonError([property: JsonPropertyName("error")] string Error);
    }
}
