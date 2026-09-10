// -------------------------------------------------------------------------------------------------
// <copyright file="PythonSpecExtractRunnerTests.cs" company="Starion Group S.A.">
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
    using System.Threading.Tasks;

    [TestFixture]
    public class PythonSpecExtractRunnerTests
    {
        private string tempDirectory = null!;

        [SetUp]
        public void SetUp()
        {
            this.tempDirectory = Path.Combine(Path.GetTempPath(), "uml4net-sage-tests", Path.GetRandomFileName());
            Directory.CreateDirectory(Path.Combine(this.tempDirectory, "src"));
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(this.tempDirectory, recursive: true);
        }

        [Test]
        public async Task ExtractAsync_skips_with_a_clear_reason_when_the_pdf_is_missing()
        {
            var runner = new PythonSpecExtractRunner(FakeProcessRunner.Returning(new ProcessRunResult(0, string.Empty, string.Empty)), FakeUvProvisioner.Unavailable());

            var outcome = await runner.ExtractAsync(
                Path.Combine(this.tempDirectory, "missing.pdf"),
                Path.Combine(this.tempDirectory, "spec"),
                this.tempDirectory,
                "2.5.1",
                "UML");

            Assert.That(outcome.Succeeded, Is.False);
            Assert.That(outcome.SkippedReason, Does.Contain("fetch"));
        }

        [Test]
        public async Task ExtractAsync_skips_with_a_clear_reason_when_the_spec_extract_project_is_missing()
        {
            var pdfPath = Path.Combine(this.tempDirectory, "UML-2.5.1.pdf");
            File.WriteAllText(pdfPath, "not a real pdf");
            var incompleteCheckout = Path.Combine(this.tempDirectory, "no-such-checkout");

            var runner = new PythonSpecExtractRunner(FakeProcessRunner.Returning(new ProcessRunResult(0, string.Empty, string.Empty)), FakeUvProvisioner.Unavailable());

            var outcome = await runner.ExtractAsync(pdfPath, Path.Combine(this.tempDirectory, "spec"), incompleteCheckout, "2.5.1", "UML");

            Assert.That(outcome.Succeeded, Is.False);
            Assert.That(outcome.SkippedReason, Does.Contain("spec-extract project"));
        }

        [Test]
        public async Task ExtractAsync_skips_with_a_clear_reason_when_neither_python_nor_uv_is_available()
        {
            var pdfPath = Path.Combine(this.tempDirectory, "UML-2.5.1.pdf");
            File.WriteAllText(pdfPath, "not a real pdf");

            var runner = new PythonSpecExtractRunner(FakeProcessRunner.NotFound(), FakeUvProvisioner.Unavailable());

            var outcome = await runner.ExtractAsync(pdfPath, Path.Combine(this.tempDirectory, "spec"), this.tempDirectory, "2.5.1", "UML");

            Assert.That(outcome.Succeeded, Is.False);
            Assert.That(outcome.SkippedReason, Does.Contain("No Python interpreter was found"));
            Assert.That(outcome.SkippedReason, Does.Contain("uv could not be provisioned"));
        }

        [Test]
        public async Task ExtractAsync_succeeds_and_invokes_the_module_with_the_expected_arguments()
        {
            var pdfPath = Path.Combine(this.tempDirectory, "UML-2.5.1.pdf");
            File.WriteAllText(pdfPath, "not a real pdf");
            var outputDirectory = Path.Combine(this.tempDirectory, "spec");
            WriteFakeIndex(outputDirectory, clauseCount: 42);

            var processRunner = FakeProcessRunner.Returning(new ProcessRunResult(0, "Extracted 42 clauses", string.Empty));
            var runner = new PythonSpecExtractRunner(processRunner, FakeUvProvisioner.Unavailable());

            var outcome = await runner.ExtractAsync(pdfPath, outputDirectory, this.tempDirectory, "2.5.1", "UML");

            Assert.That(outcome.Succeeded, Is.True);
            Assert.That(processRunner.LastArguments, Is.EqualTo(new[] { "-m", "spec_extract", "extract", "--pdf", pdfPath, "--out", outputDirectory, "--document", "UML", "--version", "2.5.1" }));
        }

        [Test]
        public async Task ExtractAsync_is_invoked_with_the_given_document_name()
        {
            var pdfPath = Path.Combine(this.tempDirectory, "XMI-2.5.1.pdf");
            File.WriteAllText(pdfPath, "not a real pdf");
            var outputDirectory = Path.Combine(this.tempDirectory, "xmi-spec");
            WriteFakeIndex(outputDirectory, clauseCount: 141);

            var processRunner = FakeProcessRunner.Returning(new ProcessRunResult(0, "Extracted 141 clauses", string.Empty));
            var runner = new PythonSpecExtractRunner(processRunner, FakeUvProvisioner.Unavailable());

            var outcome = await runner.ExtractAsync(pdfPath, outputDirectory, this.tempDirectory, "2.5.1", "XMI");

            Assert.That(outcome.Succeeded, Is.True);
            Assert.That(processRunner.LastArguments, Is.EqualTo(new[] { "-m", "spec_extract", "extract", "--pdf", pdfPath, "--out", outputDirectory, "--document", "XMI", "--version", "2.5.1" }));
        }

        [Test]
        public async Task ExtractAsync_skips_when_the_module_exits_zero_but_extracted_no_clauses()
        {
            var pdfPath = Path.Combine(this.tempDirectory, "UML-2.5.1.pdf");
            File.WriteAllText(pdfPath, "not a real pdf");
            var outputDirectory = Path.Combine(this.tempDirectory, "spec");
            WriteFakeIndex(outputDirectory, clauseCount: 0);

            var runner = new PythonSpecExtractRunner(FakeProcessRunner.Returning(new ProcessRunResult(0, string.Empty, string.Empty)), FakeUvProvisioner.Unavailable());

            var outcome = await runner.ExtractAsync(pdfPath, outputDirectory, this.tempDirectory, "2.5.1", "UML");

            Assert.That(outcome.Succeeded, Is.False);
            Assert.That(outcome.SkippedReason, Does.Contain("0 clauses"));
        }

        [Test]
        public async Task ExtractAsync_skips_when_the_module_exits_zero_but_writes_no_index()
        {
            var pdfPath = Path.Combine(this.tempDirectory, "UML-2.5.1.pdf");
            File.WriteAllText(pdfPath, "not a real pdf");
            var outputDirectory = Path.Combine(this.tempDirectory, "spec");

            var runner = new PythonSpecExtractRunner(FakeProcessRunner.Returning(new ProcessRunResult(0, string.Empty, string.Empty)), FakeUvProvisioner.Unavailable());

            var outcome = await runner.ExtractAsync(pdfPath, outputDirectory, this.tempDirectory, "2.5.1", "UML");

            Assert.That(outcome.Succeeded, Is.False);
            Assert.That(outcome.SkippedReason, Does.Contain("index.json"));
        }

        [Test]
        public async Task ExtractAsync_skips_with_the_stderr_when_the_module_exits_non_zero()
        {
            var pdfPath = Path.Combine(this.tempDirectory, "UML-2.5.1.pdf");
            File.WriteAllText(pdfPath, "not a real pdf");

            var runner = new PythonSpecExtractRunner(FakeProcessRunner.Returning(new ProcessRunResult(1, string.Empty, "boom")), FakeUvProvisioner.Unavailable());

            var outcome = await runner.ExtractAsync(pdfPath, Path.Combine(this.tempDirectory, "spec"), this.tempDirectory, "2.5.1", "UML");

            Assert.That(outcome.Succeeded, Is.False);
            Assert.That(outcome.SkippedReason, Does.Contain("boom"));
        }

        [Test]
        public async Task ExtractAsync_falls_back_to_uv_and_succeeds_when_no_python_interpreter_is_found()
        {
            var pdfPath = Path.Combine(this.tempDirectory, "UML-2.5.1.pdf");
            File.WriteAllText(pdfPath, "not a real pdf");
            var outputDirectory = Path.Combine(this.tempDirectory, "spec");
            WriteFakeIndex(outputDirectory, clauseCount: 42);

            var uvExecutable = new FileInfo(Path.Combine(this.tempDirectory, "uv.exe"));
            var processRunner = FakeProcessRunner.NotFoundExceptFor(
                uvExecutable.FullName, new ProcessRunResult(0, "Extracted 42 clauses", string.Empty));
            var runner = new PythonSpecExtractRunner(processRunner, FakeUvProvisioner.Returning(uvExecutable));

            var outcome = await runner.ExtractAsync(pdfPath, outputDirectory, this.tempDirectory, "2.5.1", "UML");

            Assert.That(outcome.Succeeded, Is.True);
            Assert.That(processRunner.LastFileName, Is.EqualTo(uvExecutable.FullName));
            Assert.That(
                processRunner.LastArguments,
                Is.EqualTo(new[] { "run", "--project", this.tempDirectory, "python", "-m", "spec_extract", "extract", "--pdf", pdfPath, "--out", outputDirectory, "--document", "UML", "--version", "2.5.1" }));
        }

        [Test]
        public async Task ExtractAsync_skips_with_the_stderr_when_the_uv_fallback_exits_non_zero()
        {
            var pdfPath = Path.Combine(this.tempDirectory, "UML-2.5.1.pdf");
            File.WriteAllText(pdfPath, "not a real pdf");

            var uvExecutable = new FileInfo(Path.Combine(this.tempDirectory, "uv.exe"));
            var processRunner = FakeProcessRunner.NotFoundExceptFor(
                uvExecutable.FullName, new ProcessRunResult(1, string.Empty, "uv boom"));
            var runner = new PythonSpecExtractRunner(processRunner, FakeUvProvisioner.Returning(uvExecutable));

            var outcome = await runner.ExtractAsync(pdfPath, Path.Combine(this.tempDirectory, "spec"), this.tempDirectory, "2.5.1", "UML");

            Assert.That(outcome.Succeeded, Is.False);
            Assert.That(outcome.SkippedReason, Does.Contain("uv boom"));
        }

        private static void WriteFakeIndex(string outputDirectory, int clauseCount)
        {
            Directory.CreateDirectory(outputDirectory);
            var rows = string.Join(",", Enumerable.Repeat("{}", clauseCount));
            File.WriteAllText(Path.Combine(outputDirectory, "index.json"), $"[{rows}]");
        }
    }
}
