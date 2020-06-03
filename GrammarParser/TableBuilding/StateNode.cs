using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GrammarParser.TableBuilding
{
    class StateNode
    {
        public StateKey Key { get; }
        public Dictionary<StackItemPattern, StateNode> Continuations;
        public StateProduction CompletedProduction;

        StateMachine Machine;
        Dictionary<string, Production[]> AllProductions;

        public int Id { get; private set; }
        public StateNode(StateKey Key, StateMachine machine, Dictionary<string, Production[]> allProductions)
        {
            this.Key = Key;

            Continuations = new Dictionary<StackItemPattern, StateNode>();
            Machine = machine;
            AllProductions = allProductions;
        }

        public void GenerateContinuations()
        {
            foreach(var continuation in Key.Where(x=>!x.IsFinished).GroupBy(x => x.NextPattern))
            {
                var key = new StateKey(continuation.Select(x=>x.NextProduction), AllProductions);
                Continuations.Add(continuation.Key, Machine.GetOrCreateNode(key));
            }

            var finished = Key.Where(x => x.IsFinished).ToArray();
            if(finished.Length == 1)
            {
                CompletedProduction = finished[0];
            }
            else if(finished.Length > 1)
            {
                var maxValue = finished.Max(x => x.Precedence.Value);
                var maxFinished = finished.Where(x => x.Precedence.Value == maxValue).ToArray();
                if(maxFinished.Length == 1)
                {
                    CompletedProduction = finished.OrderBy(x => x.Precedence.Value).First();
                }
                else
                {
                    // TODO: better error message
                    throw new InvalidOperationException("Grammar is ambiguous");
                }
            }
        }

        public void SetId(int number)
        {
            Id = number;
        }
    }
}
