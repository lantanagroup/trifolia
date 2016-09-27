using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text;

using Trifolia.Web.Models.Validation;
using Trifolia.Web.Models.GreenManagement;
using Trifolia.DB;
using Trifolia.Authorization;
using System.Web.Http.Description;

namespace Trifolia.Web.Controllers.API
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class GreenTemplateController : ApiController
    {
        private IObjectRepository tdb;

        #region Constructors

        public GreenTemplateController(IObjectRepository tdb)
        {
            this.tdb = tdb;
        }

        public GreenTemplateController()
            : this(DBContext.Create())
        {

        }

        #endregion

        [HttpGet, Route("api/Green/Template/{templateId?}"), SecurableAction(SecurableNames.GREEN_MODEL)]
        public GreenTemplateViewModel GetGreenTemplate(int? templateId = null)
        {
            DB.Template lTemplate = tdb.Templates.DefaultIfEmpty(null).SingleOrDefault(t => t.Id == templateId);

            if (lTemplate == null)
            {
                return new GreenTemplateViewModel();
            }
            else
            {
                TemplateConstraintMapper lMapper = new TemplateConstraintMapper(tdb);
                GreenTemplateViewModel lModel = lMapper.MapEntityToViewModel(lTemplate);

                return lModel;
            }
        }

        [HttpGet, Route("api/Green/DataType"), SecurableAction(SecurableNames.GREEN_MODEL)]
        public List<GreenDataTypeViewModel> GetGreenDataTypes()
        {
            List<GreenDataTypeViewModel> dataTypes = new List<GreenDataTypeViewModel>();

            foreach (DB.ImplementationGuideTypeDataType type in tdb.ImplementationGuideTypeDataTypes)
            {
                GreenDataTypeViewModel lModel = new GreenDataTypeViewModel()
                {
                    datatypeId = type.Id,
                    datatype = type.DataTypeName
                };

                dataTypes.Add(lModel);
            }

            return dataTypes;
        }

        [HttpPost, Route("api/Green/Template"), SecurableAction(SecurableNames.GREEN_MODEL)]
        public GreenTemplateSaveResult SaveGreenTemplate(GreenTemplateViewModel aUpdatedModel)
        {
            GreenTemplateSaveResult lSaveResult = new GreenTemplateSaveResult();

            DB.Template lUnderlyingTemplate = tdb.Templates.Single(t => t.Id == aUpdatedModel.TemplateId);

            ValidationRunner<GreenTemplateViewModel> lRunner
                = new ValidationRunner<GreenTemplateViewModel>(
                    new AllShallConstraintsIncludedRule(lUnderlyingTemplate),
                    new LeafLevelGreenConstraintsHaveDataType());

            ValidationResult lResult = lRunner.RunValidation(aUpdatedModel);

            if (!lResult.Pass)
            {
                StringBuilder lMessageBuilder = new StringBuilder();

                foreach (RuleValidationResult lRuleResult in lResult.Results)
                {
                    if(lRuleResult.Pass) continue;

                    lMessageBuilder.AppendLine(string.Format("Error: {0}", lRuleResult.Message));
                }

                lSaveResult.ViewModel = aUpdatedModel;
                lSaveResult.FailedValidation = true;
                lSaveResult.ValidationMessage = lMessageBuilder.ToString();

                return lSaveResult;
            }

            lSaveResult.FailedValidation = false;
            TemplateConstraintMapper lMapper = new TemplateConstraintMapper(tdb);
            lMapper.MapViewModelToEntity(aUpdatedModel);

            tdb.SaveChanges();

            DB.Template lUpdatedTemplate = tdb.Templates.Single(t => t.Id == aUpdatedModel.TemplateId);
            GreenTemplateViewModel lRefreshedModel = lMapper.MapEntityToViewModel(lUpdatedTemplate);
            lSaveResult.ViewModel = lRefreshedModel;

            return lSaveResult;
        }
    }
}
