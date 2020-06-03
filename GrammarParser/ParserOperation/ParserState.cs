using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrammarParser.ParserOperation
{
    public class ParserState
    {
        public Stack<ParserStackItem> ParsedStack;
        public Stack<int> StateStack;
        public Stack<ParserStackItem> InputStack;

        public ParserState(IEnumerable<TokenizedItem> tokens)
        {
            ParsedStack = new Stack<ParserStackItem>();
            StateStack = new Stack<int>();
            StateStack.Push(0);

            InputStack = new Stack<ParserStackItem>(tokens.Count() + 1);
            InputStack.Push(null);
            foreach(var item in tokens.Reverse())
            {
                InputStack.Push(new ParserStackItem(item));
            }
        }
    }
}
