using GrammarParser.TableBuilding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrammarParser
{
    public class ParseTable
    {
        private StackItemPattern[] ProductionPatterns;
        private StackItemPattern[] TokenPatterns;
        private ParserOperation.ParserOp[,] operations;
        private HashSet<StackItemPattern> AllPatterns;
        private StateMachine _States;
        public ParseTable(IReadOnlyList<Production> loadFrom)
        {
            Dictionary<string, Production[]> AllProductions =
                loadFrom.GroupBy(x => x.Name).ToDictionary(g => g.Key, g => g.ToArray());
            AllProductions.Add("__INIT", new Production[] { new Production("__INIT", new StackItemPattern(EPatternType.Production, loadFrom[0].Name), new StackItemPattern(EPatternType.EndOfInput, "")) });

            var stateMachine = new StateMachine(AllProductions, "__INIT");
            _States = stateMachine;

            AllPatterns = new HashSet<StackItemPattern>();
            foreach(var production in loadFrom)
            {
                var prodMatch = new StackItemPattern(EPatternType.Production, production.Name);
                if (!AllPatterns.Contains(prodMatch))
                    AllPatterns.Add(prodMatch);

                foreach (var pattern in production.Pattern)
                {
                    if (!AllPatterns.Contains(pattern))
                        AllPatterns.Add(pattern);
                }
            }
            ProductionPatterns = AllPatterns.Where(x => x.PatternType == EPatternType.Production).ToArray();
            TokenPatterns = AllPatterns.Where(x => x.PatternType == EPatternType.Literal).Concat(AllPatterns.Where(x => x.PatternType == EPatternType.TokenType)).ToArray();
            operations = new ParserOperation.ParserOp[stateMachine.NumberedNodes.Length, ProductionPatterns.Length + TokenPatterns.Length + 1]; // EOF is always the last column.

            for(var i = 0; i < stateMachine.NumberedNodes.Length; i++)
            {
                var reductionOp = stateMachine.NumberedNodes[i].CompletedProduction != null ?
                                  new ParserOperation.ParserReduceOp(stateMachine.NumberedNodes[i].CompletedProduction, IndexOfFirstMatch(ProductionPatterns, new ParserStackItem(stateMachine.NumberedNodes[i].CompletedProduction.Production.Name, new ParserStackItem[0]))) :
                                  null;

                for(var patternIndex = 0; patternIndex < operations.GetLength(1); patternIndex++)
                {
                    var pattern = patternIndex < ProductionPatterns.Length ? ProductionPatterns[patternIndex] :
                                  patternIndex < ProductionPatterns.Length + TokenPatterns.Length ? TokenPatterns[patternIndex - ProductionPatterns.Length] :
                                  new StackItemPattern(EPatternType.EndOfInput, "");
                    if(stateMachine.NumberedNodes[i].Continuations.ContainsKey(pattern))
                    {
                        operations[i, patternIndex] = new ParserOperation.ParserShiftOp(stateMachine.NumberedNodes[i].Continuations[pattern].Id);
                    }
                    else
                    {
                        operations[i, patternIndex] = reductionOp;
                    }
                }
            }
        }

        public ParserOperation.ParserOp Lookup(int CurrentState, ParserStackItem NextInput)
        {
            int lookaheadIndex;
            if(NextInput == null)
            {
                lookaheadIndex = ProductionPatterns.Length + TokenPatterns.Length;
            }
            else if(NextInput.ProductionName != null)
            {
                lookaheadIndex = IndexOfFirstMatch(ProductionPatterns, NextInput);
            }
            else
            {
                lookaheadIndex = IndexOfFirstMatch(TokenPatterns, NextInput) + ProductionPatterns.Length;
            }

            if(lookaheadIndex == -1)
            {
                throw new Exceptions.UnknownTokenException(NextInput);
            }
            else
            {
                var transition = operations[CurrentState, lookaheadIndex];
                if(transition == null)
                {
                    throw new Exceptions.SyntaxError(NextInput.Token, ValidNextPatterns(CurrentState));
                }
                return transition;
            }
        }

        public StackItemPattern[] ValidNextPatterns(int currentState)
        {
            return Enumerable.Range(0, operations.GetLength(1))
                .Select(x => Tuple.Create(x, operations[currentState, x]))
                .Where(t => t.Item2 != null)
                .Select(t => t.Item1)
                .Select(index => 
                    index < ProductionPatterns.Length ? 
                        ProductionPatterns[index] :
                    index < ProductionPatterns.Length + TokenPatterns.Length ?
                        TokenPatterns[index - ProductionPatterns.Length] :
                    new StackItemPattern(EPatternType.EndOfInput, "")).ToArray();
        }

        private int IndexOfFirstMatch(StackItemPattern[] pattern, ParserStackItem item)
        {
            for(var i = 0; i < pattern.Length; i++)
            {
                if (pattern[i].Matches(item))
                    return i;
            }
            return -1;
        }

        public override string ToString()
        {
            int stateNumberLength = operations.GetLength(0).ToString().Length;
            int tallestPattern = AllPatterns.Select(x => x.ToString().Length).Max();

            int colWidth = stateNumberLength + 1;

            StringBuilder result = new StringBuilder();
            for(var i = 0; i < tallestPattern; i++)
            {
                for(var j = 0; j < stateNumberLength + 1; j++)
                {
                    result.Append(" ");
                }
                result.Append("|");
                foreach (var pattern in ProductionPatterns.Concat(TokenPatterns).Concat(new StackItemPattern[] { new StackItemPattern(EPatternType.EndOfInput, "") }))
                {
                    var str = pattern.ToString();
                    var index = str.Length - tallestPattern + i;
                    if (index < 0)
                    {
                        result.Append("  ".PadLeft(colWidth));
                        result.Append("|");
                    }
                    else
                    {
                        result.Append((" " + str[index]).PadRight(colWidth));
                        result.Append("|");
                    }
                }
                result.AppendLine();
            }
            result.Append("-".PadLeft(colWidth, '-'));
            result.Append("+");
            for (var i = 0; i < operations.GetLength(1); i++)
            {
                result.Append("-".PadLeft(colWidth, '-'));
                result.Append("+");
            }
            result.AppendLine();
            for(var stateRow = 0; stateRow < operations.GetLength(0); stateRow++)
            {
                result.Append(stateRow.ToString().PadLeft(colWidth) + "|");

                for(var opIndex = 0; opIndex < operations.GetLength(1); opIndex++)
                {
                    var op = operations[stateRow, opIndex];
                    result.Append((op?.ToString() ?? "").PadLeft(colWidth) + "|");
                }


                result.AppendLine();
            }

            return result.ToString();
        }
    }
}
