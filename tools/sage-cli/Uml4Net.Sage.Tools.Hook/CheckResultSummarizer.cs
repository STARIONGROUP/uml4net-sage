// -------------------------------------------------------------------------------------------------
// <copyright file="CheckResultSummarizer.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Sage.Tools.Hook
{
    /// <summary>
    /// Builds the one-line <c>additionalContext</c> message from a <see cref="CheckResult"/> - or
    /// <c>null</c> when there is nothing worth telling Claude this session (everything is already
    /// generated, or the check itself couldn't run).
    /// </summary>
    public static class CheckResultSummarizer
    {
        /// <summary>
        /// Summarizes <paramref name="result"/>, or returns <c>null</c> if nothing needs saying.
        /// </summary>
        public static string? Summarize(CheckResult? result)
        {
            if (result is null)
            {
                return null;
            }

            if (!result.HasAnyVersion)
            {
                return "uml4net-sage: no UML knowledge base has been fetched yet. If the user asks a UML question, " +
                       "offer to run `uml4net-sage fetch --version 2.5.1` then `uml4net-sage generate --version 2.5.1` " +
                       "(see the knowledge-setup skill) before answering from the metamodel/spec.";
            }

            var defaultEntry = result.Versions.Find(v => v.Version == result.Default);
            if (defaultEntry is null)
            {
                return null;
            }

            if (!defaultEntry.Generated)
            {
                return $"uml4net-sage: UML {result.Default} is fetched but not generated yet. Run " +
                       $"`uml4net-sage generate --version {result.Default}` before answering UML questions from the knowledge base.";
            }

            if (!defaultEntry.SpecGenerated)
            {
                return $"uml4net-sage: UML {result.Default} metamodel/standard-profile knowledge base is ready. " +
                       "Verbatim specification citation is not yet available (the PDFs weren't fetched, or Python/spec_extract " +
                       "isn't installed) - spec-citation will degrade to clause numbers only.";
            }

            return null;
        }
    }
}
