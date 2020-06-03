using System;
using System.Linq;

namespace GrammarParser
{
    public class Production
    {
        public string Name { get; }
        public StackItemPattern[] Pattern { get; }

        internal StateProduction FirstState { get; }
        public int? Precedence { get; set; }

        public Production(string name, params StackItemPattern[] pattern)
        {
            Name = name;
            Pattern = pattern;
            FirstState = new StateProduction(this, 0);
        }

        public override string ToString()
        {
            return $"{Name} <- {string.Join(' ', Pattern.Select(x=>x.ToString()))}";
        }
    }
}
