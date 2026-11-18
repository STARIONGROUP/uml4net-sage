// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Metamodel.Tests
{
    using System.Linq;

    [TestFixture]
    public class ElementCatalogTests
    {
        [Test]
        public void Build_catalogs_classes_enumerations_primitive_types_and_stereotypes_separately()
        {
            var catalog = TestFixtures.BuildCatalog();

            Assert.That(catalog.Classes.Select(c => c.Name), Is.EquivalentTo(new[] { "Widget", "Gadget", "SuperGadget" }));
            Assert.That(catalog.Enumerations.Select(e => e.Name), Is.EquivalentTo(new[] { "Kind" }));
            Assert.That(catalog.PrimitiveTypes.Select(p => p.Name), Is.EquivalentTo(new[] { "String", "Integer" }));
            Assert.That(catalog.Associations.Select(a => a.Name), Is.EquivalentTo(new[] { "A_container_gadgets" }));
            Assert.That(catalog.Stereotypes.Select(s => s.Name), Is.EquivalentTo(new[] { "Sample", "SpecialSample" }));
        }

        [Test]
        public void Build_never_catalogs_a_stereotype_as_a_class()
        {
            var catalog = TestFixtures.BuildCatalog();

            Assert.That(catalog.Classes.Select(c => c.Name), Does.Not.Contain("Sample"));
        }

        [Test]
        public void Build_never_catalogs_an_association_as_a_class()
        {
            var catalog = TestFixtures.BuildCatalog();

            Assert.That(catalog.Classes.Select(c => c.Name), Does.Not.Contain("A_container_gadgets"));
        }
    }
}
