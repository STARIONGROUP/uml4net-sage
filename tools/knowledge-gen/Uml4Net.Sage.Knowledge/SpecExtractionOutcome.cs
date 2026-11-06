// -------------------------------------------------------------------------------------------------
// <copyright file="SpecExtractionOutcome.cs" company="Starion Group S.A.">
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
