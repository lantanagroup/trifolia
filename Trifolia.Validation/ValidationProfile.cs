using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using LantanaGroup.Schematron;

using Saxon.Api;

namespace Trifolia.ValidationService
{
    public class ValidationProfile
    {
        #region Properties

        private long id;

        public long Id
        {
            get { return id; }
            set { id = value; }
        }
        private ISchematronValidator schematronValidator = null;

        internal ISchematronValidator SchematronValidator
        {
            get { return schematronValidator; }
            set { schematronValidator = value; }
        }
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private string type;

        public string Type
        {
            get { return type; }
            set { type = value; }
        }
        private DateTime lastUpdated;

        internal DateTime LastUpdated
        {
            get { return lastUpdated; }
            set { lastUpdated = value; }
        }
        private string schemaLocation;

        internal string SchemaLocation
        {
            get { return schemaLocation; }
            set { schemaLocation = value; }
        }
        private string schemaPrefix;

        internal string SchemaPrefix
        {
            get { return schemaPrefix; }
            set { schemaPrefix = value; }
        }
        private string schemaUri;

        internal string SchemaUri
        {
            get { return schemaUri; }
            set { schemaUri = value; }
        }

        public string DisplayName
        {
            get
            {
                return string.Format("{0} ({1})", this.Name, this.Type);
            }
        }

        #endregion

        public static List<ValidationProfile> GetValidationProfiles()
        {
            ValidationService service = new ValidationService();
            return service.GetValidationProfiles();
        }
    }
}