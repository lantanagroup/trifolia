using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Xml.XPath;
using Trifolia.DB;

namespace Trifolia.Web.Controllers.API
{
    public class HelpController : ApiController
    {
        private IObjectRepository tdb;
        private XmlDocumentationProvider webXmlDocProvider;
        private XmlDocumentationProvider authorizationXmlDocProvider;

        #region Constructors

        public HelpController(IObjectRepository tdb)
        {
            this.tdb = tdb;
            this.webXmlDocProvider = new XmlDocumentationProvider(HttpContext.Current.Server.MapPath("~/App_Data/Trifolia.Web.XML"));
            this.authorizationXmlDocProvider = new XmlDocumentationProvider(HttpContext.Current.Server.MapPath("~/App_Data/Trifolia.Authorization.XML"));
            GlobalConfiguration.Configuration.Services.Replace(typeof(IDocumentationProvider), this.webXmlDocProvider);
        }

        public HelpController()
            : this(new TemplateDatabaseDataSource())
        {

        }

        #endregion

        private static string GetPropertyType(Type type)
        {
            if (type.IsGenericType)
                return type.GetGenericTypeDefinition().FullName.Replace("`1", "") + "<" + type.GenericTypeArguments[0].FullName + ">";
            else
                return type.FullName;
        }

        [HttpGet, Route("api/Help/_meta")]
        public IEnumerable<ApiDescription> GetOperations()
        {
            var apiExplorer = GlobalConfiguration.Configuration.Services.GetApiExplorer();
            return (from ae in apiExplorer.ApiDescriptions
                    select new ApiDescription(ae, this.webXmlDocProvider)).ToList();
        }

        [HttpGet, Route("api/Help/_meta/type")]
        public IEnumerable<TypeDefinition> GetTypeDefinitions()
        {
            List<TypeDefinition> webTypeDefinitions = this.webXmlDocProvider.GetTypeDefinitions();
            List<TypeDefinition> authorizationTypeDefinitions = this.authorizationXmlDocProvider.GetTypeDefinitions();

            return webTypeDefinitions.Union(authorizationTypeDefinitions);
        }

        [HttpGet, Route("api/Help")]
        public HttpResponseMessage GetHelpPage()
        {
            var path = HttpContext.Current.Server.MapPath("~/Views/Api/help.html");
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(path, FileMode.Open);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return result;

        }

        public class ApiDescription
        {
            public string Name { get; set; }
            public string RelativePath { get; set; }
            public string HttpMethod { get; set; }
            public string Documentation { get; set; }
            public string BodyType { get; set; }
            public string Controller { get; set; }
            public string ReturnType { get; set; }
            public string PermissionCref { get; set; }
            public string PermissionDescription { get; set; }

            public List<ApiParameter> Parameters { get; set; }

            public ApiDescription()
            {
                this.Parameters = new List<ApiParameter>();
            }

            public ApiDescription(System.Web.Http.Description.ApiDescription desc, XmlDocumentationProvider xmlDocProvider)
                : this()
            {
                this.Name = desc.ActionDescriptor.ActionName;
                this.RelativePath = desc.RelativePath;
                this.HttpMethod = desc.HttpMethod.Method;
                this.Controller = desc.ActionDescriptor.ControllerDescriptor.ControllerName;
                this.Documentation = xmlDocProvider.GetDocumentation(desc.ActionDescriptor);
                this.ReturnType = xmlDocProvider.GetResponseDocumentation(desc.ActionDescriptor);

                string permissionCref = null;
                this.PermissionDescription = xmlDocProvider.GetPermissionDocumentation(desc.ActionDescriptor, out permissionCref);
                this.PermissionCref = permissionCref;

                foreach (var paramDesc in desc.ParameterDescriptions)
                {
                    if (paramDesc.Source == ApiParameterSource.FromBody)
                        this.BodyType = paramDesc.ParameterDescriptor.ParameterType.ToString();
                    else
                        this.Parameters.Add(new ApiParameter(paramDesc, xmlDocProvider));
                }
            }
        }

        public class ApiParameter
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string Documentation { get; set; }

            public ApiParameter()
            {

            }

