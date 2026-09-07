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

    using Uml4Net.Sage.Knowledge;
    using Uml4Net.Sage.Tools.Commands;

    [TestFixture]
    public class UseAndRemoveCommandTests
    {
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

        [Test]
        public void Use_returns_1_when_the_version_has_not_been_generated()
        {
            new InstalledVersionsStore(this.layout.KnowledgeRoot).MarkFetched("2.5.1", setAsDefault: false);

            var exitCode = UseCommand.Build().Parse(["--repository-root", this.repositoryRoot, "--version", "2.5.1"]).Invoke();

            Assert.That(exitCode, Is.EqualTo(1));
        }

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

        [Test]
        public void Remove_returns_1_on_the_current_default_without_force()
        {
            new InstalledVersionsStore(this.layout.KnowledgeRoot).MarkFetched("2.5.1", setAsDefault: true);

            var exitCode = RemoveCommand.Build().Parse(["--repository-root", this.repositoryRoot, "--version", "2.5.1"]).Invoke();

            Assert.That(exitCode, Is.EqualTo(1));
        }

        [Test]
        public void Remove_with_force_deletes_the_sources_and_knowledge_directories()
        {
            new InstalledVersionsStore(this.layout.KnowledgeRoot).MarkFetched("2.5.1", setAsDefault: true);
            Directory.CreateDirectory(this.layout.SourcesDirectoryFor("2.5.1"));
            Directory.CreateDirectory(this.layout.KnowledgeDirectoryFor("2.5.1"));

            var exitCode = RemoveCommand.Build().Parse(["--repository-root", this.repositoryRoot, "--version", "2.5.1", "--force"]).Invoke();

            Assert.That(exitCode, Is.EqualTo(0));
            Assert.That(Directory.Exists(this.layout.SourcesDirectoryFor("2.5.1")), Is.False);
            Assert.That(Directory.Exists(this.layout.KnowledgeDirectoryFor("2.5.1")), Is.False);
        }
    }
}
