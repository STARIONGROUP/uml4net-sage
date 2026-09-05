// -------------------------------------------------------------------------------------------------
// <copyright file="ElementCatalog.cs" company="Starion Group S.A.">
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

namespace Uml4Net.Codex.MetamodelGen
{
    using System.Collections.Generic;

    using uml4net.Classification;
    using uml4net.Packages;
    using uml4net.SimpleClassifiers;
    using uml4net.StructuredClassifiers;
    using uml4net.xmi.Readers;

    /// <summary>
    /// Discovers every metamodel element of interest by walking a read model, rather than relying on a
    /// hardcoded list of metaclass names.
    /// </summary>
    /// <param name="Classes">Every <see cref="IClass"/> found in the UML metamodel (excludes Stereotypes).</param>
    /// <param name="Enumerations">Every <see cref="IEnumeration"/> found in the UML metamodel.</param>
    /// <param name="PrimitiveTypes">Every <see cref="IPrimitiveType"/> found in the UML metamodel or PrimitiveTypes library.</param>
    /// <param name="Stereotypes">Every <see cref="IStereotype"/> found in the Standard Profile.</param>
    public sealed record ElementCatalog(
        IReadOnlyList<IClass> Classes,
        IReadOnlyList<IEnumeration> Enumerations,
        IReadOnlyList<IPrimitiveType> PrimitiveTypes,
        IReadOnlyList<IStereotype> Stereotypes)
    {
        /// <summary>
        /// Builds a catalog from the result of reading UML.xmi (which pulls in PrimitiveTypes.xmi via its own
        /// external references) and, separately, StandardProfile.xmi.
        /// </summary>
        /// <param name="umlModel">
        /// The result of reading UML.xmi.
        /// </param>
        /// <param name="standardProfileModel">
        /// The result of reading StandardProfile.xmi.
        /// </param>
        /// <returns>
        /// A populated <see cref="ElementCatalog"/>.
        /// </returns>
        public static ElementCatalog Build(XmiReaderResult umlModel, XmiReaderResult standardProfileModel)
        {
            var classes = new List<IClass>();
            var enumerations = new List<IEnumeration>();
            var primitiveTypes = new List<IPrimitiveType>();

            foreach (var package in umlModel.Packages)
            {
                WalkMetamodel(package, classes, enumerations, primitiveTypes);
            }

            var stereotypes = new List<IStereotype>();
            foreach (var package in standardProfileModel.Packages)
            {
                WalkStereotypes(package, stereotypes);
            }

            return new ElementCatalog(classes, enumerations, primitiveTypes, stereotypes);
        }

        private static void WalkMetamodel(IPackage package, List<IClass> classes, List<IEnumeration> enumerations, List<IPrimitiveType> primitiveTypes)
        {
            foreach (var element in package.PackagedElement)
            {
                switch (element)
                {
                    case IPackage nested:
                        WalkMetamodel(nested, classes, enumerations, primitiveTypes);
                        break;
                    case IStereotype:
                        // Stereotypes are catalogued separately from the Standard Profile package, even
                        // though IStereotype : IClass - a stereotype is not a metaclass.
                        break;
                    case IEnumeration enumeration:
                        enumerations.Add(enumeration);
                        break;
                    case IPrimitiveType primitiveType:
                        primitiveTypes.Add(primitiveType);
                        break;
                    case IClass @class:
                        classes.Add(@class);
                        break;
                }
            }
        }

        private static void WalkStereotypes(IPackage package, List<IStereotype> stereotypes)
        {
            foreach (var element in package.PackagedElement)
            {
                if (element is IPackage nested)
                {
                    WalkStereotypes(nested, stereotypes);
                }
                else if (element is IStereotype stereotype)
                {
                    stereotypes.Add(stereotype);
                }
            }
        }
    }
}
