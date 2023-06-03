using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Juli_ProgLang.Content.AST
{
    internal class AST_MathOperation : AbstractSyntaxTree
    {
        public MathOperation MathOperation;

        public AST_MathOperation(MathOperation mathOperation)
        {
            this.MathOperation = mathOperation;
        }
    }
    public enum MathOperation
    {
        Add, Subtract, Multiply, Divide, LeftParen, RightParen, Modulo
    }
}
