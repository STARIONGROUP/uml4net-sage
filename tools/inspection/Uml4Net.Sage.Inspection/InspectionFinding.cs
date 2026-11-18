// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Inspection
{
    using System.Collections.Generic;

    /// <summary>
    /// One finding produced by <see cref="XmiInspector"/>.
    /// </summary>
    /// <param name="Severity">"error" or "warning".</param>
    /// <param name="Category">
    /// A short machine-readable category: "abstract-instantiation", "unknown-metaclass", "duplicate-xmi-id",
    /// "read-aborted", "unresolved-reference", "invalid-reference", "multiplicity-violation",
    /// "stereotype-misapplied", "stereotype-target-unresolved", "stereotype-unresolved" or "reader-diagnostic".
    /// </param>
    /// <param name="ElementXmiId">The XMI id of the offending element, when known.</param>
    /// <param name="Message">A human-readable description.</param>
    /// <param name="Line">The 1-based line in the inspected file where the offending element starts, when known.</param>
    public sealed record InspectionFinding(string Severity, string Category, string? ElementXmiId, string Message, int? Line = null);

    /// <summary>
    /// The full result of inspecting one model file.
    /// </summary>
    /// <param name="ModelPath">The path of the inspected model file.</param>
    /// <param name="UmlVersion">The UML version the model was checked against.</param>
    /// <param name="Findings">The findings, ordered by line, then category, then element id.</param>
    public sealed record InspectionReport(string ModelPath, string UmlVersion, IReadOnlyList<InspectionFinding> Findings);
}
