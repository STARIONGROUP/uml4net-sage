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

namespace Uml4Net.Sage.MetamodelGen.Tests.Generators
{
    using System.Linq;

    using Uml4Net.Sage.MetamodelGen.Generators;

    [TestFixture]
    public class MetamodelJsonGeneratorTests
    {
        /// <summary>
        /// Verifies that the class nodes carry the full descendant set of <c>Widget</c>, the full ancestor set of
        /// <c>SuperGadget</c>, and every attribute <c>SuperGadget</c> inherits.
        /// </summary>
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
            Assert.That(superGadget.InheritedAttributes.Select(a => a.Name), Is.EquivalentTo(new[] { "label", "count", "container" }));
        }

        /// <summary>
        /// Verifies that a redefined operation is not inherited: <c>Gadget</c>, which redefines
        /// <c>Widget::describe</c>, inherits no operations, and <c>SuperGadget</c> inherits only Gadget's.
        /// </summary>
        [Test]
        public void Build_excludes_redefined_members_from_inherited_operations()
        {
            var catalog = TestFixtures.BuildCatalog();
            var graph = ClassGraph.Build(catalog.Classes);

            var document = MetamodelJsonGenerator.Build(catalog, graph);

            // Gadget::describe redefines Widget::describe: Gadget inherits neither, SuperGadget only Gadget's.
            var gadget = document.Classes.Single(c => c.Name == "Gadget");
            Assert.That(gadget.InheritedOperations, Is.Empty);

            var superGadget = document.Classes.Single(c => c.Name == "SuperGadget");
            Assert.That(superGadget.InheritedOperations.Select(o => o.OwnerQualifiedName), Is.EqualTo(new[] { "Fixture::Gadget" }));
            Assert.That(superGadget.InheritedAttributes.All(a => a.Kind == "attribute"), Is.True);
        }

        /// <summary>
        /// Verifies that the serialized document uses camel-case property names and parses as JSON with the
        /// three fixture classes under <c>classes</c>.
        /// </summary>
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

        /// <summary>
        /// Verifies that the <c>describe</c> operation node carries its one parameter with name and type, and
        /// that the serialized JSON has a <c>parameters</c> property.
        /// </summary>
        [Test]
        public void Build_captures_an_operations_parameters()
        {
            var catalog = TestFixtures.BuildCatalog();
            var graph = ClassGraph.Build(catalog.Classes);
            var document = MetamodelJsonGenerator.Build(catalog, graph);

            var widget = document.Classes.Single(c => c.Name == "Widget");
            var describe = widget.OwnedOperations.Single(o => o.Name == "describe");

            Assert.That(describe.Parameters, Has.Count.EqualTo(1));
            Assert.That(describe.Parameters[0].Name, Is.EqualTo("verbosity"));
            Assert.That(describe.Parameters[0].TypeName, Is.EqualTo("Integer"));

            var json = MetamodelJsonGenerator.Serialize(document);
            Assert.That(json, Does.Contain("\"parameters\""));
        }

        /// <summary>
        /// Verifies that the document holds the one fixture association with both member ends, each flagged
        /// correctly as owned or not owned by the association, and that the JSON has an <c>associations</c> property.
        /// </summary>
        [Test]
        public void Build_captures_associations_and_their_member_ends()
        {
            var catalog = TestFixtures.BuildCatalog();
            var graph = ClassGraph.Build(catalog.Classes);
            var document = MetamodelJsonGenerator.Build(catalog, graph);

            Assert.That(document.Associations, Has.Count.EqualTo(1));
            var association = document.Associations[0];
            Assert.That(association.Name, Is.EqualTo("A_container_gadgets"));
            Assert.That(association.MemberEnds.Select(e => e.Name), Is.EquivalentTo(new[] { "container", "gadgets" }));
            Assert.That(association.MemberEnds.Single(e => e.Name == "gadgets").IsOwnedByAssociation, Is.True);
            Assert.That(association.MemberEnds.Single(e => e.Name == "container").IsOwnedByAssociation, Is.False);

            var json = MetamodelJsonGenerator.Serialize(document);
            Assert.That(json, Does.Contain("\"associations\""));
        }

        /// <summary>
        /// Verifies that the literals of <c>Kind</c> keep their XMI order (<c>A</c>, <c>B</c>, <c>Aa</c>) rather
        /// than being sorted alphabetically.
        /// </summary>
        [Test]
        public void Build_keeps_enumeration_literals_in_normative_xmi_order_not_alphabetical()
        {
            var catalog = TestFixtures.BuildCatalog();
            var graph = ClassGraph.Build(catalog.Classes);
            var document = MetamodelJsonGenerator.Build(catalog, graph);

            var kind = document.Enumerations.Single(e => e.Name == "Kind");

            Assert.That(kind.Literals, Is.EqualTo(new[] { "A", "B", "Aa" }));
        }
    }
}
