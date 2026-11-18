// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Tests
{
    using System.IO;
    using System.Text.Json;

    [TestFixture]
    public class DataPackageGeneratorTests
    {
        private string tempDirectory = null!;

        [SetUp]
        public void SetUp()
        {
            this.tempDirectory = Path.Combine(Path.GetTempPath(), "uml4net-sage-tests", Path.GetRandomFileName());
            Directory.CreateDirectory(this.tempDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(this.tempDirectory, recursive: true);
        }

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
