using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Juli_ProgLang.Content.AST
{
    internal class AST_VariableCall : AbstractSyntaxTree
    {
        public VariableCallAction VariableCallAction;
        public string Name;
        
        public AST_VariableCall(VariableCallAction variableCallAction, string name)
        {
            this.VariableCallAction = variableCallAction;
            this.Name = name;
        }
    }
    public enum VariableCallAction
    {
        Change, Read
    }
}
