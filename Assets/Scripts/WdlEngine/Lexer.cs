﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WdlEngine
{
    internal sealed class Lexer
    {
        public static IEnumerable<Token> Lex(CommentStyle commentStyle, TextReader reader)
        {
            var lexer = new Lexer(commentStyle, reader);
            while (true)
            {
                var token = lexer.Next();
                yield return token;
                if (token.Type == TokenType.EndOfInput) break;
            }
        }

        private enum Mode
        {
            Normal,
            AngleBracketString,
        }

        private static string[] CommandsWithAngleBracketStrings = new[]
        {
            "BIND",
            "INCLUDE",
            "MAPFILE",
            "PALFILE",
        };

        private readonly StringBuilder _peekBuffer = new StringBuilder();
        private readonly CommentStyle _commentStyle;
        private readonly TextReader _sourceReader;

        private Mode _mode = Mode.Normal;

        private Lexer(CommentStyle commentStyle, TextReader sourceReader)
        {
            _commentStyle = commentStyle;
            _sourceReader = sourceReader;
        }

        private Token Next()
        {
        start:
            var ch = Peek();
            if (ch == '\0') return Advance(TokenType.EndOfInput, 0);

            // Ignore space
            if (char.IsWhiteSpace(ch))
            {
                Advance();
                goto start;
            }

            // Ignore comments
            if ((_commentStyle == CommentStyle.Hashmark && ch == '#')
             || (_commentStyle == CommentStyle.DoubleSlash && ch == '/' && Peek(1) == '/'))
            {
                Advance(2);
                while (!IsNewline(Peek(0, '\n'))) Advance();
                goto start;
            }

            // String literal
            // NOTE: We check it here so angle brackets don't have a chance to be reinterpreted later
            var (stringOpen, stringClose) = _mode == Mode.Normal
                ? ('"', '"')
                : ('<', '>');
            if (ch == stringOpen)
            {
                Advance();
                var result = new StringBuilder();
                while (true)
                {
                    ch = Peek(0, stringClose);
                    if (ch == '\0') break;
                    if (ch == stringClose)
                    {
                        Advance();
                        break;
                    }
                    if (ch == '\\')
                    {
                        Advance();
                        var toEscape = Peek();
                        if (toEscape != '\0') Advance();
                        result.Append(Escape(toEscape));
                        continue;
                    }
                    result.Append(ch);
                    Advance();
                }
                return new Token(TokenType.String, result.ToString());
            }

            // Single- and two-character tokens
            switch (ch)
            {
            case '{': return Advance(TokenType.OpenBrace, 1);
            case '}': return Advance(TokenType.CloseBrace, 1);
            case '.': return Advance(TokenType.Dot, 1);
            case ',': return Advance(TokenType.Comma, 1);
            case ':': return Advance(TokenType.Colon, 1);
            case ';':
                // End of the current command, reset the mode
                // The next command will decide if we need to switch back or not
                _mode = Mode.Normal;
                return Advance(TokenType.Semicolon, 1);
            case '+': return Peek(1) == '=' 
                ? Advance(TokenType.PlusAssign, 2)
                : Advance(TokenType.Plus, 1);
            case '-': return Peek(1) == '=' 
                ? Advance(TokenType.MinusAssign, 2)
                : Advance(TokenType.Minus, 1);
            case '*': return Peek(1) == '=' 
                ? Advance(TokenType.StarAssign, 2)
                : Advance(TokenType.Star, 1);
            case '/': return Peek(1) == '=' 
                ? Advance(TokenType.SlashAssign, 2)
                : Advance(TokenType.Slash, 1);
            case '%': return Advance(TokenType.Percent, 1);
            case '^': return Advance(TokenType.BitXor, 1);
            case '&': return Peek(1) == '&'
                ? Advance(TokenType.And, 2)
                : Advance(TokenType.BitAnd, 1);
            case '|': return Peek(1) == '|'
                ? Advance(TokenType.Or, 2)
                : Advance(TokenType.BitOr, 1);
            case '!' when Peek(1) == '=': return Advance(TokenType.NotEqual, 2);
            case '<': return Peek(1) == '='
                ? Advance(TokenType.LessEqual, 2)
                : Advance(TokenType.Less, 1);
            case '>': return Peek(1) == '='
                ? Advance(TokenType.GreaterEqual, 2)
                : Advance(TokenType.Greater, 1);
            case '=': return Peek(1) == '='
                ? Advance(TokenType.Equal, 2)
                : Advance(TokenType.Assign, 1);
            }

            // Numeric
            var isNegative = ch == '-';
            if (char.IsDigit(Peek(isNegative ? 1 : 0)))
            {
                // Must be a number
                var offset = isNegative ? 2 : 1;
                var type = TokenType.Integer;
                while (char.IsDigit(Peek(offset))) ++offset;

                // Check if there's a real part
                if (Peek(offset) == '.')
                {
                    // Real
                    type = TokenType.Real;
                    ++offset;
                    while (char.IsDigit(Peek(offset))) ++offset;
                }

                var text = Take(offset);
                var value = type == TokenType.Integer
                    ? int.Parse(text)
                    : double.Parse(text);
                return new Token(type, value);
            }

            // Identifier
            if (IsIdentifier(ch))
            {
                var offset = 1;
                while (IsIdentifier(Peek(offset))) ++offset;
                var result = Take(TokenType.Identifier, offset);
                if (CommandsWithAngleBracketStrings.Contains(result.ValueString)) _mode = Mode.AngleBracketString;
                return result;
            }

            // Unknown
            return Take(TokenType.Unknown, 1);
        }

        private Token Take(TokenType tokenType, int length) =>
            new Token(tokenType, Take(length));

        private string Take(int length)
        {
            if (length < 1) return string.Empty;
            Peek(length - 1);
            var text = _peekBuffer.ToString(0, length);
            _peekBuffer.Remove(0, length);
            return text;
        }

        private Token Advance(TokenType tokenType, int length)
        {
            this.Advance(length);
            return new Token(tokenType, string.Empty);
        }

        private void Advance(int amount = 1)
        {
            if (amount < 1) return;
            Peek(amount - 1);
            _peekBuffer.Remove(0, amount);
        }

        private char Peek(int offset = 0, char @default = '\0')
        {
            while (_peekBuffer.Length <= offset)
            {
                var read = _sourceReader.Read();
                if (read == -1) return @default;
                _peekBuffer.Append((char)read);
            }
            return _peekBuffer[offset];
        }

        private static bool IsIdentifier(char ch) =>
            char.IsLetterOrDigit(ch) || ch == '_';

        private static bool IsNewline(char ch) => 
            ch == '\n' || ch == '\r';

        private static char Escape(char ch)
        {
            switch (ch)
            {
            case '\\': return '\\';
            case '"': return '"';
            case 'n': return '\n';
            case 'r': return '\r';
            case 't': return '\t';
            default: return ch;
            }
        }
    }
}
