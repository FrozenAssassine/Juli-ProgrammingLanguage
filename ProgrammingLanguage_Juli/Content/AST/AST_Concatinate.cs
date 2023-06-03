using Juli_ProgLang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgrammingLanguage_Juli.Content.AST
{
    internal class AST_Concatinate : AbstractSyntaxTree
    {
        public readonly AbstractSyntaxTree Item1;
        public readonly AbstractSyntaxTree Item2;

        public AST_Concatinate(AbstractSyntaxTree item1, AbstractSyntaxTree item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }
}
