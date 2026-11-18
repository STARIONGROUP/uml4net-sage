// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge
{
    using System.Collections.Generic;

    /// <summary>
    /// The git-ignored, local-only record of which UML versions (and companion XMI specification
    /// versions) have been fetched/generated on this machine, and which UML version is the default.
    /// Written to <c>knowledge/installed.json</c>.
    /// </summary>
    /// <param name="Default">The default UML version, or <c>null</c> when none is set.</param>
    /// <param name="Versions">The recorded UML versions.</param>
    /// <param name="XmiSpecs">The recorded XMI specification versions.</param>
    public sealed record InstalledVersionsManifest(string? Default, List<InstalledVersion> Versions, List<InstalledXmiSpec> XmiSpecs)
    {
        /// <summary>
        /// An empty manifest, used when no <c>installed.json</c> exists yet.
        /// </summary>
        public static InstalledVersionsManifest Empty => new(null, [], []);
    }
}
