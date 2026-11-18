// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Metamodel.Model
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
