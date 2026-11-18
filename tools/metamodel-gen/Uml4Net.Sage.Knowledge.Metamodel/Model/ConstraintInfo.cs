// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Metamodel.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// A render-ready description of one <c>ownedRule</c> constraint, read directly from the XMI (MODEL tier).
    /// </summary>
    /// <param name="Name">The constraint's own name, if any.</param>
    /// <param name="Languages">The specification languages declared for the constraint's body (e.g. "OCL").</param>
    /// <param name="Body">The constraint's body text, one entry per declared language.</param>
    public sealed record ConstraintInfo(string? Name, IReadOnlyList<string> Languages, IReadOnlyList<string> Body);
}
