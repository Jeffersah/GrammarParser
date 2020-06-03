using System;
using System.Collections.Generic;
using System.Text;

namespace GrammarParser
{
    public class BasicTokenizer
    {
        public BasicTokenizer()
        {
        }

        public IEnumerable<TokenizedItem> Tokenize(string input)
        {
            input = input.Replace("\r\n", "\n");

            int line = 0;
            int col = 0;

            int stoLine = 0;
            int stoCol = 0;
            StringBuilder str = new StringBuilder();
            bool isInQuotes = false;
            bool ignoreNext = false;

            for (var i = 0; i < input.Length; i++)
            {
                var c = input[i];
                if(ignoreNext)
                {
                    ignoreNext = false;
                }
                else
                {
                    if(char.IsWhiteSpace(c))
                    {
                        if(str.Length != 0 && !isInQuotes)
                        {
                            yield return new TokenizedItem(str.ToString(), "string", stoLine, stoCol);
                            str.Clear();
                        }
                        else if(isInQuotes)
                        {
                            if (str.Length == 0)
                            {
                                stoLine = line;
                                stoCol = col;
                            }
                            str.Append(c);
                        }
                    }
                    else if(c == '\\')
                    {
                        if (str.Length == 0)
                        {
                            stoLine = line;
                            stoCol = col;
                        }
                        str.Append(input[i + 1]);
                        ignoreNext = true;
                    }
                    else if(c == '"')
                    {
                        if(isInQuotes)
                        {
                            isInQuotes = false;
                            yield return new TokenizedItem(str.ToString(), "qstring", stoLine, stoCol);
                            str.Clear();
                        }
                        else
                        {
                            isInQuotes = true;
                            if(str.Length != 0)
                            {
                                yield return new TokenizedItem(str.ToString(), "string", stoLine, stoCol);
                                str.Clear();
                            }
                        }
                    }
                    else
                    {
                        if (str.Length == 0)
                        {
                            stoLine = line;
                            stoCol = col;
                        }
                        str.Append(c);
                    }
                }

                if (c == '\n')
                {
                    line++;
                    col = 0;
                }
                else
                    col++;
            }

            if (str.Length != 0)
            {
                yield return new TokenizedItem(str.ToString(), "string", stoLine, stoCol);
                str.Clear();
            }
        }
    }
}
