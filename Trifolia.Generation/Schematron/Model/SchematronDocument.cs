using System.Collections.Generic;

namespace Trifolia.Generation.Schematron.Model
{
    /// <summary>
    /// Defines a simplified model representing a standard Schemtron document
    /// </summary>
    public class SchematronDocument
    {
        #region Private Fields

        private List<Phase> _phases = new List<Phase>(); 

        #endregion

        #region Public Properties

        public List<Phase> Phases { get { return _phases; } } 

        #endregion
    }
}