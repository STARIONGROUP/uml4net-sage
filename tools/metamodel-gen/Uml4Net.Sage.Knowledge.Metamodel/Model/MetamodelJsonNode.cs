// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Metamodel.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// One class node in <c>metamodel.json</c>, with precomputed inheritance closures so consumers never
    /// have to re-walk <c>Generalization</c> chains by hand.
    /// </summary>
    public sealed record MetamodelJsonNode(
        string Name,
        string QualifiedName,
        string Package,
        bool IsAbstract,
        IReadOnlyList<string> DirectSuperClasses,
        IReadOnlyList<string> DirectSubclasses,
        IReadOnlyList<string> AllAncestors,
        IReadOnlyList<string> AllDescendants,
        IReadOnlyList<FeatureInfo> OwnedAttributes,
        IReadOnlyList<FeatureInfo> OwnedOperations,
        IReadOnlyList<FeatureInfo> InheritedAttributes,
        IReadOnlyList<FeatureInfo> InheritedOperations,
        IReadOnlyList<ConstraintInfo> Constraints);

    /// <summary>
    /// One enumeration node in <c>metamodel.json</c>.
    /// </summary>
    public sealed record EnumerationJsonNode(string Name, string QualifiedName, string Package, IReadOnlyList<string> Literals);

    /// <summary>
    /// One primitive type node in <c>metamodel.json</c>.
    /// </summary>
    public sealed record PrimitiveTypeJsonNode(string Name, string QualifiedName, string Package);

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

    /// <summary>
    /// The root document written to <c>metamodel/metamodel.json</c>.
    /// </summary>
    public sealed record MetamodelJsonDocument(
        IReadOnlyList<MetamodelJsonNode> Classes,
        IReadOnlyList<EnumerationJsonNode> Enumerations,
        IReadOnlyList<PrimitiveTypeJsonNode> PrimitiveTypes,
        IReadOnlyList<AssociationJsonNode> Associations);
}
