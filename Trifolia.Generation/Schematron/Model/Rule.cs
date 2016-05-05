using System.Collections.Generic;

namespace Trifolia.Generation.Schematron.Model
{
    public class Rule
    {
        #region Private Fields

        private string id;
        private readonly List<Assertion> _assertions = new List<Assertion>();
        private string extends;
 
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the context for this rule
        /// </summary>
        public string Context { get; set; }

        /// <summary>
        /// Gets the list of <see cref="Assertion"/> used by this rule
        /// </summary>
        public List<Assertion> Assertions
        {
            get { return _assertions; }
        }

        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        public string Extends
        {
            get { return extends; }
            set { extends = value; }
        }

        #endregion
    }
}