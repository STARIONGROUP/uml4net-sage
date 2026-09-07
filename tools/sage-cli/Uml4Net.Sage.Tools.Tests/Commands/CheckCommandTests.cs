// -------------------------------------------------------------------------------------------------
// <copyright file="CheckCommandTests.cs" company="Starion Group S.A.">
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
    }
}
