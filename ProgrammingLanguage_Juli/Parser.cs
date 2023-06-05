﻿using Juli_ProgLang;
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

        private void NextToken(params Identifiers[] identifier)
        {
            if (currentToken.Type is Identifiers id && identifier.Contains(id))
                NextToken();
            else
                throw new Exception($"Expected {string.Join(", ", identifier)}, but got {currentToken.Type}");
        }
        private void NextToken(Identifiers identifier)
        {
            if (currentToken.Type is Identifiers id && id == identifier)
                NextToken();
            else
                throw new Exception($"Expected {identifier}, but got {currentToken.Type}");
        }
        private void NextToken(Keywords keyword)
        {
            if (currentToken.Type is Keywords id && id == keyword)
                NextToken();
            else
                throw new Exception($"Expected {keyword}, but got {currentToken.Type}");
        }
        private Token NextToken()
        {
            lastToken = currentToken;
            return currentToken = lexer.NextToken();
        }
        private Identifiers GetTokenIdentifier(Token token)
        {
            if (token == null)
                throw new Exception("Missing semicolon");
            return (Identifiers)token.Type;
        }
        private AbstractSyntaxTree GetIdentifier()
        {
            string value = currentToken.Value.ToString();
            if (value.Length > 0)
            {
                Identifiers identifier = (Identifiers)NextToken().Type;

                // identifier(... -> function call:
                if (identifier == Identifiers.LeftParen)
                    return new AST_FunctionCall(value, GetFunctionCallParameter());
                else if (identifier == Identifiers.Colon)
                    return GetFunctionArgument();
                else
                    return GetVariableCall(value, identifier);
            }
            NextToken();
            return new AbstractSyntaxTree();
        }

        private AbstractSyntaxTree GetVariableCall(string value, Identifiers identifier)
        {
            //index access on an array:
            if ((Identifiers)currentToken.Type == Identifiers.LeftSqrBracket)
            {
                var access = GetArrayAccess(value);
                if (GetTokenIdentifier(currentToken) == Identifiers.Equals)
                    access.VariableCallAction = VariableCallAction.Change;
                return access;
            }

            //change the variable
            if (identifier == Identifiers.Equals)
                return GetVariableAssingValue(value);
            //read variable:
            else
                return new AST_VariableCall(VariableCallAction.Read, value);
        }
        private AbstractSyntaxTree GetVariableAssingValue(string variableName)
        {
            NextToken(Identifiers.Equals);
            Identifiers identifier = GetTokenIdentifier(currentToken);
            List<AbstractSyntaxTree> assignValues = new List<AbstractSyntaxTree>();

            //Array:
            if (identifier == Identifiers.LeftSqrBracket)
            {
                do
                {
                    var item = Identify();
                    identifier= GetTokenIdentifier(currentToken);
                    if (identifier != Identifiers.Semicolon && item != null)
                    {
                        assignValues.Add(item);
                    }
                }
                while (identifier != Identifiers.Semicolon);
                return new AST_VariableAssignment(assignValues.ToArray(), variableName, VariableType.Array);
            }

            while (identifier != Identifiers.Semicolon)
            {
                var item = Identify();
                assignValues.Add(item);

                identifier = GetTokenIdentifier(currentToken);
            }
            NextToken(Identifiers.Semicolon);
            return new AST_VariableAssignment(assignValues.ToArray(), variableName);
        }

        private AbstractSyntaxTree GetVariableCreate()
        {
            NextToken(Identifiers.Variable);
            string name = currentToken.Value?.ToString();
            Debug.WriteLine("variable: " + name);
            NextToken(Identifiers.Identifier);
            return GetVariableAssingValue(name);
        }
        private AbstractSyntaxTree GetAddOperator()
        {
            Token last = lastToken;
            NextToken(Identifiers.Add);

            //concatenate strings:
            if (GetTokenIdentifier(last) == Identifiers.String)
                return new AST_Concatinate(Identify(last), Identify());
            return new AST_MathOperation(MathOperation.Add);
        }
        private AbstractSyntaxTree GetFunctionCreate()
        {
            BracketDepth startdepth = new BracketDepth(bracketDepth);
            NextToken(Identifiers.Function);
            string functionName = currentToken.Value.ToString();
            NextToken(Identifiers.Identifier);
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
            if (currentToken != null && GetTokenIdentifier(currentToken) == Identifiers.Colon)
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
            NextToken(Identifiers.LeftSqrBracket);

            //supported array access formats: [1], [1:5], [:5], [1:], [:]
            var identifier = GetTokenIdentifier(currentToken);

            List<(AbstractSyntaxTree item, Identifiers id)> ast_items = new List<(AbstractSyntaxTree item, Identifiers id)>();
            dynamic getValue(int index)
            {
                if (ast_items[index].item is AST_Integer ast_int)
                    return int.Parse((ast_int).Value.ToString());
                else if (ast_items[index].item is AST_VariableCall var_call)
                    return var_call;
                return null;
            }
            if (identifier == Identifiers.Integer || identifier == Identifiers.Identifier || identifier == Identifiers.Colon)
            {
                do
                {
                    var identify = Identify();
                    identifier = GetTokenIdentifier(currentToken);
                    ast_items.Add((identify, identifier));
                }
                while (identifier != Identifiers.RightSqrBracket);

                NextToken(Identifiers.RightSqrBracket);

                //[:], [5]
                if (ast_items.Count == 1)
                {
                    if (ast_items[0].id == Identifiers.Colon)
                        return new AST_Arrayaccess(variableName, 0, -1);
                    return new AST_Arrayaccess(variableName, getValue(0), getValue(0));
                }
                else if (ast_items.Count == 2) //[0:], [:1]
                {
                    //[:1]
                    if (ast_items[0].id == Identifiers.Colon)
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
            NextToken(Identifiers.Colon);
            var datatype = VariableHelper.DetectDataType(currentToken);
            NextToken();
            if ((Identifiers)currentToken.Type == Identifiers.Comma)
                NextToken();

            return new AST_FunctionArgument(name, datatype);
        }
        private AbstractSyntaxTree GetReturnValue()
        {
            NextToken(Identifiers.Return);
            List<AbstractSyntaxTree> Items = new List<AbstractSyntaxTree>();

            Identifiers identifier;
            do
            {
                identifier = GetTokenIdentifier(currentToken);
                var identify = Identify();
                if (identify != null)
                    Items.Add(identify);

            } while (identifier != Identifiers.Semicolon);

            return new AST_Return(Items.ToArray());
        }

        private AbstractSyntaxTree GetIfKeyword()
        {
            BracketDepth startDepth = new BracketDepth(bracketDepth);
            NextToken(Keywords.If); //skip if

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

            NextToken(Keywords.Else);
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

            NextToken(Keywords.For);

            Identify(); //bracket
            NextToken(Identifiers.Variable);
            string iterationVariableName = currentToken.Value.ToString();
            NextToken(Identifiers.Identifier);
            NextToken(Keywords.In);

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

            NextToken(Keywords.Range);
            NextToken(Identifiers.LeftParen);
            var start = Identify(currentToken);
            NextToken(Identifiers.Comma);
            var end = Identify(currentToken);
            NextToken(Identifiers.RightParen);

            return new AST_Range(start, end);
        }

        private AbstractSyntaxTree GetLenKeyword()
        {
            NextToken(Keywords.Len);
            NextToken(Identifiers.LeftParen);
            var variable = Identify();
            NextToken(Identifiers.RightParen);
            return new AST_Len(variable);
        }

        private AbstractSyntaxTree Identify(Token token = null)
        {
            if (token == null)
                token = currentToken;

            if (token == null)
                return null;

            if (token.Type is Identifiers identifier)
            {
                switch (identifier)
                {
                    //bracketDepth:
                    case Identifiers.LeftSqrBracket:
                        NextToken(Identifiers.LeftSqrBracket);
                        bracketDepth.SquareBracket++;
                        return null;
                    case Identifiers.RightSqrBracket:
                        NextToken(Identifiers.RightSqrBracket);
                        bracketDepth.SquareBracket--;
                        return null;
                    case Identifiers.LeftCurly:
                        NextToken(Identifiers.LeftCurly);
                        bracketDepth.CurlyBracket++;
                        return null;
                    case Identifiers.RightCurly:
                        NextToken(Identifiers.RightCurly);
                        bracketDepth.CurlyBracket--;
                        return null;
                    case Identifiers.LeftParen:
                        NextToken(Identifiers.LeftParen);
                        bracketDepth.Parenthesis++;
                        return null;
                    case Identifiers.RightParen:
                        NextToken(Identifiers.RightParen);
                        bracketDepth.Parenthesis--;
                        return null;

                    case Identifiers.GreaterEquals:
                        NextToken(Identifiers.GreaterEquals);
                        return new AST_BoolOperation(BoolOperation.GreaterEquals);
                    case Identifiers.Greater:
                        NextToken(Identifiers.Greater);
                        return new AST_BoolOperation(BoolOperation.Greater);
                    case Identifiers.Smaller:
                        NextToken(Identifiers.Smaller);
                        return new AST_BoolOperation(BoolOperation.Smaller);
                    case Identifiers.SmallerEquals:
                        NextToken(Identifiers.SmallerEquals);
                        return new AST_BoolOperation(BoolOperation.SmallerEquals);
                    case Identifiers.Not:
                        NextToken(Identifiers.Not);
                        return new AST_BoolOperation(BoolOperation.Not);
                    case Identifiers.NotEquals:
                        NextToken(Identifiers.NotEquals);
                        return new AST_BoolOperation(BoolOperation.NotEquals);
                    case Identifiers.Equals:
                        NextToken(Identifiers.Equals);
                        return new AST_BoolOperation(BoolOperation.Equals);
                    case Identifiers.Or:
                        NextToken(Identifiers.Or);
                        return new AST_BoolOperation(BoolOperation.Or);
                    case Identifiers.And:
                        NextToken(Identifiers.And);
                        return new AST_BoolOperation(BoolOperation.And);
                    case Identifiers.Compare:
                        NextToken(Identifiers.Compare);
                        return new AST_BoolOperation(BoolOperation.Equals);
                    case Identifiers.Divide:
                        NextToken(Identifiers.Divide);
                        return new AST_MathOperation(MathOperation.Divide);
                    case Identifiers.Multiply:
                        NextToken(Identifiers.Multiply);
                        return new AST_MathOperation(MathOperation.Multiply);
                    case Identifiers.Subtract:
                        NextToken(Identifiers.Subtract);
                        return new AST_MathOperation(MathOperation.Subtract);
                    case Identifiers.Modulo:
                        NextToken(Identifiers.Modulo);
                        return new AST_MathOperation(MathOperation.Modulo);
                    case Identifiers.Add:
                        return GetAddOperator();
                    case Identifiers.Variable:
                        return GetVariableCreate();
                    case Identifiers.Function:
                        return GetFunctionCreate();
                    case Identifiers.Identifier:
                        return GetIdentifier();
                    case Identifiers.Return:
                        return GetReturnValue();
                    case Identifiers.Semicolon:
                        NextToken(Identifiers.Semicolon);
                        return Identify();// new AST_None("End (Semicolon)");
                    case Identifiers.Colon:
                        return GetFunctionArgument();
                    case Identifiers.Integer:
                        NextToken(Identifiers.Integer);
                        return new AST_Integer(token);
                    case Identifiers.Float:
                        NextToken(Identifiers.Float);
                        return new AST_Float(token);
                    case Identifiers.String:
                        NextToken(Identifiers.String);
                        return new AST_String(token);
                    case Identifiers.Bool:
                        NextToken(Identifiers.Bool);
                        return new AST_Bool(token);
                    case Identifiers.Comma:
                        NextToken(Identifiers.Comma);
                        return Identify();
                    default:
                        Debug.WriteLine("Emty: " + currentToken.Type);
                        NextToken();
                        return new AST_None();
                }
            }
            else if (token.Type is Keywords keyword)
            {
                switch (keyword)
                {
                    case Keywords.If:
                        return GetIfKeyword();
                    case Keywords.Else:
                        return GetElseKeyword();
                    case Keywords.For:
                        return GetForLoop();
                    case Keywords.Range:
                        return GetRangeKeyword();
                    case Keywords.Len:
                        return GetLenKeyword();
                }
                return null;
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
