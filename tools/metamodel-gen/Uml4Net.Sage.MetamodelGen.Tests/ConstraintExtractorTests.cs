// -------------------------------------------------------------------------------------------------
// <copyright file="ConstraintExtractorTests.cs" company="Starion Group S.A.">
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
    }
}
