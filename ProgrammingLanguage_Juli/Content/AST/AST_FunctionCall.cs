namespace Juli_ProgLang.Content.AST
{
    internal class AST_FunctionCall : AbstractSyntaxTree
    {
        public readonly string Name;
        public readonly AbstractSyntaxTree[] Parameter;

        public AST_FunctionCall(string Name, AbstractSyntaxTree[] parameter)
        {
            this.Name = Name;
            this.Parameter = parameter;
        }
    }
}
