using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trifolia.Web.Models.TemplateManagement
{
    /// <summary>
    /// Model that represents a template used by ListModel.
    /// </summary>
    public class BigListItem
    {
        /// <summary>
        /// The id of the template
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the template
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The identifier of the template
        /// </summary>
        public string Oid { get; set; }

        /// <summary>
        /// Whether the template is open or closed ("Yes" or "No")
        /// </summary>
        public string Open { get; set; }

        /// <summary>
        /// The id of the template type
        /// </summary>
        public int TemplateTypeId { get; set; }

        /// <summary>
        /// The name of the template type associated with the template
        /// </summary>
        public string TemplateType { get; set; }

        /// <summary>
        /// The id of the implementation guide associated with the template
        /// </summary>
        public int ImplementationGuideId { get; set; }

        /// <summary>
        /// The name of the implementation guide associated with the template
        /// </summary>
        public string ImplementationGuide { get; set; }

        /// <summary>
        /// The id of the organization associated with the template
        /// </summary>
        public int? OrganizationId { get; set; }

        /// <summary>
        /// The name of the organization associated with the template
        /// </summary>
        public string Organization { get; set; }

        /// <summary>
        /// The name of the implied template
        /// </summary>
        public string ImpliedTemplateName { get; set; }

        /// <summary>
        /// The identifier of the implied template
        /// </summary>
        public string ImpliedTemplateOid { get; set; }

        /// <summary>
        /// The number of constraints the template contains
        /// </summary>
        public int ConstraintCount { get; set; }

        /// <summary>
        /// A comma-delimited list of constraint numbers in the template
        /// </summary>
        public string Constraints { get; set; }

        /// <summary>
        /// The publish date of the template (which is really the publish date of the implementation guide)
        /// </summary>
        public DateTime? PublishDate { get; set; }

        /// <summary>
        /// True if the user has the permissions to edit the template
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// True if the template has green models defined
        /// </summary>
        public bool HasGreenModel { get; set; }

        /// <summary>
        /// The description of the template
        /// </summary>
        public string Description { get; set; }
    }
}