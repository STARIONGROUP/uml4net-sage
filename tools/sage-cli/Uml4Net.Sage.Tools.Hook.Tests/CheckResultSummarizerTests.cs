// -------------------------------------------------------------------------------------------------
// <copyright file="CheckResultSummarizerTests.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Sage.Tools.Hook.Tests
{
    [TestFixture]
    public class CheckResultSummarizerTests
    {
        [Test]
        public void Summarize_of_null_is_null()
        {
            Assert.That(CheckResultSummarizer.Summarize(null), Is.Null);
        }

        [Test]
        public void Summarize_mentions_fetch_when_nothing_is_installed()
        {
            var result = new CheckResult { HasAnyVersion = false };

            var message = CheckResultSummarizer.Summarize(result);

            Assert.That(message, Does.Contain("fetch"));
        }

        [Test]
        public void Summarize_mentions_generate_when_fetched_but_not_generated()
        {
            var result = new CheckResult
            {
                HasAnyVersion = true,
                Default = "2.5.1",
                Versions = [new CheckResultVersion { Version = "2.5.1", Fetched = true, Generated = false }],
            };

            var message = CheckResultSummarizer.Summarize(result);

            Assert.That(message, Does.Contain("generate"));
            Assert.That(message, Does.Contain("2.5.1"));
        }

        [Test]
        public void Summarize_mentions_spec_citation_limits_when_spec_not_generated()
        {
            var result = new CheckResult
            {
                HasAnyVersion = true,
                Default = "2.5.1",
                Versions = [new CheckResultVersion { Version = "2.5.1", Fetched = true, Generated = true, SpecGenerated = false }],
            };

            var message = CheckResultSummarizer.Summarize(result);

            Assert.That(message, Does.Contain("spec"));
        }

        [Test]
        public void Summarize_is_null_when_everything_is_fully_generated()
        {
            var result = new CheckResult
            {
                HasAnyVersion = true,
                Default = "2.5.1",
                Versions = [new CheckResultVersion { Version = "2.5.1", Fetched = true, Generated = true, SpecGenerated = true }],
            };

            Assert.That(CheckResultSummarizer.Summarize(result), Is.Null);
        }
    }
}
