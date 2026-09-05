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

namespace Uml4Net.Codex.Knowledge.Tests
{
    using System.IO;
    using System.Linq;
    using System.Net.Http;
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

        [SetUp]
        public void SetUp()
        {
            this.repositoryRoot = Path.Combine(Path.GetTempPath(), "uml4net-codex-tests", Path.GetRandomFileName());
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

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(this.repositoryRoot))
            {
                Directory.Delete(this.repositoryRoot, recursive: true);
            }
        }

        private static string TestFixturesDirectory() => Path.Combine(NUnit.Framework.TestContext.CurrentContext.TestDirectory, "Fixtures", "Xmi");

        [Test]
        public async Task GenerateAsync_produces_the_full_knowledge_tree_and_a_scoped_datapackage_without_a_spec_pdf()
        {
            var service = new KnowledgeGenerationService(new SourceFetcher(new HttpClient(new StubHttpMessageHandler())), new PythonSpecExtractRunner(FakeProcessRunner.NotFound()));

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

        [Test]
        public async Task GenerateAsync_records_the_generation_in_installed_json()
        {
            var service = new KnowledgeGenerationService(new SourceFetcher(new HttpClient(new StubHttpMessageHandler())), new PythonSpecExtractRunner(FakeProcessRunner.NotFound()));

            await service.GenerateAsync(this.layout, Descriptor);

            var manifest = new InstalledVersionsStore(this.layout.KnowledgeRoot).Load();
            var entry = manifest.Versions.Single(v => v.Version == "2.5.1");
            Assert.That(entry.Generated, Is.True);
            Assert.That(entry.SpecGenerated, Is.False);
        }
    }
}
