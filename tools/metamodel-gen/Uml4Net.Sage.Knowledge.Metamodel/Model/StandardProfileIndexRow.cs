// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Metamodel.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// One row of <c>standard-profile/index.json</c>.
    /// </summary>
    public sealed record StandardProfileIndexRow(string QualifiedName, string Kind, string File, string Source, string Profile, IReadOnlyList<string> BaseMetaclasses);
}
