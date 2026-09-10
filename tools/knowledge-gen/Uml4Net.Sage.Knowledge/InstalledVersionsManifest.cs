// -------------------------------------------------------------------------------------------------
// <copyright file="InstalledVersionsManifest.cs" company="Starion Group S.A.">
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
    using System.Collections.Generic;

    /// <summary>
    /// One entry of <see cref="InstalledVersionsManifest.Versions"/>.
    /// </summary>
    public sealed record InstalledVersion(string Version, bool Fetched, bool Generated, bool SpecGenerated);

    /// <summary>
    /// One entry of <see cref="InstalledVersionsManifest.XmiSpecs"/>. Tracked separately from
    /// <see cref="InstalledVersion"/> because the OMG XMI specification versions independently of UML -
    /// it is a top-level sibling corpus, not a per-UML-version one (see
    /// <c>knowledge/xmi/&lt;version&gt;/</c>).
    /// </summary>
    public sealed record InstalledXmiSpec(string Version, bool Fetched, bool Generated);

    /// <summary>
    /// The git-ignored, local-only record of which UML versions (and companion XMI specification
    /// versions) have been fetched/generated on this machine, and which UML version is the default.
    /// Written to <c>knowledge/installed.json</c>.
    /// </summary>
    public sealed record InstalledVersionsManifest(string? Default, List<InstalledVersion> Versions, List<InstalledXmiSpec> XmiSpecs)
    {
        /// <summary>
        /// An empty manifest, used when no <c>installed.json</c> exists yet.
        /// </summary>
        public static InstalledVersionsManifest Empty => new(null, [], []);
    }
}
