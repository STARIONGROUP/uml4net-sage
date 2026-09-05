// -------------------------------------------------------------------------------------------------
// <copyright file="FetchManifestEntry.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Codex.Knowledge
{
    using System;

    /// <summary>
    /// One row of <c>sources/&lt;version&gt;/fetch-manifest.json</c>: provenance for a single downloaded file.
    /// </summary>
    public sealed record FetchManifestEntry(string Url, string File, string Sha256, DateTimeOffset FetchedAt);
}
