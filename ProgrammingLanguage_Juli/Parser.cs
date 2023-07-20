using Juli_ProgLang;
using Juli_ProgLang.Content;
using Juli_ProgLang.Content.AST;
using Juli_ProgLang.Helper;
using ProgrammingLanguage_Juli.Content.AST;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Windows.Forms;

namespace ProgrammingLanguage_Juli
{
    internal class Parser
    {
        private Lexer lexer;
        private Token currentToken;
        private Token lastToken;
        public BracketDepth bracketDepth = new BracketDepth();
        private AbstractSyntaxTree root = new AbstractSyntaxTree();
        private AbstractSyntaxTree node;

        public Parser(Lexer lexer)
        {
            this.lexer = lexer;
            this.node = root;
            currentToken = lexer.NextToken();
        }

        private void NextToken(params SyntaxKind[] identifier)
        {
            if (currentToken.Type is SyntaxKind id && identifier.Contains(id))
                NextToken();
            else
                throw new Exception($"Expected {string.Join(", ", identifier)}, but got {currentToken.Type}");
        }
        private void NextToken(SyntaxKind identifier)
        {
            if (currentToken.Type is SyntaxKind id && id == identifier)
                NextToken();
            else
                throw new Exception($"Expected {identifier}, but got {currentToken.Type}");
        }
        private Token NextToken()
        {
            lastToken = currentToken;
            return currentToken = lexer.NextToken();
        }
        private SyntaxKind GetTokenIdentifier(Token token)
        {
            if (token == null)
                throw new Exception("Missing semicolon");
            return (SyntaxKind)token.Type;
        }
        private AbstractSyntaxTree GetIdentifier()
        {
            string value = currentToken.Value.ToString();
            if (value.Length > 0)
            {
                SyntaxKind identifier = (SyntaxKind)NextToken().Type;

                // identifier(... -> function call:
                if (identifier == SyntaxKind.LeftParen_ID)
                    return new AST_FunctionCall(value, GetFunctionCallParameter());
                else if (identifier == SyntaxKind.Colon_ID)
                    return GetFunctionArgument();
                else
                    return GetVariableCall(value, identifier);
            }
            NextToken();
            return new AbstractSyntaxTree();
        }

        private AbstractSyntaxTree GetVariableCall(string value, SyntaxKind identifier)
        {
            //index access on an array:
            if ((SyntaxKind)currentToken.Type == SyntaxKind.LeftSqrBracket_ID)
            {
                var access = GetArrayAccess(value);
                if (GetTokenIdentifier(currentToken) == SyntaxKind.Equals_ID)
                    access.VariableCallAction = VariableCallAction.Change;
                return access;
            }

            //change the variable
            if (identifier == SyntaxKind.Equals_ID)
                return GetVariableAssingValue(value);
            //read variable:
            else
                return new AST_VariableCall(VariableCallAction.Read, value);
        }
        private AbstractSyntaxTree GetVariableAssingValue(string variableName)
        {
            NextToken(SyntaxKind.Equals_ID);
            SyntaxKind identifier = GetTokenIdentifier(currentToken);
            List<AbstractSyntaxTree> assignValues = new List<AbstractSyntaxTree>();

            //Array:
            if (identifier == SyntaxKind.LeftSqrBracket_ID)
            {
                do
                {
                    var item = Identify();
                    identifier= GetTokenIdentifier(currentToken);
                    if (identifier != SyntaxKind.Semicolon_ID && item != null)
                    {
                        assignValues.Add(item);
                    }
                }
                while (identifier != SyntaxKind.Semicolon_ID);
                return new AST_VariableAssignment(assignValues.ToArray(), variableName, VariableType.Array);
            }

            while (identifier != SyntaxKind.Semicolon_ID)
            {
                var item = Identify();
                assignValues.Add(item);

                identifier = GetTokenIdentifier(currentToken);
            }
            NextToken(SyntaxKind.Semicolon_ID);
            return new AST_VariableAssignment(assignValues.ToArray(), variableName);
        }

