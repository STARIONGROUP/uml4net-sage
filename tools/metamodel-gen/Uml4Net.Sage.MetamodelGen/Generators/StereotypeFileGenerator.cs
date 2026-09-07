// -------------------------------------------------------------------------------------------------
// <copyright file="StereotypeFileGenerator.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Sage.MetamodelGen.Generators
{
    using System.Linq;
    using System.Text;

    using uml4net.CommonStructure;
    using uml4net.Packages;

    /// <summary>
    /// Renders one <see cref="IStereotype"/> to a markdown page. Written under <c>standard-profile/pages/</c>,
    /// never <c>metamodel/elements/</c> - a stereotype is not a metaclass, even though <see cref="IStereotype"/>
    /// happens to extend <see cref="IClass"/> in the UML metamodel itself.
    /// </summary>
    public static class StereotypeFileGenerator
    {
        private const string BaseAttributePrefix = "base_";

        /// <summary>
        /// Renders the markdown page for <paramref name="stereotype"/>.
        /// </summary>
        /// <remarks>
        /// Base metaclasses are derived from the stereotype's own "base_&lt;Metaclass&gt;" owned attributes
        /// (the OMG Standard Profile's fixed naming convention for the implicit extension-end property),
        /// rather than from <see cref="IClass.Extension"/>/<see cref="IExtension.Metaclass"/>: those derived
        /// properties are not yet implemented by uml4net's generated code as of uml4net.xmi 8.5.0.
        /// </remarks>
        public static string Render(IStereotype stereotype)
        {
            var ownedAttributes = stereotype.OwnedAttribute.ToList();

            var baseMetaclasses = ownedAttributes
                .Where(attribute => attribute.Name?.StartsWith(BaseAttributePrefix, System.StringComparison.Ordinal) == true)
                .Select(attribute => (attribute.Type as INamedElement)?.Name ?? attribute.Name[BaseAttributePrefix.Length..])
                .OrderBy(name => name, System.StringComparer.Ordinal)
                .ToList();

            var taggedValues = ownedAttributes
                .Where(attribute => attribute.Name?.StartsWith(BaseAttributePrefix, System.StringComparison.Ordinal) != true)
                .Select(attribute => attribute.Name)
                .OrderBy(name => name, System.StringComparer.Ordinal)
                .ToList();

            var documentation = stereotype.OwnedComment.Select(comment => comment.Body).FirstOrDefault(body => !string.IsNullOrWhiteSpace(body));

            var builder = new StringBuilder();
            builder.Append("---\n");
            builder.Append("name: \"").Append(stereotype.Name).Append("\"\n");
            builder.Append("kind: \"stereotype\"\n");
            builder.Append("qualifiedName: \"").Append(stereotype.QualifiedName).Append("\"\n");
            builder.Append("baseMetaclasses: [").Append(string.Join(", ", baseMetaclasses.Select(name => $"\"{name}\""))).Append("]\n");
            builder.Append("---\n\n");

            builder.Append("# «").Append(stereotype.Name).Append("»\n\n");

            builder.Append("## Base metaclasses\n\n");
            builder.Append(baseMetaclasses.Count == 0 ? "_None._" : string.Join("\n", baseMetaclasses.Select(name => $"- `{name}`"))).Append("\n\n");

            builder.Append("## Tagged values\n\n");
            builder.Append(taggedValues.Count == 0 ? "_None._" : string.Join("\n", taggedValues.Select(name => $"- `{name}`"))).Append("\n\n");

            builder.Append("## Description\n\n");
            builder.Append(string.IsNullOrWhiteSpace(documentation) ? "_No description available._" : documentation).Append('\n');

            return builder.ToString();
        }
    }
}
