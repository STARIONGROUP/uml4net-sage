// -------------------------------------------------------------------------------------------------
// <copyright file="InspectionFinding.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Codex.Tools.Inspection
{
    using System.Collections.Generic;

    /// <summary>
    /// One finding produced by <see cref="XmiInspector"/>.
    /// </summary>
    /// <param name="Severity">"error" or "warning".</param>
    /// <param name="Category">A short machine-readable category, e.g. "abstract-instantiation", "reader-diagnostic".</param>
    /// <param name="ElementXmiId">The XMI id of the offending element, when known.</param>
    /// <param name="Message">A human-readable description.</param>
    public sealed record InspectionFinding(string Severity, string Category, string? ElementXmiId, string Message);

    /// <summary>
    /// The full result of inspecting one model file.
    /// </summary>
    public sealed record InspectionReport(string ModelPath, string UmlVersion, IReadOnlyList<InspectionFinding> Findings);
}
