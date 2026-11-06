// -------------------------------------------------------------------------------------------------
// <copyright file="ElementCatalogTests.cs" company="Starion Group S.A.">
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
    public class ElementCatalogTests
    {
        /// <summary>
        /// Verifies that the fixture's classes, enumerations, primitive types, associations and stereotypes
        /// each end up in their own list of the catalog.
        /// </summary>
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

        /// <summary>
        /// Verifies that the <c>Sample</c> stereotype, although a stereotype is also a class, is not listed
        /// among the catalog's classes.
        /// </summary>
        [Test]
        public void Build_never_catalogs_a_stereotype_as_a_class()
        {
            var catalog = TestFixtures.BuildCatalog();

            Assert.That(catalog.Classes.Select(c => c.Name), Does.Not.Contain("Sample"));
        }

        /// <summary>
        /// Verifies that the <c>A_container_gadgets</c> association is not listed among the catalog's classes.
        /// </summary>
        [Test]
        public void Build_never_catalogs_an_association_as_a_class()
        {
            var catalog = TestFixtures.BuildCatalog();

            Assert.That(catalog.Classes.Select(c => c.Name), Does.Not.Contain("A_container_gadgets"));
        }
    }
}
