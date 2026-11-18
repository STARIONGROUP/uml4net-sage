// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Metamodel.Tests
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
