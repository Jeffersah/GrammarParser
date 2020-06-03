using System;
using System.Collections.Generic;
using System.Text;

namespace GrammarParser
{
    public class StateProduction
    {
        public int? Precedence => Production.Precedence;
        public Production Production { get; }
        public int Index { get; }

        private StateProduction _Next;

        public StateProduction(Production production, int index = 0)
        {
            Production = production;
            Index = index;
            _Next = null;
        }

        public StateProduction NextProduction
        {
            get
            {
                if (IsFinished)
                    return null;
                if (_Next == null)
                    _Next = new StateProduction(Production, Index + 1);
                return _Next;
            }
        }

        public StackItemPattern NextPattern => Index == Production.Pattern.Length ? null : Production.Pattern[Index];
        public bool IsFinished => Index == Production.Pattern.Length;

        public override bool Equals(object obj)
        {
            if(obj is StateProduction other)
            {
                return Production == other.Production && Index == other.Index;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Production.GetHashCode() ^ Index;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(Production.Name + " <- ");
            for(int i = 0; i < Production.Pattern.Length; i++)
            {
                if (i == Index)
                    result.Append("· ");
                result.Append(Production.Pattern[i].ToString());
                result.Append(" ");
            }
            if(Index == Production.Pattern.Length)
                result.Append("·");
            return result.ToString();
        }
    }
}
