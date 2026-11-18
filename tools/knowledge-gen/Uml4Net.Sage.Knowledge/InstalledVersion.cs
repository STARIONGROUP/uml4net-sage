// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge
{
    /// <summary>
    /// One entry of <see cref="InstalledVersionsManifest.Versions"/>.
    /// </summary>
    /// <param name="Version">The UML version string, e.g. "2.5.1".</param>
    /// <param name="Fetched">Whether the version's sources have been fetched.</param>
    /// <param name="Generated">Whether the version's metamodel and standard-profile knowledge has been generated.</param>
    /// <param name="SpecGenerated">Whether the version's specification clause text has been extracted.</param>
    public sealed record InstalledVersion(string Version, bool Fetched, bool Generated, bool SpecGenerated);
}
