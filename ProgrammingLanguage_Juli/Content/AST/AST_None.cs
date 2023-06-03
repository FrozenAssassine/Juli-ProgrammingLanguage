using Juli_ProgLang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ProgrammingLanguage_Juli.Content.AST
{
    internal class AST_None : AbstractSyntaxTree
    {
        public string Value { get; set; } = "";
        public AST_None(string value)
        {
            this.Value = value;
        }
        public AST_None() { }
    }
}
