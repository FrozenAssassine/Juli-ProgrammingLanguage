using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Juli_ProgLang.Content.AST
{
    internal class AST_VariableAssignment : AbstractSyntaxTree
    {
        public readonly string Name;
        public readonly AbstractSyntaxTree[] AssignItems;
        public readonly VariableType VariableType;

        public AST_VariableAssignment(AbstractSyntaxTree[] assignItems, string name, VariableType type = VariableType.Scalar)
        {
            this.AssignItems = assignItems;
            this.Name = name;
            this.VariableType = type;
        }
    }
    public enum VariableType
    {
        Scalar, Array
    }
}
