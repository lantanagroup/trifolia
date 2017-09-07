using System;
using System.Collections.Generic;
using System.Linq;
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
        private IIGTypePlugin plugin;
        private ImplementationGuideType igType;
        private string prefix;
        private IObjectRepository tdb;

        public TemplateContextBuilder(IObjectRepository tdb, ImplementationGuideType igType, string prefix = null)
        {
            this.tdb = tdb;
            this.igType = igType;
            this.plugin = this.igType.GetPlugin();
            this.prefix = !string.IsNullOrEmpty(prefix) ? prefix : igType.SchemaPrefix;

            if (!string.IsNullOrEmpty(this.prefix) && this.prefix.EndsWith(":"))
                this.prefix = this.prefix.Substring(0, this.prefix.Length-1);
        }

        public string BuildContextString(Template template)
        {
            if (!IdentifierExistsForContext(template.PrimaryContextType))
                return BuildContextWithoutIdentifierElement(template);

            return BuildContextWithIdentifierElement(template.Oid, template.PrimaryContext);
        }

        public string BuildContextString(string templateIdentifier, string primaryContext = null, string primaryContextType = null)
        {
            return BuildContextWithIdentifierElement(templateIdentifier, primaryContext);
        }

        private bool IdentifierExistsForContext(string primaryContextType)
        {
            // Assume identifier exists if we aren't given a context type
            if (string.IsNullOrEmpty(primaryContextType))
                return true;

            SimpleSchema igSchema = this.igType.GetSimpleSchema();
            igSchema = igSchema.GetSchemaFromContext(primaryContextType);

            if (igSchema != null)
            {
                string[] identifierElementNameSplit = this.plugin.TemplateIdentifierElementName.Split('/');
                var currentChildren = igSchema.Children;
                bool identifierExists = true;

                foreach (var identifierElementNamePart in identifierElementNameSplit)
                {
                    var foundChildElement = currentChildren.SingleOrDefault(y => y.Name.ToLower() == identifierElementNamePart.ToLower());

                    if (foundChildElement == null)
                    {
                        identifierExists = false;
                        break;
                    }

                    currentChildren = foundChildElement.Children;
                }

                return identifierExists;
            }

            // Assume identifier exists if we aren't given a context type
            return true;
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
        private string BuildContextWithoutIdentifierElement(Template template)
        {
            var containingConstraints = (from tcr in this.tdb.TemplateConstraintReferences
                                         join tc in this.tdb.TemplateConstraints on tcr.TemplateConstraintId equals tc.Id
                                         where tcr.ReferenceType == ConstraintReferenceTypes.Template &&
                                           tcr.ReferenceIdentifier == template.Oid
                                         select tc);

            if (containingConstraints.Count() == 0)
            {
                if (!template.PrimaryContext.Contains(":"))
                    return string.Format("{0}:{1}", this.prefix, template.PrimaryContext);

                return template.PrimaryContext;
            }

            List<string> containmentContexts = new List<string>();

            foreach (var containingConstraint in containingConstraints)
            {
                if (string.IsNullOrEmpty(containingConstraint.Context))
                    continue;

                string containmentContext = string.Empty;
                var currentConstraint = containingConstraint;

                while (currentConstraint != null)
                {
                    if (!string.IsNullOrEmpty(containmentContext))
                        containmentContext = "/" + containmentContext;

                    string constraintContext = currentConstraint.Context;

                    if (!constraintContext.Contains(":"))
                        constraintContext = string.Format("{0}:{1}", this.prefix, constraintContext);

                    containmentContext = constraintContext + containmentContext;
                    currentConstraint = currentConstraint.ParentConstraint;
                }

                // TODO: Enhance performance
                TemplateContextBuilder tcb = new TemplateContextBuilder(this.tdb, containingConstraint.Template.ImplementationGuideType);
                string templateContext = tcb.BuildContextString(containingConstraint.Template);

                if (!string.IsNullOrEmpty(templateContext))
                    containmentContext = templateContext + "/" + containmentContext;

                containmentContexts.Add(containmentContext);
            }

            return string.Join(" | ", containmentContexts.Distinct());
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
            string[] elementNameSplit = this.plugin.TemplateIdentifierElementName.Split('/');

            for (var i = 0; i < elementNameSplit.Length; i++)
            {
                if (!elementNameSplit[i].Contains(":"))
                    elementNameSplit[i] = this.prefix + ":" + elementNameSplit[i];
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
                    contextFormat = string.Format("{0}:{1}[{{0}}]", this.prefix, primaryContext);
            }

            if (string.IsNullOrEmpty(this.plugin.TemplateIdentifierElementName))
                throw new Exception("Plugin for implementation guide type is not configured properly: Does not specify a TemplateIdentifierElementName");

            if (string.IsNullOrEmpty(this.plugin.TemplateIdentifierRootName))
                throw new Exception("Plugin for implementation guide type is not configured properly: Does not specify a TemplateIdentifierRootName");
            
            if (IdentifierHelper.IsIdentifierOID(templateIdentifier))
                IdentifierHelper.GetIdentifierOID(templateIdentifier, out root);
            else if (IdentifierHelper.IsIdentifierII(templateIdentifier))
                IdentifierHelper.GetIdentifierII(templateIdentifier, out root, out extension);
            else if (IdentifierHelper.IsIdentifierURL(templateIdentifier))
                IdentifierHelper.GetIdentifierURL(templateIdentifier, out root);
            else
                throw new Exception("Unexpected/invalid identifier for template found when processing template reference for template identifier xpath");

            if (!string.IsNullOrEmpty(extension) && !string.IsNullOrEmpty(this.plugin.TemplateIdentifierExtensionName))
            {
                string predicate = string.Format("{0}[{1}='{3}' and {2}='{4}']",
                    this.GetTemplateIdentifierElementName(),
                    this.plugin.TemplateIdentifierRootName,
                    this.plugin.TemplateIdentifierExtensionName,
                    root,
                    extension);
                return string.Format(contextFormat, predicate);
            }
            else
            {
                string predicate = string.Format("{0}[{1}='{2}']",
                    this.GetTemplateIdentifierElementName(),
                    this.plugin.TemplateIdentifierRootName,
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
                ConstraintToDocumentElementHelper.AddElementValueAndDataType(this.prefix, element, aTemplateConstraint);
            }

            ContextBuilder builder = ContextBuilder.CreateFromElementAndAttribute(element, attribute, this.prefix);
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
                if (element.IsBranchIdentifier)
                    context = "[" + context + "]";
                else
                    context = "/" + context;
            }

            return constraintParentContext + context;
        }

    }
}
