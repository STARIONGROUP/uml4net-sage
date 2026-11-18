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
}
