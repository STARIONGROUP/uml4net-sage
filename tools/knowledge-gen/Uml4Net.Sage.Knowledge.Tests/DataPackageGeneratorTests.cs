// -------------------------------------------------------------------------------------------------
// <copyright file="DataPackageGeneratorTests.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Sage.Knowledge.Tests
{
    using System.IO;
    using System.Text.Json;

    [TestFixture]
    public class DataPackageGeneratorTests
    {
        private string tempDirectory = null!;

        /// <summary>
        /// Creates an empty temporary directory to act as the knowledge version directory.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.tempDirectory = Path.Combine(Path.GetTempPath(), "uml4net-sage-tests", Path.GetRandomFileName());
            Directory.CreateDirectory(this.tempDirectory);
        }

        /// <summary>
        /// Deletes the temporary directory and everything in it.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            Directory.Delete(this.tempDirectory, recursive: true);
        }

        /// <summary>
        /// Verifies that, without <c>spec/index.json</c>, the package lists only two resources and none
        /// of them is <c>spec-clause-index</c>.
        /// </summary>
        [Test]
        public void Generate_without_a_spec_index_omits_the_spec_clause_index_resource()
        {
            var json = DataPackageGenerator.Generate(this.tempDirectory, "2.5.1", "formal/17-12-05");

            using var document = JsonDocument.Parse(json);
            var resources = document.RootElement.GetProperty("resources");

            Assert.That(resources.GetArrayLength(), Is.EqualTo(2));
            foreach (var resource in resources.EnumerateArray())
            {
                Assert.That(resource.GetProperty("name").GetString(), Is.Not.EqualTo("spec-clause-index"));
            }
        }

        /// <summary>
        /// Verifies that, when <c>spec/index.json</c> exists, the package lists three resources.
        /// </summary>
        [Test]
        public void Generate_with_a_spec_index_present_includes_all_three_tabular_resources()
        {
            Directory.CreateDirectory(Path.Combine(this.tempDirectory, "spec"));
            File.WriteAllText(Path.Combine(this.tempDirectory, "spec", "index.json"), "[]");

            var json = DataPackageGenerator.Generate(this.tempDirectory, "2.5.1", "formal/17-12-05");

            using var document = JsonDocument.Parse(json);
            var resources = document.RootElement.GetProperty("resources");

            Assert.That(resources.GetArrayLength(), Is.EqualTo(3));
        }

        /// <summary>
        /// Verifies that every resource has the <c>tabular-data-resource</c> profile and a schema with a
        /// non-empty primary key.
        /// </summary>
        [Test]
        public void Generate_gives_every_tabular_resource_a_table_schema_with_a_primary_key()
        {
            var json = DataPackageGenerator.Generate(this.tempDirectory, "2.5.1", "formal/17-12-05");

            using var document = JsonDocument.Parse(json);
            foreach (var resource in document.RootElement.GetProperty("resources").EnumerateArray())
            {
                Assert.That(resource.GetProperty("profile").GetString(), Is.EqualTo("tabular-data-resource"));
                Assert.That(resource.GetProperty("schema").GetProperty("primaryKey").GetString(), Is.Not.Null.And.Not.Empty);
            }
        }

        /// <summary>
        /// Verifies that the generated JSON contains the version field, the OMG document ID and the
        /// "Not redistributed" source note.
        /// </summary>
        [Test]
        public void Generate_records_the_omg_document_id_and_version_in_sources_and_top_level_fields()
        {
            var json = DataPackageGenerator.Generate(this.tempDirectory, "2.5.1", "formal/17-12-05");

            Assert.That(json, Does.Contain("\"version\": \"2.5.1\""));
            Assert.That(json, Does.Contain("formal/17-12-05"));
            Assert.That(json, Does.Contain("Not redistributed"));
        }
    }
}
