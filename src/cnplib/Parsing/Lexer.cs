using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using CNP.Helper;
using System.Linq;

namespace CNP.Parsing
{
    public enum TokenType
    {
        BracketClose,
        BracketOpen,
        Colon,
        Comma,
        Pipe,
        Identifier,
        MustacheClose,
        MustacheOpen,
        MinusInt,
        ValueInt,
        ValueString,
        VariableName
    }
    public class Lexem
    {
        public readonly TokenType Type;
        public readonly string Content;
        public readonly int Position;
        public Lexem(TokenType t, string c, int p)
        {
            Type = t;
            Content = c;
            Position = p;
        }
        public override bool Equals(object obj)
        {
            return obj is Lexem le && le.Type == this.Type && le.Content == this.Content && le.Position == this.Position;
        }
        public override int GetHashCode()
        {
            return Content.GetHashCode();
        }
        public override string ToString()
        {
            return this.Position + ", " + this.Type.ToString() + ", " + this.Content;
        }
    }
    public class Lexer
    { 

        static readonly IEnumerable<(TokenType, string)> tokenRegexes = new List<(TokenType, string)>() {
            (TokenType.MustacheClose, @"^\s*\}"),
            (TokenType.MustacheOpen, @"^\s*\{"),
            (TokenType.BracketClose, @"^\s*\]"),
            (TokenType.BracketOpen, @"^\s*\["),
            (TokenType.Colon, @"^\s*:"),
            (TokenType.Comma, @"^\s*,"),
            (TokenType.Pipe, @"^\s*\|"),
            (TokenType.MinusInt, @"^\s*\-\d+"),
            (TokenType.ValueInt, @"^\s*\d+"),
            (TokenType.ValueString, @"^\s*'[^']*'"),
            (TokenType.VariableName, @"^\s*([A-Z]+[a-zA-Z0-9]*)"),
            (TokenType.Identifier, @"^\s*([a-z]+[a-zA-Z0-9]*)")

             };

        public static IEnumerable<Lexem> Tokenize(string input, int distanceToBeginning = 0)
        {
            if(!string.IsNullOrEmpty(input))
            {
                foreach((TokenType t, string rex) in tokenRegexes)
                {
                    Match m = Regex.Match(input, rex);
                    if (m.Success)
                    {
                        input = input.Substring(m.Index + m.Length);
                        string trimmedValue = m.Value.Trim();
                        int spacesPreceeding = m.Value.Length - trimmedValue.Length;
                        Lexem lex = new Lexem(t, m.Value.Trim(), distanceToBeginning+m.Index+spacesPreceeding);
                        return Iterators.Singleton(lex).Concat(Tokenize(input, distanceToBeginning+m.Index+m.Length));
                    }
                }
            }
            return Iterators.Empty<Lexem>();
        }
    }
}

