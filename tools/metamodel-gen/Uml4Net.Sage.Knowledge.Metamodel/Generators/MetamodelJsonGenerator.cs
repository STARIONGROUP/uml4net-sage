// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Metamodel.Generators
{
    using System.Linq;
    using System.Text.Json;

    using Uml4Net.Sage.Knowledge.Metamodel;
    using Uml4Net.Sage.Knowledge.Metamodel.Model;

    /// <summary>
    /// Builds and serializes <c>metamodel/metamodel.json</c>: the full class/enumeration/primitive-type
    /// graph with precomputed inheritance closures.
    /// </summary>
    public static class MetamodelJsonGenerator
    {
        /// <summary>
        /// The fixed <see cref="JsonSerializerOptions"/> used for every generated JSON file, so
        /// regeneration is byte-for-byte deterministic (see CLAUDE.md, "Determinism of generated artifacts").
        /// </summary>
        public static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        /// <summary>
        /// Builds the document for the given catalog.
        /// </summary>
        /// <param name="catalog">The catalogued metamodel elements.</param>
        /// <param name="graph">The generalization graph over <paramref name="catalog"/>'s classes.</param>
        /// <returns>
        /// The document, with each kind of node sorted by qualified name; elements without a qualified name are left out.
        /// </returns>
        public static MetamodelJsonDocument Build(ElementCatalog catalog, ClassGraph graph)
        {
            var classNodes = catalog.Classes
                .Where(c => !string.IsNullOrEmpty(c.QualifiedName))
                .Select(c => BuildClassNode(c, graph))
                .OrderBy(node => node.QualifiedName, System.StringComparer.Ordinal)
                .ToList();

            var enumerationNodes = catalog.Enumerations
                .Where(e => !string.IsNullOrEmpty(e.QualifiedName))
                .Select(e => new EnumerationJsonNode(
                    e.Name,
                    e.QualifiedName,
                    ElementNames.NamespaceOf(e),
                    // ownedLiteral is ordered: keep the XMI's (normative) literal order, not alphabetical.
                    e.OwnedLiteral.Select(l => l.Name).ToList()))
                .OrderBy(node => node.QualifiedName, System.StringComparer.Ordinal)
                .ToList();

            var primitiveTypeNodes = catalog.PrimitiveTypes
                .Where(p => !string.IsNullOrEmpty(p.QualifiedName))
                .Select(p => new PrimitiveTypeJsonNode(p.Name, p.QualifiedName, ElementNames.NamespaceOf(p)))
                .OrderBy(node => node.QualifiedName, System.StringComparer.Ordinal)
                .ToList();

            var associationNodes = catalog.Associations
                .Where(a => !string.IsNullOrEmpty(a.QualifiedName))
                .Select(a => new AssociationJsonNode(
                    a.Name,
                    a.QualifiedName,
                    ElementNames.NamespaceOf(a),
                    a.IsAbstract,
                    a.IsDerived,
                    AssociationExtractor.MemberEndsOf(a)))
                .OrderBy(node => node.QualifiedName, System.StringComparer.Ordinal)
                .ToList();

            return new MetamodelJsonDocument(classNodes, enumerationNodes, primitiveTypeNodes, associationNodes);
        }

        /// <summary>
        /// Serializes a document to its deterministic JSON text (LF line endings via the caller writing the file).
        /// </summary>
        /// <param name="document">The document to serialize.</param>
        /// <returns>The indented, camel-case JSON text.</returns>
        public static string Serialize(MetamodelJsonDocument document)
        {
            return JsonSerializer.Serialize(document, SerializerOptions);
        }

        /// <summary>
        /// Builds the node for one class: its generalization closures, owned and inherited features, and
        /// owned constraints.
        /// </summary>
        /// <param name="class">The class to describe.</param>
        /// <param name="graph">The generalization graph used to resolve the superclass and subclass closures.</param>
        /// <returns>The class's node.</returns>
        private static MetamodelJsonNode BuildClassNode(uml4net.StructuredClassifiers.IClass @class, ClassGraph graph)
        {
            var qualifiedName = @class.QualifiedName;
            var package = ElementNames.NamespaceOf(@class);

            var ownedAttributes = @class.OwnedAttribute
                .Select(a => FeatureExtractor.FromProperty(a, qualifiedName))
                .OrderBy(f => f.Name, System.StringComparer.Ordinal)
                .ToList();

            var ownedOperations = @class.OwnedOperation
                .Select(o => FeatureExtractor.FromOperation(o, qualifiedName))
                .OrderBy(f => f.Name, System.StringComparer.Ordinal)
                .ToList();

            var inheritedFeatures = FeatureExtractor.InheritedFeaturesOf(@class);

            return new MetamodelJsonNode(
                Name: @class.Name,
                QualifiedName: qualifiedName,
                Package: package,
                IsAbstract: @class.IsAbstract,
                DirectSuperClasses: graph.DirectSuperClassesOf(qualifiedName),
                DirectSubclasses: graph.DirectSubclassesOf(qualifiedName),
                AllAncestors: graph.AllAncestorsOf(qualifiedName),
                AllDescendants: graph.AllDescendantsOf(qualifiedName),
                OwnedAttributes: ownedAttributes,
                OwnedOperations: ownedOperations,
                InheritedAttributes: inheritedFeatures.Where(f => f.Kind == "attribute").ToList(),
                InheritedOperations: inheritedFeatures.Where(f => f.Kind == "operation").ToList(),
                Constraints: ConstraintExtractor.FromNamespace(@class));
        }
    }
}
