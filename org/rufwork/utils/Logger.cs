using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.rufwork.utils
{
    public static class Logger
    {
        public static void WriteLine(string strMsg)
        {
            System.Diagnostics.Debug.WriteLine(strMsg);
        }
    }
}
