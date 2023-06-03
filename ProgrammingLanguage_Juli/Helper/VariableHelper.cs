using Juli_ProgLang.Content;
using Juli_ProgLang.Content.AST;
using ProgrammingLanguage_Juli.Content;
using ProgrammingLanguage_Juli.Content.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Juli_ProgLang.Helper
{
    internal class VariableHelper
    {
        public static object DetectAndParseToDatatype(object variable)
        {
            var datatype = DetectDataType(variable as AbstractSyntaxTree);

            return ParseToDatatype(variable, datatype);
        }
        public static bool IsNumber(AbstractSyntaxTree node)
        {
            return node is AST_Float || node is AST_Integer;
        }

        public static dynamic GetNumberFromAST(AbstractSyntaxTree node)
        {
            if (node is AST_Integer ast_int)
                return ast_int.Value;
            else if (node is AST_Float ast_float)
                return ast_float.Value;
            throw new Exception($"Could not get number from {node}");
        }

        public static object ParseToDatatype(object variable, VariableDataType datatype)
        {
            switch (datatype)
            {
                case VariableDataType.String: return (variable as AST_String).Value;
                case VariableDataType.Float: return (variable as AST_Float).Value;
                case VariableDataType.Integer: return (variable as AST_Integer).Value;
                case VariableDataType.Bool: return (variable as AST_Bool).Value;
            }
            throw new Exception("Could not parse " + variable + " to datatype " + datatype);
        }

        public static VariableDataType DetectDataType(AbstractSyntaxTree data)
        {
            if (data is AST_String)
                return VariableDataType.String;
            else if (data is AST_Float)
                return VariableDataType.Float;
            else if (data is AST_Integer)
                return VariableDataType.Integer;
            else if (data is AST_Bool)
                return VariableDataType.Bool;
            return VariableDataType.None;
        }
        public static bool IsIteratable(string variable_name, Dictionary<string, IVariable> variables)
        {
            if (variables.TryGetValue(variable_name, out IVariable variable))
            {
                if (variable is ArrayVariable)
                    return true;
                else if (variable is ScalarVariable scalar_var)
                    return scalar_var.VariableDataType == VariableDataType.String;
            }
            return false;
        }

        public static VariableDataType DetectArrayDatatype(AbstractSyntaxTree[] items)
        {
            //update this to check not only the first, but all items:

            return DetectDataType(items[0]);
        }

        public static VariableDataType DetectDataType(object value)
        {
            if (value is string)
                return VariableDataType.String;
            else if (value is int)
                return VariableDataType.Integer;
            else if (value is float)
                return VariableDataType.Float;
            else if (value is bool)
                return VariableDataType.Bool;
            throw new Exception("Datatype of variable could not be determinated: " + value);
        }
        public static VariableDataType DetectDataType(Token token)
        {
            if (token.Type is Identifiers identifier)
            {
                if (identifier == Identifiers.String)
                    return VariableDataType.String;
                if (identifier == Identifiers.Integer)
                    return VariableDataType.Integer;
                if (identifier == Identifiers.Float)
                    return VariableDataType.Float;
                if (identifier == Identifiers.Bool)
                    return VariableDataType.Bool;
            }
            throw new Exception("Datatype of variable could not be determinated: " + token.Type);
        }

    }
    public enum VariableDataType
    {
        Float, String, Bool, Integer, None
    }
}
