// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Metamodel.Tests
{
    using System.Linq;

    using Uml4Net.Sage.Knowledge.Metamodel.Generators;

    [TestFixture]
    public class StereotypeFileGeneratorTests
    {
        private ElementCatalog catalog;
        private ClassGraph graph;

        [SetUp]
        public void SetUp()
        {
            this.catalog = TestFixtures.BuildCatalog();
            this.graph = ClassGraph.Build(this.catalog.Stereotypes);
        }

        [Test]
        public void Render_reads_the_base_metaclass_from_the_stereotypes_extension()
        {
            var sample = this.catalog.Stereotypes.Single(s => s.Name == "Sample");

            var markdown = StereotypeFileGenerator.Render(sample, this.graph);

            Assert.That(markdown, Does.Contain("# «Sample»"));
            Assert.That(markdown, Does.Contain("baseMetaclasses: [\"Widget\"]"));
            Assert.That(markdown, Does.Contain("- `Widget`"));
        }

        [Test]
        public void Render_does_not_report_a_specializing_stereotypes_extension_on_its_general()
        {
            // uml4net's IClass.Extension follows the OCL's endTypes.allParents() branch, so Sample's Extension
            // also contains SpecialSample's Gadget extension - it must not show up as a Sample base metaclass.
            var sample = this.catalog.Stereotypes.Single(s => s.Name == "Sample");
            var specialSample = this.catalog.Stereotypes.Single(s => s.Name == "SpecialSample");

            Assert.That(StereotypeFileGenerator.Render(sample, this.graph), Does.Not.Contain("Gadget"));
            Assert.That(StereotypeFileGenerator.Render(specialSample, this.graph), Does.Contain("baseMetaclasses: [\"Gadget\"]"));
        }

        [Test]
        public void Render_marks_a_required_extension_and_names_the_owning_profile()
        {
            var sample = this.catalog.Stereotypes.Single(s => s.Name == "Sample");
            var specialSample = this.catalog.Stereotypes.Single(s => s.Name == "SpecialSample");

            var sampleMarkdown = StereotypeFileGenerator.Render(sample, this.graph);
            var specialSampleMarkdown = StereotypeFileGenerator.Render(specialSample, this.graph);

            Assert.That(sampleMarkdown, Does.Contain("profile: \"FixtureProfile\""));
            Assert.That(sampleMarkdown, Does.Contain("- `Widget`\n"), "Sample's extension end has lower bound 0");
            Assert.That(specialSampleMarkdown, Does.Contain("- `Gadget` *(required)*"), "SpecialSample's extension end has lower bound 1");
        }

        [Test]
        public void Render_excludes_the_extension_end_attribute_from_tagged_values()
        {
            var sample = this.catalog.Stereotypes.Single(s => s.Name == "Sample");

            var markdown = StereotypeFileGenerator.Render(sample, this.graph);

            var taggedValuesSection = markdown[markdown.IndexOf("## Tagged values")..markdown.IndexOf("## Description")];
            Assert.That(taggedValuesSection, Does.Contain("- `note`"));
            Assert.That(taggedValuesSection, Does.Not.Contain("base_Widget"));
        }

        [Test]
        public void Render_lists_a_stereotypes_direct_generalizations()
        {
            var specialSample = this.catalog.Stereotypes.Single(s => s.Name == "SpecialSample");

            var markdown = StereotypeFileGenerator.Render(specialSample, this.graph);

            var generalizationsSection = markdown[markdown.IndexOf("## Generalizations")..markdown.IndexOf("## Specializations")];
            Assert.That(generalizationsSection, Does.Contain("[Sample](Sample.md)"));
        }

        [Test]
        public void Render_lists_a_stereotypes_direct_specializations()
        {
            var sample = this.catalog.Stereotypes.Single(s => s.Name == "Sample");

            var markdown = StereotypeFileGenerator.Render(sample, this.graph);

            var specializationsSection = markdown[markdown.IndexOf("## Specializations")..markdown.IndexOf("## Base metaclasses")];
            Assert.That(specializationsSection, Does.Contain("[SpecialSample](SpecialSample.md)"));
        }

        [Test]
        public void Render_says_none_for_a_stereotype_with_no_generalization()
        {
            var sample = this.catalog.Stereotypes.Single(s => s.Name == "Sample");

            var markdown = StereotypeFileGenerator.Render(sample, this.graph);

            var generalizationsSection = markdown[markdown.IndexOf("## Generalizations")..markdown.IndexOf("## Specializations")];
            Assert.That(generalizationsSection, Does.Contain("_None._"));
        }
    }
}
