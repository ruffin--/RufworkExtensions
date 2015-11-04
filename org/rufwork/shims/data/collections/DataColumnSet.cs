using System;
using System.Collections.Generic;
using org.rufwork.collections;
using org.rufwork.shims.data;

namespace org.rufwork.shims.data.collections
{
    public class DataColumnSet : DictionaryBackedSet<DataColumn>
    {
        public bool Contains(string strName)
        {
            bool foundName = false;
            foreach (KeyValuePair<object, DataColumn> kvp in dict)
            {
                if (strName.Equals((DataColumn)(object)kvp.Value.ToString()))
                {
                    foundName = true;
                    break;
                }
            }
            return foundName;
        }

        public new bool Add(DataColumn item)
        {
            bool bReturn = false;

            if (!this.Contains(item))
            {
                dict.Add(item.ColumnName, item);
                bReturn = true;
            }

            return bReturn;
        }

        public DataColumn this [string strName] {
            get {
                DataColumn t2ret = null;
                bool foundCol = false;

                foreach (DataColumn col in this) {
                    DataColumn tempCol = (DataColumn)(object)col;
                    if (tempCol.ColumnName == strName) {
                        t2ret = col;
                        foundCol = true;
                        break;
                    }
                }
                if (!foundCol)
                    throw new Exception ("Column " + strName + " was not found in this collection.");
                return t2ret;
            }
        }
    }
}

