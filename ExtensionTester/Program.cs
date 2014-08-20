using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using org.rufwork.extensions;

namespace ExtensionTester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("This is a 10¢ test".CutToUTF8Length(12) + "#");
            Console.WriteLine("This is a 10¢ test".CutToUTF8Length(13) + "#");
            Console.WriteLine("This is a 10¢ test".CutToUTF8Length(14) + "#");
            Console.WriteLine("This is a 10¢ test".CutToUTF8Length(15) + "#");

            Console.WriteLine();
            Console.WriteLine("Done. Return to end.");
            Console.ReadLine();
        }
    }
}
