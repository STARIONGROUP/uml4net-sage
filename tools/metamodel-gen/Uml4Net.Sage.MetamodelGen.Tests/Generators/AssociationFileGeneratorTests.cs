// -------------------------------------------------------------------------------------------------
// <copyright file="AssociationFileGeneratorTests.cs" company="Starion Group S.A.">
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
    public class AssociationFileGeneratorTests
    {
        /// <summary>
        /// Verifies that the association page has <c>association</c> front matter with its qualified name and
        /// <c>isDerived</c> flag, and a bullet for each member end with its type link, multiplicity and modifiers.
        /// </summary>
        [Test]
        public void Render_includes_front_matter_and_both_member_ends()
        {
            var catalog = TestFixtures.BuildCatalog();
            var association = catalog.Associations.Single(a => a.Name == "A_container_gadgets");

            var markdown = AssociationFileGenerator.Render(association);

            Assert.That(markdown, Does.Contain("kind: \"association\""));
            Assert.That(markdown, Does.Contain("qualifiedName: \"Fixture::A_container_gadgets\""));
            Assert.That(markdown, Does.Contain("isDerived: false"));
            Assert.That(markdown, Does.Contain("- **container**: [Widget](Widget.md) [0..1]\n"));
            Assert.That(markdown, Does.Contain("- **gadgets**: [Gadget](Gadget.md) [0..*] *(ordered, non-navigable)*"));
        }

        /// <summary>
        /// Verifies that only the <c>gadgets</c> end, owned by the association, carries the "owned by this
        /// association" note, and the classifier-owned <c>container</c> end does not.
        /// </summary>
        [Test]
        public void Render_flags_the_end_owned_by_the_association_but_not_the_end_owned_by_a_classifier()
        {
            var catalog = TestFixtures.BuildCatalog();
            var association = catalog.Associations.Single(a => a.Name == "A_container_gadgets");

            var markdown = AssociationFileGenerator.Render(association);

            var containerBullet = markdown[markdown.IndexOf("- **container**")..markdown.IndexOf("- **gadgets**")];
            var gadgetsBullet = markdown[markdown.IndexOf("- **gadgets**")..markdown.IndexOf("## Description")];

            Assert.That(containerBullet, Does.Not.Contain("owned by this association"));
            Assert.That(gadgetsBullet, Does.Contain("owned by this association"));
        }

        /// <summary>
        /// Verifies that the Description section of an association without an <c>ownedComment</c> says
        /// <c>_No description available._</c>.
        /// </summary>
        [Test]
        public void Render_says_no_description_available_for_an_association_with_no_ownedComment()
        {
            var catalog = TestFixtures.BuildCatalog();
            var association = catalog.Associations.Single(a => a.Name == "A_container_gadgets");

            var markdown = AssociationFileGenerator.Render(association);

            var descriptionSection = markdown[markdown.IndexOf("## Description")..];
            Assert.That(descriptionSection, Does.Contain("_No description available._"));
        }
    }
}
