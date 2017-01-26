using System;
using System.Text;
using Trifolia.DB;
using Trifolia.Export.Schematron.ConstraintToDocumentElementMap;
using Trifolia.Export.Schematron.Model;
using Trifolia.Shared;
using Trifolia.Shared.Plugins;

namespace Trifolia.Export.Schematron
{
    internal class TemplateContextBuilder
    {
        private IIGTypePlugin Plugin;

        public TemplateContextBuilder(ImplementationGuideType igType, string prefix = null)
        {
            this.Plugin = igType.GetPlugin();
            this.Prefix = !string.IsNullOrEmpty(prefix) ? prefix : igType.SchemaPrefix;

            if (!string.IsNullOrEmpty(this.Prefix) && this.Prefix.EndsWith(":"))
                this.Prefix = this.Prefix.Substring(0, this.Prefix.Length-1);
        }

        public string Prefix { get; set; }

        public string BuildContextString(Template template)
        {
            return BuildContextWithIdentifierElement(template.Oid, template.PrimaryContext);
        }

        public string BuildContextString(string templateIdentifier, string primaryContext = null)
        {
            return BuildContextWithIdentifierElement(templateIdentifier, primaryContext);
        }

        /// <summary>
        /// Creates a rule context for a template where the template does not have the identifier element
        /// </summary>
        /// <remarks>
        /// Example: Where the "AD" type template does not have "templateId" on it.
        /// Example: Where a profiled FHIR element does not have "meta/profile" on it.
        /// </remarks>
        /// <param name="template"></param>
        /// <returns></returns>
        private string BuildContextWithoutIdentifierElement(string templateIdentifier, string primaryContext = null)
        {
            return string.Empty;
        }

        /// <summary>
        /// Ensures that the element name in the plugin has the appropriate prefixes.
        /// </summary>
        /// <remarks>
        /// The element name may include multiple levels in the form of xpath. Ensures that each level has a prefix as well.
        /// Ex: templateId/item becomes hqmf:templateId/hqmf:item
        /// </remarks>
        private string GetTemplateIdentifierElementName()
        {
            string[] elementNameSplit = this.Plugin.TemplateIdentifierElementName.Split('/');

            for (var i = 0; i < elementNameSplit.Length; i++)
            {
                if (!elementNameSplit[i].Contains(":"))
                    elementNameSplit[i] = this.Prefix + ":" + elementNameSplit[i];
            }

            return string.Join("/", elementNameSplit);
        }

        /// <summary>
        /// Creates a rule context for a template where the template DOES have an identifier element
        /// </summary>
        /// <remarks>
        /// Example: Where "ClinicalDocument" has a "templateId" element
        /// Example: Where a FHIR element DOES have "meta/profile"
        /// </remarks>
        /// <param name="template"></param>
        /// <returns></returns>
        private string BuildContextWithIdentifierElement(string templateIdentifier, string primaryContext = null)
        {
            string contextFormat = "{0}";
            string root = null;
            string extension = null;

            if (!string.IsNullOrEmpty(primaryContext))
            {
                if (primaryContext.Contains(":"))       // primaryContext already contains prefix
                    contextFormat = string.Format("{0}[{{0}}]", primaryContext);
                else
                    contextFormat = string.Format("{0}:{1}[{{0}}]", this.Prefix, primaryContext);
            }

            if (string.IsNullOrEmpty(this.Plugin.TemplateIdentifierElementName))
                throw new Exception("Plugin for implementation guide type is not configured properly: Does not specify a TemplateIdentifierElementName");

            if (string.IsNullOrEmpty(this.Plugin.TemplateIdentifierRootName))
                throw new Exception("Plugin for implementation guide type is not configured properly: Does not specify a TemplateIdentifierRootName");
            
            if (IdentifierHelper.IsIdentifierOID(templateIdentifier))
                IdentifierHelper.GetIdentifierOID(templateIdentifier, out root);
            else if (IdentifierHelper.IsIdentifierII(templateIdentifier))
                IdentifierHelper.GetIdentifierII(templateIdentifier, out root, out extension);
            else if (IdentifierHelper.IsIdentifierURL(templateIdentifier))
                IdentifierHelper.GetIdentifierURL(templateIdentifier, out root);
            else
                throw new Exception("Unexpected/invalid identifier for template found when processing template reference for template identifier xpath");

            if (!string.IsNullOrEmpty(extension) && !string.IsNullOrEmpty(this.Plugin.TemplateIdentifierExtensionName))
            {
                string predicate = string.Format("{0}[{1}='{3}' and {2}='{4}']",
                    this.GetTemplateIdentifierElementName(),
                    this.Plugin.TemplateIdentifierRootName,
                    this.Plugin.TemplateIdentifierExtensionName,
                    root,
                    extension);
                return string.Format(contextFormat, predicate);
            }
            else
            {
                string predicate = string.Format("{0}[{1}='{2}']",
                    this.GetTemplateIdentifierElementName(),
                    this.Plugin.TemplateIdentifierRootName,
                    root);
                return string.Format(contextFormat, predicate);
            }
        }

