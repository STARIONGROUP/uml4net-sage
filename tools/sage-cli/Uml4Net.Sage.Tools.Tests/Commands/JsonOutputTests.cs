// -------------------------------------------------------------------------------------------------
// <copyright file="JsonOutputTests.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Sage.Tools.Tests.Commands
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using Uml4Net.Sage.Knowledge;
    using Uml4Net.Sage.Tools.Commands;

    /// <summary>
    /// Pins the <c>--json</c> contract of <c>check</c>, <c>versions</c> and <c>inspect</c>: one line on
    /// standard output and nothing else on either stream, <c>{"error": "..."}</c> on every failure, and
    /// the property names. A test that fails here because a name changed means a breaking change for the
    /// SessionStart hook, the skills and any script that reads the output; update tools/README.md along
    /// with it.
    /// </summary>
    [TestFixture]
    public class JsonOutputTests
    {
        private const string UmlVersion = "2.5.1";

        private static string FixturesDirectory => Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures");

        private string repositoryRoot = null!;
        private RepositoryLayout layout = null!;

        /// <summary>
        /// Picks a fresh repository root under the temp folder for each test, without creating it, and its layout.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.repositoryRoot = Path.Combine(Path.GetTempPath(), "uml4net-sage-tests", Path.GetRandomFileName());
            this.layout = new RepositoryLayout(this.repositoryRoot);
        }

        /// <summary>
        /// Deletes the test's repository root if the test created it.
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
        /// Verifies that each <c>--json</c> verb, on success and on findings, writes exactly one line that parses as
        /// JSON, nothing straight to standard output or standard error, and returns the expected exit code.
        /// </summary>
        /// <param name="verb">The verb and, for <c>inspect</c>, the fixture model to inspect.</param>
        /// <param name="expectedExitCode">The exit code the run must return.</param>
        [TestCase("check", 0)]
        [TestCase("versions", 0)]
        [TestCase("inspect model-valid.xmi", 0)]
        [TestCase("inspect model-abstract-type.xmi", 1)]
        [TestCase("inspect model-stereotype-unresolved.xmi", 0)]
        public async Task Json_output_is_one_line_holding_one_document_and_nothing_else(string verb, int expectedExitCode)
        {
            this.InstallGeneratedVersion();

            var run = await this.Run(verb);

            Assert.Multiple(() =>
            {
                Assert.That(run.ExitCode, Is.EqualTo(expectedExitCode));
                Assert.That(run.Lines, Has.Length.EqualTo(1), "the JSON document has to be the only line");
                Assert.That(() => JsonDocument.Parse(run.Lines[0]).Dispose(), Throws.Nothing);
                Assert.That(run.StandardOutput, Is.Empty, "nothing may bypass the JSON writer");
                Assert.That(run.StandardError, Is.Empty);
            });
        }

        /// <summary>
        /// Verifies that the <c>check</c> document and its <c>Versions</c> and <c>XmiSpecs</c> entries carry exactly the
        /// pinned property names, with 2.5.1 as the default and <c>HasAnyVersion</c> true.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task Check_json_has_the_pinned_property_names()
        {
            this.InstallGeneratedVersion();
            new InstalledVersionsStore(this.layout.KnowledgeRoot).MarkXmiSpecFetched("2.5.1");

            using var document = await this.RunDocument("check");
            var root = document.RootElement;

            Assert.Multiple(() =>
            {
                Assert.That(Names(root), Is.EquivalentTo(new[] { "Default", "HasAnyVersion", "Versions", "XmiSpecs" }));
                Assert.That(Names(root.GetProperty("Versions")[0]), Is.EquivalentTo(new[] { "Version", "Fetched", "Generated", "SpecGenerated" }));
                Assert.That(Names(root.GetProperty("XmiSpecs")[0]), Is.EquivalentTo(new[] { "Version", "Fetched", "Generated" }));
                Assert.That(root.GetProperty("Default").GetString(), Is.EqualTo(UmlVersion));
                Assert.That(root.GetProperty("HasAnyVersion").GetBoolean(), Is.True);
            });
        }

        /// <summary>
        /// Verifies that on an empty repository the <c>check</c> document has <c>Default</c> as <c>null</c>,
        /// <c>HasAnyVersion</c> false and an empty <c>Versions</c> array.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task Check_json_writes_an_absent_default_as_null_rather_than_leaving_it_out()
        {
            using var document = await this.RunDocument("check");
            var root = document.RootElement;

            Assert.Multiple(() =>
            {
                Assert.That(root.GetProperty("Default").ValueKind, Is.EqualTo(JsonValueKind.Null));
                Assert.That(root.GetProperty("HasAnyVersion").GetBoolean(), Is.False);
                Assert.That(root.GetProperty("Versions").GetArrayLength(), Is.Zero);
            });
        }

        /// <summary>
        /// Verifies that the <c>versions</c> document is an array whose first row carries exactly the pinned property
        /// names and describes 2.5.1 as the default.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task Versions_json_has_the_pinned_property_names()
        {
            this.InstallGeneratedVersion();

            using var document = await this.RunDocument("versions");
            var root = document.RootElement;

            Assert.Multiple(() =>
            {
                Assert.That(root.ValueKind, Is.EqualTo(JsonValueKind.Array));
                Assert.That(
                    Names(root[0]),
                    Is.EquivalentTo(new[] { "Version", "IsCurrent", "IsDefault", "Fetched", "Generated", "SpecGenerated" }));
                Assert.That(root[0].GetProperty("Version").GetString(), Is.EqualTo(UmlVersion));
                Assert.That(root[0].GetProperty("IsDefault").GetBoolean(), Is.True);
            });
        }

        /// <summary>
        /// Verifies that the <c>inspect</c> report and its findings carry exactly the pinned property names, and that the
        /// abstract-instantiation finding for the fixture names element <c>Vehicle</c> on line 4 as an error.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task Inspect_json_has_the_pinned_property_names()
        {
            this.InstallGeneratedVersion();

            using var document = await this.RunDocument("inspect model-abstract-type.xmi");
            var root = document.RootElement;
            var finding = FindingOf(root, "abstract-instantiation");

            Assert.Multiple(() =>
            {
                Assert.That(Names(root), Is.EquivalentTo(new[] { "ModelPath", "UmlVersion", "Findings" }));
                Assert.That(Names(finding), Is.EquivalentTo(new[] { "Severity", "Category", "ElementXmiId", "Message", "Line" }));
                Assert.That(root.GetProperty("UmlVersion").GetString(), Is.EqualTo(UmlVersion));
                Assert.That(finding.GetProperty("Severity").GetString(), Is.EqualTo("error"));
                Assert.That(finding.GetProperty("ElementXmiId").GetString(), Is.EqualTo("Vehicle"));
                Assert.That(finding.GetProperty("Line").GetInt32(), Is.EqualTo(4));
            });
        }

        /// <summary>
        /// Verifies that the read-aborted finding, which has no element or line, writes <c>ElementXmiId</c> and
        /// <c>Line</c> as <c>null</c>.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task Inspect_json_writes_an_absent_element_and_line_as_null_rather_than_leaving_them_out()
        {
            this.InstallGeneratedVersion();

            using var document = await this.RunDocument("inspect model-abstract-type.xmi");
            var finding = FindingOf(document.RootElement, "read-aborted");

            Assert.Multiple(() =>
            {
                Assert.That(finding.GetProperty("ElementXmiId").ValueKind, Is.EqualTo(JsonValueKind.Null));
                Assert.That(finding.GetProperty("Line").ValueKind, Is.EqualTo(JsonValueKind.Null));
            });
        }

        /// <summary>
        /// Verifies that the failures <c>inspect</c> reports itself (version not generated, model file missing,
        /// malformed <c>--pathmap</c>) are a single error document with the expected message and exit code 1.
        /// </summary>
        /// <param name="verb">The <c>inspect</c> command line, without <c>--json</c> and the repository root.</param>
        /// <param name="generated">Whether to install a generated version first.</param>
        /// <param name="expectedMessage">Text the error message must contain.</param>
        /// <returns>A task that completes when the test has run.</returns>
        [TestCase("inspect model-valid.xmi", false, "has not been generated yet")]
        [TestCase("inspect no-such-model.xmi", true, "File not found")]
        [TestCase("inspect model-valid.xmi --pathmap pathmap://SAMPLE_LIBRARY", true, "Invalid --pathmap")]
        public async Task An_inspect_failure_is_a_json_error(string verb, bool generated, string expectedMessage)
        {
            if (generated)
            {
                this.InstallGeneratedVersion();
            }

            var run = await this.Run(verb);

            Assert.Multiple(() =>
            {
                Assert.That(run.ExitCode, Is.EqualTo(1));
                AssertIsError(run, expectedMessage);
            });
        }

        /// <summary>
        /// Verifies that inspecting a file that is not XML gives a single "A file could not be read" error document
        /// with exit code 1.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task An_inspect_of_a_file_that_is_not_xml_is_a_json_error()
        {
            this.InstallGeneratedVersion();
            var notXml = Path.Combine(this.repositoryRoot, "not-xml.xmi");
            await File.WriteAllTextAsync(notXml, "not xml");

            var run = await this.Run(["inspect", notXml]);

            Assert.Multiple(() =>
            {
                Assert.That(run.ExitCode, Is.EqualTo(1));
                AssertIsError(run, "A file could not be read");
            });
        }

        /// <summary>
        /// Verifies that a corrupt <c>installed.json</c> gives a single error document naming the file, with exit code 1.
        /// </summary>
        /// <param name="verb">The verb to run.</param>
        /// <returns>A task that completes when the test has run.</returns>
        [TestCase("check")]
        [TestCase("versions")]
        public async Task A_corrupt_installed_manifest_is_a_json_error_naming_the_file(string verb)
        {
            await this.WriteCorruptManifest();

            var run = await this.Run(verb);

            Assert.Multiple(() =>
            {
                Assert.That(run.ExitCode, Is.EqualTo(1));
                AssertIsError(run, "installed.json is not valid JSON");
            });
        }

        /// <summary>
        /// Verifies that a <c>metamodel.json</c> that is not JSON, or lacks the expected shape (missing properties, a
        /// value of the wrong kind, a non-integer bound), gives a single error document naming the file, with exit code 1.
        /// </summary>
        /// <param name="content">The content written as <c>metamodel.json</c>.</param>
        /// <returns>A task that completes when the test has run.</returns>
        [TestCase("{ not json")]
        [TestCase("{}")]
        [TestCase("[]")]
        [TestCase("""{"classes": [{"name": "Class"}]}""")]
        [TestCase("""{"classes": [{"name": "Class", "isAbstract": "no"}]}""")]
        [TestCase("""{"classes": [{"name": "Class", "isAbstract": false, "ownedAttributes": [{"name": "a", "ownerQualifiedName": "UML::Class", "isDerived": false, "lower": 1.5, "upper": "1"}]}]}""")]
        public async Task A_malformed_metamodel_is_a_json_error_naming_the_file(string content)
        {
            var metamodelDirectory = Path.Combine(this.layout.KnowledgeDirectoryFor(UmlVersion), "metamodel");
            Directory.CreateDirectory(metamodelDirectory);
            await File.WriteAllTextAsync(Path.Combine(metamodelDirectory, "metamodel.json"), content);

            var run = await this.Run("inspect model-valid.xmi");

            Assert.Multiple(() =>
            {
                Assert.That(run.ExitCode, Is.EqualTo(1));
                AssertIsError(run, "metamodel.json is not a valid metamodel.json");
            });
        }

        /// <summary>
        /// Verifies that running <c>check --json</c> with an already cancelled token gives a single "Cancelled." error
        /// document with exit code 130.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task A_token_cancelled_before_the_verb_starts_is_a_json_error_with_exit_code_130()
        {
            using var cancelled = new CancellationTokenSource();
            await cancelled.CancelAsync();

            var run = await this.Run("check", cancelled.Token);

            Assert.Multiple(() =>
            {
                Assert.That(run.ExitCode, Is.EqualTo(130));
                AssertIsError(run, "Cancelled.");
            });
        }

        /// <summary>
        /// Verifies that an <see cref="OperationCanceledException"/> thrown from inside the action is turned by
        /// <see cref="JsonOutput.RunAsync"/> into a single "Cancelled." error document with exit code 130.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task Cancellation_thrown_by_the_action_is_a_json_error_with_exit_code_130()
        {
            using var console = new RecordedConsole();
            using var source = new CancellationTokenSource();

            var exitCode = await JsonOutput.RunAsync(Program.BuildRootCommand().Parse("check --json"), source.Token, () =>
            {
                source.Cancel();
                source.Token.ThrowIfCancellationRequested();
                return 0;
            });

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(130));
                AssertIsError(console.Lines, "Cancelled.");
            });
        }

        /// <summary>
        /// Verifies that with <c>--json</c> an unexpected exception from the action becomes a single error document
        /// naming the exception type and message, with exit code 1.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task An_unexpected_exception_is_a_json_error()
        {
            using var console = new RecordedConsole();

            var exitCode = await JsonOutput.RunAsync(
                Program.BuildRootCommand().Parse("check --json"),
                CancellationToken.None,
                () => throw new ArgumentException("something unforeseen"));

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(1));
                AssertIsError(console.Lines, "Unexpected error (ArgumentException): something unforeseen");
            });
        }

        /// <summary>
        /// Verifies that without <c>--json</c> an unexpected exception from the action propagates out of
        /// <see cref="JsonOutput.RunAsync"/>.
        /// </summary>
        [Test]
        public void Without_json_an_unexpected_exception_is_not_caught()
        {
            using var console = new RecordedConsole();

            Assert.That(
                () => JsonOutput.RunAsync(
                    Program.BuildRootCommand().Parse("check"),
                    CancellationToken.None,
                    () => throw new ArgumentException("something unforeseen")),
                Throws.ArgumentException);
        }

        /// <summary>
        /// Verifies that <see cref="Program.Main"/> turns a command line with <c>--json</c> (in any spelling) that does not
        /// parse into a single error document with exit code 1, and that System.CommandLine prints nothing else.
        /// </summary>
        /// <param name="commandLine">The command line, split on spaces.</param>
        /// <param name="expectedMessage">Text the error message must contain.</param>
        /// <returns>A task that completes when the test has run.</returns>
        [TestCase("inspect --json", "Required argument missing")]
        [TestCase("check --json --no-such-option", "--no-such-option")]
        [TestCase("versions --json unexpected-argument", "unexpected-argument")]
        [TestCase("check --json:true --no-such-option", "--no-such-option")]
        [TestCase("check --json=true --no-such-option", "--no-such-option")]
        public async Task A_command_line_that_does_not_parse_is_a_json_error_and_nothing_else(string commandLine, string expectedMessage)
        {
            using var console = new RecordedConsole();

            var exitCode = await Program.Main(commandLine.Split(' '));

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(1));
                AssertIsError(console.Lines, expectedMessage);
                Assert.That(console.StandardOutput, Is.Empty, "System.CommandLine must not also print its help");
                Assert.That(console.StandardError, Is.Empty, "System.CommandLine must not also print its errors");
            });
        }

        /// <summary>
        /// Verifies that without <c>--json</c> a command line that does not parse is reported as text naming the bad
        /// option, not as a JSON document, with exit code 1.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task Without_json_a_command_line_that_does_not_parse_is_reported_as_text()
        {
            using var console = new RecordedConsole();

            var exitCode = await Program.Main(["check", "--no-such-option"]);

            var text = console.Output + console.StandardOutput + console.StandardError;

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(1));
                Assert.That(text, Does.Contain("--no-such-option"));
                Assert.That(text.TrimStart(), Does.Not.StartWith("{"));
            });
        }

        /// <summary>
        /// Verifies that without <c>--json</c> inspecting against a version that is not generated prints the plain
        /// sentence "UML 2.5.1 has not been generated yet." and returns exit code 1.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task Without_json_a_failure_is_a_sentence_not_a_document()
        {
            using var console = new RecordedConsole();

            var exitCode = await Program.BuildRootCommand()
                .Parse(["inspect", Path.Combine(FixturesDirectory, "model-valid.xmi"), "--repository-root", this.repositoryRoot])
                .InvokeAsync();

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(1));
                Assert.That(console.Output.Trim(), Does.StartWith("UML 2.5.1 has not been generated yet."));
            });
        }

        /// <summary>
        /// Verifies that without <c>--json</c> a missing model path containing square brackets is printed as is,
        /// not read as Spectre markup, with exit code 1.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task Without_json_a_failure_message_with_brackets_is_printed_literally()
        {
            this.InstallGeneratedVersion();
            using var console = new RecordedConsole();
            var missing = Path.Combine(this.repositoryRoot, "model[1].xmi");

            var exitCode = await Program.BuildRootCommand()
                .Parse(["inspect", missing, "--repository-root", this.repositoryRoot])
                .InvokeAsync();

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(1));
                Assert.That(console.Output, Does.Contain($"File not found: {missing}"));
            });
        }

        /// <summary>
        /// Verifies that without <c>--json</c> a corrupt <c>installed.json</c> is reported as a "A file could not be
        /// read:" sentence naming the file, with exit code 1.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task Without_json_a_corrupt_installed_manifest_is_a_sentence_naming_the_file()
        {
            await this.WriteCorruptManifest();
            using var console = new RecordedConsole();

            var exitCode = await Program.BuildRootCommand()
                .Parse(["check", "--repository-root", this.repositoryRoot])
                .InvokeAsync();

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(1));
                Assert.That(console.Output.Trim(), Does.StartWith("A file could not be read:"));
                Assert.That(console.Output, Does.Contain("installed.json"));
            });
        }

        /// <summary>
        /// Verifies that an error document holding a path with an apostrophe or non-ASCII characters is pure ASCII and
        /// still decodes to the original path.
        /// </summary>
        /// <param name="fileName">The name of the missing model file.</param>
        /// <returns>A task that completes when the test has run.</returns>
        [TestCase("it's-missing.xmi")]
        [TestCase("café-模型.xmi")]
        public async Task Json_output_is_pure_ascii_and_decodes_to_the_original_text(string fileName)
        {
            // A redirected standard output on Windows uses the console code page, not UTF-8; only an
            // all-ASCII document is read back correctly whatever the code page.
            this.InstallGeneratedVersion();
            var missing = Path.Combine(this.repositoryRoot, fileName);

            var run = await this.Run(["inspect", missing]);

            using var document = JsonDocument.Parse(run.Lines[0]);

            Assert.Multiple(() =>
            {
                Assert.That(run.Lines[0].All(character => character < 128), Is.True, "every non-ASCII character has to be escaped");
                Assert.That(document.RootElement.GetProperty("error").GetString(), Is.EqualTo($"File not found: {missing}"));
            });
        }

        /// <summary>
        /// Verifies that without <c>--json</c> an already cancelled token prints only "Cancelled." and returns exit code 130.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task Without_json_a_token_cancelled_before_the_verb_starts_is_a_sentence_with_exit_code_130()
        {
            using var console = new RecordedConsole();
            using var cancelled = new CancellationTokenSource();
            await cancelled.CancelAsync();

            var exitCode = await Program.BuildRootCommand()
                .Parse(["check", "--repository-root", this.repositoryRoot])
                .InvokeAsync(cancellationToken: cancelled.Token);

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(130));
                Assert.That(console.Output.Trim(), Is.EqualTo("Cancelled."));
            });
        }

        /// <summary>
        /// Verifies that <see cref="Program.Main"/> runs <c>check --json</c> when the command line parses: exit code 0
        /// and one line holding the check document, not an error document.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task Main_with_json_and_a_command_line_that_parses_runs_the_verb()
        {
            this.InstallGeneratedVersion();
            using var console = new RecordedConsole();

            var exitCode = await Program.Main(["check", "--json", "--repository-root", this.repositoryRoot]);

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(0));
                Assert.That(console.Lines, Has.Length.EqualTo(1));
                using var document = JsonDocument.Parse(console.Lines[0]);
                Assert.That(document.RootElement.TryGetProperty("Versions", out _), Is.True);
                Assert.That(document.RootElement.TryGetProperty("error", out _), Is.False);
            });
        }

        /// <summary>
        /// Verifies that with two unknown options the single error document names both of them.
        /// </summary>
        /// <returns>A task that completes when the test has run.</returns>
        [Test]
        public async Task A_command_line_with_several_parse_errors_names_them_all()
        {
            using var console = new RecordedConsole();

            var exitCode = await Program.Main(["check", "--json", "--no-such-option", "--other-bad-option"]);

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(1));
                AssertIsError(console.Lines, "--no-such-option");
                AssertIsError(console.Lines, "--other-bad-option");
            });
        }

        /// <summary>
        /// Verifies that asking for help together with <c>--json</c> gives one error document that says how to get the
        /// help text, with exit code 1 and nothing else on either stream, instead of System.CommandLine's help text.
        /// </summary>
        /// <param name="commandLine">A command line that asks for help, separated by single spaces.</param>
        /// <returns>A task that completes when the test has run.</returns>
        [TestCase("check --json --help")]
        [TestCase("inspect model.xmi --json -h")]
        [TestCase("versions --json -?")]
        public async Task Help_with_json_is_a_json_error_and_nothing_else(string commandLine)
        {
            using var console = new RecordedConsole();

            var exitCode = await Program.Main(commandLine.Split(' '));

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(1));
                AssertIsError(console.Lines, "cannot be combined with --json");
                Assert.That(console.StandardOutput, Is.Empty);
                Assert.That(console.StandardError, Is.Empty);
            });
        }

        /// <summary>
        /// Verifies that <c>--json</c> on a verb that does not offer it, or without a verb, gives one error document
        /// that names the verbs that do offer it, with exit code 1.
        /// </summary>
        /// <param name="commandLine">The command line, separated by single spaces.</param>
        /// <param name="target">How the error names what was asked for.</param>
        /// <returns>A task that completes when the test has run.</returns>
        [TestCase("fetch --json", "not by 'fetch'")]
        [TestCase("remove --json --version 2.5.1", "not by 'remove'")]
        [TestCase("--json", "not by a command line without a verb")]
        public async Task Json_on_a_verb_that_does_not_offer_it_names_the_verbs_that_do(string commandLine, string target)
        {
            using var console = new RecordedConsole();

            var exitCode = await Program.Main(commandLine.Split(' '));

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(1));
                AssertIsError(console.Lines, "--json is only supported by versions, check, inspect");
                AssertIsError(console.Lines, target);
                Assert.That(console.StandardOutput, Is.Empty);
                Assert.That(console.StandardError, Is.Empty);
            });
        }

        /// <summary>
        /// Verifies that <see cref="JsonOutput.IsRequested"/> is true for <c>check</c>, <c>versions</c> and <c>inspect</c>
        /// with <c>--json</c>, and false without it and for <c>fetch</c>, which does not offer it.
        /// </summary>
        /// <param name="commandLine">The command line to parse.</param>
        /// <param name="expected">Whether JSON is expected to be requested.</param>
        [TestCase("check --json", true)]
        [TestCase("versions --json", true)]
        [TestCase("inspect model.xmi --json", true)]
        [TestCase("check", false)]
        [TestCase("inspect model.xmi", false)]
        [TestCase("fetch", false)]
        public void Json_is_recognised_on_every_verb_that_offers_it(string commandLine, bool expected)
        {
            Assert.That(JsonOutput.IsRequested(Program.BuildRootCommand().Parse(commandLine)), Is.EqualTo(expected));
        }

        /// <summary>
        /// Verifies that <see cref="JsonOutput.IsJsonArgument"/> accepts <c>--json</c>, <c>--json:value</c> and
        /// <c>--json=value</c>, and rejects similar-looking or other arguments.
        /// </summary>
        /// <param name="argument">The raw argument.</param>
        /// <param name="expected">Whether the argument is expected to be recognised.</param>
        [TestCase("--json", true)]
        [TestCase("--json:true", true)]
        [TestCase("--json=false", true)]
        [TestCase("--jsonx", false)]
        [TestCase("json", false)]
        [TestCase("--repository-root", false)]
        public void IsJsonArgument_recognises_every_spelling_of_the_option(string argument, bool expected)
        {
            Assert.That(JsonOutput.IsJsonArgument(argument), Is.EqualTo(expected));
        }

        /// <summary>
        /// Gets the property names of a JSON object, in document order.
        /// </summary>
        /// <param name="element">The JSON object.</param>
        /// <returns>The property names.</returns>
        private static string[] Names(JsonElement element) => element.EnumerateObject().Select(property => property.Name).ToArray();

        /// <summary>
        /// Gets the first finding of an inspection report with the given category.
        /// </summary>
        /// <param name="report">The report's root element.</param>
        /// <param name="category">The finding category to look for.</param>
        /// <returns>The first matching finding.</returns>
        /// <exception cref="InvalidOperationException">The report has no finding of that category.</exception>
        private static JsonElement FindingOf(JsonElement report, string category) =>
            report.GetProperty("Findings").EnumerateArray().First(finding => finding.GetProperty("Category").GetString() == category);

        /// <summary>
        /// Asserts that a run wrote one error document containing <paramref name="expectedMessage"/> and nothing
        /// straight to standard output or standard error.
        /// </summary>
        /// <param name="run">The run to check.</param>
        /// <param name="expectedMessage">Text the error message must contain.</param>
        private static void AssertIsError(RunResult run, string expectedMessage)
        {
            AssertIsError(run.Lines, expectedMessage);
            Assert.That(run.StandardOutput, Is.Empty);
            Assert.That(run.StandardError, Is.Empty);
        }

        /// <summary>
        /// Asserts that the output is exactly one line holding a document whose only property is <c>error</c>, and
        /// that its message contains <paramref name="expectedMessage"/>.
        /// </summary>
        /// <param name="lines">The lines written through the console.</param>
        /// <param name="expectedMessage">Text the error message must contain.</param>
        private static void AssertIsError(string[] lines, string expectedMessage)
        {
            Assert.That(lines, Has.Length.EqualTo(1), "the error document has to be the only line");
            using var document = JsonDocument.Parse(lines[0]);
            Assert.That(Names(document.RootElement), Is.EqualTo(new[] { "error" }));
            Assert.That(document.RootElement.GetProperty("error").GetString(), Does.Contain(expectedMessage));
        }

        /// <summary>
        /// Records 2.5.1 as fetched, generated and default, and places the fixture metamodel where
        /// <c>inspect</c> looks for it.
        /// </summary>
        private void InstallGeneratedVersion()
        {
            var store = new InstalledVersionsStore(this.layout.KnowledgeRoot);
            store.MarkFetched(UmlVersion, setAsDefault: true);
            store.MarkGenerated(UmlVersion, specGenerated: false);

            var metamodelDirectory = Path.Combine(this.layout.KnowledgeDirectoryFor(UmlVersion), "metamodel");
            Directory.CreateDirectory(metamodelDirectory);
            File.Copy(Path.Combine(FixturesDirectory, "metamodel-uml-subset.json"), Path.Combine(metamodelDirectory, "metamodel.json"), overwrite: true);
        }

        /// <summary>
        /// Writes an <c>installed.json</c> that is not valid JSON into the knowledge folder.
        /// </summary>
        /// <returns>A task that completes when the file is written.</returns>
        private async Task WriteCorruptManifest()
        {
            Directory.CreateDirectory(this.layout.KnowledgeRoot);
            await File.WriteAllTextAsync(Path.Combine(this.layout.KnowledgeRoot, "installed.json"), "{ not json");
        }

        /// <summary>
        /// Runs <paramref name="verb"/> with <c>--json</c> through the tool's own root command. A relative
        /// model file name after <c>inspect</c> is resolved against the fixtures.
        /// </summary>
        /// <param name="verb">
        /// The verb and its arguments, separated by single spaces. Only for fixed text: an argument that holds a
        /// path goes through <see cref="Run(string[], CancellationToken)"/>, so a space in it stays one argument.
        /// </param>
        /// <param name="cancellationToken">The token passed to the command's invocation.</param>
        /// <returns>The exit code and everything the run wrote.</returns>
        private Task<RunResult> Run(string verb, CancellationToken cancellationToken = default) =>
            this.Run(verb.Split(' '), cancellationToken);

        /// <summary>
        /// Runs the verb in <paramref name="verbArguments"/> with <c>--json</c> through the tool's own root
        /// command. A relative model file name after <c>inspect</c> is resolved against the fixtures.
        /// </summary>
        /// <param name="verbArguments">The verb and its arguments, one argument per element.</param>
        /// <param name="cancellationToken">The token passed to the command's invocation.</param>
        /// <returns>The exit code and everything the run wrote.</returns>
        private async Task<RunResult> Run(string[] verbArguments, CancellationToken cancellationToken = default)
        {
            var args = verbArguments.ToList();
            if (args[0] == "inspect" && !Path.IsPathRooted(args[1]))
            {
                args[1] = Path.Combine(FixturesDirectory, args[1]);
            }

            args.AddRange(["--json", "--repository-root", this.repositoryRoot]);

            using var console = new RecordedConsole();
            var exitCode = await Program.BuildRootCommand().Parse(args).InvokeAsync(cancellationToken: cancellationToken);

            return new RunResult(exitCode, console.Lines, console.StandardOutput, console.StandardError);
        }

        /// <summary>
        /// Runs <paramref name="verb"/> as <see cref="Run(string, CancellationToken)"/> does, asserts it wrote one line and parses that line.
        /// </summary>
        /// <param name="verb">The verb and its arguments, separated by single spaces.</param>
        /// <returns>The parsed document; the caller disposes it.</returns>
        private async Task<JsonDocument> RunDocument(string verb)
        {
            var run = await this.Run(verb);
            Assert.That(run.Lines, Has.Length.EqualTo(1));
            return JsonDocument.Parse(run.Lines[0]);
        }

        /// <summary>
        /// What one run of a verb produced.
        /// </summary>
        /// <param name="ExitCode">The verb's exit code.</param>
        /// <param name="Lines">The lines written through the console.</param>
        /// <param name="StandardOutput">What was written straight to standard output.</param>
        /// <param name="StandardError">What was written to standard error.</param>
        private sealed record RunResult(int ExitCode, string[] Lines, string StandardOutput, string StandardError);
    }
}
