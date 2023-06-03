using Juli_ProgLang.Content;
using Juli_ProgLang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Globalization;

namespace ProgrammingLanguage_Juli.Content.AST
{
    internal class AST_Float : AbstractSyntaxTree
    {
        public Token Token;
        public float Value;

        private float ReturnNumber(string input)
        {
            try
            {
                return (float)Convert.ToDecimal(input, CultureInfo.GetCultureInfo("en-US"));
            }
            catch
            {
                throw new ArgumentException("Input string is not a valid number");
            }
        }

        public AST_Float(Token token)
        {
            this.Token = token;
            this.Value = ReturnNumber(token.Value.ToString());
        }
    }
}
