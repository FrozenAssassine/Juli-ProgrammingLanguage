using Juli_ProgLang.Content;
using Juli_ProgLang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgrammingLanguage_Juli.Content.AST
{
    internal class AST_Bool : AbstractSyntaxTree
    {
        public Token Token;
        public bool? Value = null; //only null when it is used as: function(variable : bool)

        public AST_Bool(Token token)
        {
            Token = token;
            if(token.Value != null)
                Value = bool.Parse(token.Value.ToString());
        }
    }
}
