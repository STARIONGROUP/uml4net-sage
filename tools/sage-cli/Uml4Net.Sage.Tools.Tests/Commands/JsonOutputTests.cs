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

        [SetUp]
        public void SetUp()
        {
            this.repositoryRoot = Path.Combine(Path.GetTempPath(), "uml4net-sage-tests", Path.GetRandomFileName());
            this.layout = new RepositoryLayout(this.repositoryRoot);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(this.repositoryRoot))
            {
                Directory.Delete(this.repositoryRoot, recursive: true);
            }
        }

        [TestCase("check", 0)]
        [TestCase("versions", 0)]
        [TestCase("inspect model-valid.xmi", 0)]
        [TestCase("inspect model-abstract-type.xmi", 1)]
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

        [Test]
        public async Task An_inspect_of_a_file_that_is_not_xml_is_a_json_error()
        {
            this.InstallGeneratedVersion();
            var notXml = Path.Combine(this.repositoryRoot, "not-xml.xmi");
            await File.WriteAllTextAsync(notXml, "not xml");

            var run = await this.Run($"inspect {notXml}");

            Assert.Multiple(() =>
            {
                Assert.That(run.ExitCode, Is.EqualTo(1));
                AssertIsError(run, "A file could not be read");
            });
        }

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

        [TestCase("it's-missing.xmi")]
        [TestCase("café-模型.xmi")]
        public async Task Json_output_is_pure_ascii_and_decodes_to_the_original_text(string fileName)
        {
            // A redirected standard output on Windows uses the console code page, not UTF-8; only an
            // all-ASCII document is read back correctly whatever the code page.
            this.InstallGeneratedVersion();
            var missing = Path.Combine(this.repositoryRoot, fileName);

            var run = await this.Run($"inspect {missing}");

            using var document = JsonDocument.Parse(run.Lines[0]);

            Assert.Multiple(() =>
            {
                Assert.That(run.Lines[0].All(character => character < 128), Is.True, "every non-ASCII character has to be escaped");
                Assert.That(document.RootElement.GetProperty("error").GetString(), Is.EqualTo($"File not found: {missing}"));
            });
        }

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

        private static string[] Names(JsonElement element) => element.EnumerateObject().Select(property => property.Name).ToArray();

        private static JsonElement FindingOf(JsonElement report, string category) =>
            report.GetProperty("Findings").EnumerateArray().First(finding => finding.GetProperty("Category").GetString() == category);

        private static void AssertIsError(RunResult run, string expectedMessage)
        {
            AssertIsError(run.Lines, expectedMessage);
            Assert.That(run.StandardOutput, Is.Empty);
            Assert.That(run.StandardError, Is.Empty);
        }

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

        private async Task WriteCorruptManifest()
        {
            Directory.CreateDirectory(this.layout.KnowledgeRoot);
            await File.WriteAllTextAsync(Path.Combine(this.layout.KnowledgeRoot, "installed.json"), "{ not json");
        }

        /// <summary>
        /// Runs <paramref name="verb"/> with <c>--json</c> through the tool's own root command. A relative
        /// model file name after <c>inspect</c> is resolved against the fixtures.
        /// </summary>
        private async Task<RunResult> Run(string verb, CancellationToken cancellationToken = default)
        {
            var args = verb.Split(' ').ToList();
            if (args[0] == "inspect" && !Path.IsPathRooted(args[1]))
            {
                args[1] = Path.Combine(FixturesDirectory, args[1]);
            }

            args.AddRange(["--json", "--repository-root", this.repositoryRoot]);

            using var console = new RecordedConsole();
            var exitCode = await Program.BuildRootCommand().Parse(args).InvokeAsync(cancellationToken: cancellationToken);

            return new RunResult(exitCode, console.Lines, console.StandardOutput, console.StandardError);
        }

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
