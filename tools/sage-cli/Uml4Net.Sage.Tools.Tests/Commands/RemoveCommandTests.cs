// -------------------------------------------------------------------------------------------------
// <copyright file="RemoveCommandTests.cs" company="Starion Group S.A.">
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

    // Tests RemoveCommand.IsVersionDirectory directly. Through the command it is only reached with a
    // registered version or a listed version number, which always pass, so its rejections need these tests.
    [TestFixture]
    public class RemoveCommandTests
    {
        private string root = null!;

        [SetUp]
        public void SetUp()
        {
            this.root = Path.Combine(Path.GetTempPath(), "uml4net-sage-tests", "remove", "sources");
        }

        [Test]
        public void IsVersionDirectory_accepts_a_direct_child_of_the_root()
        {
            Assert.That(RemoveCommand.IsVersionDirectory(this.root, Path.Combine(this.root, "2.5.1")), Is.True);
        }

        [Test]
        public void IsVersionDirectory_accepts_a_direct_child_with_a_trailing_separator()
        {
            var directory = Path.Combine(this.root, "2.5.1") + Path.DirectorySeparatorChar;

            Assert.That(RemoveCommand.IsVersionDirectory(this.root + Path.DirectorySeparatorChar, directory), Is.True);
        }

        [TestCase("xmi")]
        [TestCase("XMI")]
        public void IsVersionDirectory_rejects_the_shared_xmi_folder(string name)
        {
            Assert.That(RemoveCommand.IsVersionDirectory(this.root, Path.Combine(this.root, name)), Is.False);
        }

        [Test]
        public void IsVersionDirectory_rejects_the_root_itself()
        {
            Assert.That(RemoveCommand.IsVersionDirectory(this.root, this.root), Is.False);
        }

        [Test]
        public void IsVersionDirectory_rejects_a_parent_directory()
        {
            Assert.That(RemoveCommand.IsVersionDirectory(this.root, Path.Combine(this.root, "..")), Is.False);
        }

        [Test]
        public void IsVersionDirectory_rejects_a_child_that_climbs_back_out()
        {
            Assert.That(RemoveCommand.IsVersionDirectory(this.root, Path.Combine(this.root, "2.5.1", "..", "..")), Is.False);
        }

        [Test]
        public void IsVersionDirectory_rejects_a_nested_folder()
        {
            Assert.That(RemoveCommand.IsVersionDirectory(this.root, Path.Combine(this.root, "a", "b")), Is.False);
        }

        [Test]
        public void IsVersionDirectory_rejects_a_folder_under_another_root()
        {
            var other = Path.Combine(Path.GetTempPath(), "uml4net-sage-tests", "remove", "elsewhere", "2.5.1");

            Assert.That(RemoveCommand.IsVersionDirectory(this.root, other), Is.False);
        }
    }
}
