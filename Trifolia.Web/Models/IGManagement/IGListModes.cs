using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.IGManagement
{
    /// <summary>
    /// Defines what type of implementation guide list the requestor desires
    /// </summary>
    public enum IGListModes
    {
        /// <summary>
        /// View and edit
        /// </summary>
        Default,

        /// <summary>
        /// Edit implementation guide files
        /// </summary>
        Files,

        /// <summary>
        /// Test the implementation guide
        /// </summary>
        Test,

        /// <summary>
        /// Export the IG to MS word
        /// </summary>
        ExportMSWord,

        /// <summary>
        /// Export the IG to XML
        /// </summary>
        ExportXML,

        /// <summary>
        /// Export the IG to vocabulary formats
        /// </summary>
        ExportVocab,

        /// <summary>
        /// Export the schematron for the IG
        /// </summary>
        ExportSchematron,

        /// <summary>
        /// Export the green models for the implementation guide
        /// </summary>
        /// <remarks>Deprecated</remarks>
        ExportGreen
    }
}