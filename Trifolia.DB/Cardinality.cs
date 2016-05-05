using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.DB
{
    public class Cardinality
    {
        public int Left { get; set; }

        public int Right { get; set; }

        public const int MANY = int.MaxValue;

        public const string MANY_TOKEN = "*";

        public bool IsZeroToZero()
        {
            return (Left == Right) && (Right == 0);
        }

        public bool IsOneToOne()
        {
            return (Left == Right) && (Right == 1);
        }

        public bool IsOneToMany()
        {
            return (Left == 1) && (Right == Cardinality.MANY);
        }

        public bool IsZeroToMany()
        {
            return (Left == 0) && (Right == Cardinality.MANY);
        }


        public override string ToString()
        {
            return string.Format("{0}..{1}", Left == Cardinality.MANY ? MANY_TOKEN : Left.ToString(),
                Right == Cardinality.MANY ? MANY_TOKEN : Right.ToString());
        }
    }
}
