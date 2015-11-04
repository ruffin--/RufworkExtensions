using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RufworkExtensionsXam.org.rufwork.extensions
{
    public static class StringExtensionsNative
    {
        // from http://msdn.microsoft.com/en-us/library/system.security.cryptography.md5(v=vs.110).aspx
        public static string Md5Hash(this string str)
        {
            throw new NotImplementedException();
            //using (MD5 md5Hash = MD5.Create())
            //{
            //    // Convert the input string to a byte array and compute the hash. 
            //    byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(str));

            //    // Create a new Stringbuilder to collect the bytes 
            //    // and create a string.
            //    StringBuilder sBuilder = new StringBuilder();

            //    // Loop through each byte of the hashed data  
            //    // and format each one as a hexadecimal string. 
            //    for (int i = 0; i < data.Length; i++)
            //    {
            //        sBuilder.Append(data[i].ToString("x2"));
            //    }

            //    // Return the hexadecimal string. 
            //    return sBuilder.ToString();
            //}
        }
    }
}
