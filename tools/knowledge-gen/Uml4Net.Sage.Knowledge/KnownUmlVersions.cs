// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The static, hand-maintained registry of UML specification versions this tool knows how to fetch.
    /// </summary>
    /// <remarks>
    /// Unlike mycelium-hypha (which discovers SysML v2 releases by querying two upstream GitHub repos
    /// and intersecting their tags), UML has no realistic near-term release cadence and no GitHub-hosted
    /// mirror - so this is a plain, hand-updated list. Adding a future UML version (e.g. a hypothetical
    /// 2.6 or 3.0) is a one-line addition here, not a discovery-system change.
    /// </remarks>
    public static class KnownUmlVersions
    {
        /// <summary>
        /// Every UML specification version this tool can fetch and generate a knowledge base for.
        /// </summary>
        public static readonly IReadOnlyList<UmlVersionDescriptor> All =
        [
            new UmlVersionDescriptor(
                Version: "2.5.1",
                IsCurrent: true,
                MetamodelXmiUrl: "https://www.omg.org/spec/UML/20161101/UML.xmi",
                PrimitiveTypesXmiUrl: "https://www.omg.org/spec/UML/20161101/PrimitiveTypes.xmi",
                StandardProfileXmiUrl: "https://www.omg.org/spec/UML/20161101/StandardProfile.xmi",
                DiagramInterchangeXmiUrl: "https://www.omg.org/spec/UML/20161101/UMLDI.xmi",
                SpecificationPdfUrl: "https://www.omg.org/spec/UML/2.5.1/PDF",
                ChangebarPdfUrl: "https://www.omg.org/spec/UML/2.5.1/PDF/changebar",
                OmgDocumentId: "formal/17-12-05"),
        ];

        /// <summary>
        /// Gets the version marked <see cref="UmlVersionDescriptor.IsCurrent"/>.
        /// </summary>
        public static UmlVersionDescriptor Current => All.First(v => v.IsCurrent);

        /// <summary>
        /// Attempts to find a known version by its version string.
        /// </summary>
        /// <param name="version">The version string to look up, e.g. "2.5.1".</param>
        /// <param name="descriptor">The matching descriptor, or <c>null</c> when the version is unknown.</param>
        /// <returns><c>true</c> when the version is known; otherwise <c>false</c>.</returns>
        public static bool TryFind(string version, out UmlVersionDescriptor? descriptor)
        {
            descriptor = All.FirstOrDefault(v => v.Version == version);
            return descriptor is not null;
        }
    }
}
