using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.Export.Schematron.Model
{
    /// <summary>
    /// Models a standard Schematron phase, which defines testing phases for a specific validation scenario
    /// </summary>
    public class Phase
    {
        #region Private Fields

        private readonly List<Pattern> _activePatterns = new List<Pattern>();
        
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the ID of this Phase
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Gets the list of active patterns used in the Schematron document
        /// </summary>
        public List<Pattern> ActivePatterns { get { return _activePatterns; } } 

        #endregion
    }
}