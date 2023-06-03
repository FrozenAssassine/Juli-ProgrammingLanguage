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
        public readonly int Start;
        public readonly int End;
        
        public AST_Range(int start, int end)
        {
            this.Start = start;
            this.End = end;
        }
    }
}
