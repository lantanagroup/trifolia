using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.DB
{
    public class CardinalityParser
    {
        private static string[] Tokenize(string aCardinality)
        {
            return aCardinality.Replace("..", "|").Split('|');
        }

        public static Cardinality Parse(string aCardinality)
        {
            if (string.IsNullOrEmpty(aCardinality))
                return new Cardinality();

            aCardinality = aCardinality.Trim();
            var cardinality = new Cardinality();
            var tokens = Tokenize(aCardinality);
            if (tokens.Length > 1)
            {
                var i = 0;

                if (tokens[0] == Cardinality.MANY_TOKEN)
                {
                    cardinality.Left = Cardinality.MANY;
                }
                else
                {
                    if (int.TryParse(tokens[0], out i))
                    {
                        cardinality.Left = i;
                    }
                    else
                    {
                        throw new ArgumentException(string.Format("Cannot parse the cardinality expressed by '{0}'. Cannot convert '{1}' to integer.", aCardinality, tokens[0]));
                    }
                }

                if (tokens[1] == Cardinality.MANY_TOKEN)
                {
                    cardinality.Right = Cardinality.MANY;
                }
                else
                {
                    if (int.TryParse(tokens[1], out i))
                    {
                        cardinality.Right = i;
                    }
                    else
                    {
                        throw new ArgumentException(string.Format("Cannot parse the cardinality expressed by '{0}'. Cannot convert '{1}' to integer.", aCardinality, tokens[1]));
                    }
                }
            }
            else
            {
                throw new ArgumentException(string.Format("Cannot parse the cardinality expressed by '{0}'. Please check input.", aCardinality));
            }
            return cardinality;
        }
    }
}
