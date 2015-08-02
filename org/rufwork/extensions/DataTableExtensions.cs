using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace org.rufwork.extensions
{
    public static class DataTableExtensions
    {
        /// <summary>
        /// This method will take the original DataTable and return another
        /// DataTable with the as-specified first `n` rows from the original in it.
        /// Note that the original DataTable needs to, obviously, already be sorted.
        /// TODO: There's got to be a better way to do this, especially if we weren't
        /// simply taking the first N, but WHERE-ing somehow.
        /// </summary>
        /// <param name="t">The `this` Datatable</param>
        /// <param name="n">The number of rows to take.</param>
        /// <returns></returns>
        public static DataTable SelectTopNRows(this DataTable t, int n)
        {
            DataRow[] aRows = t.Select("");
            DataTable tableRet = t.Clone();

            for (int i = 0; i < aRows.Length; i++)
            {
                if (i >= n) break;
                tableRet.Rows.Add(aRows[i].ItemArray);
            }

            return tableRet;
        }
    }
}
