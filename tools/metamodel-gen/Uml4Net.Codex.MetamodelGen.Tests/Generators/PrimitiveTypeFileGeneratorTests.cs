// -------------------------------------------------------------------------------------------------
// <copyright file="PrimitiveTypeFileGeneratorTests.cs" company="Starion Group S.A.">
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
