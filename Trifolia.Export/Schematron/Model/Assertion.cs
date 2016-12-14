using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.Export.Schematron.Model
{
    /// <summary>
    /// Defines a standard Schematron assertion
    /// </summary>
    public class Assertion
    {

        public Assertion()
        {
            IsInheritable = true;
        }

        #region Public Properties

        public string Id { get; set; }

        /// <summary>
        /// IdPostFix is used to create a unique id when this assertion is used more than once (e.g. branches or templates)
        /// </summary>
        public string IdPostFix { get; set; }

        /// <summary>
        /// Gets or sets the test this Assertion will contain
        /// </summary>
        public string Test { get; set; }

        /// <summary>
        /// Gets the message that's displayed for a failed test
        /// </summary>
        public string AssertionMessage { get; set; }

        /// <summary>
        /// Gets the comment that should included in the schematron document for the assertion.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Determines whether the rule is inheritable (belongs in abstract rule) or not inheritable (belongs as a concrete rule)
        /// </summary>
        public bool IsInheritable { get; set; }
        
        #endregion
    }
}