using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrammarParser.TableBuilding
{
    class StateMachine
    {
        Dictionary<StateKey, StateNode> nodes;
        Dictionary<string, Production[]> AllProductions;

        public StateNode[] NumberedNodes { get; }
        public StateMachine(Dictionary<string, Production[]> allProductions, string firstProductionName)
        {
            AllProductions = allProductions;
            nodes = new Dictionary<StateKey, StateNode>();

            var firstNode = GetOrCreateNode(new StateKey(allProductions[firstProductionName].Select(x=>x.FirstState), allProductions));

            NumberedNodes = new StateNode[] { firstNode }.Concat(nodes.Values.Except(new StateNode[] { firstNode })).ToArray();
            for(var i = 0; i < NumberedNodes.Length; i++)
            {
                NumberedNodes[i].SetId(i);
            }
        }

        public StateNode GetOrCreateNode(StateKey key)
        {
            if (!nodes.ContainsKey(key))
            {
                nodes[key] = new StateNode(key, this, AllProductions);
                // DONT do this in the constructor: We need this item to exist in the dict before this is called.
                nodes[key].GenerateContinuations();
            }
            return nodes[key];
        }
    }
}
