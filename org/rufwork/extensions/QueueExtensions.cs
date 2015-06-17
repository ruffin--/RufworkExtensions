// =========================== LICENSE ===============================
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
    public static class QueueExtensions
    {
        // Kinda would've expected this to already live on Queue somewhere, but haven't found it.
        public static void EnqueueIfNotContains<T>(this Queue<T> q, T tToAdd)
        {
            if (!q.Contains(tToAdd))
                q.Enqueue(tToAdd);
        }
        public static void EnqueueIfNotContainsCaseInsensitive<T>(this Queue<T> q, T tToAdd)
        {
            if (typeof(string) == typeof(T))
            {
                string strToAdd = tToAdd.ToString();
                if (!q.Any(s => s.ToString().ToLower().Equals(strToAdd.ToLower())))
                    q.Enqueue(tToAdd);
            }
            else
            {
                q.EnqueueIfNotContains(tToAdd);
            }
        }

    }
}
