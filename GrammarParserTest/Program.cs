using GrammarParser;
using GrammarParser.ParserOperation;
using GrammarParser.Tokenization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GrammarParserTest
{
    class Program
    {
        static void Main(string[] args)
        {

            string input = @"3+(2*4+5*(-2+4))";

            //var tokenizer = new BasicTokenizer();
            //var splitter = new BasicSplitter();

            //splitter.CharacterCategories["operator"] = new HashSet<char>(new char[]
            //{
            //    '+', '-', '*', '/', '(', ')', '=', '.', '{', '}', '<', '>'
            //});

            //splitter.CharacterCategories["eos"] = new HashSet<char>(new char[]
            //{
            //    ';'
            //});

            //splitter.CharacterCategories["number"] = new HashSet<char>(Enumerable.Range(0, 10).Select(x => (char)('0' + x)));
            //splitter.CharacterCategories["name"] = new HashSet<char>(Enumerable.Range(0, ('z'-'a')).Select(x => (char)('a' + x)));

            //var tokens = tokenizer.Tokenize(input).ToArray();
            //tokens = splitter.Split(tokens).ToArray();

            int linenum = 0;
            int col = 0;
            var tokens = input.Select(value =>
            {
                var res = ToToken(value, linenum, col);
                if (value == '\n')
                {
                    linenum++;
                    col = 0;
                }
                else
                {
                    col++;
                }
                return res;
            });

            Console.WriteLine("Reading Grammar");

            var productions = new List<Production>();
            var lines = File.ReadAllLines("MathGrammar.txt");
            foreach (var line in lines)
            {
                if (line.Trim().Length == 0 || line.Trim().StartsWith("#"))
                {
                    continue;
                }

                var caretPosition = line.IndexOf("->");
                var productionName = line.Substring(0, caretPosition).Trim();
                int priority = 0;
                if (productionName.StartsWith("["))
                {
                    priority = int.Parse(productionName.Substring(1, productionName.IndexOf(']') - 1));
                    productionName = productionName.Substring(productionName.IndexOf(']') + 1).Trim();
                }

                var spl = line.Substring(caretPosition + 2).Split(' ').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
                var patterns = new StackItemPattern[spl.Length];
                for (var i = 0; i < spl.Length; i++)
                {
                    var value = spl[i];
                    EPatternType pType = EPatternType.Literal;
                    if (value.StartsWith("{") && value.EndsWith("}"))
                    {
                        pType = EPatternType.Production;
                        value = value.Substring(1, value.Length - 2);
                    }
                    else if (value.StartsWith("<") && value.EndsWith(">"))
                    {
                        pType = EPatternType.TokenType;
                        value = value.Substring(1, value.Length - 2);
                    }
                    else if (value == "$$")
                    {
                        pType = EPatternType.EndOfInput;
                        value = "";
                    }
                    patterns[i] = new StackItemPattern(pType, value);
                }
                productions.Add(new Production(productionName, patterns) { Precedence = priority });
            }

            foreach (var production in productions)
            {
                Console.WriteLine(production);
            }


            Console.WriteLine("Next: Build Parse Table [press enter]");
            Console.ReadLine();

            ParseTable parseTable = new ParseTable(productions);
            Console.WriteLine(parseTable);
            File.WriteAllText("output.txt", parseTable.ToString());
            Console.ReadLine();


            Console.WriteLine("Next: Parse");
            Console.WriteLine(input);
            Console.ReadLine();

            Parser p = new Parser(parseTable, tokens);

            var result = p.Step();
            bool shouldSkip = false;
            while (result == null)
            {
                if (!shouldSkip)
                {
                    PrintState(p.State);
                    Console.WriteLine("(Type SKIP to skip parsing)");
                    shouldSkip = Console.ReadLine().ToUpper() == "SKIP";
                }
                result = p.Step();
            }

            PrintRecursive(result);
            Console.ReadLine();
        }

        static void PrintState(ParserState state)
        {
            Console.WriteLine("Stack: ");
            foreach(var item in state.ParsedStack.Take(10).Reverse())
            {
                WriteItem(item);
                Console.Write(" ");
            }

            Console.WriteLine();
            Console.WriteLine("Input: ");
            foreach (var item in state.InputStack.Take(10))
            {
                WriteItem(item);
                Console.Write(" ");
            }
        }
        static void WriteItem(ParserStackItem item)
        {
            if(item == null)
            {
                Console.Write("EOF");
            }
            else if(item.ProductionName != null)
            {
                Console.Write("{" + item.ProductionName + "}");
            }
            else if(item.Token.TokenType == "newline" || item.Token.TokenType == "space")
            {
                Console.Write("<" + item.Token.TokenType + ">");
            }
            else
            {
                Console.Write(item.Token.Value);
            }
        }

        static void PrintRecursive(ParserStackItem item, int tabDepth = 0)
        {
            string tabs = new string(' ', tabDepth);
            if (item == null)
            {
                Console.WriteLine(tabs + "EOF");
            }
            else if (item.Token != null)
            {
                Console.WriteLine(tabs + item.Token);
            }
            else
            {
                Console.WriteLine(tabs + "{" + item.ProductionName + "}");
                foreach (var subItem in item.ProductionComponents)
                {
                    PrintRecursive(subItem, tabDepth + 1);
                }
            }
        }

        static TokenizedItem ToToken(char c, int line, int col)
        {
            if (c >= '0' && c <= '9')
            {
                return new TokenizedItem(c.ToString(), "digit", line, col);
            }
            else if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
            {
                return new TokenizedItem(c.ToString(), "alpha", line, col);
            }
            else if (c == '\n')
            {
                return new TokenizedItem(c.ToString(), "newline", line, col);
            }
            else if (char.IsWhiteSpace(c))
            {
                return new TokenizedItem(c.ToString(), "space", line, col);
            }
            else return new TokenizedItem(c.ToString(), "other", line, col);
        }
    }
}
