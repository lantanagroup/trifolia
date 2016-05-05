using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.TemplateManagement
{
    public class MoveCompleteModel
    {
        public MoveModel Template { get; set; }
        public List<MoveConstraint> Constraints { get; set; }
    }
}