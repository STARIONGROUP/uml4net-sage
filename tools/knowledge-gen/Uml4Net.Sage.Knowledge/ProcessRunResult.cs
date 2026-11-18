// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge
{
    /// <summary>
    /// The result of running an external process.
    /// </summary>
    /// <param name="ExitCode">The process's exit code.</param>
    /// <param name="StandardOutput">Everything the process wrote to standard output.</param>
    /// <param name="StandardError">Everything the process wrote to standard error.</param>
    public sealed record ProcessRunResult(int ExitCode, string StandardOutput, string StandardError);
}
