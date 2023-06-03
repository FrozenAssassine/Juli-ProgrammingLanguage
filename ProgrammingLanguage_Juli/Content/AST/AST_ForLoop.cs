using Juli_ProgLang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgrammingLanguage_Juli.Content.AST
{
    internal class AST_ForLoop : AbstractSyntaxTree
    {
        public readonly AbstractSyntaxTree[] SubItems;
        public readonly string IterationVariableName;
        public readonly AbstractSyntaxTree IterationOperator;

        public AST_ForLoop(string iterationVariableName, AbstractSyntaxTree[] SubItems, AbstractSyntaxTree iterationOperator)
        {
            this.IterationVariableName = iterationVariableName;
            this.SubItems = SubItems;
            this.IterationOperator = iterationOperator;
        }
    }
}
