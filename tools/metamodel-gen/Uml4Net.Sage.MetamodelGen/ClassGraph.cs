// -------------------------------------------------------------------------------------------------
// <copyright file="ClassGraph.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Sage.MetamodelGen
{
    using System.Collections.Generic;
    using System.Linq;

    using uml4net.StructuredClassifiers;

    /// <summary>
    /// Precomputes generalization/specialization closures over a set of classes, so consumers never have
    /// to re-walk <c>Generalization</c> chains by hand.
    /// </summary>
    public sealed class ClassGraph
    {
        private readonly Dictionary<string, IClass> classesByQualifiedName;
        private readonly Dictionary<string, List<string>> directSubclassesByQualifiedName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassGraph"/> class from precomputed lookups.
        /// </summary>
        /// <param name="classesByQualifiedName">The classes, keyed by qualified name.</param>
        /// <param name="directSubclassesByQualifiedName">
        /// The sorted qualified names of each class's direct subclasses, keyed by the class's qualified name.
        /// </param>
        private ClassGraph(Dictionary<string, IClass> classesByQualifiedName, Dictionary<string, List<string>> directSubclassesByQualifiedName)
        {
            this.classesByQualifiedName = classesByQualifiedName;
            this.directSubclassesByQualifiedName = directSubclassesByQualifiedName;
        }

        /// <summary>
        /// Builds a <see cref="ClassGraph"/> from a flat list of classes.
        /// </summary>
        /// <param name="classes">
        /// Every class known to the catalog.
        /// </param>
        /// <returns>
        /// A populated <see cref="ClassGraph"/>.
        /// </returns>
        public static ClassGraph Build(IReadOnlyList<IClass> classes)
        {
            var byQualifiedName = classes
                .Where(c => !string.IsNullOrEmpty(c.QualifiedName))
                .GroupBy(c => c.QualifiedName)
                .ToDictionary(g => g.Key, g => g.First());

            var directSubclasses = byQualifiedName.Keys.ToDictionary(name => name, _ => new List<string>());

            foreach (var @class in classes)
            {
                if (string.IsNullOrEmpty(@class.QualifiedName))
                {
                    continue;
                }

                foreach (var superClass in @class.SuperClass)
                {
                    if (!string.IsNullOrEmpty(superClass.QualifiedName) && directSubclasses.TryGetValue(superClass.QualifiedName, out var children))
                    {
                        children.Add(@class.QualifiedName);
                    }
                }
            }

            foreach (var children in directSubclasses.Values)
            {
                children.Sort(System.StringComparer.Ordinal);
            }

            return new ClassGraph(byQualifiedName, directSubclasses);
        }

        /// <summary>
        /// Attempts to look up a catalogued class by its qualified name.
        /// </summary>
        /// <param name="qualifiedName">The qualified name of the class to find.</param>
        /// <param name="class">The class when found; otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> when the class is in the graph; otherwise <see langword="false"/>.</returns>
        public bool TryGetClass(string qualifiedName, out IClass? @class)
        {
            return this.classesByQualifiedName.TryGetValue(qualifiedName, out @class);
        }

        /// <summary>
        /// Gets the qualified names of the classes directly generalizing the class with the given qualified name.
        /// </summary>
        /// <param name="qualifiedName">The qualified name of the class.</param>
        /// <returns>
        /// The direct superclasses' qualified names, sorted ordinally, or an empty list when the class is unknown.
        /// </returns>
        public IReadOnlyList<string> DirectSuperClassesOf(string qualifiedName)
        {
            if (!this.classesByQualifiedName.TryGetValue(qualifiedName, out var @class))
            {
                return [];
            }

            return @class.SuperClass
                .Where(s => !string.IsNullOrEmpty(s.QualifiedName))
                .Select(s => s.QualifiedName)
                .OrderBy(name => name, System.StringComparer.Ordinal)
                .ToList();
        }

        /// <summary>
        /// Gets the qualified names of the classes directly specializing the class with the given qualified name.
        /// </summary>
        /// <param name="qualifiedName">The qualified name of the class.</param>
        /// <returns>
        /// The direct subclasses' qualified names, sorted ordinally, or an empty list when the class is unknown.
        /// </returns>
        public IReadOnlyList<string> DirectSubclassesOf(string qualifiedName)
        {
            return this.directSubclassesByQualifiedName.TryGetValue(qualifiedName, out var children) ? children : [];
        }

        /// <summary>
        /// Gets every ancestor (transitive superclass) of the class with the given qualified name, sorted.
        /// </summary>
        /// <param name="qualifiedName">The qualified name of the class.</param>
        /// <returns>The ancestors' qualified names, sorted ordinally; empty for a root class.</returns>
        public IReadOnlyList<string> AllAncestorsOf(string qualifiedName)
        {
            return this.Traverse(qualifiedName, this.DirectSuperClassesOf);
        }

        /// <summary>
        /// Gets every descendant (transitive subclass) of the class with the given qualified name, sorted.
        /// </summary>
        /// <param name="qualifiedName">The qualified name of the class.</param>
        /// <returns>The descendants' qualified names, sorted ordinally; empty for a leaf class.</returns>
        public IReadOnlyList<string> AllDescendantsOf(string qualifiedName)
        {
            return this.Traverse(qualifiedName, this.DirectSubclassesOf);
        }

        /// <summary>
        /// Collects every class reachable from <paramref name="qualifiedName"/> by repeatedly following
        /// <paramref name="next"/>, breadth-first. Each class is visited once, so cycles terminate.
        /// </summary>
        /// <param name="qualifiedName">The qualified name of the starting class, which is not itself included.</param>
        /// <param name="next">Gets the qualified names one step away from a class (its direct superclasses or subclasses).</param>
        /// <returns>The reachable classes' qualified names, sorted ordinally.</returns>
        private List<string> Traverse(string qualifiedName, System.Func<string, IReadOnlyList<string>> next)
        {
            var visited = new HashSet<string>();
            var queue = new Queue<string>(next(qualifiedName));

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!visited.Add(current))
                {
                    continue;
                }

                foreach (var following in next(current))
                {
                    queue.Enqueue(following);
                }
            }

            var result = visited.ToList();
            result.Sort(System.StringComparer.Ordinal);
            return result;
        }
    }
}
