// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Inspection
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// One captured log entry.
    /// </summary>
    /// <param name="Level">The entry's level.</param>
    /// <param name="Category">The logger category, i.e. the full name of the logging type.</param>
    /// <param name="Message">The formatted message.</param>
    public sealed record CapturedLogEntry(LogLevel Level, string Category, string Message);
}
