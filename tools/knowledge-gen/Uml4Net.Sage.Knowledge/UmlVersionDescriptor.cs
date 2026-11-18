// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge
{
    /// <summary>
    /// Everything needed to fetch one published version of the OMG UML specification.
    /// </summary>
    /// <param name="Version">The UML version string, e.g. "2.5.1".</param>
    /// <param name="IsCurrent">Whether this is the current/default version to use when none is specified.</param>
    /// <param name="MetamodelXmiUrl">The Abstract Syntax Metamodel XMI URL.</param>
    /// <param name="PrimitiveTypesXmiUrl">The Primitive Types XMI URL.</param>
    /// <param name="StandardProfileXmiUrl">The Standard Profile XMI URL.</param>
    /// <param name="DiagramInterchangeXmiUrl">The Diagram Interchange Metamodel XMI URL.</param>
    /// <param name="SpecificationPdfUrl">The normative specification PDF URL.</param>
    /// <param name="ChangebarPdfUrl">The informative specification changebar PDF URL.</param>
    /// <param name="OmgDocumentId">The OMG document identifier of the normative specification (e.g. "formal/17-12-05").</param>
    public sealed record UmlVersionDescriptor(
        string Version,
        bool IsCurrent,
        string MetamodelXmiUrl,
        string PrimitiveTypesXmiUrl,
        string StandardProfileXmiUrl,
        string DiagramInterchangeXmiUrl,
        string SpecificationPdfUrl,
        string ChangebarPdfUrl,
        string OmgDocumentId);
}
