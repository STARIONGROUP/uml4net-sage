// -------------------------------------------------------------------------------------------------
// <copyright file="SourceFetcherTests.cs" company="Starion Group S.A.">
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
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    [TestFixture]
    public class SourceFetcherTests
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

        private string tempDirectory = null!;

        [SetUp]
        public void SetUp()
        {
            this.tempDirectory = Path.Combine(Path.GetTempPath(), "uml4net-sage-tests", Path.GetRandomFileName());
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(this.tempDirectory))
            {
                Directory.Delete(this.tempDirectory, recursive: true);
            }
        }

        [Test]
        public async Task FetchAsync_without_specs_downloads_only_the_four_xmi_files()
        {
            var handler = new StubHttpMessageHandler();
            for (var i = 0; i < 4; i++)
            {
                handler.EnqueueSuccess(Encoding.UTF8.GetBytes("<xmi/>"));
            }

            var fetcher = new SourceFetcher(new HttpClient(handler));

            var entries = await fetcher.FetchAsync(Descriptor, this.tempDirectory, includeSpecs: false);

            Assert.That(handler.RequestCount, Is.EqualTo(4));
            Assert.That(entries, Has.Count.EqualTo(4));
            Assert.That(File.Exists(Path.Combine(this.tempDirectory, "2.5.1", "xmi", "UML.xmi")));
            Assert.That(File.Exists(Path.Combine(this.tempDirectory, "2.5.1", "specs", "UML-2.5.1.pdf")), Is.False);
        }

        [Test]
        public async Task FetchAsync_with_specs_also_downloads_the_two_pdfs_with_explicit_file_names()
        {
            var handler = new StubHttpMessageHandler();
            for (var i = 0; i < 4; i++)
            {
                handler.EnqueueSuccess(Encoding.UTF8.GetBytes("<xmi/>"));
            }

            handler.EnqueueSuccess(Encoding.UTF8.GetBytes("%PDF-1.5 fake spec"));
            handler.EnqueueSuccess(Encoding.UTF8.GetBytes("%PDF-1.5 fake changebar"));

            var fetcher = new SourceFetcher(new HttpClient(handler));

            await fetcher.FetchAsync(Descriptor, this.tempDirectory, includeSpecs: true);

            Assert.That(File.Exists(Path.Combine(this.tempDirectory, "2.5.1", "specs", "UML-2.5.1.pdf")));
            Assert.That(File.Exists(Path.Combine(this.tempDirectory, "2.5.1", "specs", "UML-2.5.1-changebar.pdf")));
        }

        [Test]
        public void FetchAsync_throws_when_a_pdf_url_does_not_return_a_pdf()
        {
            var handler = new StubHttpMessageHandler();
            for (var i = 0; i < 4; i++)
            {
                handler.EnqueueSuccess(Encoding.UTF8.GetBytes("<xmi/>"));
            }

            handler.EnqueueSuccess(Encoding.UTF8.GetBytes("<html>404 Not Found</html>"));

            var fetcher = new SourceFetcher(new HttpClient(handler));

            Assert.That(
                async () => await fetcher.FetchAsync(Descriptor, this.tempDirectory, includeSpecs: true),
                Throws.TypeOf<InvalidDataException>());
        }

        [Test]
        public async Task FetchAsync_writes_a_fetch_manifest_with_sha256_and_url_per_file()
        {
            var handler = new StubHttpMessageHandler();
            for (var i = 0; i < 4; i++)
            {
                handler.EnqueueSuccess(Encoding.UTF8.GetBytes("<xmi/>"));
            }

            var fetcher = new SourceFetcher(new HttpClient(handler));

            await fetcher.FetchAsync(Descriptor, this.tempDirectory, includeSpecs: false);

            var manifestPath = Path.Combine(this.tempDirectory, "2.5.1", "fetch-manifest.json");
            Assert.That(File.Exists(manifestPath));

            var manifestJson = File.ReadAllText(manifestPath);
            Assert.That(manifestJson, Does.Contain("\"url\""));
            Assert.That(manifestJson, Does.Contain("\"sha256\""));
            Assert.That(manifestJson, Does.Contain("https://example.com/UML.xmi"));
        }

        [Test]
        public async Task FetchAsync_retries_on_transient_failure_then_succeeds()
        {
            var handler = new StubHttpMessageHandler();
            handler.Enqueue(_ => throw new HttpRequestException("transient"));
            handler.EnqueueSuccess(Encoding.UTF8.GetBytes("<xmi/>"));
            for (var i = 0; i < 3; i++)
            {
                handler.EnqueueSuccess(Encoding.UTF8.GetBytes("<xmi/>"));
            }

            var fetcher = new SourceFetcher(new HttpClient(handler));

            var entries = await fetcher.FetchAsync(Descriptor, this.tempDirectory, includeSpecs: false);

            Assert.That(entries, Has.Count.EqualTo(4));
            Assert.That(handler.RequestCount, Is.EqualTo(5));
        }

        [Test]
        public async Task FetchXmiSpecAsync_downloads_the_pdf_into_its_own_top_level_sibling_tree()
        {
            var xmiDescriptor = new XmiSpecDescriptor(
                Version: "2.5.1",
                IsCurrent: true,
                SpecificationPdfUrl: "https://example.com/XMI/PDF",
                OmgDocumentId: "formal/15-06-07");

            var handler = new StubHttpMessageHandler();
            handler.EnqueueSuccess(Encoding.UTF8.GetBytes("%PDF-1.5 fake xmi spec"));

            var fetcher = new SourceFetcher(new HttpClient(handler));

            var entries = await fetcher.FetchXmiSpecAsync(xmiDescriptor, this.tempDirectory);

            Assert.That(handler.RequestCount, Is.EqualTo(1));
            Assert.That(entries, Has.Count.EqualTo(1));
            Assert.That(File.Exists(Path.Combine(this.tempDirectory, "xmi", "2.5.1", "specs", "XMI-2.5.1.pdf")));
            Assert.That(File.Exists(Path.Combine(this.tempDirectory, "xmi", "2.5.1", "fetch-manifest.json")));
        }

        [Test]
        public void FetchXmiSpecAsync_throws_when_the_url_does_not_return_a_pdf()
        {
            var xmiDescriptor = new XmiSpecDescriptor(
                Version: "2.5.1",
                IsCurrent: true,
                SpecificationPdfUrl: "https://example.com/XMI/PDF",
                OmgDocumentId: "formal/15-06-07");

            var handler = new StubHttpMessageHandler();
            handler.EnqueueSuccess(Encoding.UTF8.GetBytes("<html>404 Not Found</html>"));

            var fetcher = new SourceFetcher(new HttpClient(handler));

            Assert.That(
                async () => await fetcher.FetchXmiSpecAsync(xmiDescriptor, this.tempDirectory),
                Throws.TypeOf<InvalidDataException>());
        }
    }
}
