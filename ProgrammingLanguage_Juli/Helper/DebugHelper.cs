using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Juli_ProgLang.Helper
{
    internal class DebugHelper
    {
        public static string DebugArray(object[] values)
        {
            string res = "";
            foreach(object value in values)
            {
                res += value?.ToString() + ", ";
            }
            return res;
        }

    }
}
