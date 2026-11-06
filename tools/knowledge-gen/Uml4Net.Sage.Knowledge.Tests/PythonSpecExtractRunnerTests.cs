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

        /// <summary>
        /// Creates a temporary directory with a <c>src</c> subfolder, standing in for the
        /// <c>tools/spec-extract</c> project.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.tempDirectory = Path.Combine(Path.GetTempPath(), "uml4net-sage-tests", Path.GetRandomFileName());
            Directory.CreateDirectory(Path.Combine(this.tempDirectory, "src"));
        }

        /// <summary>
        /// Deletes the temporary directory and everything in it.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            Directory.Delete(this.tempDirectory, recursive: true);
        }

        /// <summary>
        /// Verifies that a missing PDF skips the extraction with a reason that points to <c>fetch</c>.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
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

        /// <summary>
        /// Verifies that a spec-extract directory without a <c>src</c> folder skips the extraction with a
        /// reason that names the spec-extract project.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
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

        /// <summary>
        /// Verifies that, when no Python interpreter starts and uv cannot be provisioned, the extraction
        /// is skipped with a reason that says both.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
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

        /// <summary>
        /// Verifies that a zero exit code with a non-empty clause index succeeds, and that the
        /// interpreter is passed the <c>spec_extract extract</c> module arguments for the UML document.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
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

        /// <summary>
        /// Verifies that the document name given to the runner, here <c>XMI</c>, is passed on as the
        /// <c>--document</c> argument.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
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

        /// <summary>
        /// Verifies that a zero exit code with an empty clause index is skipped with a reason that
        /// mentions 0 clauses.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
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

        /// <summary>
        /// Verifies that a zero exit code without an <c>index.json</c> is skipped with a reason that
        /// names <c>index.json</c>.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
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

        /// <summary>
        /// Verifies that a non-zero exit code skips the extraction with a reason that includes the
        /// process's standard error.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
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

        /// <summary>
        /// Verifies that, when no Python interpreter starts, the provisioned uv executable is run with
        /// <c>run --project &lt;dir&gt; python</c> followed by the module arguments, and the extraction
        /// succeeds.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
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

        /// <summary>
        /// Verifies that a non-zero exit code from the uv fallback skips the extraction with a reason
        /// that includes uv's standard error.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
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

        /// <summary>
        /// Verifies that a skipped reason says that metamodel and Standard Profile lookups are unaffected.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task ExtractAsync_states_the_net_effect_whenever_it_skips()
        {
            var pdfPath = Path.Combine(this.tempDirectory, "UML-2.5.1.pdf");
            File.WriteAllText(pdfPath, "not a real pdf");

            var runner = new PythonSpecExtractRunner(FakeProcessRunner.Returning(new ProcessRunResult(1, string.Empty, "boom")), FakeUvProvisioner.Unavailable());

            var outcome = await runner.ExtractAsync(pdfPath, Path.Combine(this.tempDirectory, "spec"), this.tempDirectory, "2.5.1", "UML");

            Assert.That(outcome.Succeeded, Is.False);
            Assert.That(outcome.SkippedReason, Does.Contain("Metamodel and Standard Profile lookups are unaffected"));
        }

        /// <summary>
        /// Verifies that a Python candidate failing with <c>ModuleNotFoundError</c> does not stop the uv
        /// fallback, which then succeeds.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task ExtractAsync_falls_back_to_uv_when_a_python_candidate_is_missing_its_dependencies()
        {
            var pdfPath = Path.Combine(this.tempDirectory, "UML-2.5.1.pdf");
            File.WriteAllText(pdfPath, "not a real pdf");
            var outputDirectory = Path.Combine(this.tempDirectory, "spec");
            WriteFakeIndex(outputDirectory, clauseCount: 42);

            var uvExecutable = new FileInfo(Path.Combine(this.tempDirectory, "uv.exe"));
            var moduleNotFound = new ProcessRunResult(1, string.Empty, "ModuleNotFoundError: No module named 'pdfplumber'");
            var uvSuccess = new ProcessRunResult(0, "Extracted 42 clauses", string.Empty);
            var processRunner = FakeProcessRunner.RespondingDifferentlyFor(uvExecutable.FullName, uvSuccess, moduleNotFound);
            var runner = new PythonSpecExtractRunner(processRunner, FakeUvProvisioner.Returning(uvExecutable));

            var outcome = await runner.ExtractAsync(pdfPath, outputDirectory, this.tempDirectory, "2.5.1", "UML");

            Assert.That(outcome.Succeeded, Is.True, "a missing dependency on a plain python candidate should not stop the uv fallback from being tried");
        }

        /// <summary>
        /// Verifies that, when the Python candidates fail with <c>ModuleNotFoundError</c> and uv is
        /// unavailable, the skipped reason names pdfplumber, gives the <c>pip install</c> command and
        /// states the net effect.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task ExtractAsync_names_pdfplumber_and_gives_an_install_command_when_dependencies_are_missing_and_uv_is_unavailable()
        {
            var pdfPath = Path.Combine(this.tempDirectory, "UML-2.5.1.pdf");
            File.WriteAllText(pdfPath, "not a real pdf");

            var moduleNotFound = new ProcessRunResult(1, string.Empty, "ModuleNotFoundError: No module named 'spec_extract'");
            var runner = new PythonSpecExtractRunner(FakeProcessRunner.Returning(moduleNotFound), FakeUvProvisioner.Unavailable());

            var outcome = await runner.ExtractAsync(pdfPath, Path.Combine(this.tempDirectory, "spec"), this.tempDirectory, "2.5.1", "UML");

            Assert.That(outcome.Succeeded, Is.False);
            Assert.That(outcome.SkippedReason, Does.Contain("pdfplumber"));
            Assert.That(outcome.SkippedReason, Does.Contain("pip install -e tools/spec-extract"));
            Assert.That(outcome.SkippedReason, Does.Contain("Metamodel and Standard Profile lookups are unaffected"));
        }

        /// <summary>
        /// Verifies that, when the Python candidates fail with <c>ModuleNotFoundError</c> and the uv
        /// fallback also fails, the skipped reason names pdfplumber, gives the <c>pip install</c> command,
        /// includes uv's standard error and states the net effect.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task ExtractAsync_names_pdfplumber_and_gives_an_install_command_when_dependencies_are_missing_and_the_uv_fallback_also_fails()
        {
            var pdfPath = Path.Combine(this.tempDirectory, "UML-2.5.1.pdf");
            File.WriteAllText(pdfPath, "not a real pdf");

            var uvExecutable = new FileInfo(Path.Combine(this.tempDirectory, "uv.exe"));
            var moduleNotFound = new ProcessRunResult(1, string.Empty, "ModuleNotFoundError: No module named 'pdfplumber'");
            var uvFailure = new ProcessRunResult(1, string.Empty, "uv: network error");
            var processRunner = FakeProcessRunner.RespondingDifferentlyFor(uvExecutable.FullName, uvFailure, moduleNotFound);
            var runner = new PythonSpecExtractRunner(processRunner, FakeUvProvisioner.Returning(uvExecutable));

            var outcome = await runner.ExtractAsync(pdfPath, Path.Combine(this.tempDirectory, "spec"), this.tempDirectory, "2.5.1", "UML");

            Assert.That(outcome.Succeeded, Is.False);
            Assert.That(outcome.SkippedReason, Does.Contain("pdfplumber"));
            Assert.That(outcome.SkippedReason, Does.Contain("pip install -e tools/spec-extract"));
            Assert.That(outcome.SkippedReason, Does.Contain("uv: network error"));
            Assert.That(outcome.SkippedReason, Does.Contain("Metamodel and Standard Profile lookups are unaffected"));
        }

        /// <summary>
        /// Writes an <c>index.json</c> holding a JSON array of <paramref name="clauseCount"/> empty
        /// objects, as if <c>spec_extract</c> had produced it.
        /// </summary>
        /// <param name="outputDirectory">The directory to write into; it is created if needed.</param>
        /// <param name="clauseCount">The number of clause entries to write.</param>
        private static void WriteFakeIndex(string outputDirectory, int clauseCount)
        {
            Directory.CreateDirectory(outputDirectory);
            var rows = string.Join(",", Enumerable.Repeat("{}", clauseCount));
            File.WriteAllText(Path.Combine(outputDirectory, "index.json"), $"[{rows}]");
        }
    }
}
