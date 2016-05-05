using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.Generation.Schematron.Model
{
    /// <summary>
    /// Defines a standard Schematron pattern which contains a set of rules
    /// </summary>
    public class Pattern
    {
        #region Private Fields

        private bool isImplied = false;
        private List<Rule> _rules = new List<Rule>();
        private string isA = string.Empty;
        private string customXML = string.Empty;

        #endregion

        #region Public Properties

        public string ID { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Gets the list of rules associated with this pattern
        /// </summary>
        public List<Rule> Rules { get { return _rules; } }

        public bool IsImplied
        {
            get { return isImplied; }
            set { isImplied = value; }
        }

        public string IsA
        {
            get { return isA; }
            set { isA = value; }
        }

        public string CustomXML
        {
            get { return customXML; }
            set { customXML = value; }
        }

        #endregion
    }
}
