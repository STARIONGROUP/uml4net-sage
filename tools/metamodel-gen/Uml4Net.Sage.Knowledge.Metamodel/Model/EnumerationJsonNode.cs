// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Metamodel.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// One enumeration node in <c>metamodel.json</c>.
    /// </summary>
    public sealed record EnumerationJsonNode(string Name, string QualifiedName, string Package, IReadOnlyList<string> Literals);
}
