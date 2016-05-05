using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trifolia.DB;

namespace Trifolia.Generation.Schematron.Utilities
{
    public class TemplateUtil
    {
        /// <summary>
        /// Given a template, returns all child template oid's by recursively walking the child collection
        /// </summary>
        /// <param name="parentTemplate"></param>
        /// <param name="aChildOids"></param>
        /// <returns>string list of all unique oids</returns>
        public static IList<string> GetAllChildTemplateOids(Template parentTemplate, List<string> aChildOids = null)
        {
            var childOids = aChildOids == null ? new List<string>() : aChildOids;

            if (parentTemplate.ImpliedTemplate != null)
            {
                if (!childOids.Contains(parentTemplate.ImpliedTemplate.Oid))
                {
                    string oid = parentTemplate.ImpliedTemplate.Oid;
                    childOids.Add(oid);
                }
            }

            foreach (var childConstraint in parentTemplate.ChildConstraints)
            {
                if (childConstraint.ContainedTemplateId != null && !childOids.Contains(childConstraint.ContainedTemplate.Oid))
                {
                    string oid = childConstraint.ContainedTemplate.Oid;
                    childOids.Add(oid);
                    GetAllChildTemplateOids(childConstraint.ContainedTemplate, childOids);                    
                }
            }

            return childOids;
        }

    }
}
