// -------------------------------------------------------------------------------------------------
// <copyright file="StereotypeFileGeneratorTests.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Sage.MetamodelGen.Tests.Generators
{
    using System.Linq;

    using Uml4Net.Sage.MetamodelGen.Generators;

    [TestFixture]
    public class StereotypeFileGeneratorTests
    {
        private ElementCatalog catalog;
        private ClassGraph graph;

        /// <summary>
        /// Builds the fixture catalog and a <see cref="ClassGraph"/> over its stereotypes before each test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.catalog = TestFixtures.BuildCatalog();
            this.graph = ClassGraph.Build(this.catalog.Stereotypes);
        }

        /// <summary>
        /// Verifies that the page for <c>Sample</c> has a guillemet heading and names <c>Widget</c>, the
        /// metaclass its extension points at, in the front matter and the Base metaclasses list.
        /// </summary>
        [Test]
        public void Render_reads_the_base_metaclass_from_the_stereotypes_extension()
        {
            var sample = this.catalog.Stereotypes.Single(s => s.Name == "Sample");

            var markdown = StereotypeFileGenerator.Render(sample, this.graph);

            Assert.That(markdown, Does.Contain("# «Sample»"));
            Assert.That(markdown, Does.Contain("baseMetaclasses: [\"Widget\"]"));
            Assert.That(markdown, Does.Contain("- `Widget`"));
        }

        /// <summary>
        /// Verifies that the <c>Gadget</c> extension of <c>SpecialSample</c> is reported on SpecialSample's page
        /// and not on the page of its general, <c>Sample</c>.
        /// </summary>
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

        /// <summary>
        /// Verifies that the front matter names the owning profile, and that only the required extension
        /// (SpecialSample's, whose extension end has lower bound 1) is marked <c>(required)</c>.
        /// </summary>
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

        /// <summary>
        /// Verifies that the Tagged values section lists the <c>note</c> attribute but not <c>base_Widget</c>,
        /// the attribute that belongs to the extension.
        /// </summary>
        [Test]
        public void Render_excludes_the_extension_end_attribute_from_tagged_values()
        {
            var sample = this.catalog.Stereotypes.Single(s => s.Name == "Sample");

            var markdown = StereotypeFileGenerator.Render(sample, this.graph);

            var taggedValuesSection = markdown[markdown.IndexOf("## Tagged values")..markdown.IndexOf("## Description")];
            Assert.That(taggedValuesSection, Does.Contain("- `note`"));
            Assert.That(taggedValuesSection, Does.Not.Contain("base_Widget"));
        }

        /// <summary>
        /// Verifies that the Generalizations section of <c>SpecialSample</c> links to <c>Sample</c>.
        /// </summary>
        [Test]
        public void Render_lists_a_stereotypes_direct_generalizations()
        {
            var specialSample = this.catalog.Stereotypes.Single(s => s.Name == "SpecialSample");

            var markdown = StereotypeFileGenerator.Render(specialSample, this.graph);

            var generalizationsSection = markdown[markdown.IndexOf("## Generalizations")..markdown.IndexOf("## Specializations")];
            Assert.That(generalizationsSection, Does.Contain("[Sample](Sample.md)"));
        }

        /// <summary>
        /// Verifies that the Specializations section of <c>Sample</c> links to <c>SpecialSample</c>.
        /// </summary>
        [Test]
        public void Render_lists_a_stereotypes_direct_specializations()
        {
            var sample = this.catalog.Stereotypes.Single(s => s.Name == "Sample");

            var markdown = StereotypeFileGenerator.Render(sample, this.graph);

            var specializationsSection = markdown[markdown.IndexOf("## Specializations")..markdown.IndexOf("## Base metaclasses")];
            Assert.That(specializationsSection, Does.Contain("[SpecialSample](SpecialSample.md)"));
        }

        /// <summary>
        /// Verifies that the Generalizations section of <c>Sample</c>, which has no generalization, says
        /// <c>_None._</c>.
        /// </summary>
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
