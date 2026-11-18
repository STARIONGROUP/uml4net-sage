// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Metamodel.Tests
{
    using System.Linq;

    [TestFixture]
    public class ConstraintExtractorTests
    {
        [Test]
        public void FromNamespace_reads_the_ocl_body_and_language_of_an_owned_rule()
        {
            var catalog = TestFixtures.BuildCatalog();
            var widget = catalog.Classes.Single(c => c.Name == "Widget");

            var constraints = ConstraintExtractor.FromNamespace(widget);

            Assert.That(constraints, Has.Count.EqualTo(1));
            Assert.That(constraints[0].Name, Is.EqualTo("label_not_empty"));
            Assert.That(constraints[0].Languages, Is.EqualTo(new[] { "OCL" }));
            Assert.That(constraints[0].Body, Is.EqualTo(new[] { "label->notEmpty()" }));
        }

        [Test]
        public void FromNamespace_is_empty_for_a_class_with_no_owned_rules()
        {
            var catalog = TestFixtures.BuildCatalog();
            var gadget = catalog.Classes.Single(c => c.Name == "Gadget");

            Assert.That(ConstraintExtractor.FromNamespace(gadget), Is.Empty);
        }

        [Test]
        public void FromOperationBody_reads_the_ocl_body_of_an_operation_bodyCondition()
        {
            var catalog = TestFixtures.BuildCatalog();
            var widget = catalog.Classes.Single(c => c.Name == "Widget");
            var describe = widget.OwnedOperation.Single(o => o.Name == "describe");

            var constraints = ConstraintExtractor.FromOperationBody(describe);

            Assert.That(constraints, Has.Count.EqualTo(1));
            Assert.That(constraints[0].Name, Is.EqualTo("describe_body"));
            Assert.That(constraints[0].Languages, Is.EqualTo(new[] { "OCL" }));
            Assert.That(constraints[0].Body, Is.EqualTo(new[] { "result = (label)" }));
        }
    }
}
