// -------------------------------------------------------------------------------------------------
// <copyright file="IndexRows.cs" company="Starion Group S.A.">
//
//   Copyright (C) 2019-2026 Starion Group S.A.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace Uml4Net.Codex.MetamodelGen.Model
{
    /// <summary>
    /// One row of <c>metamodel/index.json</c> - a Frictionless tabular-data-resource row, hence a flat
    /// array rather than a keyed map (see the root <c>datapackage.json</c>).
    /// </summary>
    public sealed record MetamodelIndexRow(string Name, string Kind, string Package, string QualifiedName, bool IsAbstract, string File);

    /// <summary>
    /// One row of <c>standard-profile/index.json</c>.
    /// </summary>
    public sealed record StandardProfileIndexRow(string QualifiedName, string Kind, string File, string Source);
}
