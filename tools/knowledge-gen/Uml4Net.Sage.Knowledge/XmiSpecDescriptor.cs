// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge
{
    /// <summary>
    /// Everything needed to fetch one published version of the OMG XMI specification - the XML
    /// serialization standard UML models are themselves written in. Deliberately independent of
    /// <see cref="UmlVersionDescriptor"/>: XMI versions on its own schedule, not UML's, even though
    /// both happen to be at "2.5.1" today.
    /// </summary>
    /// <param name="Version">The XMI version string, e.g. "2.5.1".</param>
    /// <param name="IsCurrent">Whether this is the current/default version to use when none is specified.</param>
    /// <param name="SpecificationPdfUrl">The normative specification PDF URL.</param>
    /// <param name="OmgDocumentId">The OMG document identifier of the normative specification (e.g. "formal/15-06-07").</param>
    /// <param name="XsdUrl">
    /// The generic XMI wrapper schema (<c>xmi:id</c>/<c>idref</c>/<c>href</c>/<c>type</c>, the
    /// <c>Extension</c>/<c>Documentation</c>/<c>Difference</c> elements). Published under a dated path
    /// distinct from <see cref="SpecificationPdfUrl"/>'s version-numbered one - OMG's own convention,
    /// not a typo.
    /// </param>
    /// <param name="CanonicalXsdUrl">
    /// The narrower "Canonical XMI" profile of the same wrapper schema, published under its own,
    /// separately-dated path.
    /// </param>
    public sealed record XmiSpecDescriptor(
        string Version,
        bool IsCurrent,
        string SpecificationPdfUrl,
        string OmgDocumentId,
        string XsdUrl,
        string CanonicalXsdUrl);
}
