using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.ValidationService
{
    public class ValidationResult
    {
        #region Properties

        private int lineNumber;

        public int LineNumber
        {
            get { return lineNumber; }
            set { lineNumber = value; }
        }
        private string testContext;

        public string TestContext
        {
            get { return testContext; }
            set { testContext = value; }
        }
        private string location;

        public string Location
        {
            get { return location; }
            set { location = value; }
        }
        private string test;

        public string Test
        {
            get { return test; }
            set { test = value; }
        }
        private string errorMessage;

        public string ErrorMessage
        {
            get { return errorMessage; }
            set { errorMessage = value; }
        }
        private string severity;

        public string Severity
        {
            get { return severity; }
            set { severity = value; }
        }
        private string xmlChunk;

        public string XmlChunk
        {
            get { return xmlChunk; }
            set { xmlChunk = value; }
        }
        private string fileName;

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        #endregion

        #region Session Properties

        private const string CurrentResultsProperty = "ValidationCurrentResults";

        internal static List<ValidationResult> CurrentResults
        {
            get
            {
                if (HttpContext.Current.Session[CurrentResultsProperty] == null)
                    HttpContext.Current.Session[CurrentResultsProperty] = new List<ValidationResult>();

                return (List<ValidationResult>)HttpContext.Current.Session[CurrentResultsProperty];
            }
            set
            {
                HttpContext.Current.Session[CurrentResultsProperty] = value;
            }
        }

        #endregion

        public static List<ValidationResult> GetCurrentResults()
        {
            return CurrentResults;
        }
    }
}