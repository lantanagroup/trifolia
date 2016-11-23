using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.Import.Terminology.External
{
    public class ImportValueSet
    {
        #region Properties

        private string importStatus;

        public string ImportStatus
        {
            get { return importStatus; }
            set { importStatus = value; }
        }
        private string importSource;

        public string ImportSource
        {
            get { return importSource; }
            set { importSource = value; }
        }
        private string oid;

        public string Oid
        {
            get { return oid; }
            set { oid = value; }
        }
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private string description;

        public string Description
        {
            get { return description; }
            set { description = value; }
        }
        private string code;

        public string Code
        {
            get { return code; }
            set { code = value; }
        }
        private List<ImportValueSetMember> members = new List<ImportValueSetMember>();

        public List<ImportValueSetMember> Members
        {
            get { return members; }
            set { members = value; }
        }

        #endregion
    }
}
