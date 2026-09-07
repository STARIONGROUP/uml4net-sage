// -------------------------------------------------------------------------------------------------
// <copyright file="ConstraintExtractor.cs" company="Starion Group S.A.">
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

    using uml4net.CommonStructure;
    using uml4net.Values;

    using Uml4Net.Sage.MetamodelGen.Model;

    /// <summary>
    /// Extracts <c>ownedRule</c> constraints from a namespace (the MODEL provenance tier: read directly
    /// from the metamodel XMI, not the specification text).
    /// </summary>
    public static class ConstraintExtractor
    {
        /// <summary>
        /// Converts every owned constraint whose specification is an <see cref="IOpaqueExpression"/>.
        /// </summary>
        public static IReadOnlyList<ConstraintInfo> FromNamespace(INamespace @namespace)
        {
            var result = new List<ConstraintInfo>();

            foreach (var constraint in @namespace.OwnedRule)
            {
                foreach (var specification in constraint.Specification)
                {
                    if (specification is IOpaqueExpression opaqueExpression)
                    {
                        result.Add(new ConstraintInfo(constraint.Name, opaqueExpression.Language, opaqueExpression.Body));
                    }
                }
            }

            return result;
        }
    }
}
