using Juli_ProgLang;

namespace ProgrammingLanguage_Juli.Content.AST
{
    internal class AST_Len : AbstractSyntaxTree
    {
        public readonly AbstractSyntaxTree Variable;
        public AST_Len(AbstractSyntaxTree variable)
        {
            this.Variable = variable;
        }
    }
}
