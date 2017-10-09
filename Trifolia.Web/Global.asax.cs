using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Threading;
using System.Security;

using Trifolia.Authentication;
using Trifolia.Logging;
using Trifolia.Web.Filters;
using Trifolia.Shared.FHIR.Profiles.STU3;
using System.Web.Http.ExceptionHandling;

namespace Trifolia.Web
{
    public class Global : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.LowercaseUrls = true;

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{resource}.ashx/{*pathInfo}");
            routes.IgnoreRoute("favicon.ico");

            routes.MapRoute(
                name: "Edit Group",
                url: "Admin/Group/{groupId}",
                defaults: new { controller = "Admin", action = "EditGroup" }
            );

            routes.MapRoute(
                name: "Edit My Group",
                url: "Account/Group/{groupId}",
                defaults: new { controller = "Account", action = "Group" }
            );

            routes.MapRoute(
                name: "Edit IG Type Schema Choices",
                url: "IGTypeManagement/SchemaChoice/{implementationGuideTypeId}",
                defaults: new { controller = "IGTypeManagement", action = "EditSchemaChoices" }
            );

            routes.MapRoute(
                name: "Import Terminology from External Source",
                url: "TerminologyManagement/Import/External",
                defaults: new { controller = "TerminologyManagement", action = "ImportExternal" }
            );

            routes.MapRoute(
                name: "Import Terminology from Excel",
                url: "TerminologyManagement/Import/Excel",
                defaults: new { controller = "TerminologyManagement", action = "ImportExcel" }
            );

            routes.MapRoute(
                name: "IG Types List",
                url: "IGTypeManagement/List",
                defaults: new { controller = "IGTypeManagement", action = "List" }
            );

            routes.MapRoute(
                name: "IG Authorization Request Approved",
                url: "IGManagement/AuthorizationRequest/{accessRequestId}/$approve",
                defaults: new { controller = "IGManagement", action = "ApproveAuthorizationRequest" });

            routes.MapRoute(
                name: "IG Authorization Request Denied",
                url: "IGManagement/AuthorizationRequest/{accessRequestId}/$deny",
                defaults: new { controller = "IGManagement", action = "DenyAuthorizationRequest" });

            routes.MapRoute(
                name: "IG Edit",
                url: "IGManagement/Edit/{implementationGuideId}",
                defaults: new { controller = "IGManagement", action = "Edit" }
            );

            routes.MapRoute(
                name: "IG Files",
                url: "IGManagement/Files/{action}",
                defaults: new { controller = "IGManagementFiles", action = "Index" }
            );

            routes.MapRoute(
                name: "List IGs",
                url: "IGManagement/List/{listMode}",
                defaults: new { controller = "IGManagement", action = "List" }
            );

            routes.MapRoute(
                name: "Delete IG",
                url: "IGManagement/Delete/{implementationGuideId}",
                defaults: new { controller = "IGManagement", action = "Delete" }
            );

            routes.MapRoute(
                name: "Export IG to XML",
                url: "Export/Xml/{implementationGuideId}",
                defaults: new { controller = "Export", action = "Xml" }
            );

            routes.MapRoute(
                name: "Export IG to MS Word",
                url: "Export/MSWord/{implementationGuideId}",
                defaults: new { controller = "Export", action = "MSWord" }
            );

            routes.MapRoute(
                name: "Export IG to Vocabulary",
                url: "Export/Vocabulary/{implementationGuideId}",
                defaults: new { controller = "Export", action = "Vocabulary" }
            );

            routes.MapRoute(
                name: "Export IG to Green",
                url: "Export/Green/{implementationGuideId}",
                defaults: new { controller = "Export", action = "Green" }
            );

            routes.MapRoute(
                name: "Export IG to Schematron",
                url: "Export/Schematron/{implementationGuideId}",
                defaults: new { controller = "Export", action = "Schematron" }
            );

            routes.MapRoute(
                name: "View IG",
                url: "IGManagement/View/{implementationGuideId}",
                defaults: new { controller = "IGManagement", action = "View" }
            );

            routes.MapRoute(
                name: "Template Copy",
                url: "TemplateManagement/Copy/{templateId}",
                defaults: new { controller = "TemplateManagement", action = "Copy" }
            );

            routes.MapRoute(
                name: "Delete Template",
                url: "TemplateManagement/Delete/{templateId}",
                defaults: new { controller = "TemplateManagement", action = "Delete" }
            );

            routes.MapRoute(
                name: "View Template by OID",
                url: "TemplateManagement/View/OID/{oid}",
                defaults: new { controller = "TemplateManagement", action = "ViewOid" }
            );

