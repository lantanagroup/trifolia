using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.Export.MSWord.Models
{
    internal class HeaderDescriptor
    {
        public HeaderDescriptor()
        { }

        public HeaderDescriptor(string headerName)
        {
            this.HeaderName = headerName;
        }

        #region Public Properties

        /// <summary>
        /// Gets or sets the name of this table column
        /// </summary>
        public string HeaderName { get; set; }

        /// <summary>
        /// Gets or sets the width, in inches, for this table cell
        /// </summary>
        public double CellWidth { get; set; }

        /// <summary>
        /// Gets or sets the width of the column in the table
        /// </summary>
        public string ColumnWidth { get; set; }

        /// <summary>
        /// Gets or sets whether this table column is auto-wrapped
        /// </summary>
        public bool AutoWrap { get; set; }

        /// <summary>
        /// Gets or sets whether this table column shall auto-resize (based on content)
        /// </summary>
        public bool AutoResize { get; set; }

        #endregion
    }
}