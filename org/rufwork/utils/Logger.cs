using org.rufwork.shims.io;

namespace org.rufwork.utils
{
    public static class Logger
    {
        public static ILogToFile LogWriter = null;
        public static string LogPath = string.Empty;

        public static void WriteLine(string strMsg)
        {
            System.Diagnostics.Debug.WriteLine(strMsg);
        }

        public static void LogIt(string strMsg, string filePath = "", bool alsoWriteLine = true)
        {
            if (Logger.LogWriter != null && !string.IsNullOrWhiteSpace(filePath))
            {
                Logger.LogWriter.AppendAllText(strMsg, strMsg);
            }

            if (alsoWriteLine)
            {
                Logger.WriteLine(strMsg);
            }
        }
    }
}
