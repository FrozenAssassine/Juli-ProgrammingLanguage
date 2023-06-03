using Juli_ProgLang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgrammingLanguage_Juli.Content.AST
{
    internal class AST_Return : AbstractSyntaxTree
    {
        public AbstractSyntaxTree[] SubItems;
        public AST_Return(AbstractSyntaxTree[] subItems)
        {
            this.SubItems = subItems;
        }

    }
}
