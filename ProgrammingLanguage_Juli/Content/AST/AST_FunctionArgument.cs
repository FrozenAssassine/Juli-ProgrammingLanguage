using Juli_ProgLang;
using Juli_ProgLang.Content.AST;
using Juli_ProgLang.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgrammingLanguage_Juli.Content.AST
{
    internal class AST_FunctionArgument : AbstractSyntaxTree
    {
        public readonly string Name;
        public readonly VariableDataType Type;

        public AST_FunctionArgument(string name, VariableDataType type)
        {
            Name = name;
            Type = type;
        }
    }
}
