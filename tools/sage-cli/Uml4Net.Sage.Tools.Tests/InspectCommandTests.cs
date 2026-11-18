// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Tools.Tests
{
    using System.IO;

    using Uml4Net.Sage.Tools.Commands;

    [TestFixture]
    public class InspectCommandTests
    {
        private const string UmlVersion = "2.5.1";

        private static string FixturesDirectory => Path.Combine(NUnit.Framework.TestContext.CurrentContext.TestDirectory, "Fixtures");

        private static string ValidModelPath => Path.Combine(FixturesDirectory, "model-valid.xmi");

        private string repositoryRoot = null!;

        [SetUp]
        public void SetUp()
        {
            this.repositoryRoot = Path.Combine(Path.GetTempPath(), "uml4net-sage-tests", Path.GetRandomFileName());
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(this.repositoryRoot))
            {
                Directory.Delete(this.repositoryRoot, recursive: true);
            }
        }

        [Test]
        public void Invoke_returns_1_when_the_version_has_not_been_generated_yet()
        {
            var exitCode = this.Invoke(ValidModelPath);

            Assert.That(exitCode, Is.EqualTo(1));
        }

        [TestCase("model-stereotype-unresolved.xmi", 0)]
        [TestCase("model-abstract-type.xmi", 1)]
        public void Invoke_fails_only_on_error_findings(string fixture, int expectedExitCode)
        {
            this.InstallMetamodel();

            var exitCode = this.Invoke(Path.Combine(FixturesDirectory, fixture));

            Assert.That(exitCode, Is.EqualTo(expectedExitCode));
        }

        [Test]
        public void Invoke_returns_0_for_a_valid_model()
        {
            this.InstallMetamodel();

            var exitCode = this.Invoke(ValidModelPath);

            Assert.That(exitCode, Is.EqualTo(0));
        }

        [Test]
        public void Invoke_accepts_repeated_pathmap_options()
        {
            this.InstallMetamodel();

            var exitCode = this.Invoke(
                ValidModelPath,
                "--pathmap", $"pathmap://SAMPLE_LIBRARY={Path.Combine(FixturesDirectory, "library")}",
                "--pathmap", "pathmap://OTHER/Other.xmi=Other.xmi");

            Assert.That(exitCode, Is.EqualTo(0));
        }

        [Test]
        public void Invoke_returns_1_for_a_pathmap_without_a_path()
        {
            this.InstallMetamodel();

            var exitCode = this.Invoke(ValidModelPath, "--pathmap", "pathmap://SAMPLE_LIBRARY");

            Assert.That(exitCode, Is.EqualTo(1));
        }

        [Test]
        public void Invoke_returns_1_when_the_model_file_does_not_exist()
        {
            this.InstallMetamodel();

            var exitCode = this.Invoke(Path.Combine(this.repositoryRoot, "no-such-model.xmi"));

            Assert.That(exitCode, Is.EqualTo(1));
        }

        /// <summary>
        /// Places the fixture metamodel where <c>inspect</c> looks for the generated <c>metamodel.json</c>.
        /// </summary>
        private void InstallMetamodel()
        {
            var metamodelDirectory = Path.Combine(this.repositoryRoot, "knowledge", UmlVersion, "metamodel");
            Directory.CreateDirectory(metamodelDirectory);
            File.Copy(Path.Combine(FixturesDirectory, "metamodel-uml-subset.json"), Path.Combine(metamodelDirectory, "metamodel.json"));
        }

        /// <summary>
        /// Invokes <c>inspect</c> on <paramref name="modelPath"/> with <paramref name="options"/>, against this test's
        /// repository root and <see cref="UmlVersion"/>.
        /// </summary>
        /// <param name="modelPath">The model file to inspect.</param>
        /// <param name="options">Further command-line arguments, placed after the model path.</param>
        /// <returns>The command's exit code.</returns>
        private int Invoke(string modelPath, params string[] options)
        {
            return InspectCommand.Build().Parse(
            [
                modelPath,
                .. options,
                "--repository-root", this.repositoryRoot,
                "--version", UmlVersion,
            ]).Invoke();
        }
    }
}
