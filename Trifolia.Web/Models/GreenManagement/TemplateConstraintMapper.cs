using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using Trifolia.Generation.IG;
using Trifolia.Generation.IG.ConstraintGeneration;
using Trifolia.Shared;
using Trifolia.Shared.Plugins;

namespace Trifolia.Web.Models.GreenManagement
{
    public class TemplateConstraintMapper
    {
        #region Private Fields

        private DB.IObjectRepository _tdb;

        #endregion

        #region Ctor

        public TemplateConstraintMapper(DB.IObjectRepository tdb)
        {
            _tdb = tdb;
        }

        #endregion

        #region Public Methods

        public GreenTemplateViewModel MapEntityToViewModel(DB.Template aTemplate)
        {
            DB.GreenTemplate lGreenTemplate = aTemplate.GreenTemplates.DefaultIfEmpty(null).FirstOrDefault();
            GreenTemplateViewModel lViewModel = new GreenTemplateViewModel();

            if (lGreenTemplate == null)
            {
                lViewModel.Name = this.GetCamelCaseTemplateName(aTemplate.Name);
                lViewModel.IsNew = true;
            }
            else
            {
                lViewModel.Id = lGreenTemplate.Id;
                lViewModel.Name = lGreenTemplate.Name;
            }

            lViewModel.TemplateId = aTemplate.Id;
            lViewModel.TemplateName = aTemplate.Name;
            lViewModel.TemplateOid = aTemplate.Oid;

            IGSettingsManager igManager = new IGSettingsManager(_tdb, aTemplate.OwningImplementationGuideId);
            IIGTypePlugin igTypePlugin = aTemplate.OwningImplementationGuide.ImplementationGuideType.GetPlugin();
            string baseLink = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path) + "?Id=";

            int constraintCount = 0;
            foreach (DB.TemplateConstraint cDbConstraint
                in aTemplate.ChildConstraints
                .Where(y => y.Parent == null)
                .Where(c => c.IsPrimitive == false)
                .Where(c => string.IsNullOrEmpty(c.Value)).OrderBy(y => y.Order))
            {
                ConstraintViewModel lConstraintView = this.BuildConstraint(_tdb, baseLink, igManager, igTypePlugin, cDbConstraint, constraintCount);
                this.CreateIfRequiredGreenTemplateTree(lConstraintView);

                lViewModel.childConstraints.Add(lConstraintView);
            }

            return lViewModel;
        }

        public void MapViewModelToEntity(GreenTemplateViewModel aModel)
        {
            DB.GreenTemplate lTemplate = this.GetOrCreateGreenTemplate(aModel);

            foreach (ConstraintViewModel lConstraint in aModel.childConstraints)
            {
                this.UpdateConstraint(lTemplate, lConstraint);
            }
        }

        #endregion

        #region Private Methods

        private void CreateIfRequiredGreenTemplateTree(ConstraintViewModel aStartConstraint)
        {
            if (aStartConstraint.hasGreenConstraint) return;
            if (aStartConstraint.conformance != "SHALL" && aStartConstraint.conformance != "SHOULD") return;
            if (!string.IsNullOrEmpty(aStartConstraint.value)) return;

            var lChildren = aStartConstraint.children
                .Where(c => c.conformance == "SHALL" || c.conformance == "SHOULD")
                .Where(c => string.IsNullOrEmpty(c.value));

            foreach (ConstraintViewModel lChild in lChildren)
            {
                CreateIfRequiredGreenTemplateTree(lChild);
            }

            if (aStartConstraint.children.Count > 0) return; //Leaf-level only
            DB.TemplateConstraint lRootConstraint = _tdb.TemplateConstraints.Single(c => c.Id == aStartConstraint.id);

            aStartConstraint.Use(c =>
            {
                c.businessName = c.elementName = lRootConstraint.Context;
                c.hasGreenConstraint = true;

                string lDataTypeToUse = null;

                // --- Identify data type to use, which will come from data types contained in the IG
                if (!string.IsNullOrEmpty(lRootConstraint.DataType) && lRootConstraint.DataType != "ANY")
                {
                    lDataTypeToUse = lRootConstraint.DataType;
                }
                else
                {
                    ConstraintXpathBuilder lBuilder = new ConstraintXpathBuilder(_tdb);
                    string lXpath = lBuilder.GenerateXpath(lRootConstraint);

                    ConstraintDataTypeResolver lResolver = new ConstraintDataTypeResolver();
                    string lDataType = lResolver.GetConstraintDataType(lRootConstraint, lXpath);

                    lDataTypeToUse = lDataType;
                }

                // --- Load data type, only use if constraint is leaf-level
                DB.ImplementationGuideTypeDataType lUnderlyingType
                    = _tdb.ImplementationGuideTypeDataTypes.SingleOrDefault(d => d.DataTypeName == lDataTypeToUse);

                if (lUnderlyingType != null && lRootConstraint.ChildConstraints.Count(y => !y.IsPrimitive) == 0)
                {
                    c.datatypeId = lUnderlyingType.Id;
                    c.datatype = lUnderlyingType.DataTypeName;
                }
            });
        }

