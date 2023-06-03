namespace Juli_ProgLang.Content.AST
{
    internal class AST_String : AbstractSyntaxTree
    {
        public readonly Token Token;
        public readonly string Value = null;

        public AST_String(Token token)
        {
            Token = token;
            if(token.Value != null)
                Value = token.Value.ToString();
        }
    }
}
