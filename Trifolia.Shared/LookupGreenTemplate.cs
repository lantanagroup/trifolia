using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Trifolia.DB;

namespace Trifolia.Shared
{
    public class LookupGreenTemplate
    {
        #region Properties

        private int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private string templateName;

        public string TemplateName
        {
            get { return templateName; }
            set { templateName = value; }
        }
        private string templateOid;

        public string TemplateOid
        {
            get { return templateOid; }
            set { templateOid = value; }
        }
        private string templateType;

        public string TemplateType
        {
            get { return templateType; }
            set { templateType = value; }
        }
        private string templateIgType;

        public string TemplateIgType
        {
            get { return templateIgType; }
            set { templateIgType = value; }
        }

        public string TemplateTypeDisplay
        {
            get
            {
                return string.Format("{0} ({1})", this.templateType, this.templateIgType);
            }
        }

        #endregion

        public static List<LookupGreenTemplate> GetAll()
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                return (from gt in tdb.GreenTemplates
                        select new LookupGreenTemplate()
                        {
                            Id = gt.Id,
                            Name = gt.Name,
                            TemplateName = gt.Template.Name,
                            TemplateOid = gt.Template.Oid,
                            TemplateType = gt.Template.TemplateType.Name,
                            TemplateIgType = gt.Template.TemplateType.ImplementationGuideType.Name
                        }).ToList();
            }
        }

        public static List<LookupGreenTemplate> GetForTemplate(int templateId)
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                return (from gt in tdb.GreenTemplates
                        where gt.TemplateId == templateId
                        select new LookupGreenTemplate()
                        {
                            Id = gt.Id,
                            Name = gt.Name,
                            TemplateName = gt.Template.Name,
                            TemplateOid = gt.Template.Oid,
                            TemplateType = gt.Template.TemplateType.Name,
                            TemplateIgType = gt.Template.TemplateType.ImplementationGuideType.Name
                        }).ToList();
            }
        }

        public static void Delete(LookupGreenTemplate lookupGreenTemplate)
        {
            using (IObjectRepository tdb = DBContext.Create())
            {
                GreenTemplate greenTemplate = tdb.GreenTemplates.Single(y => y.Id == lookupGreenTemplate.Id);

                // Remove all green constraints associated with the green template
                greenTemplate.ChildGreenConstraints.ToList().ForEach(y => tdb.GreenConstraints.DeleteObject(y));

                // Remove the green template
                tdb.GreenTemplates.DeleteObject(greenTemplate);

                tdb.SaveChanges();
            }
        }
    }
}
