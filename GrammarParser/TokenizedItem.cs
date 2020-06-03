using System;
using System.Collections.Generic;
using System.Text;

namespace GrammarParser
{
    public class TokenizedItem
    {
        public int LineNumber { get; }
        public int ColNumber { get; }
        public string Value { get; }
        public string TokenType { get; }

        public TokenizedItem(string value, string type, int lineNumber = -1, int colNumber = -1)
        {
            LineNumber = lineNumber;
            ColNumber = colNumber;
            Value = value;
            TokenType = type;
        }

        public override bool Equals(object obj)
        {
            if(obj is TokenizedItem other)
            {
                return Value.Equals(other.Value) && TokenType.Equals(other.TokenType);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode() ^ TokenType.GetHashCode();
        }
        public override string ToString()
        {
            return Value;
        }
    }
}
