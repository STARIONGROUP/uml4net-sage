// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Metamodel
{
    using uml4net.CommonStructure;

    /// <summary>
    /// Naming helpers shared by the extractors and generators.
    /// </summary>
    public static class ElementNames
    {
        /// <summary>
        /// Gets the qualified name of <paramref name="element"/>'s owning namespace (e.g. the package a metaclass
        /// lives in, or the class that owns a feature), or <see cref="string.Empty"/> when it has none - or, as of
        /// uml4net.xmi 9.0.0, when that namespace's qualified name is undefined because it or one of its own
        /// namespaces is unnamed.
        /// </summary>
        /// <param name="element">The element whose namespace is queried.</param>
        /// <returns>The namespace's qualified name, or <see cref="string.Empty"/>.</returns>
        public static string NamespaceOf(INamedElement element)
        {
            return (element.Namespace as INamedElement)?.QualifiedName ?? string.Empty;
        }
    }
}
