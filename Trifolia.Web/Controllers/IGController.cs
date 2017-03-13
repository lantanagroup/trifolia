using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Ionic.Zip;

using Trifolia.DB;
using Trifolia.Authorization;
using Trifolia.Web.Controllers.API;
using Trifolia.Shared;
using System.Text;

namespace Trifolia.Web.Controllers
{
    public class IGController : Controller
    {
        private IObjectRepository tdb;

        #region Constructors

        public IGController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public IGController()
        {
            this.tdb = DBContext.CreateAuditable(CheckPoint.Instance.UserName, CheckPoint.Instance.HostAddress);
        }

        #endregion

        [Securable(SecurableNames.WEB_IG)]
        public ActionResult View(int implementationGuideId, int? fileId)
        {
            string viewName = "View";
            string dataLocation = string.Format("/api/ImplementationGuide/ViewData/{0}?fileId={1}", implementationGuideId, fileId);
            ImplementationGuideFile file = fileId != null ? this.tdb.ImplementationGuideFiles.Single(y => y.Id == fileId) : null;
            DateTime viewDate = file != null ? file.GetLatestData().UpdatedDate : DateTime.Now;

            // Anytime a modification is made to the View after initial release of the web-based IG
            // a new if() needs to be added below so that the web-published implementation guides do not change
            //if (viewDate <= new DateTime(2015, 3, 11, 23, 59, 59))
            //    viewName = "ViewOld";

            return View(viewName, (object)dataLocation);
        }

        [Securable(SecurableNames.WEB_IG)]
        public ActionResult Snapshot(string url)
        {
            var snapshotFile = this.tdb.ImplementationGuideFiles.Single(y => y.Url.ToLower() == url.ToLower());
            return View(snapshotFile.ImplementationGuideId, snapshotFile.Id);
        }

        private FileResult PrepareDownload(ImplementationGuide ig, string jsonData)
        {
            var resourceMappings = new Dictionary<string, string>
            {
                // Styles
                { "/Styles/bootstrap.min.css", "css\\bootstrap.min.css" },
                { "/Styles/Site.css", "css\\Site.css" },
                { "/Styles/IGView.css", "css\\IGView.css" },
                { "/Styles/highlight.css", "css\\highlight.css" },
                { "/Styles/joint.min.css", "css\\joint.min.css" },

                // Scripts
                { "/Scripts/lib/jquery/jquery-1.10.2.min.js", "js\\jquery-1.10.2.min.js" },
                { "/Scripts/lib/bootstrap/bootstrap.js", "js\\bootstrap.js" },
                { "/Scripts/lib/angular/angular.min.js", "js\\angular.min.js" },
                { "/Scripts/lib/angular/angular-route.min.js", "js\\angular-route.min.js" },
                { "/Scripts/lib/angular/ui-bootstrap-tpls-0.12.1.min.js", "js\\ui-bootstrap-tpls-0.12.1.min.js" },
                { "/Scripts/lib/angular/highlight.min.js", "js\\highlight.min.js" },
                { "/Scripts/lib/angular/angular-highlight.min.js", "js\\angular-highlight.min.js" },
                { "/Scripts/lib/jquery/jquery-highlight-5.js", "js\\jquery-highlight-5.js" },
                { "/Scripts/IG/View.js", "js\\IGView.js" },
                { "/Scripts/lib/vkbeautify.0.99.00.beta.js", "js\\vkbeautify.0.99.00.beta.js" },

                // Joint.JS for UML diagram
                { "/Scripts/lib/joint.min.js", "js\\joint.min.js" },
                { "/Scripts/lib/lodash.min.js", "js\\lodash.min.js" },
                { "/Scripts/lib/backbone-min.js", "js\\backbone-min.js" },
                { "/Scripts/lib/dagre.core.min.js", "js\\dagre.core.min.js" },
                { "/Scripts/lib/graphlib.core.min.js", "js\\graphlib.core.min.js" },
                { "/Scripts/lib/joint.layout.DirectedGraph.min.js", "js\\joint.layout.DirectedGraph.min.js" }
            };

            string viewContent;

            using (var writer = new StringWriter())
            {
                // Get the rendered view
                var view = new WebFormView(this.ControllerContext, "~/Views/IG/View.aspx");
                var viewCxt = new ViewContext(ControllerContext, view, ViewData, TempData, writer);
                viewCxt.View.Render(viewCxt, writer);
                viewContent = writer.ToString();

                foreach (string resourceMappingKey in resourceMappings.Keys)
                {
                    viewContent = viewContent.Replace(resourceMappingKey, resourceMappings[resourceMappingKey].Replace("\\", "/"));
                }
            }

            viewContent = viewContent.Replace("\"%DATA%\"", jsonData);

            foreach (var image in ig.Files.Where(x => x.ContentType == "Image"))
            {
                var oldUrl = string.Format("/api/ImplementationGuide/{0}/Image/{1}", ig.Id, image.FileName);
                var newUrl = string.Format("images/{0}", image.FileName);
                viewContent = viewContent.Replace(oldUrl, newUrl);
            }

            // Package the view, JS and JSON data into a zip
            using (ZipFile zip = new ZipFile())
            {
                zip.AddEntry("index.html", viewContent);

                zip.AddEntry("fonts/glyphicons-halflings-regular.eot", ReadFontContents("~/Fonts/glyphicons-halflings-regular.eot"));
                zip.AddEntry("fonts/glyphicons-halflings-regular.svg", ReadFontContents("~/Fonts/glyphicons-halflings-regular.svg"));
                zip.AddEntry("fonts/glyphicons-halflings-regular.ttf", ReadFontContents("~/Fonts/glyphicons-halflings-regular.ttf"));
                zip.AddEntry("fonts/glyphicons-halflings-regular.woff", ReadFontContents("~/Fonts/glyphicons-halflings-regular.woff"));

                // Add all resources used by the view to the package
                foreach (var resourceMapping in resourceMappings.Keys)
                {
                    var resourceContent = ReadFileContent("~" + resourceMapping);
                    zip.AddEntry(resourceMappings[resourceMapping], resourceContent);
                }

                foreach (var image in ig.Files.Where(x => x.ContentType == "Image"))
                {
                    var file = image.GetLatestData();
                    zip.AddEntry("images/" + image.FileName, file.Data);
                }

                PopulateReadme(zip, ig);

                using (MemoryStream ms = new MemoryStream())
                {
                    zip.Save(ms);

                    string packageFileName = string.Format("{0}_web.zip", ig.GetDisplayName(true));
                    byte[] data = ms.ToArray();

                    return File(
                        data, System.Net.Mime.MediaTypeNames.Application.Zip, packageFileName);
                }
            }
        }

