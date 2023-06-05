using Juli_ProgLang;
using Juli_ProgLang.Content.AST;
using ProgrammingLanguage_Juli.Content.AST;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace ProgrammingLanguage_Juli
{
    internal class Program
    {
        private static void PrintArray(AbstractSyntaxTree[] list, string indention)
        {
            foreach (var item in list)
                PrintNext(item, indention);
        }

        private static void PrintNext(AbstractSyntaxTree item, string indent)
        {
            if (item == null)
                return;

            if (item is AST_None none)
                Console.WriteLine(none.Value == "" ? "None" : none.Value);
            else if (item is AST_FunctionCreate fun_create)
            {
                Console.WriteLine(indent + "Create function: " + fun_create.FunctionName);
                Console.WriteLine(indent + "\t" + "Returntype: " + fun_create.ReturnType);
                PrintArray(fun_create.Arguments, indent + "\t");
                PrintArray(fun_create.Actions, indent + "\t");
            }
            else if (item is AST_FunctionCall fun_call)
            {
                Console.WriteLine("Call function: " + fun_call.Name);
                PrintArray(fun_call.Parameter, "\t" + indent);
            }
            else if (item is AST_Return ast_return)
            {
                Console.WriteLine(indent + "Return: ");
                PrintArray(ast_return.SubItems, indent + "\t");
            }
            else if (item is AST_FunctionArgument fun_argument)
                Console.WriteLine(indent + "Argument: " + fun_argument.Name + " : " + fun_argument.Type);
            else if (item is AST_BoolOperation bool_op)
                Console.WriteLine(indent + "Bool operation: " + bool_op.BoolOperation);
            else if (item is AST_MathOperation math_op)
                Console.WriteLine(indent + "Math operation: " + math_op.MathOperation);
            else if (item is AST_Integer ast_int)
                Console.WriteLine(indent + "Integer: " + ast_int.Value);
            else if (item is AST_Float ast_float)
                Console.WriteLine(indent + "Float: " + ast_float.Value);
            else if (item is AST_String str)
                Console.WriteLine(indent + "String: " + str.Value);
            else if (item is AST_VariableCall var_call)
                Console.WriteLine(indent + "Call variable: " + var_call.Name + " : " + var_call.VariableCallAction);
            else if (item is AST_VariableAssignment var_assign)
            {
                Console.WriteLine(indent + "Assign variable: " + var_assign.Name);
                PrintArray(var_assign.AssignItems, indent + "\t");
            }
            else if (item is AST_If if_call)
            {
                Console.WriteLine(indent + "If: ");
                PrintArray(if_call.Condition, indent + "\t");
                Console.WriteLine(indent + "\tAction");
                PrintArray(if_call.SubItems, indent + "\t\t");
            }
            else if (item is AST_Else else_call)
            {
                Console.WriteLine(indent + "Else: ");
                PrintArray(else_call.SubItems, indent + "\t");
            }
            else if (item is AST_Range range_call)
                Console.WriteLine(indent + "Range: " + range_call.Start + " - " + range_call.End);
            else if (item is AST_ForLoop for_loop)
            {
                Console.WriteLine(indent + "For loop: (" + for_loop.IterationVariableName + ")");
                Console.WriteLine(indent + "\tIteration: " + for_loop.IterationOperator);
                PrintArray(for_loop.SubItems, indent + "\t");
            }
            else if (item is AST_Concatinate concatinate)
            {
                Console.WriteLine(indent + "Concatinate: ");
                PrintNext(concatinate.Item1, "\t");
                PrintNext(concatinate.Item2, "\t");
            }
            else if (item is AST_Arrayaccess access)
            {
                Console.WriteLine(indent + "Array access: ");
                Console.WriteLine(indent + "\t" + access.VariableName + " [" + (access.Start == access.End ? access.Start.ToString() : (access.Start + "-" + access.End)) + "]");
            }
        }

        static void Main(string[] args)
        {
            int Action = 1;
            string data = File.ReadAllText("code1.juli");

            if (Action == 0)
            {
                Lexer lexer = new Lexer(data);
                Parser parser = new Parser(lexer);
                var root = parser.Parse();
                while (root.NextItem != null)
                {
                    PrintNext(root.NextItem, "");
                    root = root.NextItem;
                }

                Console.ReadLine();
            }
            else
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                Interpreter interpreter = new Interpreter();
                interpreter.Interpret(data);

                sw.Stop();
                Console.WriteLine("\n-----------------------------------------------------------------");
                Console.WriteLine(sw.ElapsedMilliseconds + ":" + sw.ElapsedTicks);
                Console.ReadLine();
            }
        }
    }
}
