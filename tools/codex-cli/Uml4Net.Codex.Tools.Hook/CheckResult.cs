// -------------------------------------------------------------------------------------------------
// <copyright file="CheckResult.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Codex.Tools.Hook
{
    using System.Collections.Generic;

    /// <summary>
    /// Mirrors the JSON shape of <c>uml4net-codex check --json</c>'s output (defined independently here,
    /// not by referencing <c>Uml4Net.Codex.Knowledge</c>, so this hook stays dependency-free - see the
    /// root CLAUDE.md's "small hook, big CLI" split).
    /// </summary>
    public sealed class CheckResult
    {
        public string? Default { get; set; }

        public bool HasAnyVersion { get; set; }

        public List<CheckResultVersion> Versions { get; set; } = [];
    }

    /// <summary>
    /// One entry of <see cref="CheckResult.Versions"/>.
    /// </summary>
    public sealed class CheckResultVersion
    {
        public string Version { get; set; } = string.Empty;

        public bool Fetched { get; set; }

        public bool Generated { get; set; }

        public bool SpecGenerated { get; set; }
    }
}