        private void UpdateConstraint(DB.GreenTemplate aTemplate, ConstraintViewModel aViewModel, DB.GreenConstraint aParentConstraint = null)
        {
            if ((aViewModel.hasGreenConstraint || aViewModel.isDeleted) && aViewModel.greenConstraintId.HasValue)
            {
                this.UpdateConstraint(aViewModel);
            }

            if (aViewModel.hasGreenConstraint && aViewModel.greenConstraintId.HasValue == false && aViewModel.isDeleted == false) //Is New
            {
                DB.GreenConstraint lNewConstraint = this.AddNewConstraint(aViewModel);

                if (aParentConstraint != null)
                {
                    aParentConstraint.ChildGreenConstraints.Add(lNewConstraint);
                }

                aTemplate.ChildGreenConstraints.Add(lNewConstraint);

                foreach (ConstraintViewModel lChild in aViewModel.children)
                {
                    this.UpdateConstraint(aTemplate, lChild, lNewConstraint);
                }
            }
            else
            {
                foreach (ConstraintViewModel lChild in aViewModel.children)
                {
                    this.UpdateConstraint(aTemplate, lChild);
                }
            }
        }

        private void UpdateConstraint(ConstraintViewModel aConstraint)
        {
            if (aConstraint.greenConstraintId.HasValue && aConstraint.isDeleted)
            {
                DB.GreenConstraint lModelToDelete = _tdb.GreenConstraints.Single(gc => gc.Id == aConstraint.greenConstraintId);
                
                foreach (DB.GreenConstraint lChild in lModelToDelete.ChildGreenConstraints.ToList())
                {
                    lChild.ParentGreenConstraint = null;
                    lChild.ParentGreenConstraintId = null;
                }

                _tdb.GreenConstraints.Remove(lModelToDelete);
            }
            else if (aConstraint.greenConstraintId.HasValue && aConstraint.isDeleted == false)
            {
                DB.GreenConstraint lModelToUpdate = _tdb.GreenConstraints.DefaultIfEmpty(null).SingleOrDefault(gc => gc.Id == aConstraint.greenConstraintId);
                if (lModelToUpdate == null) return;

                if (!string.Equals(lModelToUpdate.Name, aConstraint.elementName)) lModelToUpdate.Name = aConstraint.elementName;
                if (!string.Equals(lModelToUpdate.Description, aConstraint.businessName)) lModelToUpdate.Description = aConstraint.businessName;
                if (lModelToUpdate.TemplateConstraintId != aConstraint.id) lModelToUpdate.TemplateConstraintId = aConstraint.id;
                if (lModelToUpdate.ImplementationGuideTypeDataTypeId != aConstraint.datatypeId)
                    lModelToUpdate.ImplementationGuideTypeDataTypeId = aConstraint.datatypeId;
            }
        }

        private DB.GreenConstraint AddNewConstraint(ConstraintViewModel aNewConstraint)
        {
            DB.TemplateConstraint lUnderlyingConstraint = _tdb.TemplateConstraints.Single(tc => tc.Id == aNewConstraint.id);

            ConstraintXpathBuilder lXpathBuilder = new ConstraintXpathBuilder(_tdb);
            string lXpath = lXpathBuilder.GenerateXpath(lUnderlyingConstraint);

            DB.GreenConstraint lNewModel = new DB.GreenConstraint()
            {
                Description = aNewConstraint.businessName,
                Name = aNewConstraint.elementName,
                TemplateConstraintId = aNewConstraint.id,
                RootXpath = lXpath
            };

            if (aNewConstraint.datatypeId.HasValue)
            {
                DB.ImplementationGuideTypeDataType lDataType = _tdb.ImplementationGuideTypeDataTypes.Single(dt => dt.Id == aNewConstraint.datatypeId.Value);
                lNewModel.ImplementationGuideTypeDataType = lDataType;
            }

            return lNewModel;
        }

