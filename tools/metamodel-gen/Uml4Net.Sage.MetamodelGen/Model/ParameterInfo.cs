// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterInfo.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Sage.MetamodelGen.Model
{
    /// <summary>
    /// A render-ready description of one <c>in</c>/<c>inout</c>/<c>out</c> parameter of an operation
    /// (the <c>return</c> parameter is not represented here - it's already captured by the owning
    /// <see cref="FeatureInfo"/>'s own <c>TypeName</c>/<c>Lower</c>/<c>Upper</c>).
    /// </summary>
    /// <param name="Name">The parameter's own name.</param>
    /// <param name="TypeName">The simple name of the parameter's type, if any.</param>
    /// <param name="TypeQualifiedName">The fully qualified name of the parameter's type, if any, for linking.</param>
    /// <param name="Lower">The lower multiplicity bound.</param>
    /// <param name="Upper">The upper multiplicity bound ("*" for unbounded).</param>
    /// <param name="Direction">"in", "inout", or "out" (never "return" - see above).</param>
    public sealed record ParameterInfo(
        string Name,
        string? TypeName,
        string? TypeQualifiedName,
        int Lower,
        string Upper,
        string Direction);
}
