// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge
{
    using System;

    /// <summary>
    /// One row of <c>sources/&lt;version&gt;/fetch-manifest.json</c>: provenance for a single downloaded file.
    /// </summary>
    /// <param name="Url">The URL the file was downloaded from.</param>
    /// <param name="File">The file name under <c>sources/&lt;version&gt;/</c>.</param>
    /// <param name="Sha256">The lowercase hexadecimal SHA-256 hash of the downloaded content.</param>
    /// <param name="FetchedAt">When the file was downloaded.</param>
    public sealed record FetchManifestEntry(string Url, string File, string Sha256, DateTimeOffset FetchedAt);
}
