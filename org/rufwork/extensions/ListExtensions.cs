﻿// =========================== LICENSE ===============================
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
// ======================== EO LICENSE ===============================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.rufwork.extensions
{
    public static class ListExtensions
    {
        public static void AddIfNotContains<T>(this List<T> l, T tToAdd)
        {
            if (!l.Contains(tToAdd))
                l.Add(tToAdd);
        }

        public static bool EqualsXList<T>(this IList<T> listA, IList<T> listB)
        {
            return listA.Count.Equals(listB.Count)
                && 0 == listA.Where(o => !listB.Contains(o)).Count()
                && 0 == listB.Where(o => !listA.Contains(o)).Count();
        }
    }
}
