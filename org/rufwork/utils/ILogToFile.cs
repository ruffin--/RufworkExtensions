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

namespace org.rufwork.utils
{
    public interface ILogToFile
    {
        void AppendAllText(string path, string message);
        // Needs to check that log loc exists, etc.
        //if (!string.IsNullOrWhiteSpace(ErrHand.StrLogFileHome) && System.IO.Directory.Exists(ErrHand.StrLogFileHome))
        //{
        //    DateTime now = DateTime.Now;
        //    string strErrFileLoc = System.IO.Path.Combine(ErrHand.StrLogFileHome, now.ToString("yyyy-MM-dd"));
        //    System.IO.File.AppendAllText(strErrFileLoc, now.ToString("yyyy-MM-dd HH:mm:ss.fff\t") + "Error in " + strLocation + "\n\t" + strErrMsg);
        //}

}
}
