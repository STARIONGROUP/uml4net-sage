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

    /// <summary>
    /// Tests <see cref="RemoveCommand.IsVersionDirectory"/> directly. Through the command it is only
    /// reached with a registered UML version, which always passes, so its rejections need these tests.
    /// </summary>
    [TestFixture]
    public class RemoveCommandTests
    {
        private string root = null!;

        /// <summary>
        /// Sets the root the paths are checked against. Nothing is created on disk; the checks only compare paths.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.root = Path.Combine(Path.GetTempPath(), "uml4net-sage-tests", "remove", "sources");
        }

        /// <summary>
        /// Verifies that a folder directly under the root, such as <c>2.5.1</c>, counts as a version folder.
        /// </summary>
        [Test]
        public void IsVersionDirectory_accepts_a_direct_child_of_the_root()
        {
            Assert.That(RemoveCommand.IsVersionDirectory(this.root, Path.Combine(this.root, "2.5.1")), Is.True);
        }

        /// <summary>
        /// Verifies that a direct child is still accepted when both the root and the child end in a directory separator.
        /// </summary>
        [Test]
        public void IsVersionDirectory_accepts_a_direct_child_with_a_trailing_separator()
        {
            var directory = Path.Combine(this.root, "2.5.1") + Path.DirectorySeparatorChar;

            Assert.That(RemoveCommand.IsVersionDirectory(this.root + Path.DirectorySeparatorChar, directory), Is.True);
        }

        /// <summary>
        /// Verifies that the shared <c>xmi</c> folder is rejected, whatever its letter case.
        /// </summary>
        /// <param name="name">The folder name under the root.</param>
        [TestCase("xmi")]
        [TestCase("XMI")]
        public void IsVersionDirectory_rejects_the_shared_xmi_folder(string name)
        {
            Assert.That(RemoveCommand.IsVersionDirectory(this.root, Path.Combine(this.root, name)), Is.False);
        }

        /// <summary>
        /// Verifies that the root folder itself is rejected.
        /// </summary>
        [Test]
        public void IsVersionDirectory_rejects_the_root_itself()
        {
            Assert.That(RemoveCommand.IsVersionDirectory(this.root, this.root), Is.False);
        }

        /// <summary>
        /// Verifies that <c>root/..</c>, the root's parent, is rejected.
        /// </summary>
        [Test]
        public void IsVersionDirectory_rejects_a_parent_directory()
        {
            Assert.That(RemoveCommand.IsVersionDirectory(this.root, Path.Combine(this.root, "..")), Is.False);
        }

        /// <summary>
        /// Verifies that <c>root/2.5.1/../..</c> is rejected, since it resolves to the root's parent.
        /// </summary>
        [Test]
        public void IsVersionDirectory_rejects_a_child_that_climbs_back_out()
        {
            Assert.That(RemoveCommand.IsVersionDirectory(this.root, Path.Combine(this.root, "2.5.1", "..", "..")), Is.False);
        }

        /// <summary>
        /// Verifies that a folder two levels below the root is rejected.
        /// </summary>
        [Test]
        public void IsVersionDirectory_rejects_a_nested_folder()
        {
            Assert.That(RemoveCommand.IsVersionDirectory(this.root, Path.Combine(this.root, "a", "b")), Is.False);
        }

        /// <summary>
        /// Verifies that a version-named folder directly under a different root is rejected.
        /// </summary>
        [Test]
        public void IsVersionDirectory_rejects_a_folder_under_another_root()
        {
            var other = Path.Combine(Path.GetTempPath(), "uml4net-sage-tests", "remove", "elsewhere", "2.5.1");

            Assert.That(RemoveCommand.IsVersionDirectory(this.root, other), Is.False);
        }
    }
}
