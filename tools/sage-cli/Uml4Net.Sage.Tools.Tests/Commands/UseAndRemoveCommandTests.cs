// -------------------------------------------------------------------------------------------------
// <copyright file="UseAndRemoveCommandTests.cs" company="Starion Group S.A.">
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
    using System.Linq;

    using Uml4Net.Sage.Knowledge;
    using Uml4Net.Sage.Tools.Commands;

    [TestFixture]
    public class UseAndRemoveCommandTests
    {
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
        /// Verifies that <c>use</c> returns exit code 1 for a version that is fetched but not generated.
        /// </summary>
        [Test]
        public void Use_returns_1_when_the_version_has_not_been_generated()
        {
            new InstalledVersionsStore(this.layout.KnowledgeRoot).MarkFetched("2.5.1", setAsDefault: false);

            var exitCode = UseCommand.Build().Parse(["--repository-root", this.repositoryRoot, "--version", "2.5.1"]).Invoke();

            Assert.That(exitCode, Is.EqualTo(1));
        }

        /// <summary>
        /// Verifies that <c>use</c> returns exit code 0 for a generated version and records it as the default in the manifest.
        /// </summary>
        [Test]
        public void Use_returns_0_and_updates_the_default_once_generated()
        {
            var store = new InstalledVersionsStore(this.layout.KnowledgeRoot);
            store.MarkFetched("2.5.1", setAsDefault: false);
            store.MarkGenerated("2.5.1", specGenerated: false);

            var exitCode = UseCommand.Build().Parse(["--repository-root", this.repositoryRoot, "--version", "2.5.1"]).Invoke();

            Assert.That(exitCode, Is.EqualTo(0));
            Assert.That(store.Load().Default, Is.EqualTo("2.5.1"));
        }

        /// <summary>
        /// Verifies that <c>remove</c> without <c>--force</c> returns exit code 1 for the current default version.
        /// </summary>
        [Test]
        public void Remove_returns_1_on_the_current_default_without_force()
        {
            new InstalledVersionsStore(this.layout.KnowledgeRoot).MarkFetched("2.5.1", setAsDefault: true);

            var exitCode = RemoveCommand.Build().Parse(["--repository-root", this.repositoryRoot, "--version", "2.5.1"]).Invoke();

            Assert.That(exitCode, Is.EqualTo(1));
        }

        /// <summary>
        /// Verifies that <c>remove --force</c> on the default version deletes its sources and knowledge folders,
        /// keeps the shared XMI specification folders, removes the version from the manifest and returns exit code 0.
        /// </summary>
        [Test]
        public void Remove_with_force_deletes_the_sources_and_knowledge_directories()
        {
            new InstalledVersionsStore(this.layout.KnowledgeRoot).MarkFetched("2.5.1", setAsDefault: true);
            Directory.CreateDirectory(this.layout.SourcesDirectoryFor("2.5.1"));
            Directory.CreateDirectory(this.layout.KnowledgeDirectoryFor("2.5.1"));
            Directory.CreateDirectory(this.layout.XmiSourcesDirectoryFor("2.5.1"));
            Directory.CreateDirectory(this.layout.XmiKnowledgeDirectoryFor("2.5.1"));

            var exitCode = RemoveCommand.Build().Parse(["--repository-root", this.repositoryRoot, "--version", "2.5.1", "--force"]).Invoke();

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(0));
                Assert.That(Directory.Exists(this.layout.SourcesDirectoryFor("2.5.1")), Is.False);
                Assert.That(Directory.Exists(this.layout.KnowledgeDirectoryFor("2.5.1")), Is.False);
                Assert.That(Directory.Exists(this.layout.XmiSourcesDirectoryFor("2.5.1")), Is.True);
                Assert.That(Directory.Exists(this.layout.XmiKnowledgeDirectoryFor("2.5.1")), Is.True);
                Assert.That(new InstalledVersionsStore(this.layout.KnowledgeRoot).Load().Versions, Is.Empty);
            });
        }

        /// <summary>
        /// Verifies that <c>remove --force</c> with a version that is not a registered UML version (a path escape,
        /// the shared <c>xmi</c> folder, an empty or unknown version) returns exit code 1 and deletes nothing.
        /// </summary>
        /// <param name="version">The <c>--version</c> value to pass.</param>
        [TestCase("..")]
        [TestCase("xmi")]
        [TestCase("")]
        [TestCase("9.9.9")]
        [TestCase("2.5.1/..")]
        public void Remove_with_force_refuses_a_version_that_is_not_a_known_uml_version(string version)
        {
            var store = new InstalledVersionsStore(this.layout.KnowledgeRoot);
            store.MarkFetched("2.5.1", setAsDefault: true);
            Directory.CreateDirectory(this.layout.SourcesDirectoryFor("2.5.1"));
            Directory.CreateDirectory(this.layout.KnowledgeDirectoryFor("2.5.1"));
            Directory.CreateDirectory(this.layout.XmiSourcesDirectoryFor("2.5.1"));
            Directory.CreateDirectory(this.layout.XmiKnowledgeDirectoryFor("2.5.1"));
            var sentinel = Path.Combine(this.repositoryRoot, "sentinel.txt");
            File.WriteAllText(sentinel, "keep");

            var exitCode = RemoveCommand.Build().Parse(["--repository-root", this.repositoryRoot, "--version", version, "--force"]).Invoke();

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(1));
                Assert.That(File.Exists(sentinel), Is.True);
                Assert.That(Directory.Exists(this.layout.SourcesDirectoryFor("2.5.1")), Is.True);
                Assert.That(Directory.Exists(this.layout.KnowledgeDirectoryFor("2.5.1")), Is.True);
                Assert.That(Directory.Exists(this.layout.XmiSourcesDirectoryFor("2.5.1")), Is.True);
                Assert.That(Directory.Exists(this.layout.XmiKnowledgeDirectoryFor("2.5.1")), Is.True);
                Assert.That(store.Load().Versions, Has.Count.EqualTo(1));
            });
        }

        /// <summary>
        /// Verifies that <c>remove --force</c> with an absolute folder path as the version returns exit code 1 and
        /// leaves that folder in place.
        /// </summary>
        [Test]
        public void Remove_with_force_refuses_an_absolute_path()
        {
            new InstalledVersionsStore(this.layout.KnowledgeRoot).MarkFetched("2.5.1", setAsDefault: true);
            var outside = Path.Combine(this.repositoryRoot, "outside");
            Directory.CreateDirectory(outside);

            var exitCode = RemoveCommand.Build().Parse(["--repository-root", this.repositoryRoot, "--version", outside, "--force"]).Invoke();

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(1));
                Assert.That(Directory.Exists(outside), Is.True);
            });
        }

        /// <summary>
        /// Verifies that <c>remove --force</c> refuses a version that the manifest lists but that is not a registered
        /// UML version: exit code 1, its folders kept and its manifest entry kept.
        /// </summary>
        [Test]
        public void Remove_with_force_refuses_a_version_listed_in_the_manifest_but_not_known()
        {
            var store = new InstalledVersionsStore(this.layout.KnowledgeRoot);
            store.MarkFetched("2.5.1", setAsDefault: true);
            store.MarkFetched("9.9.9", setAsDefault: false);
            Directory.CreateDirectory(this.layout.SourcesDirectoryFor("9.9.9"));
            Directory.CreateDirectory(this.layout.KnowledgeDirectoryFor("9.9.9"));

            var exitCode = RemoveCommand.Build().Parse(["--repository-root", this.repositoryRoot, "--version", "9.9.9", "--force"]).Invoke();

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(1));
                Assert.That(Directory.Exists(this.layout.SourcesDirectoryFor("9.9.9")), Is.True);
                Assert.That(Directory.Exists(this.layout.KnowledgeDirectoryFor("9.9.9")), Is.True);
                Assert.That(store.Load().Versions.Select(v => v.Version), Does.Contain("9.9.9"));
            });
        }

        /// <summary>
        /// Verifies that <c>remove</c> without <c>--force</c> refuses the version <c>..</c> with exit code 1 and leaves
        /// a file in the repository root in place.
        /// </summary>
        [Test]
        public void Remove_without_force_refuses_a_parent_directory_version()
        {
            new InstalledVersionsStore(this.layout.KnowledgeRoot).MarkFetched("2.5.1", setAsDefault: false);
            var sentinel = Path.Combine(this.repositoryRoot, "sentinel.txt");
            File.WriteAllText(sentinel, "keep");

            var exitCode = RemoveCommand.Build().Parse(["--repository-root", this.repositoryRoot, "--version", ".."]).Invoke();

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(1));
                Assert.That(File.Exists(sentinel), Is.True);
            });
        }
    }
}
