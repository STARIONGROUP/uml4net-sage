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

namespace Uml4Net.Codex.Knowledge.Tests
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

        public IReadOnlyList<string>? LastArguments { get; private set; }

        public string? LastFileName { get; private set; }

        private FakeProcessRunner(ProcessRunResult? result, bool throwNotFound)
        {
            this.result = result;
            this.throwNotFound = throwNotFound;
        }

        public static FakeProcessRunner Returning(ProcessRunResult result) => new(result, false);

        public static FakeProcessRunner NotFound() => new(null, true);

        public Task<ProcessRunResult> RunAsync(string fileName, IReadOnlyList<string> arguments, string? workingDirectory = null, CancellationToken cancellationToken = default)
        {
            this.LastFileName = fileName;
            this.LastArguments = arguments;

            if (this.throwNotFound)
            {
                throw new Win32Exception("The system cannot find the file specified.");
            }

            return Task.FromResult(this.result!);
        }
    }
}