        [Securable(SecurableNames.WEB_IG)]
        public FileResult Download(int implementationGuideId, int[] templateIds, bool inferred)
        {
            var ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);

            // Get the data from the API controller
            ImplementationGuideController ctrl = new ImplementationGuideController(this.tdb);
            var dataModel = ctrl.GetViewData(implementationGuideId, null, templateIds, inferred);

            // Serialize the data to JSON
            var jsonSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            jsonSerializer.MaxJsonLength = Int32.MaxValue;
            var dataContent = jsonSerializer.Serialize(dataModel);

            return PrepareDownload(ig, dataContent);
        }

        [Securable(SecurableNames.WEB_IG)]
        public FileResult DownloadSnapshot(int implementationGuideId, int? fileId)
        {
            var ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);

            // Get the data from the API controller
            ImplementationGuideController ctrl = new ImplementationGuideController(this.tdb);
            var dataModel = ctrl.GetViewData(implementationGuideId, fileId, null, true);

            // Serialize the data to JSON
            var jsonSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            jsonSerializer.MaxJsonLength = Int32.MaxValue;
            var dataContent = jsonSerializer.Serialize(dataModel);

            return PrepareDownload(ig, dataContent);
        }

        private byte[] ReadFontContents(string virtualPath)
        {
            string absPath = Server.MapPath(virtualPath);
            return System.IO.File.ReadAllBytes(absPath);
        }

        private string ReadFileContent(string virtualPath)
        {
            string absPath = Server.MapPath(virtualPath);
            return System.IO.File.ReadAllText(absPath);
        }

        private void PopulateReadme(ZipFile zip, ImplementationGuide ig)
        {
            var categories = new Dictionary<string, string> {
                { "Fonts:", "fonts" },
                { "Styles:", "css" },
                { "Scripts:", "js" },
                { "Images:", "images" }
            };

            var sb = new StringBuilder();
            sb.AppendLine(ig.WebReadmeOverview);
            sb.AppendLine();
            sb.AppendLine("Structure of this package:");
            sb.AppendLine();
            sb.AppendLine("index.html");

            foreach (var c in categories)
            {
                sb.AppendLine();
                sb.AppendLine(c.Key);

                var fileNames = zip.EntryFileNames.Where(x => x.StartsWith(c.Value));

                foreach (var s in fileNames)
                {
                    sb.AppendLine(s);
                }
            }

            zip.AddEntry("README.txt", sb.ToString());
        }
    }
}
