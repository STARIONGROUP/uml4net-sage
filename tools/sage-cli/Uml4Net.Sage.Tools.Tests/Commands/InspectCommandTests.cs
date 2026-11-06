// -------------------------------------------------------------------------------------------------
// <copyright file="InspectCommandTests.cs" company="Starion Group S.A.">
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
    using System.IO;

    using Uml4Net.Sage.Tools.Commands;

    [TestFixture]
    public class InspectCommandTests
    {
        private const string UmlVersion = "2.5.1";

        private static string FixturesDirectory => Path.Combine(NUnit.Framework.TestContext.CurrentContext.TestDirectory, "Fixtures");

        private static string ValidModelPath => Path.Combine(FixturesDirectory, "model-valid.xmi");

        private string repositoryRoot = null!;

        /// <summary>
        /// Picks a fresh repository root under the temp folder for each test, without creating it.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.repositoryRoot = Path.Combine(Path.GetTempPath(), "uml4net-sage-tests", Path.GetRandomFileName());
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
        /// Verifies that <c>inspect</c> returns exit code 1 when no <c>metamodel.json</c> has been generated for the version.
        /// </summary>
        [Test]
        public void Invoke_returns_1_when_the_version_has_not_been_generated_yet()
        {
            var exitCode = this.Invoke(ValidModelPath);

            Assert.That(exitCode, Is.EqualTo(1));
        }

        /// <summary>
        /// Verifies that <c>inspect</c> fails only on error-severity findings: a model whose only finding is a
        /// warning (an unresolvable stereotype) exits with 0, and a model with an error exits with 1.
        /// </summary>
        /// <param name="fixture">The fixture model to inspect.</param>
        /// <param name="expectedExitCode">The exit code <c>inspect</c> should return.</param>
        [TestCase("model-stereotype-unresolved.xmi", 0)]
        [TestCase("model-abstract-type.xmi", 1)]
        public void Invoke_fails_only_on_error_findings(string fixture, int expectedExitCode)
        {
            this.InstallMetamodel();

            var exitCode = this.Invoke(Path.Combine(FixturesDirectory, fixture));

            Assert.That(exitCode, Is.EqualTo(expectedExitCode));
        }

        /// <summary>
        /// Verifies that <c>inspect</c> returns exit code 0 for the valid fixture model once the metamodel is installed.
        /// </summary>
        [Test]
        public void Invoke_returns_0_for_a_valid_model()
        {
            this.InstallMetamodel();

            var exitCode = this.Invoke(ValidModelPath);

            Assert.That(exitCode, Is.EqualTo(0));
        }

        /// <summary>
        /// Verifies that <c>inspect</c> accepts two <c>--pathmap</c> options, one mapping a prefix to a directory and
        /// one mapping a document to a file, and still returns exit code 0 for the valid model.
        /// </summary>
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

        /// <summary>
        /// Verifies that <c>inspect</c> returns exit code 1 for a <c>--pathmap</c> value without <c>=&lt;path&gt;</c>.
        /// </summary>
        [Test]
        public void Invoke_returns_1_for_a_pathmap_without_a_path()
        {
            this.InstallMetamodel();

            var exitCode = this.Invoke(ValidModelPath, "--pathmap", "pathmap://SAMPLE_LIBRARY");

            Assert.That(exitCode, Is.EqualTo(1));
        }

        /// <summary>
        /// Verifies that <c>inspect</c> returns exit code 1 when the model file does not exist.
        /// </summary>
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
