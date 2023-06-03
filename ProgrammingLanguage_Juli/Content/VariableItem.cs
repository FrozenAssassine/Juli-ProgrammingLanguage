using Juli_ProgLang.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgrammingLanguage_Juli.Content
{
    internal interface IVariable
    {
        VariableDataType VariableDataType { get; set; }
        object Value { get; set; }
        int CurlyBracketDepth { get; set; }
    }

    internal class ArrayVariable : IVariable
    {
        public ArrayVariable(VariableDataType datatype, object value, int curlyBracketDepth = 0)
        {
            VariableDataType = datatype;
            Value = value;
            CurlyBracketDepth = curlyBracketDepth;
        }
        public VariableDataType VariableDataType { get; set; }
        public object Value { get; set; }
        public int CurlyBracketDepth { get; set; }
    }
    internal class ScalarVariable : IVariable
    {
        public ScalarVariable(VariableDataType datatype, object value, int curlyBracketDepth = 0)
        {
            VariableDataType = datatype;
            Value = value;
            CurlyBracketDepth = curlyBracketDepth;
        }

        public VariableDataType VariableDataType { get; set; }
        public object Value { get; set; }
        public int CurlyBracketDepth { get; set; }
    }
}
