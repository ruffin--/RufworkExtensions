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

namespace com.rufwork.utils
{
    // Kinda nervous putting this in an "Extensions" project, but I think it works, since
    // we might want to handle errors there too. I do want a standardized location for all
    // error handling, and this is the only project that really works like that.
    public class ErrHand
    {
        public static string StrLogFileHome = "";

        public static void LogErr(string strErrMsg, string strLocation, string strAddlInfo = "")
        {
            string strErr = "Error in " + strLocation + "\n\t" + strErrMsg;

            if (!string.IsNullOrWhiteSpace(strAddlInfo))
            {
                strErr += "\t" + strAddlInfo;
            }

            ErrHand.logMsg(strErr);

            // Check if we have a directory set up for logging errors.
            if (!string.IsNullOrWhiteSpace(ErrHand.StrLogFileHome) && System.IO.Directory.Exists(ErrHand.StrLogFileHome))
            {
                DateTime now = DateTime.Now;
                string strErrFileLoc = System.IO.Path.Combine(ErrHand.StrLogFileHome, now.ToString("yyyy-MM-dd"));
                System.IO.File.AppendAllText(strErrFileLoc, now.ToString("yyyy-MM-dd HH:mm:ss.fff\t" + strErr));
            }
        }

        public static void LogErr(Exception e, string strLocation, string strAddlInfo = "")
        {
            ErrHand.LogErr(e.ToString(), strLocation, strAddlInfo);
        }

        public static void LogMsg(string strMsg, string strLocation = "")
        {
            ErrHand.logMsg(strMsg);

            if (!string.IsNullOrWhiteSpace(strLocation))
            {
                ErrHand.logMsg("\t" + strLocation);
            }
        }

        private static void logMsg(string strMessage)
        {
            System.Diagnostics.Debug.Print(strMessage);
            Console.WriteLine(strMessage);
        }
    }
}
