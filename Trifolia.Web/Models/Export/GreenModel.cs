using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Trifolia.DB;
using Trifolia.Shared;

namespace Trifolia.Web.Models.Export
{
    public class GreenModel
    {
        public GreenModel()
        {
            this.Templates = new List<GreenModel.Template>();
        }

        public string Name { get; set; }
        public int ImplementationGuideId { get; set; }
        public List<GreenModel.Template> Templates { get; set; }

        public class Template
        {
            public Template(IObjectRepository tdb, Trifolia.DB.Template fromTemplate)
            {
                Id = fromTemplate.Id;
                Oid = fromTemplate.Oid;
                Title = fromTemplate.Name;
                IsOpen = fromTemplate.IsOpen;
                ImplementationGuide = fromTemplate.OwningImplementationGuide != null ? fromTemplate.OwningImplementationGuide.Name : string.Empty;
                IgType = fromTemplate.ImplementationGuideType.Name;
                TemplateType = fromTemplate.TemplateType.Name;
                TemplateTypeDisplay = fromTemplate.TemplateType.Name + " (" + fromTemplate.ImplementationGuideType.Name + ")";
                Organization = fromTemplate.OwningImplementationGuide != null && fromTemplate.OwningImplementationGuide.Organization != null ? fromTemplate.OwningImplementationGuide.Organization.Name : string.Empty;
                PublishDate = fromTemplate.OwningImplementationGuide != null ? fromTemplate.OwningImplementationGuide.PublishDate : null;
                ImpliedTemplateOid = fromTemplate.ImpliedTemplate != null ? fromTemplate.ImpliedTemplate.Oid : null;
                ImpliedTemplateTitle = fromTemplate.ImpliedTemplate != null ? fromTemplate.ImpliedTemplate.Name : null;
                ConstraintCount = fromTemplate.ChildConstraints.Count;
                ImpliedByCount = fromTemplate.ImplyingTemplates.Count;

                this.ContainedByCount = (from tcr in tdb.TemplateConstraintReferences
                                         join tc in tdb.TemplateConstraints on tcr.TemplateConstraintId equals tc.Id
                                         where tcr.ReferenceType == ConstraintReferenceTypes.Template
                                           && tcr.ReferenceIdentifier == fromTemplate.Oid
                                         select tc.Template)
                                         .Distinct()
                                         .Count();
            }

            #region Properties

            public int Id { get; set; }
            public string Title { get; set; }
            public string Oid { get; set; }
            public string Open { get; set; }
            public string IgType { get; set; }
            public string TemplateType { get; set; }
            public string TemplateTypeDisplay { get; set; }
            public string ImplementationGuide { get; set; }
            public string Organization { get; set; }
            public DateTime? PublishDate { get; set; }
            public string ImpliedTemplateOid { get; set; }
            public string ImpliedTemplateTitle { get; set; }
            public int ConstraintCount { get; set; }
            public int ContainedByCount { get; set; }
            public int ImpliedByCount { get; set; }

            private bool? isOpen;

            public bool? IsOpen
            {
                get { return this.isOpen; }
                set
                {
                    this.isOpen = value;

                    if (value == null)
                        this.Open = "N/A";
                    else if (value == true)
                        this.Open = "Yes";
                    else if (value == false)
                        this.Open = "No";
                }
            }

            #endregion
        }
    }
}