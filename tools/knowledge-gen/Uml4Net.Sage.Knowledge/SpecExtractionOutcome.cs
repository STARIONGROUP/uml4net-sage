// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge
{
    /// <summary>
    /// The outcome of the <c>spec</c> generation step, which never throws - it either succeeds or is
    /// skipped with a human-readable reason (PDFs not fetched, Python unavailable, extraction failed).
    /// </summary>
    /// <param name="Succeeded">Whether the extraction succeeded.</param>
    /// <param name="SkippedReason">Why the step was skipped, or <c>null</c> when it succeeded.</param>
    public sealed record SpecExtractionOutcome(bool Succeeded, string? SkippedReason)
    {
        /// <summary>
        /// Creates a successful outcome.
        /// </summary>
        /// <returns>An outcome with <see cref="Succeeded"/> set and no skipped reason.</returns>
        public static SpecExtractionOutcome Ok() => new(true, null);

        /// <summary>
        /// Creates a skipped outcome with the given human-readable reason.
        /// </summary>
        /// <param name="reason">Why the step was skipped.</param>
        /// <returns>An unsuccessful outcome carrying <paramref name="reason"/>.</returns>
        public static SpecExtractionOutcome Skipped(string reason) => new(false, reason);
    }
}
