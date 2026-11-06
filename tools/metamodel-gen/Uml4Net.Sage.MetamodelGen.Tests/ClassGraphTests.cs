// -------------------------------------------------------------------------------------------------
// <copyright file="ClassGraphTests.cs" company="Starion Group S.A.">
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
    [TestFixture]
    public class ClassGraphTests
    {
        private ClassGraph graph = null!;

        [SetUp]
        public void SetUp()
        {
            var catalog = TestFixtures.BuildCatalog();
            this.graph = ClassGraph.Build(catalog.Classes);
        }

        [Test]
        public void DirectSuperClassesOf_returns_the_immediate_parent_only()
        {
            var superClasses = this.graph.DirectSuperClassesOf("Fixture::Gadget");

            Assert.That(superClasses, Is.EqualTo(new[] { "Fixture::Widget" }));
        }

        [Test]
        public void DirectSubclassesOf_returns_the_immediate_child_only()
        {
            var subclasses = this.graph.DirectSubclassesOf("Fixture::Widget");

            Assert.That(subclasses, Is.EqualTo(new[] { "Fixture::Gadget" }));
        }

        [Test]
        public void AllDescendantsOf_transitively_includes_grandchildren()
        {
            var descendants = this.graph.AllDescendantsOf("Fixture::Widget");

            Assert.That(descendants, Is.EquivalentTo(new[] { "Fixture::Gadget", "Fixture::SuperGadget" }));
        }

        [Test]
        public void AllAncestorsOf_transitively_includes_grandparents()
        {
            var ancestors = this.graph.AllAncestorsOf("Fixture::SuperGadget");

            Assert.That(ancestors, Is.EquivalentTo(new[] { "Fixture::Gadget", "Fixture::Widget" }));
        }

        [Test]
        public void AllAncestorsOf_a_root_class_is_empty()
        {
            Assert.That(this.graph.AllAncestorsOf("Fixture::Widget"), Is.Empty);
        }
    }
}
