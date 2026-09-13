// -------------------------------------------------------------------------------------------------
// <copyright file="AssociationExtractorTests.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Sage.MetamodelGen.Tests
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
