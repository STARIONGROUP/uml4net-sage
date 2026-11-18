// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Metamodel.Tests
{
    using System.Linq;

    [TestFixture]
    public class AssociationExtractorTests
    {
        [Test]
        public void MemberEndsOf_marks_the_end_owned_by_a_classifier_as_not_owned_by_the_association()
        {
            var catalog = TestFixtures.BuildCatalog();
            var association = catalog.Associations.Single(a => a.Name == "A_container_gadgets");

            var ends = AssociationExtractor.MemberEndsOf(association);

            var container = ends.Single(e => e.Name == "container");
            Assert.That(container.TypeName, Is.EqualTo("Widget"));
            Assert.That(container.IsOwnedByAssociation, Is.False);
        }

        [Test]
        public void MemberEndsOf_marks_the_associations_own_ownedEnd_as_owned_by_the_association()
        {
            var catalog = TestFixtures.BuildCatalog();
            var association = catalog.Associations.Single(a => a.Name == "A_container_gadgets");

            var ends = AssociationExtractor.MemberEndsOf(association);

            var gadgets = ends.Single(e => e.Name == "gadgets");
            Assert.That(gadgets.TypeName, Is.EqualTo("Gadget"));
            Assert.That(gadgets.IsOwnedByAssociation, Is.True);
            Assert.That(gadgets.IsOrdered, Is.True);
            Assert.That(gadgets.Upper, Is.EqualTo("*"));
        }
    }
}
