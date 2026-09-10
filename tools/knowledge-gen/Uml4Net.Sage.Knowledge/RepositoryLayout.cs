// -------------------------------------------------------------------------------------------------
// <copyright file="RepositoryLayout.cs" company="Starion Group S.A.">
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
    using System.IO;

    /// <summary>
    /// The fixed directory layout of a <c>uml4net-sage</c> plugin checkout: where sources, knowledge and
    /// the <c>spec-extract</c> Python project live relative to the repository root.
    /// </summary>
    public sealed class RepositoryLayout
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryLayout"/> class.
        /// </summary>
        public RepositoryLayout(string repositoryRoot)
        {
            this.RepositoryRoot = repositoryRoot;
        }

        /// <summary>
        /// Gets the plugin repository's root directory.
        /// </summary>
        public string RepositoryRoot { get; }

        /// <summary>
        /// Gets the git-ignored <c>sources/</c> root, under which per-version raw XMI/PDF files live.
        /// </summary>
        public string SourcesRoot => Path.Combine(this.RepositoryRoot, "sources");

        /// <summary>
        /// Gets the <c>knowledge/</c> root, under which per-version generated knowledge and the local
        /// <c>installed.json</c> live.
        /// </summary>
        public string KnowledgeRoot => Path.Combine(this.RepositoryRoot, "knowledge");

        /// <summary>
        /// Gets the <c>tools/spec-extract</c> Python project directory.
        /// </summary>
        public string SpecExtractProjectDirectory => Path.Combine(this.RepositoryRoot, "tools", "spec-extract");

        /// <summary>
        /// Gets <c>sources/&lt;version&gt;</c>.
        /// </summary>
        public string SourcesDirectoryFor(string version) => Path.Combine(this.SourcesRoot, version);

        /// <summary>
        /// Gets <c>knowledge/&lt;version&gt;</c>.
        /// </summary>
        public string KnowledgeDirectoryFor(string version) => Path.Combine(this.KnowledgeRoot, version);

        /// <summary>
        /// Gets <c>sources/xmi/&lt;version&gt;</c> - the companion OMG XMI specification's sources, kept as
        /// a top-level sibling of the per-UML-version directories since XMI versions independently of UML.
        /// </summary>
        public string XmiSourcesDirectoryFor(string version) => Path.Combine(this.SourcesRoot, "xmi", version);

        /// <summary>
        /// Gets <c>knowledge/xmi/&lt;version&gt;</c>.
        /// </summary>
        public string XmiKnowledgeDirectoryFor(string version) => Path.Combine(this.KnowledgeRoot, "xmi", version);
    }
}
