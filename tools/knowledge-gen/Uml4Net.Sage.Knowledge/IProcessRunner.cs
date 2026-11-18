// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The result of running an external process.
    /// </summary>
    /// <param name="ExitCode">The process's exit code.</param>
    /// <param name="StandardOutput">Everything the process wrote to standard output.</param>
    /// <param name="StandardError">Everything the process wrote to standard error.</param>
    public sealed record ProcessRunResult(int ExitCode, string StandardOutput, string StandardError);

    /// <summary>
    /// Abstracts running an external process, so the one place C# shells out to Python (the <c>spec</c>
    /// generation step) can be tested without actually invoking Python.
    /// </summary>
    public interface IProcessRunner
    {
        /// <summary>
        /// Runs <paramref name="fileName"/> with <paramref name="arguments"/> and returns its result.
        /// </summary>
        /// <param name="fileName">The executable to run.</param>
        /// <param name="arguments">The arguments, passed one by one without shell quoting.</param>
        /// <param name="workingDirectory">The working directory; <c>null</c> keeps the current one.</param>
        /// <param name="cancellationToken">Cancels waiting for the process.</param>
        /// <returns>The exit code and everything the process wrote to standard output and standard error.</returns>
        /// <exception cref="System.ComponentModel.Win32Exception">
        /// Thrown when <paramref name="fileName"/> cannot be found/started.
        /// </exception>
        Task<ProcessRunResult> RunAsync(string fileName, IReadOnlyList<string> arguments, string? workingDirectory = null, CancellationToken cancellationToken = default);
    }
}
