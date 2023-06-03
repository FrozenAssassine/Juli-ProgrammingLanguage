using Juli_ProgLang.Content.AST;
using Juli_ProgLang.Helper;
using ProgrammingLanguage_Juli;
using ProgrammingLanguage_Juli.Content;
using ProgrammingLanguage_Juli.Content.AST;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Juli_ProgLang
{
    internal class Interpreter
    {
        private Parser parser;

        public Dictionary<string, IVariable> Variables = new Dictionary<string, IVariable>();
        public Dictionary<string, FunctionItem> Functions = new Dictionary<string, FunctionItem>();

        private dynamic CalculateMath(dynamic value1, dynamic value2, MathOperation operation)
        {
            switch (operation)
            {
                case MathOperation.Add:
                    return value1 + value2;
                case MathOperation.Subtract:
                    return value1 - value2;
                case MathOperation.Multiply:
                    return value1 * value2;
                case MathOperation.Divide:
                    return value1 / value2;
                case MathOperation.Modulo:
                    return value1 % value2;
            }
            throw new Exception($"Could not calculate {value1} and {value2} with {operation}");
        }
        private (float, AbstractSyntaxTree[] lastingNodes) BreakDownMath(AbstractSyntaxTree[] nodes)
        {
            MathOperation operation = MathOperation.Add;
            dynamic result = 0;
            foreach (var node in nodes)
            {
                if (node is AST_MathOperation math_op)
                {
                    operation = math_op.MathOperation;
                }
                else if (VariableHelper.IsNumber(node))
                {
                    result = CalculateMath(result, VariableHelper.GetNumberFromAST(node), operation);
                }
                else if (node is AST_VariableCall variable_call)
                {
                    if (variable_call.VariableCallAction == VariableCallAction.Change)
                        throw new Exception("Currently not possible to change variable in assignment");
                    else
                    {
                        var variableItem = GetVariableValue(variable_call);
                        var datatype = VariableHelper.DetectDataType(variableItem);
                        if (datatype == VariableDataType.Float)
                            result = CalculateMath(result, float.Parse(variableItem.ToString()), operation);
                        else if(datatype == VariableDataType.Integer)
                            result = CalculateMath(result, int.Parse(variableItem.ToString()), operation);
                        else
                            throw new Exception($"Variable {variable_call.Name} has invalid datatype");
                    }
                }
                else
                    return (result, nodes.Skip(Array.IndexOf(nodes, node)).ToArray());
            }
            return (result, null);
        }


        private IVariable GetVariableByName(string name)
        {
            if (Variables.ContainsKey(name))
            {
                Variables.TryGetValue(name, out IVariable item);
                return item;
            }
            throw new Exception($"The variable {name} does not exist");
        }
        private object GetVariableCallValue(AST_VariableCall variable_call)
        {
            return GetVariableByName(variable_call.Name).Value;
        }
        private object GetVariableValue(AbstractSyntaxTree[] nodes)
        {
            if (nodes.Length == 1)
                return GetVariableValue(nodes[0]);

            StringBuilder output = new StringBuilder();
            if ((nodes[0] is AST_VariableCall && nodes[1] is AST_MathOperation) || VariableHelper.IsNumber(nodes[0]) && nodes[1] is AST_MathOperation)
            {
                var result = BreakDownMath(nodes);
                if (result.lastingNodes == null)
                    return result.Item1;
                else
                {
                    output.Append(result.Item1);
                    output.Append(GetVariableValue(result.lastingNodes));
                    return output.ToString();
                }
            }
            else if (nodes[0] is AST_String str && nodes[1] is AST_MathOperation math_op && math_op.MathOperation == MathOperation.Multiply && nodes[2] is AST_Integer integer)
            {
                //string repeat syntax eg. "Hello" * 10 
                return string.Join("", Enumerable.Repeat(str.Value, integer.Value));
            }
            else
            {
                foreach (var item in nodes)
                {
                    output.Append(GetVariableValue(item));
                }
                return output.ToString();
            }
            throw new Exception("Could not get value of variable -> GetVariableValue(nodes[])");
        }
        private object GetVariableValue(AbstractSyntaxTree node)
        {
            if (node is AST_VariableCall variable_call)
                return GetVariableCallValue(variable_call);
            else if (node is AST_String ast_string)
                return ast_string.Value;
            else if (node is AST_Integer ast_int)
                return ast_int.Value;
            else if (node is AST_Float ast_float)
                return ast_float.Value;
            else if (node is AST_Concatinate ast_concat)
                return GetVariableValue(ast_concat.Item2);
            else if (node is AST_Bool ast_bool)
                return ast_bool.Value;
            else if (node is AST_Arrayaccess array_access)
                return GetArrayValueByIndex(array_access);
            else if (node is AST_None ast_none)
                Console.WriteLine(ast_none.Value);
            else if (node is AST_FunctionCall function_call)
                return CallFunction(function_call);
            throw new Exception($"Could not get value of variable {node} -> GetVariableValue(node)");
        }
        private object GetArrayValueByIndex(AST_Arrayaccess array_access)
        {
            var array = GetVariableByName(array_access.VariableName).Value as AbstractSyntaxTree[];

            if (array_access.Start == array_access.End)
                return GetVariableValue(array[array_access.Start]);
            else if(array_access.End == -1)    
                return array.Skip(array_access.Start).ToArray();
            return array.Skip(array_access.Start).Take(array_access.End - array_access.Start).ToArray();
        }

        private IVariable AssignFunctionParameterVariable(string name, VariableDataType datatype, object value, int minbracketDepth)
        {
            Variables.Add(name, new ScalarVariable(datatype, value, minbracketDepth));

            Variables.TryGetValue(name, out var variable);
            return variable;
        }

        private void DeleteVariable(string name)
        {
            Debug.WriteLine("delete variable: " + name);
            Variables.Remove(name);
        }
        private void ChangeIterableVariable(string name, object newValue)
        {
            if(Variables.TryGetValue(name, out var variable))
                variable.Value = newValue;
        }
        private IVariable AssignIterableVariable(string name, VariableDataType datatype, object value, int minbracketDepth)
        {
            Variables.Add(name, new ScalarVariable(datatype, value, minbracketDepth));

            Variables.TryGetValue(name, out var variable);
            return variable;
        }
        private void AssignVariable(AST_VariableAssignment variable_assign)
        {
            VariableDataType dataType;
            object value;

            //do not assign single underscores as variable
            if (variable_assign.Name.Equals("_"))
                return;

            if (variable_assign.VariableType == VariableType.Array)
            {
                Debug.WriteLine("Change array value: " + variable_assign.Name);
                dataType = VariableHelper.DetectArrayDatatype(variable_assign.AssignItems);
                value = variable_assign.AssignItems;
            }
            else
            {
                value = GetVariableValue(variable_assign.AssignItems);
                dataType = VariableHelper.DetectDataType(value);
            }

            if (Variables.ContainsKey(variable_assign.Name))
            {
                Variables.TryGetValue(variable_assign.Name, out IVariable item);
                item.Value = value;
            } //throw new Exception($"The variable {variable_assign.Name} is already assigned");
            else
            {
                if (variable_assign.VariableType == VariableType.Array)
                    Variables.Add(variable_assign.Name, new ArrayVariable(dataType, value));
                else
                    Variables.Add(variable_assign.Name, new ScalarVariable(dataType, value));
            }
        }
        private void CallVariable(AST_VariableCall variable_call)
        {
            if (variable_call.VariableCallAction == VariableCallAction.Change) //when: variable = x
            {
                InterpretNext(variable_call.NextItem);
            }
            //no need for else, why call a variable without doing anything with it
        }
        private void CallArrayAccess(AST_Arrayaccess array_access)
        {
            if (array_access.VariableCallAction == VariableCallAction.Read)
                CallVariable(new AST_VariableCall(array_access.VariableCallAction, array_access.VariableName));
            //ChangeArrayVariable(array_access);

        }

        private void RegisterBuiltinFunction(string name, AbstractSyntaxTree[] parameter)
        {
            Functions.Add(name, new FunctionItem(parameter, null, null));
        }
        private bool ParameterAndArgumentMatch(AbstractSyntaxTree[] parameter, AbstractSyntaxTree[] arguments)
        {
            return parameter.Length == arguments.Length;
        }
        private object CallFunction(AST_FunctionCall function_call)
        {
            if (Functions.TryGetValue(function_call.Name, out FunctionItem item))
            {
                if (!ParameterAndArgumentMatch(function_call.Parameter, item.Parameter))
                    throw new Exception($"The parameters for the function {function_call.Name} do not match");

                for (int i = 0; i < function_call.Parameter.Length; i++)
                {
                    object variableValue = GetVariableValue(function_call.Parameter[i]);
                    var variableItem = item.Parameter[i] as AST_FunctionArgument;
                    AssignFunctionParameterVariable(variableItem.Name, variableItem.Type, variableValue, parser.bracketDepth.CurlyBracket);
                }

                var interpretResult = InterpretNext(item.Actions);
                if (interpretResult == null)
                    return null;
                if (interpretResult is AST_Return ast_return)
                    return GetVariableValue(ast_return.SubItems);
            }
            else if (!GetBuiltinFunction(function_call.Name, function_call))
                throw new Exception($"No function with the name {function_call.Name} was found");
            
            return "Hall0o";
        }
        private bool GetBuiltinFunction(string name, AST_FunctionCall function_call)
        {
            Debug.WriteLine("function: " + name);

            if (name.Equals("print", StringComparison.Ordinal))
            {
                Console.WriteLine(GetParameterValue(function_call.Parameter));
                return true;
            }
            else if (name.Equals("input", StringComparison.Ordinal))
            {
                Console.WriteLine(function_call.Parameter);
                string input = Console.ReadLine();
                Debug.WriteLine(input);
            }
            return false;
        }
        private void CreateFunction(AST_FunctionCreate function_create)
        {
            Functions.Add(function_create.FunctionName, new FunctionItem(function_create.Arguments, function_create.Actions, function_create.ReturnType));
        }

        private void HandleForLoop(AST_ForLoop for_loop)
        {
            //when the iteration operator is of type range:
            if (for_loop.IterationOperator is AST_Range range)
            {
                //create the iteration variable, with the value 0 and assign the current bracket depth to it.
                AssignIterableVariable(for_loop.IterationVariableName, VariableDataType.Integer, 0, parser.bracketDepth.CurlyBracket);

                for (int i = range.Start; i < range.End; i++)
                {
                    //update the value of the variable
                    ChangeIterableVariable(for_loop.IterationVariableName, i);
                    
                    InterpretNext(for_loop.SubItems);
                }
            }
            //When the iteration opoerator is of type array:
            else if (for_loop.IterationOperator is AST_VariableCall variable_call)
            {
                if (!Variables.TryGetValue(variable_call.Name, out IVariable variable))
                    throw new Exception("Variable " + variable_call.Name + " was not found");

                if (!VariableHelper.IsIteratable(variable_call.Name, Variables))
                    throw new Exception("Variable " + variable_call.Name + " is not iterable");

                AbstractSyntaxTree[] value = GetVariableValue(variable_call) as AbstractSyntaxTree[];
                //create the iteration variable, with null and assign the current bracket depth to it.
                AssignIterableVariable(for_loop.IterationVariableName, variable.VariableDataType, null, parser.bracketDepth.CurlyBracket);

                for(int i = 0; i < value.Length; i++)
                {
                    //update the value of the variable
                    ChangeIterableVariable(for_loop.IterationVariableName, VariableHelper.ParseToDatatype(value[i], variable.VariableDataType));
                    InterpretNext(for_loop.SubItems);
                }
            }

            //remove the variable after the for loop finished:
            DeleteVariable(for_loop.IterationVariableName);
        }
        private string GetParameterValue(AbstractSyntaxTree[] items)
        {
            var res = GetVariableValue(items);

            //print the array as actual values:
            if (res is AbstractSyntaxTree[] arr) 
                return "[" + string.Join(",", arr.Select(x => GetVariableValue(x))) + "]";
            return res.ToString();
        }

        private object InterpretNext(AbstractSyntaxTree[] nodes)
        {
            foreach (var node in nodes)
            {
                var next = InterpretNext(node);
                if (next != null)
                    return next;
            }
            return null;
        }
        private object InterpretNext(AbstractSyntaxTree node)
        {
            if (node == null || node is AST_None)
            {
                Debug.WriteLine("Node is none");
                return null;
            }

            if (node is AST_VariableAssignment variable_assign)
                AssignVariable(variable_assign);
            else if (node is AST_VariableCall variable_call)
                CallVariable(variable_call);
            else if (node is AST_FunctionCall function_call) //call function without using return value:
                CallFunction(function_call);
            else if (node is AST_FunctionCreate function_create)
                CreateFunction(function_create);
            else if (node is AST_ForLoop for_loop)
                HandleForLoop(for_loop);
            else if (node is AST_Arrayaccess arrayaccess)
                CallArrayAccess(arrayaccess);
            else if (node is AST_Return ast_return)
                return ast_return;
            else if (node is AST_BracketChanged bracket_changed)
            {
                Parser_BracketDepthChanged(bracket_changed.currentDepth);
            }
            return null;
        }

        public void Interpret(string code)
        {
            Lexer lexer = new Lexer(code);
            parser = new Parser(lexer);

            var root = parser.Parse();
            while (root.NextItem != null)
            {
                InterpretNext(root.NextItem);
                root = root.NextItem;
            }
        }

        private void Parser_BracketDepthChanged(BracketDepth currentdepth)
        {
            //remove unused variables when the curlybracket depth changes:

            var variables = Variables.Where(x => x.Value.CurlyBracketDepth == 0);
            foreach (var variable in variables)
            {
                Debug.WriteLine("Can be removed: " + variable);
            }
        }
    }
}
