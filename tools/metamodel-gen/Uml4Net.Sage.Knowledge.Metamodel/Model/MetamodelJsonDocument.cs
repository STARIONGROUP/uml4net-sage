// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Metamodel.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// The root document written to <c>metamodel/metamodel.json</c>.
    /// </summary>
    public sealed record MetamodelJsonDocument(
        IReadOnlyList<MetamodelJsonNode> Classes,
        IReadOnlyList<EnumerationJsonNode> Enumerations,
        IReadOnlyList<PrimitiveTypeJsonNode> PrimitiveTypes,
        IReadOnlyList<AssociationJsonNode> Associations);
}
