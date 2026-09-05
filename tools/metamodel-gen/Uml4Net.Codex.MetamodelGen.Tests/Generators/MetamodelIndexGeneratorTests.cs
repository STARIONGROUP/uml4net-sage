// -------------------------------------------------------------------------------------------------
// <copyright file="MetamodelIndexGeneratorTests.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Codex.MetamodelGen.Tests.Generators
{
    using System.Linq;

    using Uml4Net.Codex.MetamodelGen.Generators;

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
