using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GrammarParser.TableBuilding
{
    class StateKey : IReadOnlyCollection<StateProduction>
    {
        private HashSet<StateProduction> Productions { get; }

        public int Count => ((IReadOnlyCollection<StateProduction>)Productions).Count;

        public StateKey(IEnumerable<StateProduction> RootProductions, Dictionary<string, Production[]> allProductions)
        {
            Productions = new HashSet<StateProduction>();
            var pendingQueue = new Queue<StateProduction>(RootProductions);
            HashSet<string> followedContinuations = new HashSet<string>();
            while(pendingQueue.Count != 0)
            {
                var next = pendingQueue.Dequeue();

                // Duplicates are possible, but safe to ignore.
                if (!Productions.Contains(next))
                {
                    Productions.Add(next);

                    if (!next.IsFinished)
                    {
                        var following = next.NextPattern;
                        if (following.PatternType == EPatternType.Production && !followedContinuations.Contains(following.Value))
                        {
                            followedContinuations.Add(following.Value);
                            foreach (var matching in allProductions[following.Value])
                            {
                                pendingQueue.Enqueue(matching.FirstState);
                            }
                        }
                    }
                }
            }
        }

        public override bool Equals(object obj)
        {
            if(obj is StateKey other)
            {
                return other.Productions.SetEquals(Productions);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Productions.Aggregate(0, (a, b) => a ^ b.GetHashCode());
        }

        public IEnumerator<StateProduction> GetEnumerator()
        {
            return ((IReadOnlyCollection<StateProduction>)Productions).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IReadOnlyCollection<StateProduction>)Productions).GetEnumerator();
        }
    }
}
