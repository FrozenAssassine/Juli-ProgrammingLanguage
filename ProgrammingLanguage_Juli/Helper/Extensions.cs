using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgrammingLanguage_Juli.Helper
{
    internal static class Extensions
    {
        public static int TryToInt(this object value, string exceptionMessage)
        {
            if(int.TryParse(value.ToString(), out int res))
                return res;
            throw new Exception(exceptionMessage);
        }
    }
}
