// -------------------------------------------------------------------------------------------------
// <copyright file="XmiSpecDescriptor.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Sage.Knowledge
{
    /// <summary>
    /// Everything needed to fetch one published version of the OMG XMI specification - the XML
    /// serialization standard UML models are themselves written in. Deliberately independent of
    /// <see cref="UmlVersionDescriptor"/>: XMI versions on its own schedule, not UML's, even though
    /// both happen to be at "2.5.1" today.
    /// </summary>
    /// <param name="Version">The XMI version string, e.g. "2.5.1".</param>
    /// <param name="IsCurrent">Whether this is the current/default version to use when none is specified.</param>
    /// <param name="SpecificationPdfUrl">The normative specification PDF URL.</param>
    /// <param name="OmgDocumentId">The OMG document identifier of the normative specification (e.g. "formal/15-06-07").</param>
    public sealed record XmiSpecDescriptor(
        string Version,
        bool IsCurrent,
        string SpecificationPdfUrl,
        string OmgDocumentId);
}
