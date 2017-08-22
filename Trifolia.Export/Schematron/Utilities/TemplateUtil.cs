using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trifolia.DB;

namespace Trifolia.Export.Schematron.Utilities
{
    public class TemplateUtil
    {
        /// <summary>
        /// Given a template, returns all child template oid's by recursively walking the child collection
        /// </summary>
        /// <param name="parentTemplate"></param>
        /// <param name="aChildOids"></param>
        /// <returns>string list of all unique oids</returns>
        public static IList<string> GetAllChildTemplateOids(IObjectRepository tdb, Template parentTemplate, List<string> aChildOids = null)
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
                var containedTemplates = (from tcr in childConstraint.References
                                          join t in tdb.Templates on tcr.ReferenceIdentifier equals t.Oid
                                          where tcr.ReferenceType == ConstraintReferenceTypes.Template
                                          select t);

                foreach (var containedTemplate in containedTemplates)
                {
                    if (!childOids.Contains(containedTemplate.Oid))
                    {
                        childOids.Add(containedTemplate.Oid);
                        GetAllChildTemplateOids(tdb, containedTemplate, childOids);
                    }
                }
            }

            return childOids;
        }

    }
}
