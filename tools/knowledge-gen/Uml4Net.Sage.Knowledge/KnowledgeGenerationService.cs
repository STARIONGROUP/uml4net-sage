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

namespace Uml4Net.Sage.Knowledge
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using Uml4Net.Sage.MetamodelGen;

    /// <summary>
    /// The result of a <see cref="KnowledgeGenerationService.FetchAsync"/> call.
    /// </summary>
    public sealed record FetchOutcome(IReadOnlyList<FetchManifestEntry> Entries);

    /// <summary>
    /// The result of a <see cref="KnowledgeGenerationService.FetchXmiSpecAsync"/> call.
    /// </summary>
    /// <param name="Succeeded">Whether the download completed.</param>
    /// <param name="Warning">A human-readable reason when <paramref name="Succeeded"/> is <see langword="false"/>.</param>
    /// <param name="Entries">The downloaded file's manifest entry, empty on failure.</param>
    public sealed record XmiSpecFetchOutcome(bool Succeeded, string? Warning, IReadOnlyList<FetchManifestEntry> Entries);

    /// <summary>
    /// The result of a <see cref="KnowledgeGenerationService.GenerateAsync"/> call.
    /// </summary>
    public sealed record GenerateOutcome(SpecExtractionOutcome SpecExtraction, SpecExtractionOutcome XmiSpecExtraction);

    /// <summary>
    /// Orchestrates the two knowledge-base steps the <c>uml4net-sage</c> CLI's <c>fetch</c> and
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
        /// Downloads the companion OMG XMI specification PDF and records the fetch in
        /// <c>knowledge/installed.json</c>. Unlike <see cref="FetchAsync"/>, a failure here degrades to a
        /// warning rather than throwing: XMI text is a supplementary corpus, and a rotted XMI URL must not
        /// block the UML setup the caller actually asked for.
        /// </summary>
        public async Task<XmiSpecFetchOutcome> FetchXmiSpecAsync(RepositoryLayout layout, XmiSpecDescriptor descriptor, CancellationToken cancellationToken = default)
        {
            try
            {
                var entries = await this.sourceFetcher.FetchXmiSpecAsync(descriptor, layout.SourcesRoot, cancellationToken);
                new InstalledVersionsStore(layout.KnowledgeRoot).MarkXmiSpecFetched(descriptor.Version);
                return new XmiSpecFetchOutcome(true, null, entries);
            }
            catch (Exception exception) when (exception is HttpRequestException or InvalidDataException or IOException)
            {
                return new XmiSpecFetchOutcome(false, $"Could not fetch the XMI {descriptor.Version} specification PDF: {exception.Message}", []);
            }
        }

        /// <summary>
        /// Generates the metamodel, standard-profile, (if the PDF and Python are available) spec text, the
        /// companion XMI specification's clause text, and the scoped <c>datapackage.json</c> for
        /// <paramref name="descriptor"/> from its already-fetched sources, and records the generation in
        /// <c>knowledge/installed.json</c>.
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
                "UML",
                cancellationToken);

            var xmiSpecOutcome = await this.GenerateXmiSpecAsync(layout, cancellationToken);

            var dataPackageJson = DataPackageGenerator.Generate(knowledgeVersionDirectory, descriptor.Version, descriptor.OmgDocumentId);
            File.WriteAllText(Path.Combine(knowledgeVersionDirectory, "datapackage.json"), dataPackageJson);

            new InstalledVersionsStore(layout.KnowledgeRoot).MarkGenerated(descriptor.Version, specOutcome.Succeeded);

            return new GenerateOutcome(specOutcome, xmiSpecOutcome);
        }

        /// <summary>
        /// Extracts the companion OMG XMI specification's clause text into
        /// <c>knowledge/xmi/&lt;version&gt;/spec/</c>, once per machine rather than once per
        /// <see cref="GenerateAsync"/> call: the XMI corpus does not vary by UML version, so re-running
        /// <c>generate</c> for a second UML version must not re-extract it. Never throws, and never fails
        /// the overall <see cref="GenerateAsync"/> call - mirrors <see cref="PythonSpecExtractRunner"/>'s
        /// own degrade-to-skip contract.
        /// </summary>
        private async Task<SpecExtractionOutcome> GenerateXmiSpecAsync(RepositoryLayout layout, CancellationToken cancellationToken)
        {
            var xmiDescriptor = KnownXmiVersions.Current;
            var xmiSpecOutputDirectory = Path.Combine(layout.XmiKnowledgeDirectoryFor(xmiDescriptor.Version), "spec");

            if (File.Exists(Path.Combine(xmiSpecOutputDirectory, "index.json")))
            {
                return SpecExtractionOutcome.Ok();
            }

            var xmiPdfPath = Path.Combine(layout.XmiSourcesDirectoryFor(xmiDescriptor.Version), "specs", $"XMI-{xmiDescriptor.Version}.pdf");
            var outcome = await this.specExtractRunner.ExtractAsync(
                xmiPdfPath,
                xmiSpecOutputDirectory,
                layout.SpecExtractProjectDirectory,
                xmiDescriptor.Version,
                "XMI",
                cancellationToken);

            new InstalledVersionsStore(layout.KnowledgeRoot).MarkXmiSpecGenerated(xmiDescriptor.Version, outcome.Succeeded);
            return outcome;
        }
    }
}
