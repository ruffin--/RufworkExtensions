using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using org.rufwork.shims.data;

namespace org.rufwork.extensions
{
    public static class DataRowArrayExtensions
    {
        public static DataTable CopyToDataTable(this DataRow[] rows)
        {
            DataTable table = new DataTable();

            if (rows.Length > 1)
            {
                foreach (DataColumn col in rows[0].Table.Columns)
                {
                    DataColumn colNew = new DataColumn(col.ColumnName);
                    colNew.DataType = col.DataType;
                    table.Columns.Add(colNew);
                }

                foreach (DataRow existingRow in rows)
                {
                    DataRow newRow = table.NewRow();
                    foreach (DataColumn dc in newRow.Table.Columns)
                    {
                        newRow[dc] = existingRow[dc.ColumnName];    // ????: I *think* I can't reuse the DataColumn here, maybe? Is dc === dc2?
                    }
                }
            }

            return table;
        }
    }
}