        private AbstractSyntaxTree GetVariableCreate()
        {
            NextToken(SyntaxKind.Variable_ID);
            string name = currentToken.Value?.ToString();
            Debug.WriteLine("variable: " + name);
            NextToken(SyntaxKind.Identifier_ID);
            return GetVariableAssingValue(name);
        }
        private AbstractSyntaxTree GetAddOperator()
        {
            Token last = lastToken;
            NextToken(SyntaxKind.Add_ID);

            //concatenate strings:
            if (GetTokenIdentifier(last) == SyntaxKind.String_ID)
                return new AST_Concatinate(Identify(last), Identify());
            return new AST_MathOperation(MathOperation.Add);
        }
        private AbstractSyntaxTree GetFunctionCreate()
        {
            BracketDepth startdepth = new BracketDepth(bracketDepth);
            NextToken(SyntaxKind.Function_ID);
            string functionName = currentToken.Value.ToString();
            NextToken(SyntaxKind.Identifier_ID);
            List<AbstractSyntaxTree> arguments = new List<AbstractSyntaxTree>();
            AbstractSyntaxTree returnType = null;

            do
            {
                var item = Identify();
                if (item == null)
                    continue;
                arguments.Add(item);
            }
            while (startdepth.Parenthesis != bracketDepth.Parenthesis);

            //if function has a return value:
            if (currentToken != null && GetTokenIdentifier(currentToken) == SyntaxKind.Colon_ID)
            {
                NextToken();
                returnType = Identify();

                if (!(returnType is AST_String || returnType is AST_Integer || returnType is AST_Float || returnType is AST_Bool))
                    throw new Exception($"Invalid return type of function {functionName}");
            }

            Identify(currentToken);

            List<AbstractSyntaxTree> actions = new List<AbstractSyntaxTree>();
            while (startdepth.CurlyBracket != bracketDepth.CurlyBracket)
            {
                var item = Identify();
                if (item == null)
                    continue;

                actions.Add(item);
            }
            return new AST_FunctionCreate(functionName, returnType, arguments.ToArray(), actions.ToArray());
        }
        private AbstractSyntaxTree[] GetFunctionCallParameter()
        {
            BracketDepth startdepth = new BracketDepth(bracketDepth);
            List<AbstractSyntaxTree> parameter = new List<AbstractSyntaxTree>();

            do
            {
                var item = Identify();
                if (item == null)
                    continue;

                parameter.Add(item);
            }
            while (startdepth.Parenthesis != bracketDepth.Parenthesis);

            return parameter.ToArray();
        }
        private AST_Arrayaccess GetArrayAccess(string variableName)
        {
            NextToken(SyntaxKind.LeftSqrBracket_ID);

            //supported array access formats: [1], [1:5], [:5], [1:], [:]
            var identifier = GetTokenIdentifier(currentToken);

            List<(AbstractSyntaxTree item, SyntaxKind id)> ast_items = new List<(AbstractSyntaxTree item, SyntaxKind id)>();
            dynamic getValue(int index)
            {
                if (ast_items[index].item is AST_Integer ast_int)
                    return int.Parse((ast_int).Value.ToString());
                else if (ast_items[index].item is AST_VariableCall var_call)
                    return var_call;
                return null;
            }
            if (identifier == SyntaxKind.Integer_ID || identifier == SyntaxKind.Identifier_ID || identifier == SyntaxKind.Colon_ID)
            {
                do
                {
                    var identify = Identify();
                    identifier = GetTokenIdentifier(currentToken);
                    ast_items.Add((identify, identifier));
                }
                while (identifier != SyntaxKind.RightSqrBracket_ID);

                NextToken(SyntaxKind.RightSqrBracket_ID);

                //[:], [5]
                if (ast_items.Count == 1)
                {
                    if (ast_items[0].id == SyntaxKind.Colon_ID)
                        return new AST_Arrayaccess(variableName, 0, -1);
                    return new AST_Arrayaccess(variableName, getValue(0), getValue(0));
                }
                else if (ast_items.Count == 2) //[0:], [:1]
                {
                    //[:1]
                    if (ast_items[0].id == SyntaxKind.Colon_ID)
                        return new AST_Arrayaccess(variableName, 0, getValue(1));
                    else //[0:]
                        return new AST_Arrayaccess(variableName, getValue(0), -1);
                }
                else if (ast_items.Count == 3) //[0:5]
                {
                    return new AST_Arrayaccess(variableName, getValue(0), getValue(2));
                }
                throw new Exception("Invalid array access format. Supported: [1:5], [:5], [1:], [:], [5]");
            }
            return null;
        }

        private AbstractSyntaxTree GetFunctionArgument()
        {
            string name = lastToken.Value.ToString();
            NextToken(SyntaxKind.Colon_ID);
            var datatype = VariableHelper.DetectDataType(currentToken);
            NextToken();
            if ((SyntaxKind)currentToken.Type == SyntaxKind.Comma_ID)
                NextToken();

            return new AST_FunctionArgument(name, datatype);
        }
        private AbstractSyntaxTree GetReturnValue()
        {
            NextToken(SyntaxKind.Return_ID);
            List<AbstractSyntaxTree> Items = new List<AbstractSyntaxTree>();

            SyntaxKind identifier;
            do
            {
                identifier = GetTokenIdentifier(currentToken);
                var identify = Identify();
                if (identify != null)
                    Items.Add(identify);

            } while (identifier != SyntaxKind.Semicolon_ID);

            return new AST_Return(Items.ToArray());
        }

