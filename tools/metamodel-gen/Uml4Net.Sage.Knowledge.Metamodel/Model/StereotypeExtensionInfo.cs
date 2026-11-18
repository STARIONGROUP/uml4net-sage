// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Knowledge.Metamodel.Model
{
    /// <summary>
    /// One metaclass a stereotype extends, read from the stereotype's own <c>Extension</c>.
    /// </summary>
    /// <param name="Metaclass">The simple name of the extended metaclass.</param>
    /// <param name="IsRequired">
    /// Whether every instance of the metaclass must carry the stereotype (UML <c>Extension::isRequired</c>, i.e. the
    /// extension end's lower bound is 1).
    /// </param>
    public sealed record StereotypeExtensionInfo(string Metaclass, bool IsRequired);
}