            public ApiParameter(System.Web.Http.Description.ApiParameterDescription param, XmlDocumentationProvider xmlDocProvider)
                : this()
            {
                this.Name = param.Name;
                this.Type = GetPropertyType(param.ParameterDescriptor.ParameterType);
                this.Documentation = param.Documentation;
            }
        }

        public class XmlDocumentationProvider : IDocumentationProvider
        {
            private XPathNavigator _documentNavigator;
            private const string _methodExpression = "/doc/members/member[@name='M:{0}']";
            private const string _parameterExpression = "param[@name='{0}']";

            /// <summary>
            /// Initializes a new instance of the <see cref="XmlDocumentationProvider"/> class.
            /// </summary>
            /// <param name="documentPath">The physical path to XML document.</param>
            public XmlDocumentationProvider(string documentPath)
            {
                if (documentPath == null)
                {
                    throw new ArgumentNullException("documentPath");
                }
                XPathDocument xpath = new XPathDocument(documentPath);
                _documentNavigator = xpath.CreateNavigator();
            }

            public virtual string GetDocumentation(HttpControllerDescriptor controllerDescriptor)
            {
                return null;
            }

            public virtual string GetResponseDocumentation(HttpActionDescriptor actionDescriptor)
            {
                XPathNavigator methodNode = GetMethodNode(actionDescriptor);
                if (methodNode != null)
                {
                    XPathNavigator returnsNode = methodNode.SelectSingleNode("returns");

                    if (returnsNode != null)
                        return returnsNode.Value.Trim();
                }

                if (actionDescriptor.ReturnType != null)
                    return GetPropertyType(actionDescriptor.ReturnType);

                return null;
            }

            public string GetPermissionDocumentation(HttpActionDescriptor actionDescriptor, out string cref)
            {
                XPathNavigator methodNode = GetMethodNode(actionDescriptor);
                if (methodNode != null)
                {
                    XPathNavigator permissionNode = methodNode.SelectSingleNode("permission");
                    if (permissionNode != null)
                    {
                        cref = permissionNode.SelectSingleNode("@cref").Value.Trim();
                        return permissionNode.Value.Trim();
                    }
                }

                cref = null;
                return null;
            }

            public virtual string GetDocumentation(HttpActionDescriptor actionDescriptor)
            {
                XPathNavigator methodNode = GetMethodNode(actionDescriptor);
                if (methodNode != null)
                {
                    XPathNavigator summaryNode = methodNode.SelectSingleNode("summary");
                    if (summaryNode != null)
                    {
                        return summaryNode.Value.Trim();
                    }
                }

                return null;
            }

            public virtual string GetDocumentation(HttpParameterDescriptor parameterDescriptor)
            {
                ReflectedHttpParameterDescriptor reflectedParameterDescriptor = parameterDescriptor as ReflectedHttpParameterDescriptor;
                if (reflectedParameterDescriptor != null)
                {
                    XPathNavigator methodNode = GetMethodNode(reflectedParameterDescriptor.ActionDescriptor);
                    if (methodNode != null)
                    {
                        string parameterName = reflectedParameterDescriptor.ParameterInfo.Name;
                        XPathNavigator parameterNode = methodNode.SelectSingleNode(String.Format(CultureInfo.InvariantCulture, _parameterExpression, parameterName));
                        if (parameterNode != null)
                        {
                            return parameterNode.Value.Trim();
                        }
                    }
                }

                return null;
            }

            private Type GetType(string fullName)
            {
                if (string.IsNullOrEmpty(fullName))
                    return null;

                Type type = Type.GetType(fullName);

                if (type == null)
                {
                    while (fullName.IndexOf('.') > 0)
                    {
                        fullName = fullName.Substring(0, fullName.LastIndexOf('.')) + "+" + fullName.Substring(fullName.LastIndexOf('.') + 1);
                        type = Type.GetType(fullName);

                        if (type != null)
                            break;
                    }
                }

                if (fullName.StartsWith("IEnumerable<") || fullName.StartsWith("List<") || fullName.StartsWith("Nullable<"))
                    type = type.GenericTypeArguments[0];

                return type;
            }

