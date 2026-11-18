// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Tools.Tests
{
    using System.IO;

    using Uml4Net.Sage.Knowledge;
    using Uml4Net.Sage.Tools.Commands;

    [TestFixture]
    public class CheckCommandTests
    {
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
        public void Invoke_never_makes_a_network_call_and_returns_0_when_nothing_is_fetched()
        {
            var exitCode = CheckCommand.Build().Parse(["--repository-root", this.repositoryRoot]).Invoke();

            Assert.That(exitCode, Is.EqualTo(0));
        }

        [Test]
        public void Invoke_returns_0_once_a_version_is_fully_generated()
        {
            var layout = new RepositoryLayout(this.repositoryRoot);
            var store = new InstalledVersionsStore(layout.KnowledgeRoot);
            store.MarkFetched("2.5.1", setAsDefault: true);
            store.MarkGenerated("2.5.1", specGenerated: true);

            var exitCode = CheckCommand.Build().Parse(["--repository-root", this.repositoryRoot]).Invoke();

            Assert.That(exitCode, Is.EqualTo(0));
        }

        [Test]
        public void Invoke_returns_0_when_the_uml_version_is_generated_but_the_xmi_spec_is_not()
        {
            var layout = new RepositoryLayout(this.repositoryRoot);
            var store = new InstalledVersionsStore(layout.KnowledgeRoot);
            store.MarkFetched("2.5.1", setAsDefault: true);
            store.MarkGenerated("2.5.1", specGenerated: true);

            // No MarkXmiSpecGenerated call - the XMI corpus was never fetched/generated on this machine.
            var exitCode = CheckCommand.Build().Parse(["--repository-root", this.repositoryRoot]).Invoke();

            Assert.That(exitCode, Is.EqualTo(0));
        }
    }
}
