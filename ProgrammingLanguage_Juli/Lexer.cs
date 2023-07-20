using Juli_ProgLang.Content;
using System;
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

        private (string, SyntaxKind) GetNumberString()
        {
            StringBuilder result = new StringBuilder();
            while (currentChar != '\0' && char.IsDigit(currentChar) || currentChar == '.')
            {
                result.Append(currentChar);
                Advance();
            }
            return (result.ToString(), result.ToString().Contains(".") ? SyntaxKind.Float_ID : SyntaxKind.Integer_ID);
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

            //example: 'in' is in the word 'index':
            if (char.IsLetter(Text[Position + sequence.Length]))
                return false;

            return currentChar == sequence[0] && (Position + sequence.Length) < Text.Length && Text.Substring(Position, sequence.Length).Equals(sequence);
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
                    return new Token(SyntaxKind.String_ID, GetString());
                if (currentChar == '(')
                {
                    Advance();
                    return new Token(SyntaxKind.LeftParen_ID);
                }
                if (currentChar == ')')
                {
                    Advance();
                    return new Token(SyntaxKind.RightParen_ID);
                }
                if (currentChar == '}')
                {
                    Advance();
                    return new Token(SyntaxKind.RightCurly_ID);
                }
                if (currentChar == '{')
                {
                    Advance();
                    return new Token(SyntaxKind.LeftCurly_ID);
                }
                if (currentChar == '[')
                {
                    Advance();
                    return new Token(SyntaxKind.LeftSqrBracket_ID);
                }
                if (currentChar == ']')
                {
                    Advance();
                    return new Token(SyntaxKind.RightSqrBracket_ID);
                }
                if (currentChar == ';')
                {
                    Advance();
                    return new Token(SyntaxKind.Semicolon_ID);
                }
                if (currentChar == '=')
                {
                    Advance();
                    if (currentChar == '=') //check also for == (compare)
                    {
                        Advance();
                        return new Token(SyntaxKind.Compare_ID);
                    }
                    return new Token(SyntaxKind.Equals_ID);
                }
                if (currentChar == '>')
                {
                    Advance();
                    if (currentChar == '=') //check also for >= (greater equals)
                    {
                        Advance();
                        return new Token(SyntaxKind.GreaterEquals_ID);
                    }
                    return new Token(SyntaxKind.Greater_ID);
                }
                if (currentChar == '<')
                {
                    Advance();
                    if (currentChar == '=') //check also for <= (smaller equals)
                    {
                        Advance();
                        return new Token(SyntaxKind.SmallerEquals_ID);
                    }
                    return new Token(SyntaxKind.Smaller_ID);
                }
                if (currentChar == '!')
                {
                    Advance();
                    if (currentChar == '=')
                    {
                        Advance();
                        return new Token(SyntaxKind.NotEquals_ID);
                    }
                    return new Token(SyntaxKind.Not_ID);
                }
                if (currentChar == ':')
                {
                    Advance();
                    return new Token(SyntaxKind.Colon_ID);
                }
                if (currentChar == '+')
                {
                    Advance();
                    return new Token(SyntaxKind.Add_ID);
                }
                if (currentChar == '-')
                {
                    Advance();
                    return new Token(SyntaxKind.Subtract_ID);
                }
                if (currentChar == '*')
                {
                    Advance();
                    return new Token(SyntaxKind.Multiply_ID);
                }
                if (currentChar == '/')
                {
                    Advance();
                    return new Token(SyntaxKind.Divide_ID);
                }
                if (currentChar == '%')
                {
                    Advance();
                    return new Token(SyntaxKind.Modulo_ID);
                }
                if (currentChar == ',')
                {
                    Advance();
                    return new Token(SyntaxKind.Comma_ID);
                }
                if (IsSequence("if"))
                {
                    Advance(2);
                    return new Token(SyntaxKind.If_KW);
                }
                if (IsSequence("else"))
                {
                    Advance(4);
                    return new Token(SyntaxKind.Else_KW);
                }
                if (IsSequence("for"))
                {
                    Advance(3);
                    return new Token(SyntaxKind.For_KW);
                }
                if (IsSequence("var"))
                {
                    Advance(3);
                    return new Token(SyntaxKind.Variable_ID);
                }
                if (IsSequence("func"))
                {
                    Advance(4);
                    return new Token(SyntaxKind.Function_ID);
                }
                if (IsSequence("null"))
                {
                    Advance(4);
                    return new Token(SyntaxKind.Null_ID);
                }
                if (IsSequence("&&"))
                {
                    Advance(2);
                    return new Token(SyntaxKind.And_ID);
                }
                if (IsSequence("||"))
                {
                    Advance(2);
                    return new Token(SyntaxKind.Or_ID);
                }
                if (IsSequence("true"))
                {
                    Advance(4);
                    return new Token(SyntaxKind.True_KW);
                }
                if (IsSequence("false"))
                {
                    Advance(5);
                    return new Token(SyntaxKind.False_KW);
                }
                if (IsSequence("string"))
                {
                    Advance(6);
                    return new Token(SyntaxKind.String_ID);
                }
                if (IsSequence("int"))
                {
                    Advance(3);
                    return new Token(SyntaxKind.Integer_ID);
                }
                if (IsSequence("float"))
                {
                    Advance(5);
                    return new Token(SyntaxKind.Float_ID);
                }
                if (IsSequence("bool"))
                {
                    Advance(4);
                    return new Token(SyntaxKind.Bool_ID);
                }
                if (IsSequence("in"))
                {
                    Advance(2);
                    return new Token(SyntaxKind.In_KW);
                }
                if (IsSequence("range"))
                {
                    Advance(5);
                    return new Token(SyntaxKind.Range_KW);
                }
                if (IsSequence("return"))
                {
                    Advance(6);
                    return new Token(SyntaxKind.Return_ID);
                }
                if (IsSequence("len"))
                {
                    Advance(3);
                    return new Token(SyntaxKind.Len_KW);
                }
                if (char.IsLetter(currentChar) || currentChar == '_')
                {
                    return new Token(SyntaxKind.Identifier_ID, GetIdentifier());
                }
                throw new Exception($"Wrong character: {currentChar}");
            }
            return null;
        }
    }
    public enum SyntaxKind
    {
        If_KW, For_KW, In_KW, Else_KW, True_KW, False_KW, Range_KW, Len_KW,
        String_ID, Integer_ID, Float_ID, Bool_ID, Identifier_ID, Variable_ID, LeftParen_ID, RightParen_ID, Semicolon_ID, Equals_ID, FunctionCall_ID, Function_ID, Add_ID, Subtract_ID, Multiply_ID, Divide_ID, Modulo_ID, LeftCurly_ID, RightCurly_ID, Comma_ID, Greater_ID, Smaller_ID, Not_ID, LeftSqrBracket_ID, RightSqrBracket_ID, Null_ID, Compare_ID, And_ID, Or_ID, NotEquals_ID, GreaterEquals_ID, SmallerEquals_ID, Colon_ID, Return_ID
    }
}
