using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.ValidationService
{
    public class ValidationContent
    {
        #region Properties

        private string fileName;

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }
        private byte[] data;

        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }

        #endregion
    }
}