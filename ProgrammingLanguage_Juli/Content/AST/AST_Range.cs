using Juli_ProgLang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgrammingLanguage_Juli.Content.AST
{
    internal class AST_Range : AbstractSyntaxTree
    {
        public readonly AbstractSyntaxTree Start;
        public readonly AbstractSyntaxTree End;
        
        public AST_Range(AbstractSyntaxTree start, AbstractSyntaxTree end)
        {
            this.Start = start;
            this.End = end;
        }
    }
}
