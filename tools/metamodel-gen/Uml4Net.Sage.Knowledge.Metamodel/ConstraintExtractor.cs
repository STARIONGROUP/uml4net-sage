// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Metamodel
{
    using System.Collections.Generic;

    using uml4net.Classification;
    using uml4net.CommonStructure;
    using uml4net.Values;

    using Uml4Net.Sage.Knowledge.Metamodel.Model;

    /// <summary>
    /// Extracts <c>ownedRule</c> constraints from a namespace, and <c>bodyCondition</c> constraints from an
    /// operation (the MODEL provenance tier: read directly from the metamodel XMI, not the specification text).
    /// </summary>
    public static class ConstraintExtractor
    {
        /// <summary>
        /// Converts every owned constraint whose specification is an <see cref="IOpaqueExpression"/>.
        /// </summary>
        /// <param name="namespace">The namespace whose <c>ownedRule</c> constraints are read.</param>
        /// <returns>One <see cref="ConstraintInfo"/> per opaque-expression specification, in declaration order.</returns>
        public static IReadOnlyList<ConstraintInfo> FromNamespace(INamespace @namespace)
        {
            return FromConstraints(@namespace.OwnedRule);
        }

        /// <summary>
        /// Converts a query operation's <c>bodyCondition</c> - the OCL specification that backs a derived
        /// property's accessor, e.g. <c>Connector::kind()</c> or <c>Message::messageKind()</c>.
        /// </summary>
        /// <param name="operation">The operation whose <c>bodyCondition</c> constraints are read.</param>
        /// <returns>One <see cref="ConstraintInfo"/> per opaque-expression specification, in declaration order.</returns>
        public static IReadOnlyList<ConstraintInfo> FromOperationBody(IOperation operation)
        {
            return FromConstraints(operation.BodyCondition);
        }

        /// <summary>
        /// Converts each <see cref="IOpaqueExpression"/> specification of <paramref name="constraints"/> to a
        /// <see cref="ConstraintInfo"/>. Specifications of any other kind are ignored.
        /// </summary>
        /// <param name="constraints">The constraints to convert.</param>
        /// <returns>The converted constraints, in declaration order.</returns>
        private static IReadOnlyList<ConstraintInfo> FromConstraints(IEnumerable<IConstraint> constraints)
        {
            var result = new List<ConstraintInfo>();

            foreach (var constraint in constraints)
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
