// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Metamodel.Tests
{
    using System.Linq;

    using Uml4Net.Sage.Knowledge.Metamodel.Generators;

    [TestFixture]
    public class StandardProfileIndexGeneratorTests
    {
        [Test]
        public void BuildRows_includes_every_stereotype()
        {
            var catalog = TestFixtures.BuildCatalog();

            var rows = StandardProfileIndexGenerator.BuildRows(catalog);

            Assert.That(rows, Has.Count.EqualTo(2));
            var sampleRow = rows.Single(r => r.QualifiedName == "FixtureProfile::Sample");
            Assert.That(sampleRow.File, Is.EqualTo("pages/Sample.md"));
        }

        [Test]
        public void BuildRows_records_each_stereotypes_profile_and_own_base_metaclasses()
        {
            var rows = StandardProfileIndexGenerator.BuildRows(TestFixtures.BuildCatalog());

            var sampleRow = rows.Single(r => r.QualifiedName == "FixtureProfile::Sample");
            Assert.That(sampleRow.Profile, Is.EqualTo("FixtureProfile"));
            Assert.That(sampleRow.BaseMetaclasses, Is.EqualTo(new[] { "Widget" }));

            var specialSampleRow = rows.Single(r => r.QualifiedName == "FixtureProfile::SpecialSample");
            Assert.That(specialSampleRow.BaseMetaclasses, Is.EqualTo(new[] { "Gadget" }));
        }

        [Test]
        public void RenderMarkdown_includes_a_row_per_stereotype()
        {
            var rows = StandardProfileIndexGenerator.BuildRows(TestFixtures.BuildCatalog());

            var markdown = StandardProfileIndexGenerator.RenderMarkdown(rows);

            Assert.That(markdown, Does.Contain("FixtureProfile::Sample"));
        }
    }
}
