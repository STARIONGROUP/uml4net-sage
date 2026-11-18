// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Inspection
{
    using System.Collections.Generic;

    /// <summary>
    /// What a plain XML pass over the inspected document finds - independent of whether <c>uml4net.xmi</c> can
    /// read it, so a file the reader aborts on (e.g. because of an abstract <c>xmi:type</c>) still gets
    /// line-accurate findings.
    /// </summary>
    /// <param name="Elements">Every UML element declared (not merely referenced through an <c>href</c>), in document order.</param>
    /// <param name="XmiIdLines">Every <c>xmi:id</c> with the line(s) declaring it; more than one line means a duplicate.</param>
    /// <param name="PathmapDocuments">Every <c>pathmap://</c> document referenced through an <c>href</c> (without the fragment), sorted.</param>
    public sealed record XmiDocumentScan(
        IReadOnlyList<ScannedElement> Elements,
        IReadOnlyDictionary<string, IReadOnlyList<int>> XmiIdLines,
        IReadOnlyList<string> PathmapDocuments);
}
