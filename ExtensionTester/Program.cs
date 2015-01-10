using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using org.rufwork.extensions;
using System.IO;

namespace ExtensionTester
{
    class Program
    {
        static void Main(string[] args)
        {
            string hash = "beau@dentedreality.com.au".Md5Hash();
            Console.WriteLine (hash);

            string strContents = @"<li><p>spam</p></li>
                <p>Lauren Dragan&#8217;s guide at The Wirecutter, <a href=""http://thewirecutter.com/reviews/best-300ish-headphone/"">The Best $300 Over-Ear Headphones</a>, 
                was updated yesterday, the same day I published my <a href=""http://www.marco.org/headphones-closed-portable"">similar mega-review</a>.
                <sup id=""fnref:pCv7cBGCg1""><a href=""#fn:pCv7cBGCg1"" rel=""footnote"">1</a></sup></p> 
                <p>We agree on a lot: our opinions are very close on the PSB M4U 1 (their top pick), B&amp;O H6, and B&amp;W P7. We only have a few major disagreements:</p> 
                <p>They dismissed my top pick, the AKG K551, as being &#8220;tinny&#8221; and bass-light, but did not test it this time. They didn&#8217;t mention the newer AKG K545 at all, which is unfortunate since it improves on the K551 in some key areas.</p>
                <hr /> <ol> <li id=""fn:pCv7cBGCg1""> <p>For the same reason: the Blue Mo-Fi embargo lifted yesterday.&#160;<a href=""#fnref:pCv7cBGCg1"" rev=""footnote"">&#8617;</a></p> </li> </ol>";

            strContents = strContents.StripHTML();
            Console.Write(strContents.Replace('\r','\n'));
            Console.WriteLine();

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
