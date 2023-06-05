using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Juli_ProgLang.Content.AST
{
    internal class AST_If : AbstractSyntaxTree
    {
        public readonly AbstractSyntaxTree[] Condition;
        public readonly AbstractSyntaxTree[] SubItems;
        public AST_If(AbstractSyntaxTree[] condition, AbstractSyntaxTree[] subitems)
        {
            this.SubItems = subitems;
            this.Condition = condition;
        }
    }
}