        private DB.GreenTemplate GetOrCreateGreenTemplate(GreenTemplateViewModel aModel)
        {
            DB.GreenTemplate lTemplate = _tdb.GreenTemplates.DefaultIfEmpty(null).SingleOrDefault(gt => gt.Id == aModel.Id);

            if (lTemplate == null)
            {
                lTemplate = new DB.GreenTemplate()
                {
                    Name = aModel.Name,
                    TemplateId = aModel.TemplateId
                };

                _tdb.GreenTemplates.Add(lTemplate);
            }

            return lTemplate;
        }

        private string GetConstraintDescription(DB.TemplateConstraint constraint)
        {
            StringBuilder description = new StringBuilder();

            if (constraint.IsHeading)
            {
                description.Append(string.Format("<p><b>{0}</b></p>\r\n", constraint.Context));

                if (!string.IsNullOrEmpty(constraint.HeadingDescription))
                    description.Append(string.Format("<p>{0}</p>\r\n", constraint.HeadingDescription));
            }

            if (!string.IsNullOrEmpty(constraint.Description))
                description.Append(string.Format("<p>{0}</p>\r\n", constraint.Description));

            if (!string.IsNullOrEmpty(constraint.Label))
                description.Append(string.Format("<p>Label: {0}</p>\r\n", constraint.Label));

            return description.ToString();
        }

        private ConstraintViewModel BuildConstraint(
            DB.IObjectRepository tdb, 
            string baseLink,
            IGSettingsManager igSettings, 
            IIGTypePlugin igTypePlugin,
            DB.TemplateConstraint dbConstraint, 
            int constraintCount, 
            int? aParentConstraintId = null)
        {
            IFormattedConstraint fc = FormattedConstraintFactory.NewFormattedConstraint(tdb, igSettings, igTypePlugin, dbConstraint);

            ConstraintViewModel lGreenViewModel = new ConstraintViewModel()
            {
                constraintLabel = dbConstraint.Label,
                headingDescription = dbConstraint.HeadingDescription,
                id = dbConstraint.Id,
                order = dbConstraint.Order,
                isHeading = dbConstraint.IsHeading,
                isPrimitive = dbConstraint.IsPrimitive,
                primitiveText = dbConstraint.PrimitiveText,
                templateId = dbConstraint.TemplateId,
                number = dbConstraint.Number,
                text = fc.GetPlainText(false, false, false),
                conformance = dbConstraint.Conformance,
                value = dbConstraint.Value
            };

            lGreenViewModel.constraintDescription = GetConstraintDescription(dbConstraint);

            DB.GreenConstraint lGreenConstraint = dbConstraint.GreenConstraints.DefaultIfEmpty(null).FirstOrDefault();

            if (lGreenConstraint != null)
            {
                lGreenViewModel.Use(c =>
                {
                    c.businessName = lGreenConstraint.Description;
                    c.elementName = lGreenConstraint.Name;
                    c.xPath = lGreenConstraint.RootXpath;
                    c.greenConstraintId = lGreenConstraint.Id;
                    c.hasGreenConstraint = true;

                    if (lGreenConstraint.ImplementationGuideTypeDataTypeId.HasValue)
                    {
                        c.datatype = lGreenConstraint.ImplementationGuideTypeDataType.DataTypeName;
                        c.datatypeId = lGreenConstraint.ImplementationGuideTypeDataType.Id;
                    }
                });
            }

            if (aParentConstraintId.HasValue) lGreenViewModel.parentConstraintId = aParentConstraintId.Value;

            int nextConstraintCount = 0;
            foreach (DB.TemplateConstraint cDbConstraint in dbConstraint.ChildConstraints.Where(cc => cc.IsPrimitive == false).OrderBy(y => y.Order))
            {
                ConstraintViewModel nextNewConstraint
                    = BuildConstraint(tdb, baseLink, igSettings, igTypePlugin, cDbConstraint, ++nextConstraintCount, dbConstraint.Id);
                lGreenViewModel.children.Add(nextNewConstraint);
            }

            return lGreenViewModel;
        }

        private string GetCamelCaseTemplateName(string aTemplateName)
        {
            if (string.IsNullOrEmpty(aTemplateName)) return string.Empty;

            string [] lWords = aTemplateName.Split(' ');
            string lFirstWord = lWords[0].ToLower();
            string lFullWord = null;

            if (lWords.Length == 1)
            {
                lFullWord = lFirstWord;
            }
            else
            {
                string lRemainingWords = string.Join(" ", lWords.Skip(1));
                TextInfo myTI = CultureInfo.CurrentCulture.TextInfo;
                string lCasedPortion = myTI.ToTitleCase(lRemainingWords);
                lFullWord = lFirstWord + lCasedPortion.Replace(" ", string.Empty);
            }

            return lFullWord;
        }

        #endregion
    }
}