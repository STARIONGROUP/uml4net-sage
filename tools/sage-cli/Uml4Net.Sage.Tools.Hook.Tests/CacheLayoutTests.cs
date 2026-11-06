// -------------------------------------------------------------------------------------------------
// <copyright file="CacheLayoutTests.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Sage.Tools.Hook.Tests
{
    [TestFixture]
    public class CacheLayoutTests
    {
        /// <summary>
        /// Verifies that hashing the same plugin root path twice gives the same install key.
        /// </summary>
        [Test]
        public void InstallKey_is_stable_for_the_same_path()
        {
            var first = CacheLayout.InstallKey(@"C:\repo\uml4net-sage");
            var second = CacheLayout.InstallKey(@"C:\repo\uml4net-sage");

            Assert.That(first, Is.EqualTo(second));
        }

        /// <summary>
        /// Verifies that two different plugin root paths get different install keys.
        /// </summary>
        [Test]
        public void InstallKey_differs_for_different_paths()
        {
            var first = CacheLayout.InstallKey(@"C:\repo\checkout-one");
            var second = CacheLayout.InstallKey(@"C:\repo\checkout-two");

            Assert.That(first, Is.Not.EqualTo(second));
        }

        /// <summary>
        /// Verifies that the CLI cache directory lies under <see cref="CacheLayout.CacheRoot"/> and contains
        /// the version, the runtime identifier and the install key of the plugin root.
        /// </summary>
        [Test]
        public void CliDirectoryFor_includes_version_rid_and_install_key()
        {
            var directory = CacheLayout.CliDirectoryFor(@"C:\repo\uml4net-sage", "1.0.0", "win-x64");

            Assert.That(directory, Does.Contain("1.0.0"));
            Assert.That(directory, Does.Contain("win-x64"));
            Assert.That(directory, Does.Contain(CacheLayout.InstallKey(@"C:\repo\uml4net-sage")));
            Assert.That(directory, Does.StartWith(CacheLayout.CacheRoot));
        }
    }
}
