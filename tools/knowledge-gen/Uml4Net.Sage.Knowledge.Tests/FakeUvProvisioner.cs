// -------------------------------------------------------------------------------------------------
// <copyright file="FakeUvProvisioner.cs" company="Starion Group S.A.">
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
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Uml4Net.Sage.Knowledge.Toolchain;

    /// <summary>
    /// A hand-rolled <see cref="IUvProvisioner"/> test double: no real download/cache lookup in unit tests.
    /// </summary>
    public sealed class FakeUvProvisioner : IUvProvisioner
    {
        private readonly FileInfo? executable;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeUvProvisioner"/> class. Use the static factory
        /// methods instead.
        /// </summary>
        /// <param name="executable">The executable <see cref="EnsureAsync"/> returns, or <c>null</c>.</param>
        private FakeUvProvisioner(FileInfo? executable)
        {
            this.executable = executable;
        }

        /// <summary>
        /// Creates a provisioner that reports <c>uv</c> as unavailable.
        /// </summary>
        /// <returns>A provisioner whose <see cref="EnsureAsync"/> returns <c>null</c>.</returns>
        public static FakeUvProvisioner Unavailable() => new(null);

        /// <summary>
        /// Creates a provisioner that reports <paramref name="executable"/> as the cached <c>uv</c>.
        /// </summary>
        /// <param name="executable">The executable to return.</param>
        /// <returns>A provisioner whose <see cref="EnsureAsync"/> returns <paramref name="executable"/>.</returns>
        public static FakeUvProvisioner Returning(FileInfo executable) => new(executable);

        /// <summary>
        /// Returns the configured executable without downloading anything.
        /// </summary>
        /// <param name="cancellationToken">Ignored.</param>
        /// <returns>The configured executable, or <c>null</c> when <c>uv</c> is unavailable.</returns>
        public Task<FileInfo?> EnsureAsync(CancellationToken cancellationToken = default) => Task.FromResult(this.executable);
    }
}
