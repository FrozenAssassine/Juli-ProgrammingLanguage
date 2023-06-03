using Juli_ProgLang.Content;
using Juli_ProgLang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgrammingLanguage_Juli.Content.AST
{
    internal class AST_Integer : AbstractSyntaxTree
    {
        public Token Token;
        public int Value;
        public bool IsValue = true; //either the integer is used as a value or as a information for a function

        private int ReturnNumber(string input)
        {
            if (int.TryParse(input, out int result))
                return result;
            else
                throw new ArgumentException("Input string is not a valid number");
        }

        public AST_Integer(Token token)
        {
            this.Token = token;
            if (token.Value != null)
                this.Value = ReturnNumber(token.Value.ToString());
            else
                IsValue = false;
        }
    }
}
