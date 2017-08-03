using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

using Trifolia.Shared;
using Trifolia.Shared.ImportExport.Model;
using Trifolia.Authentication;

namespace Trifolia.DB
{
    public static class TemplateExtensions
    {
        #region Schema

        public static SimpleSchema GetSchema(this Template template, SimpleSchema igSchema = null)
        {
            if (igSchema == null)
                igSchema = template.OwningImplementationGuide.ImplementationGuideType.GetSimpleSchema();

            if (!string.IsNullOrEmpty(template.PrimaryContextType))
                return igSchema.GetSchemaFromContext(template.PrimaryContextType);
            else if (!string.IsNullOrEmpty(template.TemplateType.RootContextType))
                return igSchema.GetSchemaFromContext(template.TemplateType.RootContextType);

            return igSchema;
        }

        #endregion

        /// <summary>
        /// Determines if the identifier for the template is an "urn:oid:" identifier
        /// </summary>
        public static bool IsIdentifierOID(this Template template)
        {
            return IdentifierHelper.IsIdentifierOID(template.Oid);
        }

        public static bool GetIdentifierOID(this Template template, out string oid)
        {
            return IdentifierHelper.GetIdentifierOID(template.Oid, out oid);
        }

        /// <summary>
        /// Determines if the identifier for the template is an "urn:hl7ii:" (instance identifier) identifier
        /// </summary>
        public static bool IsIdentifierII(this Template template)
        {
            return IdentifierHelper.IsIdentifierII(template.Oid);
        }

        public static bool GetIdentifierII(this Template template, out string root, out string extension)
        {
            return IdentifierHelper.GetIdentifierII(template.Oid, out root, out extension);
        }

        /// <summary>
        /// Determines if the identifier for the template is a "urn:" identifier
        /// </summary>
        public static bool IsIdentifierURL(this Template template)
        {
            return IdentifierHelper.IsIdentifierURL(template.Oid);
        }

        public static bool GetIdentifierURL(this Template template, out string uri)
        {
            return IdentifierHelper.GetIdentifierURL(template.Oid, out uri);
        }

        public static string GetViewUrl(this Template template, string linkBase = null)
        {
            string oid;
            string root;
            string extension;
            string uri;

            if (template.GetIdentifierOID(out oid))
            {
                return string.Format("{0}/TemplateManagement/View/OID/{1}", linkBase, oid);
            }
            else if (template.GetIdentifierII(out root, out extension))
            {
                return string.Format("{0}/TemplateManagement/View/II/{1}/{2}", linkBase, root, extension);
            }
            else if (template.GetIdentifierURL(out uri))
            {
                if (uri.IndexOf(':') < 0 && uri.IndexOf('/') < 0 && uri.IndexOf('.') != uri.Length - 1)
                    return string.Format("{0}/TemplateManagement/View/URI/{1}", linkBase, uri);
            }

            return string.Format("{0}/TemplateManagement/View/Id/{1}", linkBase, template.Id);
        }

        public static string GetEditUrl(this Template template)
        {
            string oid;
            string root;
            string extension;
            string uri;

            if (template.GetIdentifierOID(out oid))
            {
                return string.Format("/TemplateManagement/Edit/OID/{0}", oid);
            }
            else if (template.GetIdentifierII(out root, out extension))
            {
                return string.Format("/TemplateManagement/Edit/II/{0}/{1}", root, extension);
            }
            else if (template.GetIdentifierURL(out uri))
            {
                if (uri.IndexOf(':') < 0 && uri.IndexOf('/') < 0 && uri.IndexOf('.') != uri.Length - 1)
                    return string.Format("/TemplateManagement/Edit/URI/{0}", uri);
            }

            return string.Format("/TemplateManagement/Edit/Id/{0}", template.Id);
        }

        public static string GetMoveUrl(this Template template)
        {
            string oid;
            string root;
            string extension;
            string uri;

            if (template.GetIdentifierOID(out oid))
            {
                return string.Format("/TemplateManagement/Move/OID/{0}", oid);
            }
            else if (template.GetIdentifierII(out root, out extension))
            {
                return string.Format("/TemplateManagement/Move/II/{0}/{1}", root, extension);
            }
            else if (template.GetIdentifierURL(out uri))
            {
                if (uri.IndexOf(':') < 0 && uri.IndexOf('/') < 0 && uri.IndexOf('.') != uri.Length - 1)
                    return string.Format("/TemplateManagement/Move/URI/{0}", uri);
            }

            return string.Format("/TemplateManagement/Move/Id/{0}", template.Id);
        }
    }
}
