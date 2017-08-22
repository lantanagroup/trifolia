using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Trifolia.DB;

namespace Trifolia.Web.Models.TemplateEditing
{
    public class ConstraintReferenceModel : IConstraintReference
    {
        public int Id { get; set; }

        public string ReferenceIdentifier { get; set; }

        public ConstraintReferenceTypes ReferenceType { get; set; }
        public string ReferenceDisplay { get; set; }
    }
}