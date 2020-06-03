using System;
using System.Collections.Generic;
using System.Text;

namespace GrammarParser.ParserOperation
{
    public abstract class ParserOp
    {
        public abstract void Apply(ParserState state);
    }

    public class ParserShiftOp : ParserOp
    {
        public int ShiftToState { get; }
        public ParserShiftOp(int state)
        {
            ShiftToState = state;
        }
        public override void Apply(ParserState state)
        {
            state.StateStack.Push(ShiftToState);
            state.ParsedStack.Push(state.InputStack.Pop());
        }

        public override string ToString()
        {
            return "s" + ShiftToState;
        }
    }

    public class ParserReduceOp: ParserOp
    {
        public StateProduction ReduceWith { get; }
        public int Index { get; }
        public ParserReduceOp(StateProduction reduction, int index)
        {
            ReduceWith = reduction;
            Index = index;
        }

        public override void Apply(ParserState state)
        {
            ParserStackItem[] subItems = new ParserStackItem[ReduceWith.Production.Pattern.Length];
            for(var i = subItems.Length - 1; i >= 0; i--)
            {
                subItems[i] = state.ParsedStack.Pop();
                state.StateStack.Pop();
            }
            ParserStackItem reduction = new ParserStackItem(ReduceWith.Production.Name, subItems);
            state.InputStack.Push(reduction);
        }

        public override string ToString()
        {
            return "r" + Index;
        }
    }
}
