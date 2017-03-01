using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;

namespace Trifolia.DB
{
    public partial class AuditEntry
    {
        private static List<Type> auditableTypes = null;

        public static bool IsAuditable(object entity)
        {
            if (auditableTypes == null)
            {
                auditableTypes = new List<System.Type>();
                auditableTypes.Add(typeof(ImplementationGuide));
                auditableTypes.Add(typeof(Template));
                auditableTypes.Add(typeof(TemplateConstraint));
                auditableTypes.Add(typeof(CodeSystem));
                auditableTypes.Add(typeof(ValueSet));
                auditableTypes.Add(typeof(ValueSetMember));
            }

            return auditableTypes.Contains(entity.GetType());
        }

        public AuditEntry() { }

        public AuditEntry(object auditableEntity, DbPropertyValues current, DbPropertyValues original)
        {
            if (auditableEntity is Template)
                SetPropeties(auditableEntity as Template);
            else if (auditableEntity is TemplateConstraint)
                SetPropeties(auditableEntity as TemplateConstraint);
            else if (auditableEntity is ImplementationGuide)
                SetProperties(auditableEntity as ImplementationGuide);
            else if (auditableEntity is ValueSet)
                SetProperties(auditableEntity as ValueSet);
            else if (auditableEntity is ValueSetMember)
                SetProperties(auditableEntity as ValueSetMember);
            else if (auditableEntity is CodeSystem)
                SetProperties(auditableEntity as CodeSystem);

            SetProperties(current, original);
        }

        private void SetProperties(DbPropertyValues current, DbPropertyValues original)
        {
            // TODO
        }

        private void SetProperties(ValueSet valueSet)
        {

        }

        private void SetProperties(ValueSetMember valueSetMember)
        {

        }

        private void SetProperties(CodeSystem codeSystem)
        {

        }

        private void SetProperties(ImplementationGuide ig)
        {

        }

        private void SetPropeties(Template template)
        {

        }

        private void SetPropeties(TemplateConstraint constraint)
        {

        }
    }
}
