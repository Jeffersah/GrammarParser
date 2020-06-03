using System;
using System.Collections.Generic;
using System.Text;

namespace GrammarParser.Exceptions
{
    public class UnknownTokenException: Exception
    {
        public TokenizedItem UnrecognizedItem { get; }

        public UnknownTokenException(ParserStackItem stackItem)
            :base($"An input item does not match any column of the parse table. Value = '{stackItem.Token.Value}', Type = '{stackItem.Token.TokenType}'")
        {
            UnrecognizedItem = stackItem.Token;
        }
    }
}
