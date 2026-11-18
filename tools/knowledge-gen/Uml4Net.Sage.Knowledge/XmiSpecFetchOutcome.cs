// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge
{
    using System.Collections.Generic;

    /// <summary>
    /// The result of a <see cref="KnowledgeGenerationService.FetchXmiSpecAsync"/> call.
    /// </summary>
    /// <param name="Succeeded">Whether the download completed.</param>
    /// <param name="Warning">A human-readable reason when <paramref name="Succeeded"/> is <see langword="false"/>.</param>
    /// <param name="Entries">The downloaded file's manifest entry, empty on failure.</param>
    public sealed record XmiSpecFetchOutcome(bool Succeeded, string? Warning, IReadOnlyList<FetchManifestEntry> Entries);
}
