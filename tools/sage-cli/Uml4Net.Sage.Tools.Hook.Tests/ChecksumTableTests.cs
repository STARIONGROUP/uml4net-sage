// -------------------------------------------------------------------------------------------------
// <copyright file="ChecksumTableTests.cs" company="Starion Group S.A.">
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
    public class ChecksumTableTests
    {
        [Test]
        public void Parse_extracts_one_entry_per_asset_line()
        {
            var body = """
                ## uml4net-sage tools v1.0.0

                Checksums:

                uml4net-sage-1.0.0-win-x64.zip sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
                uml4net-sage-1.0.0-linux-x64.zip sha256:bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb
                """;

            var checksums = ChecksumTable.Parse(body);

            Assert.That(checksums, Has.Count.EqualTo(2));
            Assert.That(checksums["uml4net-sage-1.0.0-win-x64.zip"], Is.EqualTo(new string('a', 64)));
            Assert.That(checksums["uml4net-sage-1.0.0-linux-x64.zip"], Is.EqualTo(new string('b', 64)));
        }

        [Test]
        public void Parse_lowercases_the_hash()
        {
            var body = $"uml4net-sage-1.0.0-osx-arm64.zip sha256:{new string('A', 64)}";

            var checksums = ChecksumTable.Parse(body);

            Assert.That(checksums["uml4net-sage-1.0.0-osx-arm64.zip"], Is.EqualTo(new string('a', 64)));
        }

        [Test]
        public void Parse_returns_empty_for_a_body_with_no_checksum_lines()
        {
            Assert.That(ChecksumTable.Parse("Just some release notes, no checksums here."), Is.Empty);
        }
    }
}
