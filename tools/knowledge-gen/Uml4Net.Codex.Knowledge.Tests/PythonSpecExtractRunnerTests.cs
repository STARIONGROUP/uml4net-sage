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

namespace Uml4Net.Codex.Knowledge.Tests
{
    using System.IO;
    using System.Threading.Tasks;

    [TestFixture]
    public class PythonSpecExtractRunnerTests
    {
        private string tempDirectory = null!;

        [SetUp]
        public void SetUp()
        {
            this.tempDirectory = Path.Combine(Path.GetTempPath(), "uml4net-codex-tests", Path.GetRandomFileName());
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
            var runner = new PythonSpecExtractRunner(FakeProcessRunner.Returning(new ProcessRunResult(0, string.Empty, string.Empty)));

            var outcome = await runner.ExtractAsync(
                Path.Combine(this.tempDirectory, "missing.pdf"),
                Path.Combine(this.tempDirectory, "spec"),
                this.tempDirectory,
                "2.5.1");

            Assert.That(outcome.Succeeded, Is.False);
            Assert.That(outcome.SkippedReason, Does.Contain("fetch"));
        }

        [Test]
        public async Task ExtractAsync_skips_with_a_clear_reason_when_the_spec_extract_project_is_missing()
        {
            var pdfPath = Path.Combine(this.tempDirectory, "UML-2.5.1.pdf");
            File.WriteAllText(pdfPath, "not a real pdf");
            var incompleteCheckout = Path.Combine(this.tempDirectory, "no-such-checkout");

            var runner = new PythonSpecExtractRunner(FakeProcessRunner.Returning(new ProcessRunResult(0, string.Empty, string.Empty)));

            var outcome = await runner.ExtractAsync(pdfPath, Path.Combine(this.tempDirectory, "spec"), incompleteCheckout, "2.5.1");

            Assert.That(outcome.Succeeded, Is.False);
            Assert.That(outcome.SkippedReason, Does.Contain("spec-extract project"));
        }

        [Test]
        public async Task ExtractAsync_skips_with_a_clear_reason_when_python_is_not_available()
        {
            var pdfPath = Path.Combine(this.tempDirectory, "UML-2.5.1.pdf");
            File.WriteAllText(pdfPath, "not a real pdf");

            var runner = new PythonSpecExtractRunner(FakeProcessRunner.NotFound());

            var outcome = await runner.ExtractAsync(pdfPath, Path.Combine(this.tempDirectory, "spec"), this.tempDirectory, "2.5.1");

            Assert.That(outcome.Succeeded, Is.False);
            Assert.That(outcome.SkippedReason, Does.Contain("Python was not found"));
        }

        [Test]
        public async Task ExtractAsync_succeeds_and_invokes_the_module_with_the_expected_arguments()
        {
            var pdfPath = Path.Combine(this.tempDirectory, "UML-2.5.1.pdf");
            File.WriteAllText(pdfPath, "not a real pdf");
            var outputDirectory = Path.Combine(this.tempDirectory, "spec");

            var processRunner = FakeProcessRunner.Returning(new ProcessRunResult(0, "Extracted 42 clauses", string.Empty));
            var runner = new PythonSpecExtractRunner(processRunner);

            var outcome = await runner.ExtractAsync(pdfPath, outputDirectory, this.tempDirectory, "2.5.1");

            Assert.That(outcome.Succeeded, Is.True);
            Assert.That(processRunner.LastArguments, Is.EqualTo(new[] { "-m", "spec_extract", "extract", "--pdf", pdfPath, "--out", outputDirectory, "--document", "UML", "--version", "2.5.1" }));
        }

        [Test]
        public async Task ExtractAsync_skips_with_the_stderr_when_the_module_exits_non_zero()
        {
            var pdfPath = Path.Combine(this.tempDirectory, "UML-2.5.1.pdf");
            File.WriteAllText(pdfPath, "not a real pdf");

            var runner = new PythonSpecExtractRunner(FakeProcessRunner.Returning(new ProcessRunResult(1, string.Empty, "boom")));

            var outcome = await runner.ExtractAsync(pdfPath, Path.Combine(this.tempDirectory, "spec"), this.tempDirectory, "2.5.1");

            Assert.That(outcome.Succeeded, Is.False);
            Assert.That(outcome.SkippedReason, Does.Contain("boom"));
        }
    }
}
