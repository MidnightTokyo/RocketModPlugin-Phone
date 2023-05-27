using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Phone
{
    internal class Utils
    {
        internal static readonly string DirPath = Path.Combine(Directory.GetCurrentDirectory(), $"Plugins/{Assembly.GetExecutingAssembly().GetName().Name}/");

        internal static string SavingPath(string filename)
        {
            return Path.Combine(DirPath, filename);
        }

        internal static string NumberToString(ulong number, ulong max_number)
        {
            string num = number.ToString();
            string result = "";
            ulong i = max_number;
            while (i > 0) 
            {
                i = i / 10;
                if (number < i)
                {
                    result += "0";
                }
            }

            result += num;
            return result;
        }
    }
}
