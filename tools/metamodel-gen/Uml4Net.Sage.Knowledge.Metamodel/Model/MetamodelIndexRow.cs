// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Metamodel.Model
{
    /// <summary>
    /// One row of <c>metamodel/index.json</c> - a Frictionless tabular-data-resource row, hence a flat
    /// array rather than a keyed map (see the root <c>datapackage.json</c>).
    /// </summary>
    public sealed record MetamodelIndexRow(string Name, string Kind, string Package, string QualifiedName, bool IsAbstract, string File);
}
