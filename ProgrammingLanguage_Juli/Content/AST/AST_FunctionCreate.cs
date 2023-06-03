using Juli_ProgLang;
using Juli_ProgLang.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgrammingLanguage_Juli.Content.AST
{
    internal class AST_FunctionCreate : AbstractSyntaxTree
    {
        public readonly AbstractSyntaxTree[] Arguments;
        public readonly AbstractSyntaxTree[] Actions;
        public readonly string FunctionName;
        public AbstractSyntaxTree ReturnType;
        public AST_FunctionCreate(string functionName, AbstractSyntaxTree returnType, AbstractSyntaxTree[] arguments, AbstractSyntaxTree[] actions)
        {
            this.FunctionName = functionName;
            this.Arguments = arguments;
            this.Actions = actions;
            this.ReturnType = returnType;
        }
    }
}
