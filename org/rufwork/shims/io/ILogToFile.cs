namespace org.rufwork.shims.io
{
    public interface ILogToFile
    {
        void AppendAllText(string path, string text);
    }

    public class DebugLogWriter : ILogToFile
    {
        public void AppendAllText(string path, string text)
        {
            System.Diagnostics.Debug.WriteLine(text);
        }
    }

}
