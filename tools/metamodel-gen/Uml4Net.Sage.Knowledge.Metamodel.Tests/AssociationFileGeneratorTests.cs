// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Metamodel.Tests
{
    using System.Linq;

    using Uml4Net.Sage.Knowledge.Metamodel.Generators;

    [TestFixture]
    public class AssociationFileGeneratorTests
    {
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
