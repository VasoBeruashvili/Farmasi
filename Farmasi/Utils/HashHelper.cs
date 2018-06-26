using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Farmasi.Utils
{
    public static class HashHelper
    {
        public static string Calc(string src)
        {
            SHA256Managed m = new SHA256Managed();

            byte[] b = Encoding.UTF8.GetBytes(src);

            byte[] hash = m.ComputeHash(b);

            string r = string.Empty;

            foreach (byte l in hash)
            {
                r += string.Format("{0:x2}", l);
            }

            return r;
        }
    }
}