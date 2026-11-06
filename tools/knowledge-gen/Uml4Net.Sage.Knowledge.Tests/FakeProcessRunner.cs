// -------------------------------------------------------------------------------------------------
// <copyright file="FakeProcessRunner.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Sage.Knowledge.Tests
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A hand-rolled <see cref="IProcessRunner"/> test double: no real Python/process invocation in unit tests.
    /// </summary>
    public sealed class FakeProcessRunner : IProcessRunner
    {
        private readonly ProcessRunResult? result;
        private readonly bool throwNotFound;
        private readonly string? onlyRespondsToFileName;
        private readonly ProcessRunResult? resultForOnlyRespondsToFileName;

        public IReadOnlyList<string>? LastArguments { get; private set; }

        public string? LastFileName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeProcessRunner"/> class. Use the static
        /// factory methods instead.
        /// </summary>
        /// <param name="result">The result returned for any file name without a special result.</param>
        /// <param name="throwNotFound">
        /// Whether to throw a <see cref="Win32Exception"/> for every file name other than
        /// <paramref name="onlyRespondsToFileName"/>.
        /// </param>
        /// <param name="onlyRespondsToFileName">The file name that is treated differently, or <c>null</c>.</param>
        /// <param name="resultForOnlyRespondsToFileName">
        /// The result returned for <paramref name="onlyRespondsToFileName"/>; <c>null</c> falls back to
        /// <paramref name="result"/>.
        /// </param>
        private FakeProcessRunner(ProcessRunResult? result, bool throwNotFound, string? onlyRespondsToFileName = null, ProcessRunResult? resultForOnlyRespondsToFileName = null)
        {
            this.result = result;
            this.throwNotFound = throwNotFound;
            this.onlyRespondsToFileName = onlyRespondsToFileName;
            this.resultForOnlyRespondsToFileName = resultForOnlyRespondsToFileName;
        }

        /// <summary>
        /// Creates a runner that returns <paramref name="result"/> for every file name.
        /// </summary>
        /// <param name="result">The result to return.</param>
        /// <returns>The configured runner.</returns>
        public static FakeProcessRunner Returning(ProcessRunResult result) => new(result, false);

        /// <summary>
        /// Creates a runner that throws a <see cref="Win32Exception"/> for every file name, as if no
        /// executable could be found.
        /// </summary>
        /// <returns>The configured runner.</returns>
        public static FakeProcessRunner NotFound() => new(null, true);

        /// <summary>
        /// Throws "not found" for every file name except <paramref name="respondingFileName"/>,
        /// which returns <paramref name="result"/> - simulates no Python being on <c>PATH</c> while a
        /// specific (e.g. provisioned <c>uv</c>) executable is runnable.
        /// </summary>
        /// <param name="respondingFileName">The only file name that does not throw.</param>
        /// <param name="result">The result returned for <paramref name="respondingFileName"/>.</param>
        /// <returns>The configured runner.</returns>
        public static FakeProcessRunner NotFoundExceptFor(string respondingFileName, ProcessRunResult result) => new(result, true, respondingFileName);

        /// <summary>
        /// Returns <paramref name="defaultResult"/> for every file name except
        /// <paramref name="specialFileName"/>, which returns <paramref name="specialResult"/> - simulates a
        /// plain Python candidate failing one way (e.g. a missing dependency) while a provisioned <c>uv</c>
        /// succeeds (or fails differently).
        /// </summary>
        /// <param name="specialFileName">The file name that gets <paramref name="specialResult"/>.</param>
        /// <param name="specialResult">The result returned for <paramref name="specialFileName"/>.</param>
        /// <param name="defaultResult">The result returned for every other file name.</param>
        /// <returns>The configured runner.</returns>
        public static FakeProcessRunner RespondingDifferentlyFor(string specialFileName, ProcessRunResult specialResult, ProcessRunResult defaultResult) =>
            new(defaultResult, false, specialFileName, specialResult);

        /// <summary>
        /// Records <paramref name="fileName"/> and <paramref name="arguments"/>, then returns the
        /// configured result or throws, without starting a process.
        /// </summary>
        /// <param name="fileName">The executable that would be run.</param>
        /// <param name="arguments">The arguments that would be passed.</param>
        /// <param name="workingDirectory">Ignored.</param>
        /// <param name="cancellationToken">Ignored.</param>
        /// <returns>The configured result for <paramref name="fileName"/>.</returns>
        /// <exception cref="Win32Exception">The runner is configured to report <paramref name="fileName"/> as not found.</exception>
        public Task<ProcessRunResult> RunAsync(string fileName, IReadOnlyList<string> arguments, string? workingDirectory = null, CancellationToken cancellationToken = default)
        {
            this.LastFileName = fileName;
            this.LastArguments = arguments;

            if (this.throwNotFound && fileName != this.onlyRespondsToFileName)
            {
                throw new Win32Exception("The system cannot find the file specified.");
            }

            if (fileName == this.onlyRespondsToFileName && this.resultForOnlyRespondsToFileName is not null)
            {
                return Task.FromResult(this.resultForOnlyRespondsToFileName);
            }

            return Task.FromResult(this.result!);
        }
    }
}
