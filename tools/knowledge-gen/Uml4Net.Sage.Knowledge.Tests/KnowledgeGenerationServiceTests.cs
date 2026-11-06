// -------------------------------------------------------------------------------------------------
// <copyright file="KnowledgeGenerationServiceTests.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Sage.Knowledge.Tests
{
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    [TestFixture]
    public class KnowledgeGenerationServiceTests
    {
        private static readonly UmlVersionDescriptor Descriptor = new(
            Version: "2.5.1",
            IsCurrent: true,
            MetamodelXmiUrl: "https://example.com/UML.xmi",
            PrimitiveTypesXmiUrl: "https://example.com/PrimitiveTypes.xmi",
            StandardProfileXmiUrl: "https://example.com/StandardProfile.xmi",
            DiagramInterchangeXmiUrl: "https://example.com/UMLDI.xmi",
            SpecificationPdfUrl: "https://example.com/PDF",
            ChangebarPdfUrl: "https://example.com/PDF/changebar",
            OmgDocumentId: "formal/17-12-05");

        private string repositoryRoot = null!;
        private RepositoryLayout layout = null!;

        /// <summary>
        /// Creates a temporary repository root and copies the fixture XMI files into
        /// <c>sources/2.5.1/xmi/</c>, as a real <c>fetch</c> would.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.repositoryRoot = Path.Combine(Path.GetTempPath(), "uml4net-sage-tests", Path.GetRandomFileName());
            this.layout = new RepositoryLayout(this.repositoryRoot);

            // Stand in for a real `fetch`: copy the hand-authored fixture XMI straight into sources/2.5.1/xmi/,
            // named exactly as the real OMG files would be, so GenerateAsync exercises the full wiring
            // (MetamodelGenerator -> DataPackageGenerator -> InstalledVersionsStore) without any network call.
            var xmiDestination = Path.Combine(this.layout.SourcesDirectoryFor("2.5.1"), "xmi");
            Directory.CreateDirectory(xmiDestination);
            foreach (var fileName in new[] { "UML.xmi", "PrimitiveTypes.xmi", "StandardProfile.xmi" })
            {
                File.Copy(Path.Combine(TestFixturesDirectory(), fileName), Path.Combine(xmiDestination, fileName));
            }
        }

        /// <summary>
        /// Deletes the temporary repository root.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(this.repositoryRoot))
            {
                Directory.Delete(this.repositoryRoot, recursive: true);
            }
        }

        /// <summary>
        /// Gets the directory that holds the hand-authored fixture XMI files.
        /// </summary>
        /// <returns>The <c>Fixtures/Xmi</c> directory under the test output directory.</returns>
        private static string TestFixturesDirectory() => Path.Combine(NUnit.Framework.TestContext.CurrentContext.TestDirectory, "Fixtures", "Xmi");

        /// <summary>
        /// Verifies that generating without a spec PDF reports the spec extraction as not succeeded but
        /// still writes the metamodel and standard-profile indexes, an element file and a
        /// <c>datapackage.json</c> without the <c>spec-clause-index</c> resource.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task GenerateAsync_produces_the_full_knowledge_tree_and_a_scoped_datapackage_without_a_spec_pdf()
        {
            var service = new KnowledgeGenerationService(new SourceFetcher(new HttpClient(new StubHttpMessageHandler())), new PythonSpecExtractRunner(FakeProcessRunner.NotFound(), FakeUvProvisioner.Unavailable()));

            var outcome = await service.GenerateAsync(this.layout, Descriptor);

            Assert.That(outcome.SpecExtraction.Succeeded, Is.False, "no PDF was fetched in this test");

            var knowledgeDirectory = this.layout.KnowledgeDirectoryFor("2.5.1");
            Assert.That(File.Exists(Path.Combine(knowledgeDirectory, "metamodel", "index.json")));
            Assert.That(File.Exists(Path.Combine(knowledgeDirectory, "metamodel", "elements", "Widget.md")));
            Assert.That(File.Exists(Path.Combine(knowledgeDirectory, "standard-profile", "index.json")));
            Assert.That(File.Exists(Path.Combine(knowledgeDirectory, "datapackage.json")));

            var dataPackageJson = File.ReadAllText(Path.Combine(knowledgeDirectory, "datapackage.json"));
            Assert.That(dataPackageJson, Does.Not.Contain("spec-clause-index"), "spec text was never generated in this test");
        }

        /// <summary>
        /// Verifies that generating records the version in <c>installed.json</c> as generated, with the
        /// spec-generated flag left false when no spec text was extracted.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task GenerateAsync_records_the_generation_in_installed_json()
        {
            var service = new KnowledgeGenerationService(new SourceFetcher(new HttpClient(new StubHttpMessageHandler())), new PythonSpecExtractRunner(FakeProcessRunner.NotFound(), FakeUvProvisioner.Unavailable()));

            await service.GenerateAsync(this.layout, Descriptor);

            var manifest = new InstalledVersionsStore(this.layout.KnowledgeRoot).Load();
            var entry = manifest.Versions.Single(v => v.Version == "2.5.1");
            Assert.That(entry.Generated, Is.True);
            Assert.That(entry.SpecGenerated, Is.False);
        }

        /// <summary>
        /// Verifies that, without an XMI specification PDF, generating still completes and reports the
        /// XMI extraction as skipped with a reason that mentions XMI.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task GenerateAsync_attempts_xmi_spec_extraction_and_does_not_fail_generate_when_it_is_skipped()
        {
            var service = new KnowledgeGenerationService(new SourceFetcher(new HttpClient(new StubHttpMessageHandler())), new PythonSpecExtractRunner(FakeProcessRunner.NotFound(), FakeUvProvisioner.Unavailable()));

            var outcome = await service.GenerateAsync(this.layout, Descriptor);

            Assert.That(outcome.XmiSpecExtraction.Succeeded, Is.False, "no XMI PDF was fetched in this test");
            Assert.That(outcome.XmiSpecExtraction.SkippedReason, Does.Contain("XMI"));
        }

        /// <summary>
        /// Verifies that, when the XMI clause index already exists, generating reports the XMI extraction
        /// as succeeded without needing the XMI PDF.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task GenerateAsync_does_not_re_extract_the_xmi_spec_when_it_already_exists()
        {
            // Simulates a previous generate call (for this or another UML version) having already
            // extracted the version-independent XMI corpus - no XMI PDF exists in sources/ at all here, so
            // if the re-extraction guard didn't short-circuit, ExtractAsync would have to hit its
            // "PDF not found" branch and return Skipped, not Ok.
            var xmiSpecDirectory = Path.Combine(this.layout.XmiKnowledgeDirectoryFor("2.5.1"), "spec");
            Directory.CreateDirectory(xmiSpecDirectory);
            File.WriteAllText(Path.Combine(xmiSpecDirectory, "index.json"), "[{}]");

            var service = new KnowledgeGenerationService(new SourceFetcher(new HttpClient(new StubHttpMessageHandler())), new PythonSpecExtractRunner(FakeProcessRunner.NotFound(), FakeUvProvisioner.Unavailable()));

            var outcome = await service.GenerateAsync(this.layout, Descriptor);

            Assert.That(outcome.XmiSpecExtraction.Succeeded, Is.True);
        }

        private static readonly XmiSpecDescriptor XmiDescriptor = new(
            Version: "2.5.1",
            IsCurrent: true,
            SpecificationPdfUrl: "https://example.com/XMI/PDF",
            OmgDocumentId: "formal/15-06-07",
            XsdUrl: "https://example.com/XMI/XMI.xsd",
            CanonicalXsdUrl: "https://example.com/XMI/XMI-Canonical.xsd");

        /// <summary>
        /// Verifies that fetching valid XMI PDF and schema responses succeeds and records the XMI
        /// specification as fetched in <c>installed.json</c>.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task FetchXmiSpecAsync_succeeds_and_records_the_fetch_in_installed_json()
        {
            var handler = new StubHttpMessageHandler();
            handler.EnqueueSuccess(Encoding.UTF8.GetBytes("%PDF-1.5 fake xmi spec"));
            handler.EnqueueSuccess(Encoding.UTF8.GetBytes("<xsd:schema>fake XMI.xsd</xsd:schema>"));
            handler.EnqueueSuccess(Encoding.UTF8.GetBytes("<xsd:schema>fake XMI-Canonical.xsd</xsd:schema>"));
            var service = new KnowledgeGenerationService(new SourceFetcher(new HttpClient(handler)), new PythonSpecExtractRunner(FakeProcessRunner.NotFound(), FakeUvProvisioner.Unavailable()));

            var outcome = await service.FetchXmiSpecAsync(this.layout, XmiDescriptor);

            Assert.That(outcome.Succeeded, Is.True);
            var manifest = new InstalledVersionsStore(this.layout.KnowledgeRoot).Load();
            Assert.That(manifest.XmiSpecs.Single(x => x.Version == "2.5.1").Fetched, Is.True);
        }

        /// <summary>
        /// Verifies that an HTML page served in place of the XMI PDF makes the fetch return an
        /// unsuccessful outcome with a warning that mentions XMI, instead of throwing.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task FetchXmiSpecAsync_degrades_to_a_warning_instead_of_throwing_on_failure()
        {
            var handler = new StubHttpMessageHandler();
            handler.EnqueueSuccess(Encoding.UTF8.GetBytes("<html>404 Not Found</html>"));
            var service = new KnowledgeGenerationService(new SourceFetcher(new HttpClient(handler)), new PythonSpecExtractRunner(FakeProcessRunner.NotFound(), FakeUvProvisioner.Unavailable()));

            var outcome = await service.FetchXmiSpecAsync(this.layout, XmiDescriptor);

            Assert.That(outcome.Succeeded, Is.False);
            Assert.That(outcome.Warning, Does.Contain("XMI"));
        }

        /// <summary>
        /// Verifies that a successful XMI download followed by a corrupt <c>installed.json</c> throws an
        /// <see cref="InvalidDataException"/> naming <c>installed.json</c>, rather than being reported as a
        /// failed download.
        /// </summary>
        [Test]
        public void FetchXmiSpecAsync_with_a_corrupt_manifest_throws_rather_than_reporting_a_failed_download()
        {
            var handler = new StubHttpMessageHandler();
            handler.EnqueueSuccess(Encoding.UTF8.GetBytes("%PDF-1.5 fake xmi spec"));
            handler.EnqueueSuccess(Encoding.UTF8.GetBytes("<xsd:schema>fake XMI.xsd</xsd:schema>"));
            handler.EnqueueSuccess(Encoding.UTF8.GetBytes("<xsd:schema>fake XMI-Canonical.xsd</xsd:schema>"));
            var service = new KnowledgeGenerationService(new SourceFetcher(new HttpClient(handler)), new PythonSpecExtractRunner(FakeProcessRunner.NotFound(), FakeUvProvisioner.Unavailable()));
            Directory.CreateDirectory(this.layout.KnowledgeRoot);
            File.WriteAllText(Path.Combine(this.layout.KnowledgeRoot, "installed.json"), "{ not json");

            Assert.That(
                () => service.FetchXmiSpecAsync(this.layout, XmiDescriptor),
                Throws.InstanceOf<InvalidDataException>().With.Message.Contains("installed.json"));
        }
    }
}
