using System;
using System.Collections.Generic;
using System.Text;

namespace GrammarParser
{
    public enum EPatternType
    {
        Production,
        TokenType,
        Literal,
        EndOfInput
    }

    public class StackItemPattern
    {
        public EPatternType PatternType { get; }
        public string Value { get; }
        public StackItemPattern(EPatternType type, string value)
        {
            PatternType = type;
            Value = value;
        }

        internal bool Matches(ParserStackItem stackItem)
        {
            if(stackItem == null)
            {
                return PatternType == EPatternType.EndOfInput;
            }
            switch(PatternType)
            {
                case EPatternType.Literal:
                    return stackItem.Token != null && stackItem.Token.Value == Value;
                case EPatternType.TokenType:
                    return stackItem.Token != null && stackItem.Token.TokenType == Value;
                case EPatternType.Production:
                    return stackItem.ProductionName != null && stackItem.ProductionName == Value;
                case EPatternType.EndOfInput:
                    return false;
                default:
                    throw new InvalidOperationException("Unknown Pattern Type");
            }
        }

        public override bool Equals(object obj)
        {
            if(obj is StackItemPattern other)
            {
                return PatternType == other.PatternType && (Value.Equals(other.Value) || PatternType == EPatternType.EndOfInput);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return PatternType.GetHashCode() ^ (PatternType == EPatternType.EndOfInput ? 0 : Value.GetHashCode());
        }

        public override string ToString()
        {
            switch (PatternType)
            {
                case EPatternType.Literal:
                    return Value;
                case EPatternType.TokenType:
                    return '<' + Value + '>';
                case EPatternType.Production:
                    return '{' + Value + '}';
                case EPatternType.EndOfInput:
                    return "$$";
                default:
                    throw new InvalidOperationException("Unknown Pattern Type");
            }
        }
    }
}
