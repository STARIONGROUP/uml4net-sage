// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge
{
    using System.Collections.Generic;

    /// <summary>
    /// The result of a <see cref="KnowledgeGenerationService.FetchAsync"/> call.
    /// </summary>
    /// <param name="Entries">The manifest entries of the downloaded files.</param>
    public sealed record FetchOutcome(IReadOnlyList<FetchManifestEntry> Entries);
}
