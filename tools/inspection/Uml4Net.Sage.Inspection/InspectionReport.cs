// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Inspection
{
    using System.Collections.Generic;

    /// <summary>
    /// The full result of inspecting one model file.
    /// </summary>
    /// <param name="ModelPath">The path of the inspected model file.</param>
    /// <param name="UmlVersion">The UML version the model was checked against.</param>
    /// <param name="Findings">The findings, ordered by line, then category, then element id.</param>
    public sealed record InspectionReport(string ModelPath, string UmlVersion, IReadOnlyList<InspectionFinding> Findings);
}