        /// <summary>
        /// Uses forward algorithm to go from this constraint forward through the tree
        /// </summary>
        /// <param name="aTemplate"></param>
        /// <param name="aTemplateConstraint"></param>
        /// <returns></returns>
        public string CreateFullBranchedParentContext(Template aTemplate, TemplateConstraint aTemplateConstraint)
        {
            TemplateConstraint current = aTemplateConstraint;
            var igTypePlugin = aTemplate.ImplementationGuideType.GetPlugin();

            string templateContext = this.BuildContextString(aTemplate) + "/";
            return templateContext + CreateFullBranchedParentContext(aTemplateConstraint, isTarget: true);
        }

        /// <summary>
        /// Builds the full context from the given aTemplateConstraint forward. If the given aTemplateConstraint has a parent context then this will be built also,
        /// using the aPerspectiveConstraint. aPerspectiveConstraint is always the constraint we are requesting the context for. This ensures that we only traverse
        /// the perspective's children 1 time.
        /// </summary>
        /// <param name="aPrefix"></param>
        /// <param name="aTemplateConstraint"></param>
        /// <param name="aPerspectiveConstraint"></param>
        /// <param name="aIgnoreParent">Specifies whether to walk the the parent tree</param>
        /// <returns></returns>
        private string CreateFullBranchedParentContext(TemplateConstraint aTemplateConstraint, TemplateConstraint aPerspectiveConstraint = null, bool aIgnoreParent = false, bool isTarget = false)
        {
            string constraintParentContext = string.Empty;
            if (aTemplateConstraint.Parent != null && !aIgnoreParent)
            {
                constraintParentContext = CreateFullBranchedParentContext(aTemplateConstraint.ParentConstraint, aTemplateConstraint);
            }

            DocumentTemplateElement element = null;
            DocumentTemplateElementAttribute attribute = null;
            ContextParser parser = new ContextParser(aTemplateConstraint.Context);
            parser.Parse(out element, out attribute);
            
            if (attribute != null) //we are only looking for attributes
            {
                attribute.SingleValue = aTemplateConstraint.Value;
            }
            
            if (element != null)
            {
                element.IsBranch = aTemplateConstraint.IsBranch;
                element.IsBranchIdentifier = aTemplateConstraint.IsBranchIdentifier;
                ConstraintToDocumentElementHelper.AddElementValueAndDataType(this.Prefix, element, aTemplateConstraint);
            }

            ContextBuilder builder = ContextBuilder.CreateFromElementAndAttribute(element, attribute, this.Prefix);
            StringBuilder childStrings = new StringBuilder();
            foreach (var child in aTemplateConstraint.ChildConstraints)
            {
                if (aPerspectiveConstraint == null || aPerspectiveConstraint.Id != child.Id) //since we call ourselves recursively, this ensures we only go down the path of the original caller once.
                {
                    if (child.IsBranchIdentifier == true)
                    {
                        childStrings.Append(CreateFullBranchedParentContext(child, aIgnoreParent:true));
                    }
                }
            }

            string context = builder.GetFullyQualifiedContextString() + childStrings;

            if (element != null && aTemplateConstraint.Parent != null)
            {
                if (element.IsBranchIdentifier || (element.IsBranch && !isTarget))
                    context = "[" + context + "]";
                else
                    context = "/" + context;
            }

            return constraintParentContext + context;
        }

    }
}
