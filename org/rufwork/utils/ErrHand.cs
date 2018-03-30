// =========================== LICENSE ===============================
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
// ======================== EO LICENSE ===============================

using System;

namespace org.rufwork.utils
{
    // Kinda nervous putting this in an "Extensions" project, but I think it works, since
    // we might want to handle errors there too. I do want a standardized location for all
    // error handling, and this is the only project that really works like that.
    // TODO: Create an interface and inject something to write logs.
    public static class ErrHand
    {
        public static string StrLogFileHome = "";

        public static void Log(this Exception e, string strLocation, string strAddlInfo = "")
        {
            ErrHand.LogErr(e, strLocation, strAddlInfo);
        }

        public static void LogErr(string strErrMsg, string strLocation, string strAddlInfo = "")
        {
            if (!string.IsNullOrWhiteSpace(strAddlInfo))
            {
                strErrMsg += "\t" + strAddlInfo;
            }
            ErrHand.LogMsg(strErrMsg, strLocation);

            DateTime now = DateTime.Now;
            string strErrLogMsg = now.ToString("yyyy-MM-dd HH:mm:ss.fff\t") + "Error in " + strLocation + "\n\t" + strErrMsg;
            Logger.LogIt(strErrLogMsg, ErrHand.StrLogFileHome);
        }

        public static void Stop()
        {
            System.Diagnostics.Debugger.Break();
        }

        public static void LogErr(Exception e, string strLocation, string strAddlInfo = "")
        {
            ErrHand.LogErr(e.Message + "\n\t" + e.StackTrace, strLocation, strAddlInfo);
        }

        public static void LogMsg(string strMsg, string strLocation = "")
        {
            string message = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")  + ": " + strMsg;

            if (!string.IsNullOrWhiteSpace(strLocation))
            {
                message += "\t" + strLocation;
            }

            Logger.LogIt(message, ErrHand.StrLogFileHome);
        }
    }
}
