using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.rufwork.extensions
{
    public static class DateTimeExtensions
    {
        public static string ToRufString(this DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
    }
}
