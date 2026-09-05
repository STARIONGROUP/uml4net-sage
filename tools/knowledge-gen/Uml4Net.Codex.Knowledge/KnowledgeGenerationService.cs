// -------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeGenerationService.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Codex.Knowledge
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Uml4Net.Codex.MetamodelGen;

    /// <summary>
    /// The result of a <see cref="KnowledgeGenerationService.FetchAsync"/> call.
    /// </summary>
    public sealed record FetchOutcome(IReadOnlyList<FetchManifestEntry> Entries);

    /// <summary>
    /// The result of a <see cref="KnowledgeGenerationService.GenerateAsync"/> call.
    /// </summary>
    public sealed record GenerateOutcome(SpecExtractionOutcome SpecExtraction);

    /// <summary>
    /// Orchestrates the two knowledge-base steps the <c>uml4net-codex</c> CLI's <c>fetch</c> and
    /// <c>generate</c> verbs drive: downloading OMG's XMI/PDF sources, then generating the metamodel,
    /// standard-profile, (optionally) spec text, and the scoped <c>datapackage.json</c>.
    /// </summary>
    public sealed class KnowledgeGenerationService
    {
        private readonly SourceFetcher sourceFetcher;
        private readonly PythonSpecExtractRunner specExtractRunner;

        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeGenerationService"/> class.
        /// </summary>
        public KnowledgeGenerationService(SourceFetcher sourceFetcher, PythonSpecExtractRunner specExtractRunner)
        {
            this.sourceFetcher = sourceFetcher;
            this.specExtractRunner = specExtractRunner;
        }

        /// <summary>
        /// Downloads <paramref name="descriptor"/>'s XMI (and, unless <paramref name="includeSpecs"/> is
        /// false, PDF) files, and records the fetch in <c>knowledge/installed.json</c>.
        /// </summary>
        public async Task<FetchOutcome> FetchAsync(RepositoryLayout layout, UmlVersionDescriptor descriptor, bool includeSpecs, bool setAsDefault, CancellationToken cancellationToken = default)
        {
            var entries = await this.sourceFetcher.FetchAsync(descriptor, layout.SourcesRoot, includeSpecs, cancellationToken);
            new InstalledVersionsStore(layout.KnowledgeRoot).MarkFetched(descriptor.Version, setAsDefault);
            return new FetchOutcome(entries);
        }

        /// <summary>
        /// Generates the metamodel, standard-profile, (if the PDF and Python are available) spec text, and
        /// the scoped <c>datapackage.json</c> for <paramref name="descriptor"/> from its already-fetched
        /// sources, and records the generation in <c>knowledge/installed.json</c>.
        /// </summary>
        public async Task<GenerateOutcome> GenerateAsync(RepositoryLayout layout, UmlVersionDescriptor descriptor, CancellationToken cancellationToken = default)
        {
            var xmiDirectory = Path.Combine(layout.SourcesDirectoryFor(descriptor.Version), "xmi");
            var knowledgeVersionDirectory = layout.KnowledgeDirectoryFor(descriptor.Version);

            MetamodelGenerator.Generate(xmiDirectory, knowledgeVersionDirectory);

            var pdfPath = Path.Combine(layout.SourcesDirectoryFor(descriptor.Version), "specs", $"UML-{descriptor.Version}.pdf");
            var specOutcome = await this.specExtractRunner.ExtractAsync(
                pdfPath,
                Path.Combine(knowledgeVersionDirectory, "spec"),
                layout.SpecExtractProjectDirectory,
                descriptor.Version,
                cancellationToken);

            var dataPackageJson = DataPackageGenerator.Generate(knowledgeVersionDirectory, descriptor.Version, descriptor.OmgDocumentId);
            File.WriteAllText(Path.Combine(knowledgeVersionDirectory, "datapackage.json"), dataPackageJson);

            new InstalledVersionsStore(layout.KnowledgeRoot).MarkGenerated(descriptor.Version, specOutcome.Succeeded);

            return new GenerateOutcome(specOutcome);
        }
    }
}
