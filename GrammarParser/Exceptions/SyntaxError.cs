using System;
using System.Collections.Generic;
using System.Text;

namespace GrammarParser.Exceptions
{
    public class SyntaxError: Exception
    {
        public TokenizedItem NextItem { get; }
        public StackItemPattern[] ValidNextTokens { get; }

        public SyntaxError(TokenizedItem next, StackItemPattern[] valid)
            :base($"Syntax error: Unexpected '{next.Value}' at {next.LineNumber}:{next.ColNumber}")
        {
            NextItem = next;
            ValidNextTokens = valid;
        }
    }
}
