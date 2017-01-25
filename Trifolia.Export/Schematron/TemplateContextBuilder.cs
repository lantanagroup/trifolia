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
        public TemplateContextBuilder(string prefix, string templateIdentifierXpathFormat, string templateVersionIdentifierXpathFormat)
        {
            this.Prefix = prefix;
            this.TemplateIdentifierXpathFormat = templateIdentifierXpathFormat;
            this.TemplateVersionIdentifierXpathFormat = templateVersionIdentifierXpathFormat;
        }

        public TemplateContextBuilder(ImplementationGuideType igType)
        {
            var plugin = igType.GetPlugin();

            this.Prefix = igType.SchemaPrefix;
            this.TemplateIdentifierXpathFormat = plugin.TemplateIdentifierXpath;
            this.TemplateVersionIdentifierXpathFormat = plugin.TemplateVersionIdentifierXpath;
        }

        public string Prefix { get; set; }
        public string TemplateIdentifierXpathFormat { get; set; }
        public string TemplateVersionIdentifierXpathFormat { get; set; }

        public string BuildContextString(Template aTemplate)
        {
            string schemaPrefix = this.Prefix;

            if (!string.IsNullOrEmpty(schemaPrefix) && !schemaPrefix.EndsWith(":"))
                schemaPrefix += ":";

            StringBuilder context = new StringBuilder();
            string templateContext = aTemplate.PrimaryContext;

            if (string.IsNullOrEmpty(templateContext))
                templateContext = aTemplate.TemplateType.RootContext;

            if (!string.IsNullOrEmpty(templateContext) && templateContext.IndexOf(':') < 0)
                templateContext = schemaPrefix + templateContext;

            context.Append(templateContext);

            string oid;
            string root;
            string extension;
            string urn;

            string identifierFormat = "[" + this.TemplateIdentifierXpathFormat + "]";
            string versionIdentifierFormat = "[" + this.TemplateVersionIdentifierXpathFormat + "]";

            if (IdentifierHelper.GetIdentifierOID(aTemplate.Oid, out oid))
            {
                context.Append(string.Format(identifierFormat, schemaPrefix, oid));
            }
            else if (IdentifierHelper.GetIdentifierII(aTemplate.Oid, out root, out extension))
            {
                if (string.IsNullOrEmpty(extension))
                    context.Append(string.Format(identifierFormat, schemaPrefix, root));
                else
                    context.Append(string.Format(versionIdentifierFormat, schemaPrefix, root, extension));
            }
            else if (IdentifierHelper.GetIdentifierURL(aTemplate.Oid, out urn))
            {
                context.Append(string.Format(identifierFormat, schemaPrefix, urn));
            }
            else
            {
                throw new Exception("Unexpected/invalid identifier for template found when processing template reference for closed template identifier xpath");
            }

            return context.ToString();
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
