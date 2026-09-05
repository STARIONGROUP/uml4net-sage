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

namespace Uml4Net.Codex.MetamodelGen.Tests
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
            Assert.That(catalog.Stereotypes.Select(s => s.Name), Is.EquivalentTo(new[] { "Sample" }));
        }

        [Test]
        public void Build_never_catalogs_a_stereotype_as_a_class()
        {
            var catalog = TestFixtures.BuildCatalog();

            Assert.That(catalog.Classes.Select(c => c.Name), Does.Not.Contain("Sample"));
        }
    }
}
