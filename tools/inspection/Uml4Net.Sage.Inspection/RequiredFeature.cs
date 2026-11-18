// SPDX-FileCopyrightText: 2026 Starion Group S.A.
// SPDX-License-Identifier: Apache-2.0

namespace Uml4Net.Sage.Inspection
{
    /// <summary>
    /// A feature a metaclass requires a value for: non-derived, with a lower bound of at least 1 and no default value.
    /// </summary>
    /// <param name="Name">The feature's name, e.g. <c>general</c>.</param>
    /// <param name="OwnerName">The simple name of the metaclass that declares it.</param>
    /// <param name="Lower">The lower bound.</param>
    /// <param name="Upper">The upper bound (<c>*</c> for unbounded).</param>
    public sealed record RequiredFeature(string Name, string OwnerName, int Lower, string Upper)
    {
        /// <summary>
        /// Gets <see cref="Upper"/> as a number, <see cref="int.MaxValue"/> when it is unbounded (<c>*</c>).
        /// </summary>
        public int UpperBound => int.TryParse(this.Upper, out var upper) ? upper : int.MaxValue;
    }
}
