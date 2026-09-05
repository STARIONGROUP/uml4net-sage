// -------------------------------------------------------------------------------------------------
// <copyright file="EnumerationFileGenerator.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Codex.MetamodelGen.Generators
{
    using System.Linq;
    using System.Text;

    using uml4net.CommonStructure;
    using uml4net.SimpleClassifiers;

    /// <summary>
    /// Renders one <see cref="IEnumeration"/> to a markdown page: front matter and its owned literals.
    /// </summary>
    public static class EnumerationFileGenerator
    {
        /// <summary>
        /// Renders the markdown page for <paramref name="enumeration"/>.
        /// </summary>
        public static string Render(IEnumeration enumeration)
        {
            var package = (enumeration.Namespace as INamedElement)?.QualifiedName ?? string.Empty;

            var builder = new StringBuilder();
            builder.Append("---\n");
            builder.Append("name: \"").Append(enumeration.Name).Append("\"\n");
            builder.Append("kind: \"enumeration\"\n");
            builder.Append("package: \"").Append(package).Append("\"\n");
            builder.Append("qualifiedName: \"").Append(enumeration.QualifiedName).Append("\"\n");
            builder.Append("---\n\n");

            builder.Append("# ").Append(enumeration.Name).Append("\n\n");
            builder.Append("## Literals\n\n");

            var literals = enumeration.OwnedLiteral.Select(literal => literal.Name).OrderBy(name => name, System.StringComparer.Ordinal).ToList();
            if (literals.Count == 0)
            {
                builder.Append("_None._\n");
            }
            else
            {
                foreach (var literal in literals)
                {
                    builder.Append("- `").Append(literal).Append("`\n");
                }
            }

            return builder.ToString();
        }
    }
}
