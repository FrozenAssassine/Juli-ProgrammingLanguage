using Juli_ProgLang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgrammingLanguage_Juli.Content.AST
{
    internal class AST_BracketChanged : AbstractSyntaxTree
    {
        public readonly BracketDepth currentDepth;
        public AST_BracketChanged(BracketDepth currentDepth)
        {
            this.currentDepth = currentDepth;
        }
    }
}
