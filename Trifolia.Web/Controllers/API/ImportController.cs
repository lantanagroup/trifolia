using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Trifolia.Authorization;
using Trifolia.DB;
using NativeModel = Trifolia.Shared.ImportExport.Model.Trifolia;
using Trifolia.Shared.ImportExport;
using System.Data.Entity.Core.Objects;
using Trifolia.Import.Models;
using Trifolia.Import.Native;
using Trifolia.Web.Models;
using Trifolia.Import.VSAC;
using Trifolia.Import.Terminology.External;
using Trifolia.Shared;

namespace Trifolia.Web.Controllers.API
{
    public class ImportController : ApiController
    {
        private IObjectRepository tdb;

        #region Construct/Dispose

        public ImportController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public ImportController()
            : this(DBContext.Create())
        {

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this.tdb.Dispose();

            base.Dispose(disposing);
        }

        #endregion

        /// <summary>
        /// Imports data from the native Trifolia format. See https://github.com/lantanagroup/trifolia/blob/master/Trifolia.Shared/ImportExport/Model/TemplateExport.xsd for the schema that is used by the import.
        /// </summary>
        /// <param name="model">The data to import (including implementation guides and templates)</param>
        /// <returns></returns>
        [HttpPost, Route("api/Import/Trifolia"), SecurableAction(SecurableNames.IMPORT)]
        public ImportStatusModel ImportTrifoliaModel(NativeModel model)
        {
            TrifoliaImporter importer = new TrifoliaImporter(this.tdb);
            return importer.Import(model);
        }
        
        [HttpPost, Route("api/Import/ValueSet"), SecurableAction(SecurableNames.IMPORT)]
        public ImportValueSetResponseModel ImportValueSet(ImportValueSetModel model)
        {
            ImportValueSetResponseModel responseModel = new ImportValueSetResponseModel();

            using (IObjectRepository auditedTdb = DBContext.CreateAuditable(CheckPoint.Instance.UserName, CheckPoint.Instance.HostAddress))
            {
                try
                {
                    switch (model.Source)
                    {
                        case ValueSetImportSources.VSAC:
                            User currentUser = CheckPoint.Instance.GetUser(auditedTdb);
                            VSACImporter importer = new VSACImporter(auditedTdb);

                            if (string.IsNullOrEmpty(currentUser.UMLSUsername) || string.IsNullOrEmpty(currentUser.UMLSPassword))
                            {
                                responseModel.Message = "Your profile does not have your UMLS credentials. <a href=\"/Account/MyProfile\">Update your profile</a> to import from VSAC.";
                                break;
                            }

                            if (!importer.Authenticate(currentUser.UMLSUsername.DecryptStringAES(), currentUser.UMLSPassword.DecryptStringAES()))
                            {
                                responseModel.Message = "Invalid UMLS username/password associated with your profile.";
                                break;
                            }

                            importer.ImportValueSet(model.Id);
                            responseModel.Success = true;

                            break;
                        case ValueSetImportSources.PHINVADS:
                            PhinVadsValueSetImportProcessor<ImportValueSet, ImportValueSetMember> processor =
                                new PhinVadsValueSetImportProcessor<ImportValueSet, ImportValueSetMember>();

                            ImportValueSet valueSet = processor.FindValueSet(auditedTdb, model.Id);

                            processor.SaveValueSet(auditedTdb, valueSet);
                            responseModel.Success = true;

                            break;
                        default:
                            responseModel.Message = "Unknown or unsupported source";
                            break;
                    }

                    if (responseModel.Success)
                        auditedTdb.SaveChanges();
                }
                catch (WebException wex)
                {
                    if (wex.Response != null && wex.Response is HttpWebResponse && ((HttpWebResponse)wex.Response).StatusCode == HttpStatusCode.NotFound)
                        responseModel.Message = string.Format("Value set with identifier \"{0}\" was not found on the terminology server.", model.Id);
                    else
                        responseModel.Message = wex.Message;
                }
                catch (Exception ex)
                {
                    responseModel.Message = ex.Message;
                }
            }

            return responseModel;
        }
    }
}
