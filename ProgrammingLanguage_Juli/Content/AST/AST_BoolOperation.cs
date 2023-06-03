using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Juli_ProgLang.Content.AST
{
    internal class AST_BoolOperation : AbstractSyntaxTree
    {
        public readonly BoolOperation BoolOperation;
        public AST_BoolOperation(BoolOperation boolOperation)
        {
            this.BoolOperation = boolOperation;
        }
    }
    public enum BoolOperation
    {
        Equals, And, Or, Not, NotEquals, Greater, Smaller, GreaterEquals, SmallerEquals
    }
}