            routes.MapRoute(
                name: "View Template by II",
                url: "TemplateManagement/View/II/{root}/{extension}",
                defaults: new { controller = "TemplateManagement", action = "ViewInstanceIdentifier" }
            );

            routes.MapRoute(
                name: "View Template by URI",
                url: "TemplateManagement/View/URI",
                defaults: new { controller = "TemplateManagement", action = "ViewUri" }
            );

            routes.MapRoute(
                name: "View Template by ID",
                url: "TemplateManagement/View/Id/{templateId}",
                defaults: new { controller = "TemplateManagement", action = "ViewId" }
            );

            routes.MapRoute(
                name: "Move Template by OID",
                url: "TemplateManagement/Move/OID/{oid}",
                defaults: new { controller = "TemplateManagement", action = "MoveOid" }
            );

            routes.MapRoute(
                name: "Move Template by ID",
                url: "TemplateManagement/Move/Id/{templateId}",
                defaults: new { controller = "TemplateManagement", action = "MoveId" }
            );

            routes.MapRoute(
                name: "Move Template by II",
                url: "TemplateManagement/Move/II/{root}/{extension}",
                defaults: new { controller = "TemplateManagement", action = "MoveInstanceIdentifier" }
            );

            routes.MapRoute(
                name: "Move Template by URI",
                url: "TemplateManagement/Move/URI/{uri}",
                defaults: new { controller = "TemplateManagement", action = "MoveUri" }
            );

            routes.MapRoute(
                name: "Create New Template",
                url: "TemplateManagement/Edit/New",
                defaults: new { controller = "TemplateManagement", action = "EditNew" }
            );

            routes.MapRoute(
                name: "Edit Template by OID",
                url: "TemplateManagement/Edit/OID/{oid}",
                defaults: new { controller = "TemplateManagement", action = "EditOid", newEditor = true }
            );

            routes.MapRoute(
                name: "Edit Template by Instance Identifier",
                url: "TemplateManagement/Edit/II/{root}/{extension}",
                defaults: new { controller = "TemplateManagement", action = "EditInstanceIdentifier", newEditor = true }
            );

            routes.MapRoute(
                name: "Edit Template by URI",
                url: "TemplateManagement/Edit/URI/{uri}",
                defaults: new { controller = "TemplateManagement", action = "EditUri", newEditor = true }
            );

            routes.MapRoute(
                name: "Edit Template by ID",
                url: "TemplateManagement/Edit/Id/{templateId}",
                defaults: new { controller = "TemplateManagement", action = "EditId", newEditor = true }
            );

            routes.MapRoute(
                name: "Edit ValueSet Concepts",
                url: "TerminologyManagement/ValueSet/Edit/{valueSetId}/Concept",
                defaults: new { controller = "TerminologyManagement", action = "EditValueSetConcepts" }
            );

            routes.MapRoute(
                name: "View ValueSet",
                url: "TerminologyManagement/ValueSet/View/{valueSetId}",
                defaults: new { controller = "TerminologyManagement", action = "ViewValueSet" }
            );

            routes.MapRoute(
                name: "Realtime Web IG View",
                url: "IG/View/{implementationGuideId}",
                defaults: new { controller = "IG", action = "View" }
            );

            routes.MapRoute(
                name: "Snapshot Web IG View",
                url: "IG/Web/{*url}",
                defaults: new { controller = "IG", action = "Snapshot" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }

        void Application_Start(object sender, EventArgs e)
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(System.Web.Http.GlobalConfiguration.Configuration);

            RegisterRoutes(RouteTable.Routes);

            FilterProviders.Providers.Add(new SecurableAuthorizationFilter());

            ValueProviderFactories.Factories.Remove(ValueProviderFactories.Factories.OfType<System.Web.Mvc.JsonValueProviderFactory>().FirstOrDefault());
            ValueProviderFactories.Factories.Add(new TrifoliaJsonValueProviderFactory());
        }

        void Application_End(object sender, EventArgs e)
        {
            //  Code that runs on application shutdown

        }

        void Application_Error(object sender, EventArgs e)
        {
            // ...  
            // Use HttpContext.Current to get a Web request processing helper  
            HttpServerUtility server = HttpContext.Current.Server;
            Exception exception = server.GetLastError();
            // Log an exception  
            Log.For(this).Error(exception.Message, exception);

            Application[HttpContext.Current.Request.UserHostAddress.ToString()] = exception;
        }

        void Session_Start(object sender, EventArgs e)
        {
            // Code that runs when a new session is started

        }

        void Session_End(object sender, EventArgs e)
        {
            // Code that runs when a session ends. 
            // Note: The Session_End event is raised only when the sessionstate mode
            // is set to InProc in the Web.config file. If session mode is set to StateServer 
            // or SQLServer, the event is not raised.

        }
    }
}
