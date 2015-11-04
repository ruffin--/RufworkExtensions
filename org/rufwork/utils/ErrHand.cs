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

using org.rufwork.shims.io;

namespace org.rufwork.utils
{
    // Kinda nervous putting this in an "Extensions" project, but I think it works, since
    // we might want to handle errors there too. I do want a standardized location for all
    // error handling, and this is the only project that really works like that.
    public class ErrHand
    {
        public static string StrLogFileHome = "";
        public static ILogToFile Logger = null;

        public static void LogErr(string strErrMsg, string strLocation, string strAddlInfo = "")
        {
            if (!string.IsNullOrWhiteSpace(strAddlInfo))
            {
                strErrMsg += "\t" + strAddlInfo;
            }
            ErrHand.LogMsg(strErrMsg, strLocation);

            DateTime now = DateTime.Now;
            string strErrLogMsg = now.ToString("yyyy-MM-dd HH:mm:ss.fff\t") + "Error in " + strLocation + "\n\t" + strErrMsg;
            ErrHand.Logger.AppendAllText(ErrHand.StrLogFileHome, strErrLogMsg);
        }

        public static void LogErr(Exception e, string strLocation, string strAddlInfo = "")
        {
            ErrHand.LogErr(e.ToString(), strLocation, strAddlInfo);
        }

        public static void LogMsg(string strMsg, string strLocation = "")
        {
            ErrHand.LogMsg(strMsg);

            if (!string.IsNullOrWhiteSpace(strLocation))
            {
                ErrHand.LogMsg("\t" + strLocation);
            }
        }
    }
}
