using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.Import.Terminology.External
{
    public class ImportValueSetMember
    {
        private string code;

        public string Code
        {
            get { return code; }
            set { code = value; }
        }
        private string codeSystemOid;

        public string CodeSystemOid
        {
            get { return codeSystemOid; }
            set { codeSystemOid = value; }
        }
        private string codeSystemName;

        public string CodeSystemName
        {
            get { return codeSystemName; }
            set { codeSystemName = value; }
        }
        private string displayName;

        public string DisplayName
        {
            get { return displayName; }
            set { displayName = value; }
        }
        private string status;

        public string Status
        {
            get { return status; }
            set { status = value; }
        }
        private DateTime? statusDate;

        public DateTime? StatusDate
        {
            get { return statusDate; }
            set { statusDate = value; }
        }
        private string importStatus;

        public string ImportStatus
        {
            get { return importStatus; }
            set { importStatus = value; }
        }
    }
}
