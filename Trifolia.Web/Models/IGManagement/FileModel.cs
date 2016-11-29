using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.IGManagement
{
    public class FileModel
    {
        public enum Types
        {
            ImplementationGuide = 0,
            Schematron = 1,
            SchematronHelper = 2,
            Vocabulary = 3,
            DeliverableSample = 4,
            GoodSample = 5,
            BadSample = 6,
            DataSnapshot = 7,
            Image = 8,
            FHIRResourceInstance = 9
        }

        public int? FileId { get; set; }
        public int? VersionId { get; set; }
        public Types Type { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
        public byte[] Data { get; set; }
        public string MimeType { get; set; }
        public bool IsRemoved { get; set; }
        public string Url { get; set; }
    }
}