            private void PopulateProperties(TypeDefinition newTypeDefinition, Type type, string prefix)
            {
                string propertyNodesStartWith = prefix + newTypeDefinition.FullName + ".";
                var propertyNodes = _documentNavigator.Select("/doc/members/member[starts-with(@name, '" + propertyNodesStartWith + "')]");

                foreach (XPathNavigator propertyNode in propertyNodes)
                {
                    string nameAttribute = propertyNode.SelectSingleNode("@name").Value.Replace(propertyNodesStartWith, "");

                    if (nameAttribute.IndexOf(".") > 0)
                        continue;

                    PropertyDefinition newPropDefinition = new PropertyDefinition();
                    newPropDefinition.Name = nameAttribute;

                    var propertySummaryNode = propertyNode.SelectSingleNode("summary");
                    if (propertySummaryNode != null)
                        newPropDefinition.Description = propertySummaryNode.Value.Trim();

                    if (type != null)
                    {
                        var propertyInfo = type.GetProperty(newPropDefinition.Name);

                        if (propertyInfo != null)
                            newPropDefinition.Type = GetPropertyType(propertyInfo.PropertyType);
                    }

                    newTypeDefinition.Properties.Add(newPropDefinition);
                }
            }

            public List<TypeDefinition> GetTypeDefinitions()
            {
                List<TypeDefinition> typeDefinitions = new List<TypeDefinition>();
                var typeNodes = _documentNavigator.Select("/doc/members/member[starts-with(@name, 'T:')]");

                foreach (XPathNavigator typeNode in typeNodes)
                {
                    TypeDefinition newTypeDefinition = new TypeDefinition();
                    newTypeDefinition.FullName = typeNode.SelectSingleNode("@name").Value.Substring(2);
                    newTypeDefinition.Name = newTypeDefinition.FullName.Substring(newTypeDefinition.FullName.LastIndexOf(".") + 1);

                    var typeSummaryNode = typeNode.SelectSingleNode("summary");
                    if (typeSummaryNode != null)
                        newTypeDefinition.Description = typeSummaryNode.Value.Trim();

                    typeDefinitions.Add(newTypeDefinition);

                    Type type = this.GetType(newTypeDefinition.FullName);

                    this.PopulateProperties(newTypeDefinition, type, "P:");
                    this.PopulateProperties(newTypeDefinition, type, "F:");
                }

                return typeDefinitions;
            }

            private XPathNavigator GetMethodNode(HttpActionDescriptor actionDescriptor)
            {
                ReflectedHttpActionDescriptor reflectedActionDescriptor = actionDescriptor as ReflectedHttpActionDescriptor;
                if (reflectedActionDescriptor != null)
                {
                    string selectExpression = String.Format(CultureInfo.InvariantCulture, _methodExpression, GetMemberName(reflectedActionDescriptor.MethodInfo));
                    return _documentNavigator.SelectSingleNode(selectExpression);
                }

                return null;
            }

            private static string GetMemberName(MethodInfo method)
            {
                string name = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", method.DeclaringType.FullName, method.Name);
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length != 0)
                {
                    string[] parameterTypeNames = parameters.Select(param => GetTypeName(param.ParameterType)).ToArray();
                    name += String.Format(CultureInfo.InvariantCulture, "({0})", String.Join(",", parameterTypeNames));
                }

                return name;
            }

            private static string GetTypeName(Type type)
            {
                if (type.IsGenericType)
                {
                    // Format the generic type name to something like: Generic{System.Int32,System.String}
                    Type genericType = type.GetGenericTypeDefinition();
                    Type[] genericArguments = type.GetGenericArguments();
                    string typeName = genericType.FullName;

                    // Trim the generic parameter counts from the name
                    typeName = typeName.Substring(0, typeName.IndexOf('`'));
                    string[] argumentTypeNames = genericArguments.Select(t => GetTypeName(t)).ToArray();
                    return String.Format(CultureInfo.InvariantCulture, "{0}{{{1}}}", typeName, String.Join(",", argumentTypeNames));
                }

                return type.FullName;
            }
        }

        public class TypeDefinition
        {
            public TypeDefinition()
            {
                this.Properties = new List<PropertyDefinition>();
            }

            public string Name { get; set; }
            public string FullName { get; set; }
            public string Description { get; set; }
            public List<PropertyDefinition> Properties { get; set; }
        }

        public class PropertyDefinition
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string Description { get; set; }
        }
    }
}
