// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The static, hand-maintained registry of OMG XMI specification versions this tool knows how to
    /// fetch - the companion serialization standard alongside <see cref="KnownUmlVersions"/>. Like UML,
    /// XMI has no realistic near-term release cadence and no GitHub-hosted mirror, so this is a plain,
    /// hand-updated list rather than a discovery system.
    /// </summary>
    public static class KnownXmiVersions
    {
        /// <summary>
        /// Every XMI specification version this tool can fetch and extract a knowledge base for.
        /// </summary>
        public static readonly IReadOnlyList<XmiSpecDescriptor> All =
        [
            new XmiSpecDescriptor(
                Version: "2.5.1",
                IsCurrent: true,
                SpecificationPdfUrl: "https://www.omg.org/spec/XMI/2.5.1/PDF",
                OmgDocumentId: "formal/15-06-07",
                // Both schema files are published under dated paths, not the "2.5.1" version path
                // the PDF uses above - confirmed against the real files, not a typo.
                XsdUrl: "https://www.omg.org/spec/XMI/20131001/XMI.xsd",
                CanonicalXsdUrl: "https://www.omg.org/spec/XMI/20110501/XMI-Canonical.xsd"),
        ];

        /// <summary>
        /// Gets the version marked <see cref="XmiSpecDescriptor.IsCurrent"/>.
        /// </summary>
        public static XmiSpecDescriptor Current => All.First(v => v.IsCurrent);

        /// <summary>
        /// Attempts to find a known version by its version string.
        /// </summary>
        /// <param name="version">The version string to look up, e.g. "2.5.1".</param>
        /// <param name="descriptor">The matching descriptor, or <c>null</c> when the version is unknown.</param>
        /// <returns><c>true</c> when the version is known; otherwise <c>false</c>.</returns>
        public static bool TryFind(string version, out XmiSpecDescriptor? descriptor)
        {
            descriptor = All.FirstOrDefault(v => v.Version == version);
            return descriptor is not null;
        }
    }
}
