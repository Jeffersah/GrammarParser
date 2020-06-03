using System;
using System.Collections.Generic;
using System.Text;

namespace GrammarParser
{
    public class ParserStackItem
    {
        public TokenizedItem Token { get; }
        public string ProductionName { get; }
        public ParserStackItem[] ProductionComponents { get; }

        public ParserStackItem(TokenizedItem item)
        {
            Token = item;
            ProductionName = null;
            ProductionComponents = null;
        }

        public ParserStackItem(string productionName, ParserStackItem[] components)
        {
            Token = null;
            ProductionName = productionName;
            ProductionComponents = components;
        }
    }
}
