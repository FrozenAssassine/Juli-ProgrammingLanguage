using Juli_ProgLang.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Juli_ProgLang
{
    internal class Lexer
    {
        int Position = 0;
        private char currentChar;
        readonly string Text;

        public Lexer(string text)
        {
            Text = text;
            Position = 0;
            currentChar = text[0];
        }

        private void SkipWhitespace()
        {
            while (currentChar != '\0' && char.IsWhiteSpace(currentChar))
            {
                Advance();
            }
        }
        private void SkipComments()
        {
            while (currentChar != '\0' && currentChar != '\n')
            {
                Advance();
            }
        }
        private void Advance(int count = 1)
        {
            Position = Position + count;
            currentChar = Position < Text.Length ? Text[Position] : '\0';
        }

        private (string, Identifiers) GetNumberString()
        {
            StringBuilder result = new StringBuilder();
            while (currentChar != '\0' && char.IsDigit(currentChar) || currentChar == '.')
            {
                result.Append(currentChar);
                Advance();
            }
            return (result.ToString(), result.ToString().Contains(".") ? Identifiers.Float : Identifiers.Integer);
        }
        private string GetString()
        {
            StringBuilder result = new StringBuilder();

            Advance();//skip the first character, because it is "
            while (currentChar != '\0' && currentChar != '"')
            {
                result.Append(currentChar);
                Advance();
            }
            Advance();
            return result.ToString();
        }
        private string GetIdentifier()
        {
            StringBuilder result = new StringBuilder();
            while (currentChar != '\0' && (char.IsLetterOrDigit(currentChar) || currentChar == '_'))
            {
                result.Append(currentChar);
                Advance();
            }
            return result.ToString();
        }
        private bool IsSequence(string sequence)
        {
            return currentChar == sequence[0] && Text.Substring(Position, sequence.Length).Equals(sequence);
        }

        public Token NextToken()
        {
            while (currentChar != '\0')
            {
                if (char.IsWhiteSpace(currentChar))
                {
                    SkipWhitespace();
                    continue;
                }
                if (currentChar == '#')
                {
                    SkipComments();
                    continue;
                }
                if (char.IsDigit(currentChar))
                {
                    var res = GetNumberString();
                    return new Token(res.Item2, res.Item1);
                }
                if (currentChar == '"')
                    return new Token(Identifiers.String, GetString());
                if (currentChar == '(')
                {
                    Advance();
                    return new Token(Identifiers.LeftParen);
                }
                if (currentChar == ')')
                {
                    Advance();
                    return new Token(Identifiers.RightParen);
                }
                if (currentChar == '}')
                {
                    Advance();
                    return new Token(Identifiers.RightCurly);
                }
                if (currentChar == '{')
                {
                    Advance();
                    return new Token(Identifiers.LeftCurly);
                }
                if (currentChar == '[')
                {
                    Advance();
                    return new Token(Identifiers.LeftSqrBracket);
                }
                if (currentChar == ']')
                {
                    Advance();
                    return new Token(Identifiers.RightSqrBracket);
                }
                if (currentChar == ';')
                {
                    Advance();
                    return new Token(Identifiers.Semicolon);
                }
                if (currentChar == '=')
                {
                    Advance();
                    if (currentChar == '=') //check also for == (compare)
                    {
                        Advance();
                        return new Token(Identifiers.Compare);
                    }
                    return new Token(Identifiers.Equals);
                }
                if (currentChar == '>')
                {
                    Advance();
                    if (currentChar == '=') //check also for >= (greater equals)
                    {
                        Advance();
                        return new Token(Identifiers.GreaterEquals);
                    }
                    return new Token(Identifiers.Greater);
                }
                if (currentChar == '<')
                {
                    Advance();
                    if (currentChar == '=') //check also for <= (smaller equals)
                    {
                        Advance();
                        return new Token(Identifiers.SmallerEquals);
                    }
                    return new Token(Identifiers.Smaller);
                }
                if (currentChar == '!')
                {
                    Advance();
                    if (currentChar == '=')
                    {
                        Advance();
                        return new Token(Identifiers.NotEquals);
                    }
                    return new Token(Identifiers.Not);
                }
                if (currentChar == ':')
                {
                    Advance();
                    return new Token(Identifiers.Colon);
                }
                if (currentChar == '+')
                {
                    Advance();
                    return new Token(Identifiers.Add);
                }
                if (currentChar == '-')
                {
                    Advance();
                    return new Token(Identifiers.Subtract);
                }
                if (currentChar == '*')
                {
                    Advance();
                    return new Token(Identifiers.Multiply);
                }
                if (currentChar == '/')
                {
                    Advance();
                    return new Token(Identifiers.Divide);
                }
                if (currentChar == '%')
                {
                    Advance();
                    return new Token(Identifiers.Modulo);
                }
                if (currentChar == ',')
                {
                    Advance();
                    return new Token(Identifiers.Comma);
                }
                if (IsSequence("if"))
                {
                    Advance(2);
                    return new Token(Keywords.If);
                }
                if (IsSequence("else"))
                {
                    Advance(4);
                    return new Token(Keywords.Else);
                }
                if (IsSequence("for"))
                {
                    Advance(3);
                    return new Token(Keywords.For);
                }
                if (IsSequence("var"))
                {
                    Advance(3);
                    return new Token(Identifiers.Variable);
                }
                if (IsSequence("func"))
                {
                    Advance(4);
                    return new Token(Identifiers.Function);
                }
                if (IsSequence("null"))
                {
                    Advance(4);
                    return new Token(Identifiers.Null);
                }
                if (IsSequence("&&"))
                {
                    Advance(2);
                    return new Token(Identifiers.And);
                }
                if (IsSequence("||"))
                {
                    Advance(2);
                    return new Token(Identifiers.Or);
                }
                if (IsSequence("true"))
                {
                    Advance(4);
                    return new Token(Keywords.True);
                }
                if (IsSequence("false"))
                {
                    Advance(5);
                    return new Token(Keywords.False);
                }
                if (IsSequence("string"))
                {
                    Advance(6);
                    return new Token(Identifiers.String);
                }
                if (IsSequence("int"))
                {
                    Advance(3);
                    return new Token(Identifiers.Integer);
                }
                if (IsSequence("float"))
                {
                    Advance(5);
                    return new Token(Identifiers.Float);
                }
                if (IsSequence("bool"))
                {
                    Advance(4);
                    return new Token(Identifiers.Bool);
                }
                if (IsSequence("in"))
                {
                    Advance(2);
                    return new Token(Keywords.In);
                }
                if (IsSequence("range"))
                {
                    Advance(5);
                    return new Token(Keywords.Range);
                }
                if (IsSequence("return"))
                {
                    Advance(6);
                    return new Token(Identifiers.Return);
                }
                if (char.IsLetter(currentChar) || currentChar == '_')
                {
                    return new Token(Identifiers.Identifier, GetIdentifier());
                }
                throw new Exception($"Wrong character: {currentChar}");
            }
            return null;
        }
    }
    public enum Keywords
    {
        If, For, In, Else, True, False, Range
    }
    public enum Identifiers
    {
        String, Integer, Float, Bool, Identifier, Variable, LeftParen, RightParen, Semicolon, Equals, FunctionCall, Function, Add, Subtract, Multiply, Divide, Modulo, LeftCurly, RightCurly, Comma, Greater, Smaller, Not, LeftSqrBracket, RightSqrBracket, Null, Compare, And, Or, NotEquals, GreaterEquals,SmallerEquals,Colon, Return
    }
}
