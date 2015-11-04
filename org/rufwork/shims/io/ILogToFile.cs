using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.rufwork.shims.io
{
    public interface ILogToFile
    {
        void AppendAllText(string path, string text);
        // TODO: Make sure the path exists.
    }
}
