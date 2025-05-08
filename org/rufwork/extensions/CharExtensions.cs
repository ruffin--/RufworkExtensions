// =========================== LICENSE ===============================
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
// ======================== EO LICENSE ===============================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.rufwork.extensions
{
    public static class CharExtensions
    {
        public static bool EqualsCaseInsensitive(this char chrOrig, char chrCompare)
        {
            return char.ToUpperInvariant(chrOrig).Equals(char.ToUpperInvariant(chrCompare));
        }

        // I'm not going to pretend this treats each StringComparison type correctly.
        // Basically if it's IgnoreCase, we'll compare Upper to Upper.
        public static bool EqualsWithComparisonType(this char chrOrig, char chrCompare, StringComparison stringComparison)
        {
            switch (stringComparison)
            {
                case StringComparison.CurrentCultureIgnoreCase:
                //case StringComparison.InvariantCultureIgnoreCase: // Not valid in PCL, apparently.
                case StringComparison.OrdinalIgnoreCase:
                    return chrOrig.EqualsCaseInsensitive(chrCompare);

                default:
                    return chrOrig.Equals(chrCompare);
            }
        }

        public static bool ContainsCaseInsensitive(this IEnumerable<char> chars, char chrContains)
        {
            return chars.Any(c => c.EqualsCaseInsensitive(chrContains));
        }
    }
}
