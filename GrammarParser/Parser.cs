using GrammarParser.ParserOperation;
using System;
using System.Collections.Generic;
using System.Text;

namespace GrammarParser
{
    public class Parser
    {
        public ParseTable ParseTable { get; }
        public ParserState State { get; }
        public Parser(ParseTable table, IEnumerable<TokenizedItem> tokens)
        {
            ParseTable = table;
            State = new ParserState(tokens);
        }

        public ParserStackItem Step()
        {
            var stateNumber = State.StateStack.Peek();
            var operation = ParseTable.Lookup(stateNumber, State.InputStack.Count > 0 ? State.InputStack.Peek() : null);
            operation.Apply(this.State);
            if(operation is ParserReduceOp && (operation as ParserReduceOp).Index == -1)
            {
                // Done!
                return State.InputStack.Pop();
            }
            else
            {
                return null;
            }
        }

        public ParserStackItem Run()
        {
            ParserStackItem result = null;
            while ((result = Step()) == null) { }
            return result;
        }
    }
}
