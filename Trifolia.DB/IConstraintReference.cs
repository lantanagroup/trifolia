using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trifolia.DB
{
    public interface IConstraintReference
    {
        int Id { get; set; }
        string ReferenceIdentifier { get; set; }
        ConstraintReferenceTypes ReferenceType { get; set; }
    }
}
