// -------------------------------------------------------------------------------------------------
// <copyright file="KnownXmiVersions.cs" company="Starion Group S.A.">
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
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The static, hand-maintained registry of OMG XMI specification versions this tool knows how to
    /// fetch - the companion serialization standard alongside <see cref="KnownUmlVersions"/>. Like UML,
    /// XMI has no realistic near-term release cadence and no GitHub-hosted mirror, so this is a plain,
    /// hand-updated list rather than a discovery system.
    /// </summary>
    public static class KnownXmiVersions
    {
        /// <summary>
        /// Every XMI specification version this tool can fetch and extract a knowledge base for.
        /// </summary>
        public static readonly IReadOnlyList<XmiSpecDescriptor> All =
        [
            new XmiSpecDescriptor(
                Version: "2.5.1",
                IsCurrent: true,
                SpecificationPdfUrl: "https://www.omg.org/spec/XMI/2.5.1/PDF",
                OmgDocumentId: "formal/15-06-07"),
        ];

        /// <summary>
        /// Gets the version marked <see cref="XmiSpecDescriptor.IsCurrent"/>.
        /// </summary>
        public static XmiSpecDescriptor Current => All.First(v => v.IsCurrent);

        /// <summary>
        /// Attempts to find a known version by its version string.
        /// </summary>
        public static bool TryFind(string version, out XmiSpecDescriptor? descriptor)
        {
            descriptor = All.FirstOrDefault(v => v.Version == version);
            return descriptor is not null;
        }
    }
}
