using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.Generation.IG.ConstraintGeneration
{
    public class ConstraintPart
    {
        public enum PartTypes
        {
            PrimitiveText,
            Context,
            Constraint,
            Link,
            Template,
            Vocabulary,
            Keyword,
            General
        }

        public bool IsAnchor { get; set; }
        public string LinkDestination { get; set; }
        public string Text { get; set; }
        public PartTypes PartType { get; set; }

        public ConstraintPart(PartTypes type, string text)
        {
            this.PartType = type;
            this.Text = text;
        }

        public ConstraintPart(string text)
        {
            this.PartType = PartTypes.General;
            this.Text = text;
        }
    }
}
