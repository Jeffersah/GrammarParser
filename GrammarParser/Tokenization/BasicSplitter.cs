using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrammarParser.Tokenization
{
    public class BasicSplitter
    {
        public Dictionary<string, HashSet<char>> CharacterCategories { get; }
        public Dictionary<char, HashSet<string>> Mappings { get; private set; }

        public BasicSplitter()
        {
            CharacterCategories = new Dictionary<string, HashSet<char>>();
            Mappings = new Dictionary<char, HashSet<string>>();
        }
        public void UpdateCharacterMapping()
        {
            Mappings = CharacterCategories.SelectMany(kvp => kvp.Value.Select(v => Tuple.Create(kvp.Key, v))).GroupBy(t => t.Item2).ToDictionary(g => g.Key, g => new HashSet<string>(g.Select(v => v.Item1)));
        }

        public IEnumerable<TokenizedItem> Split(IEnumerable<TokenizedItem> input)
        {
            UpdateCharacterMapping();
            return input.SelectMany(Split);
        }

        public IEnumerable<TokenizedItem> Split(TokenizedItem input)
        {
            if(input.TokenType == "qstring")
            {
                yield return input;
                yield break;
            }
            else
            {
                HashSet<string> ValidNames = new HashSet<string>(Mappings[input.Value[0]]);
                int lastSplit = 0;
                for(var i = 1; i < input.Value.Length; i++)
                {
                    char c = input.Value[i];
                    HashSet<string> valid = Mappings[c];

                    var intersection = new HashSet<string>(ValidNames.Intersect(valid));
                    if(intersection.Count == 0)
                    {
                        yield return new TokenizedItem(input.Value.Substring(lastSplit, i - lastSplit), ValidNames.First(), input.LineNumber, input.ColNumber + lastSplit);
                        lastSplit = i;
                        ValidNames = valid;
                    }
                    else
                    {
                        ValidNames = intersection;
                    }
                }
                yield return new TokenizedItem(input.Value.Substring(lastSplit), ValidNames.First(), input.LineNumber, input.ColNumber + lastSplit);
            }
        }
    }
}
