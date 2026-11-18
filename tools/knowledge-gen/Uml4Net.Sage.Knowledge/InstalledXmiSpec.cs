// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge
{
    /// <summary>
    /// One entry of <see cref="InstalledVersionsManifest.XmiSpecs"/>. Tracked separately from
    /// <see cref="InstalledVersion"/> because the OMG XMI specification versions independently of UML -
    /// it is a top-level sibling corpus, not a per-UML-version one (see
    /// <c>knowledge/xmi/&lt;version&gt;/</c>).
    /// </summary>
    /// <param name="Version">The XMI specification version string, e.g. "2.5.1".</param>
    /// <param name="Fetched">Whether the specification's PDF has been fetched.</param>
    /// <param name="Generated">Whether the specification's clause text has been extracted.</param>
    public sealed record InstalledXmiSpec(string Version, bool Fetched, bool Generated);
}
