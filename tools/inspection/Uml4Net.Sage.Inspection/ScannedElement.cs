// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Inspection
{
    /// <summary>
    /// A UML element declared in the inspected document: its metaclass (from <c>xmi:type</c>, or the element
    /// name for a root element such as <c>&lt;uml:Package&gt;</c>), its <c>xmi:id</c>, and where it starts.
    /// </summary>
    /// <param name="Metaclass">The UML metaclass name, e.g. <c>Class</c>.</param>
    /// <param name="XmiId">The element's <c>xmi:id</c>, if it has one.</param>
    /// <param name="Line">The 1-based line the element starts on.</param>
    public sealed record ScannedElement(string Metaclass, string? XmiId, int Line);
}
