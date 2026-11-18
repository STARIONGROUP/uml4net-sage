// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Metamodel
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    using uml4net.Classification;
    using uml4net.CommonStructure;
    using uml4net.StructuredClassifiers;

    using Uml4Net.Sage.Knowledge.Metamodel.Model;

    /// <summary>
    /// Converts an <see cref="IAssociation"/>'s <c>memberEnd</c> properties read from XMI into render-ready
    /// <see cref="AssociationEndInfo"/> records (the MODEL provenance tier: read directly from the metamodel XMI).
    /// </summary>
    public static class AssociationExtractor
    {
        /// <summary>
        /// Converts every member end of <paramref name="association"/>, in declaration order.
        /// </summary>
        /// <param name="association">The association whose member ends are converted.</param>
        /// <returns>One <see cref="AssociationEndInfo"/> per member end.</returns>
        public static IReadOnlyList<AssociationEndInfo> MemberEndsOf(IAssociation association)
        {
            return association.MemberEnd
                .Select(end =>
                {
                    var type = end.Type as INamedElement;

                    return new AssociationEndInfo(
                        Name: end.Name,
                        TypeName: type?.Name,
                        TypeQualifiedName: type?.QualifiedName,
                        Lower: end.Lower,
                        Upper: end.Upper,
                        IsOrdered: end.IsOrdered,
                        IsUnique: end.IsUnique,
                        IsComposite: end.IsComposite,
                        IsOwnedByAssociation: ReferenceEquals(end.OwningAssociation, association),
                        IsDerived: end.IsDerived,
                        Aggregation: end.Aggregation.ToString().ToLowerInvariant(),
                        IsNavigable: end.OwningAssociation is null || association.NavigableOwnedEnd.Contains(end),
                        Opposite: end.Opposite?.QualifiedName);
                })
                .ToList();
        }
    }
}
