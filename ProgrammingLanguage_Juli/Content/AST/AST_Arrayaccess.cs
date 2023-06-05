using Juli_ProgLang;
using Juli_ProgLang.Content.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgrammingLanguage_Juli.Content.AST
{
    internal class AST_Arrayaccess : AbstractSyntaxTree
    {
        public readonly dynamic Start;
        public readonly dynamic End;
        public readonly string VariableName;
        public VariableCallAction VariableCallAction = VariableCallAction.Read;

        public AST_Arrayaccess(string variableName, dynamic start, dynamic end)
        {
            VariableName = variableName;
            Start = start;
            End = end;
        }
    }
}
