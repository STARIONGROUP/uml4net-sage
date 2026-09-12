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

        [SetUp]
        public void SetUp()
        {
            this.catalog = TestFixtures.BuildCatalog();
            this.graph = ClassGraph.Build(this.catalog.Stereotypes);
        }

        [Test]
        public void Render_derives_the_base_metaclass_from_the_base_prefixed_owned_attribute()
        {
            // Regression test: IClass.Extension / IExtension.Metaclass throw NotSupportedException in
            // uml4net.xmi 8.5.0 - StereotypeFileGenerator must derive the base metaclass from the
            // stereotype's own "base_<Metaclass>" owned attribute instead.
            var sample = this.catalog.Stereotypes.Single(s => s.Name == "Sample");

            var markdown = StereotypeFileGenerator.Render(sample, this.graph);

            Assert.That(markdown, Does.Contain("# «Sample»"));
            Assert.That(markdown, Does.Contain("baseMetaclasses: [\"Widget\"]"));
            Assert.That(markdown, Does.Contain("- `Widget`"));
        }

        [Test]
        public void Render_excludes_the_base_prefixed_attribute_from_tagged_values()
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
