using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.DB
{
    [Table("v_constraintReferences")]
    public class ViewConstraintReference
    {
        [Column("templateConstraintId")]
        int TemplateConstraintId { get; set; }

        [Column("templateId")]
        int? TemplateId { get; set; }

        [Column("referenceIdentifier")]
        string ReferenceIdentifier { get; set; }

        [Column("referenceType")]
        ConstraintReferenceTypes ReferenceType { get; set; }
    }
}
