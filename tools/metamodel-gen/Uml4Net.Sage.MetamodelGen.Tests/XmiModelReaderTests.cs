// -------------------------------------------------------------------------------------------------
// <copyright file="XmiModelReaderTests.cs" company="Starion Group S.A.">
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
    using System.IO;
    using System.Linq;

    using uml4net.Packages;

    [TestFixture]
    public class XmiModelReaderTests
    {
        /// <summary>
        /// Verifies that reading <c>Fixture.xmi</c> returns the <c>Fixture</c> and <c>FixturePrimitiveTypes</c>
        /// top-level packages.
        /// </summary>
        [Test]
        public void Read_of_the_fixture_root_package_returns_expected_top_level_packages()
        {
            var result = XmiModelReader.Read(Path.Combine(TestFixtures.XmiDirectory, "Fixture.xmi"));

            var names = result.Packages.Select(p => p.Name).OrderBy(n => n).ToList();

            Assert.That(names, Does.Contain("Fixture"));
            Assert.That(names, Does.Contain("FixturePrimitiveTypes"));
        }

        /// <summary>
        /// Verifies that the type of <c>Widget::label</c>, defined in a sibling XMI file, resolves to the
        /// <c>String</c> primitive type.
        /// </summary>
        [Test]
        public void Read_resolves_cross_file_type_references_via_local_reference_base_path()
        {
            var result = XmiModelReader.Read(Path.Combine(TestFixtures.XmiDirectory, "Fixture.xmi"));

            var fixturePackage = result.Packages.Single(p => p.Name == "Fixture");
            var widget = (uml4net.StructuredClassifiers.IClass)fixturePackage.PackagedElement.Single(e => e.Name == "Widget");
            var labelType = widget.OwnedAttribute.Single(a => a.Name == "label").Type;

            Assert.That(labelType, Is.Not.Null);
            Assert.That((labelType as uml4net.CommonStructure.INamedElement)?.Name, Is.EqualTo("String"));
        }

        /// <summary>
        /// Verifies that, in <c>FixtureProfile.xmi</c>, the <c>base_Widget</c> attribute of the <c>Sample</c>
        /// stereotype resolves to the <c>Widget</c> metaclass in the fixture metamodel.
        /// </summary>
        [Test]
        public void Read_of_the_fixture_profile_resolves_the_extended_metaclass()
        {
            var result = XmiModelReader.Read(Path.Combine(TestFixtures.XmiDirectory, "FixtureProfile.xmi"));

            var profile = result.Packages.Single(p => p.Name == "FixtureProfile");
            var sample = (uml4net.Packages.IStereotype)profile.PackagedElement.Single(e => e.Name == "Sample");
            var baseWidget = sample.OwnedAttribute.Single(a => a.Name == "base_Widget");

            Assert.That((baseWidget.Type as uml4net.CommonStructure.INamedElement)?.Name, Is.EqualTo("Widget"));
        }
    }
}
