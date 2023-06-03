using Juli_ProgLang;

namespace ProgrammingLanguage_Juli.Content
{
    internal class FunctionItem
    {
        public readonly AbstractSyntaxTree[] Parameter;
        public readonly AbstractSyntaxTree[] Actions;
        public readonly AbstractSyntaxTree ReturnValue;

        public FunctionItem(AbstractSyntaxTree[] parameter, AbstractSyntaxTree[] actions, AbstractSyntaxTree returnValue)
        {
            Parameter = parameter;
            Actions = actions;
            ReturnValue = returnValue;
        }
    }
}
