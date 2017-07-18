using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trifolia.DB
{
    public interface ITemplate
    {
        string Name { get; }
        string Oid { get; }
        List<IConstraint> Constraints { get; }
        string Description { get; }
        string Bookmark { get; }
        int OwningImplementationGuideId { get; }
        bool IsOpen { get; }
        int? ImpliedTemplateId { get; }
        string Status { get; }
    }
}