        private AbstractSyntaxTree GetIfKeyword()
        {
            BracketDepth startDepth = new BracketDepth(bracketDepth);
            NextToken(SyntaxKind.If_KW); //skip if

            List<AbstractSyntaxTree> actions = new List<AbstractSyntaxTree>();
            List<AbstractSyntaxTree> condition = new List<AbstractSyntaxTree>();
            //if-condition:
            do
            {
                var identify = Identify();
                if(identify != null)
                    condition.Add(identify);
            }
            while (startDepth.Parenthesis != bracketDepth.Parenthesis);

            actions.Add(Identify());
            while (startDepth.CurlyBracket != bracketDepth.CurlyBracket)
            {
                actions.Add(Identify());
            }

            return new AST_If(condition.ToArray(), actions.ToArray());
        }
        private AbstractSyntaxTree GetElseKeyword()
        {
            BracketDepth startDepth = new BracketDepth(bracketDepth);

            NextToken(SyntaxKind.Else_KW);
            Identify(); //skip curly bracket

            List<AbstractSyntaxTree> items = new List<AbstractSyntaxTree>();

            do
            {
                items.Add(Identify());
            }
            while (startDepth.CurlyBracket != bracketDepth.CurlyBracket);

            return new AST_Else(items.ToArray());
        }

        private AbstractSyntaxTree GetForLoop()
        {
            //for(var item in range(0,100)) { }
            //for(var item in variable) { }

            BracketDepth startDepth = new BracketDepth(bracketDepth);

            NextToken(SyntaxKind.For_KW);

            Identify(); //bracket
            NextToken(SyntaxKind.Variable_ID);
            string iterationVariableName = currentToken.Value.ToString();
            NextToken(SyntaxKind.Identifier_ID);
            NextToken(SyntaxKind.In_KW);

            AbstractSyntaxTree iterationOperator = null;
            while (startDepth.Parenthesis != bracketDepth.Parenthesis)
            {
                var item = Identify();
                if (item == null)
                    continue;

                iterationOperator = item;
            }

            //actions:
            List<AbstractSyntaxTree> subItems = new List<AbstractSyntaxTree>();
            do
            {
                var item = Identify();
                subItems.Add(item);
            }
            while (startDepth.CurlyBracket != bracketDepth.CurlyBracket);

            return new AST_ForLoop(iterationVariableName, subItems.ToArray(), iterationOperator);
        }
        private AbstractSyntaxTree GetRangeKeyword()
        {

            NextToken(SyntaxKind.Range_KW);
            NextToken(SyntaxKind.LeftParen_ID);
            var start = Identify(currentToken);
            NextToken(SyntaxKind.Comma_ID);
            var end = Identify(currentToken);
            NextToken(SyntaxKind.RightParen_ID);

            return new AST_Range(start, end);
        }

        private AbstractSyntaxTree GetLenKeyword()
        {
            NextToken(SyntaxKind.Len_KW);
            NextToken(SyntaxKind.LeftParen_ID);
            var variable = Identify();
            NextToken(SyntaxKind.RightParen_ID);
            return new AST_Len(variable);
        }

