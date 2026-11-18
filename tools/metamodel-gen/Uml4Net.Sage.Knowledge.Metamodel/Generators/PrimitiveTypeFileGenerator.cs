// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Metamodel.Generators
{
    using System.Text;

    using uml4net.SimpleClassifiers;

    using Uml4Net.Sage.Knowledge.Metamodel.Markdown;

    /// <summary>
    /// Renders one <see cref="IPrimitiveType"/> to a markdown page: front matter and its own documentation comment.
    /// </summary>
    public static class PrimitiveTypeFileGenerator
    {
        /// <summary>
        /// Renders the markdown page for <paramref name="primitiveType"/>.
        /// </summary>
        /// <param name="primitiveType">The primitive type to render.</param>
        /// <returns>The markdown page, with LF line endings.</returns>
        public static string Render(IPrimitiveType primitiveType)
        {
            var package = ElementNames.NamespaceOf(primitiveType);
            var documentation = MarkdownHelpers.Documentation(primitiveType);

            var builder = new StringBuilder();
            builder.Append("---\n");
            builder.Append("name: \"").Append(primitiveType.Name).Append("\"\n");
            builder.Append("kind: \"primitiveType\"\n");
            builder.Append("package: \"").Append(package).Append("\"\n");
            builder.Append("qualifiedName: \"").Append(primitiveType.QualifiedName).Append("\"\n");
            builder.Append("---\n\n");

            builder.Append("# ").Append(primitiveType.Name).Append("\n\n");
            builder.Append(string.IsNullOrWhiteSpace(documentation) ? "_No description available._" : documentation).Append('\n');

            return builder.ToString();
        }
    }
}
