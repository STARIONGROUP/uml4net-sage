// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Metamodel.Tests
{
    using System.Linq;

    using Uml4Net.Sage.Knowledge.Metamodel.Generators;

    [TestFixture]
    public class MetamodelIndexGeneratorTests
    {
        [Test]
        public void BuildRows_includes_classes_enumerations_and_primitive_types_sorted_by_qualified_name()
        {
            var catalog = TestFixtures.BuildCatalog();

            var rows = MetamodelIndexGenerator.BuildRows(catalog);

            Assert.That(rows.Select(r => r.QualifiedName), Is.Ordered.Using<string>(System.StringComparer.Ordinal));
            Assert.That(rows.Select(r => r.Name), Does.Contain("Widget"));
            Assert.That(rows.Select(r => r.Name), Does.Contain("Kind"));
            Assert.That(rows.Select(r => r.Name), Does.Contain("String"));
            Assert.That(rows.Single(r => r.Name == "Kind").Kind, Is.EqualTo("enumeration"));
            Assert.That(rows.Single(r => r.Name == "String").Kind, Is.EqualTo("primitiveType"));
            Assert.That(rows.Single(r => r.Name == "Widget").Kind, Is.EqualTo("class"));
        }

        [Test]
        public void BuildRows_includes_associations()
        {
            var catalog = TestFixtures.BuildCatalog();

            var rows = MetamodelIndexGenerator.BuildRows(catalog);

            var associationRow = rows.Single(r => r.Name == "A_container_gadgets");
            Assert.That(associationRow.Kind, Is.EqualTo("association"));
            Assert.That(associationRow.File, Is.EqualTo("elements/A_container_gadgets.md"));
        }

        [Test]
        public void BuildRows_never_includes_stereotypes()
        {
            var catalog = TestFixtures.BuildCatalog();

            var rows = MetamodelIndexGenerator.BuildRows(catalog);

            Assert.That(rows.Select(r => r.Name), Does.Not.Contain("Sample"));
        }

        [Test]
        public void SerializeJson_produces_a_flat_array_not_a_keyed_map()
        {
            var rows = MetamodelIndexGenerator.BuildRows(TestFixtures.BuildCatalog());

            var json = MetamodelIndexGenerator.SerializeJson(rows);

            Assert.That(json.TrimStart(), Does.StartWith("["));
        }
    }
}