        private AbstractSyntaxTree Identify(Token token = null)
        {
            if (token == null)
                token = currentToken;

            if (token == null)
                return null;

            if (token.Type is SyntaxKind identifier)
            {
                switch (identifier)
                {
                    //bracketDepth:
                    case SyntaxKind.LeftSqrBracket_ID:
                        NextToken(SyntaxKind.LeftSqrBracket_ID);
                        bracketDepth.SquareBracket++;
                        return null;
                    case SyntaxKind.RightSqrBracket_ID:
                        NextToken(SyntaxKind.RightSqrBracket_ID);
                        bracketDepth.SquareBracket--;
                        return null;
                    case SyntaxKind.LeftCurly_ID:
                        NextToken(SyntaxKind.LeftCurly_ID);
                        bracketDepth.CurlyBracket++;
                        return null;
                    case SyntaxKind.RightCurly_ID:
                        NextToken(SyntaxKind.RightCurly_ID);
                        bracketDepth.CurlyBracket--;
                        return null;
                    case SyntaxKind.LeftParen_ID:
                        NextToken(SyntaxKind.LeftParen_ID);
                        bracketDepth.Parenthesis++;
                        return null;
                    case SyntaxKind.RightParen_ID:
                        NextToken(SyntaxKind.RightParen_ID);
                        bracketDepth.Parenthesis--;
                        return null;

                    case SyntaxKind.GreaterEquals_ID:
                        NextToken(SyntaxKind.GreaterEquals_ID);
                        return new AST_BoolOperation(BoolOperation.GreaterEquals);
                    case SyntaxKind.Greater_ID:
                        NextToken(SyntaxKind.Greater_ID);
                        return new AST_BoolOperation(BoolOperation.Greater);
                    case SyntaxKind.Smaller_ID:
                        NextToken(SyntaxKind.Smaller_ID);
                        return new AST_BoolOperation(BoolOperation.Smaller);
                    case SyntaxKind.SmallerEquals_ID:
                        NextToken(SyntaxKind.SmallerEquals_ID);
                        return new AST_BoolOperation(BoolOperation.SmallerEquals);
                    case SyntaxKind.Not_ID:
                        NextToken(SyntaxKind.Not_ID);
                        return new AST_BoolOperation(BoolOperation.Not);
                    case SyntaxKind.NotEquals_ID:
                        NextToken(SyntaxKind.NotEquals_ID);
                        return new AST_BoolOperation(BoolOperation.NotEquals);
                    case SyntaxKind.Equals_ID:
                        NextToken(SyntaxKind.Equals_ID);
                        return new AST_BoolOperation(BoolOperation.Equals);
                    case SyntaxKind.Or_ID:
                        NextToken(SyntaxKind.Or_ID);
                        return new AST_BoolOperation(BoolOperation.Or);
                    case SyntaxKind.And_ID:
                        NextToken(SyntaxKind.And_ID);
                        return new AST_BoolOperation(BoolOperation.And);
                    case SyntaxKind.Compare_ID:
                        NextToken(SyntaxKind.Compare_ID);
                        return new AST_BoolOperation(BoolOperation.Equals);
                    case SyntaxKind.Divide_ID:
                        NextToken(SyntaxKind.Divide_ID);
                        return new AST_MathOperation(MathOperation.Divide);
                    case SyntaxKind.Multiply_ID:
                        NextToken(SyntaxKind.Multiply_ID);
                        return new AST_MathOperation(MathOperation.Multiply);
                    case SyntaxKind.Subtract_ID:
                        NextToken(SyntaxKind.Subtract_ID);
                        return new AST_MathOperation(MathOperation.Subtract);
                    case SyntaxKind.Modulo_ID:
                        NextToken(SyntaxKind.Modulo_ID);
                        return new AST_MathOperation(MathOperation.Modulo);
                    case SyntaxKind.Add_ID:
                        return GetAddOperator();
                    case SyntaxKind.Variable_ID:
                        return GetVariableCreate();
                    case SyntaxKind.Function_ID:
                        return GetFunctionCreate();
                    case SyntaxKind.Identifier_ID:
                        return GetIdentifier();
                    case SyntaxKind.Return_ID:
                        return GetReturnValue();
                    case SyntaxKind.Semicolon_ID:
                        NextToken(SyntaxKind.Semicolon_ID);
                        return Identify();// new AST_None("End (Semicolon)");
                    case SyntaxKind.Colon_ID:
                        return GetFunctionArgument();
                    case SyntaxKind.Integer_ID:
                        NextToken(SyntaxKind.Integer_ID);
                        return new AST_Integer(token);
                    case SyntaxKind.Float_ID:
                        NextToken(SyntaxKind.Float_ID);
                        return new AST_Float(token);
                    case SyntaxKind.String_ID:
                        NextToken(SyntaxKind.String_ID);
                        return new AST_String(token);
                    case SyntaxKind.Bool_ID:
                        NextToken(SyntaxKind.Bool_ID);
                        return new AST_Bool(token);
                    case SyntaxKind.Comma_ID:
                        NextToken(SyntaxKind.Comma_ID);
                        return Identify();
                    case SyntaxKind.If_KW:
                        return GetIfKeyword();
                    case SyntaxKind.Else_KW:
                        return GetElseKeyword();
                    case SyntaxKind.For_KW:
                        return GetForLoop();
                    case SyntaxKind.Range_KW:
                        return GetRangeKeyword();
                    case SyntaxKind.Len_KW:
                        return GetLenKeyword();
                    default:
                        Debug.WriteLine("Emty: " + currentToken.Type);
                        NextToken();
                        return new AST_None();
                }
            }
            return null;
        }
        public AbstractSyntaxTree Parse()
        {
            while (node != null)
            {
                node = node.NextItem = Identify();
            }
            return root;
        }
    }

    public class BracketDepth
    {
        public BracketDepth() { }
        public BracketDepth(BracketDepth brackedDepth)
        {
            this.CurlyBracket = brackedDepth.CurlyBracket;
            this.SquareBracket = brackedDepth.SquareBracket;
            this.Parenthesis = brackedDepth.Parenthesis;
        }
        public int CurlyBracket { get; set; } = 0;
        public int Parenthesis { get; set; } = 0;
        public int SquareBracket { get; set; } = 0;
    }
}
