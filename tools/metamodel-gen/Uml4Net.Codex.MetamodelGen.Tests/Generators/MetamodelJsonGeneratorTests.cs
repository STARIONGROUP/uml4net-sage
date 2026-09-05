// -------------------------------------------------------------------------------------------------
// <copyright file="MetamodelJsonGeneratorTests.cs" company="Starion Group S.A.">
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
    public class MetamodelJsonGeneratorTests
    {
        [Test]
        public void Build_precomputes_ancestor_and_descendant_closures()
        {
            var catalog = TestFixtures.BuildCatalog();
            var graph = ClassGraph.Build(catalog.Classes);

            var document = MetamodelJsonGenerator.Build(catalog, graph);

            var widget = document.Classes.Single(c => c.Name == "Widget");
            Assert.That(widget.AllDescendants, Is.EquivalentTo(new[] { "Fixture::Gadget", "Fixture::SuperGadget" }));

            var superGadget = document.Classes.Single(c => c.Name == "SuperGadget");
            Assert.That(superGadget.AllAncestors, Is.EquivalentTo(new[] { "Fixture::Gadget", "Fixture::Widget" }));
            Assert.That(superGadget.InheritedAttributes.Select(a => a.Name), Is.EquivalentTo(new[] { "label", "count" }));
        }

        [Test]
        public void Serialize_produces_valid_camel_case_json()
        {
            var catalog = TestFixtures.BuildCatalog();
            var graph = ClassGraph.Build(catalog.Classes);
            var document = MetamodelJsonGenerator.Build(catalog, graph);

            var json = MetamodelJsonGenerator.Serialize(document);

            Assert.That(json, Does.Contain("\"qualifiedName\""));
            Assert.That(json, Does.Contain("\"allDescendants\""));

            using var parsed = System.Text.Json.JsonDocument.Parse(json);
            Assert.That(parsed.RootElement.GetProperty("classes").GetArrayLength(), Is.EqualTo(3));
        }
    }
}
