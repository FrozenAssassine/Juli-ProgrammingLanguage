using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Juli_ProgLang.Content.AST
{
    internal class AST_Else : AbstractSyntaxTree
    {
        public readonly AbstractSyntaxTree[] SubItems;
        public AST_Else(AbstractSyntaxTree[] subitems)
        {
            this.SubItems = subitems;
        }
    }
}
