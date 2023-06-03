using Juli_ProgLang.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgrammingLanguage_Juli.Content
{
    internal class MathNumber
    {
        public object value { get; set; }
        public VariableDataType datatype { get; set; }
        
        public MathNumber(object value, VariableDataType datatype)
        {
            this.value = value;
            this.datatype = datatype;
        }
    }
}
