// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Metamodel.Tests
{
    using System.Linq;

    using Uml4Net.Sage.Knowledge.Metamodel.Generators;

    [TestFixture]
    public class PrimitiveTypeFileGeneratorTests
    {
        [Test]
        public void Render_includes_front_matter_and_the_owned_comment_as_the_description()
        {
            var catalog = TestFixtures.BuildCatalog();
            var stringType = catalog.PrimitiveTypes.Single(p => p.Name == "String");

            var markdown = PrimitiveTypeFileGenerator.Render(stringType);

            Assert.That(markdown, Does.Contain("kind: \"primitiveType\""));
            Assert.That(markdown, Does.Contain("qualifiedName: \"FixturePrimitiveTypes::String\""));
            Assert.That(markdown, Does.Contain("A fixture primitive type standing in for text."));
        }
    }
}
