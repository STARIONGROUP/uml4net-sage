// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Metamodel.Model
{
    /// <summary>
    /// A render-ready description of one of an <see cref="uml4net.StructuredClassifiers.IAssociation"/>'s
    /// <c>memberEnd</c> properties.
    /// </summary>
    /// <param name="Name">The end's own name.</param>
    /// <param name="TypeName">The simple name of the end's type (the classifier it's typed by), if any.</param>
    /// <param name="TypeQualifiedName">The fully qualified name of the end's type, if any, for linking.</param>
    /// <param name="Lower">The lower multiplicity bound.</param>
    /// <param name="Upper">The upper multiplicity bound ("*" for unbounded).</param>
    /// <param name="IsOrdered">Whether the end is ordered.</param>
    /// <param name="IsUnique">Whether the end is unique.</param>
    /// <param name="IsComposite">Whether the end is a composite (owning) end.</param>
    /// <param name="IsOwnedByAssociation">
    /// True when this end is one of the association's own <c>ownedEnd</c> properties - the standard XMI
    /// encoding for a non-navigable/opposite end that neither endpoint classifier owns as its own
    /// attribute (e.g. the reverse of <c>Class::nestedClassifier</c>). False means the end is instead
    /// owned by one of the endpoint classifiers and already appears on that classifier's own page.
    /// </param>
    /// <param name="IsDerived">Whether the end is derived.</param>
    /// <param name="Aggregation">The end's aggregation: <c>none</c>, <c>shared</c> or <c>composite</c>.</param>
    /// <param name="IsNavigable">
    /// Whether the end is navigable (UML <c>Property::isNavigable()</c>): owned by a classifier, or listed in the
    /// association's <c>navigableOwnedEnd</c>.
    /// </param>
    /// <param name="Opposite">The qualified name of the other end of a binary association, otherwise null.</param>
    public sealed record AssociationEndInfo(
        string Name,
        string? TypeName,
        string? TypeQualifiedName,
        int Lower,
        string Upper,
        bool IsOrdered,
        bool IsUnique,
        bool IsComposite,
        bool IsOwnedByAssociation,
        bool IsDerived,
        string Aggregation,
        bool IsNavigable,
        string? Opposite);
}
