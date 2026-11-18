// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Metamodel.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// One association node in <c>metamodel.json</c> - a metamodel association *link* (e.g. the association
    /// behind <c>Class::ownedAttribute</c>), not a metaclass.
    /// </summary>
    public sealed record AssociationJsonNode(
        string Name,
        string QualifiedName,
        string Package,
        bool IsAbstract,
        bool IsDerived,
        IReadOnlyList<AssociationEndInfo> MemberEnds);
}
