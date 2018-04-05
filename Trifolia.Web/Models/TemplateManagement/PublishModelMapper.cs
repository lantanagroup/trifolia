using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Trifolia.Export.MSWord;
using Trifolia.Export.MSWord.ConstraintGeneration;
using Trifolia.Shared;
using Trifolia.Shared.Plugins;

namespace Trifolia.Web.Models.TemplateManagement
{
    public class PublishModelMapper
    {
        #region Private Fields

        private DB.IObjectRepository _tdb;

        #endregion

        #region Ctor

        public PublishModelMapper(DB.IObjectRepository tdb)
        {
            _tdb = tdb;
        }

        #endregion

        #region Public Methods

        public PublishModel MapEntityToViewModel(DB.Template aTemplate)
        {
            PublishModel lModel = new PublishModel();
            lModel.TemplateId = aTemplate.Id;
            lModel.TemplateName = aTemplate.Name;
            lModel.TemplateOid = aTemplate.Oid;

            foreach (DB.TemplateSample lSample in aTemplate.TemplateSamples)
            {
                XmlSample lViewSample = new XmlSample()
                {
                    Id = lSample.Id,
                    Name = lSample.Name,
                    SampleText = lSample.XmlSample,
                    TemplateId = aTemplate.Id
                };

                lModel.XmlSamples.Add(lViewSample);
            }

            IGSettingsManager igManager = new IGSettingsManager(_tdb, aTemplate.OwningImplementationGuideId);
            IIGTypePlugin igTypePlugin = aTemplate.OwningImplementationGuide.ImplementationGuideType.GetPlugin();
            string baseLink = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path) + "?Id=";

            int constraintCount = 0;
            foreach (DB.TemplateConstraint cDbConstraint in aTemplate.ChildConstraints.Where(y => y.Parent == null).OrderBy(y => y.Order))
            {
                PublishConstraint lConstraintView = this.BuildConstraint(_tdb, baseLink, igManager, igTypePlugin, cDbConstraint, constraintCount);
                lModel.Constraints.Add(lConstraintView);
            }

            return lModel;
        }

        public void MapViewModelToEntity(PublishModel aModel)
        {
            DB.Template lTemplate = _tdb.Templates.Single(t => t.Id == aModel.TemplateId);

            this.UpdateTemplateSamples(aModel, lTemplate);
            this.UpdateTemplateConstraints(aModel, lTemplate);
        }

        #endregion

        #region Private Methods

        private PublishConstraint BuildConstraint(
            DB.IObjectRepository tdb, 
            string baseLink, 
            IGSettingsManager igSettings, 
            IIGTypePlugin igTypePlugin,
            DB.TemplateConstraint dbConstraint, 
            int constraintCount, 
            int? aParentConstraintId = null)
        {
            IFormattedConstraint fc = FormattedConstraintFactory.NewFormattedConstraint(tdb, igSettings, igTypePlugin, dbConstraint);
            WIKIParser wikiParser = new WIKIParser(tdb);

            PublishConstraint newConstraint = new PublishConstraint(dbConstraint, fc);

            foreach (DB.TemplateConstraintSample lSample in dbConstraint.Samples)
            {
                ConstraintSample lSampleView = new ConstraintSample()
                {
                    Id = lSample.Id,
                    Name = lSample.Name,
                    SampleText = lSample.SampleText,
                    ConstraintId = dbConstraint.Id
                };
                newConstraint.Samples.Add(lSampleView);
            }

            if (aParentConstraintId.HasValue) newConstraint.ParentConstraintId = aParentConstraintId.Value;

            int nextConstraintCount = 0;
            foreach (DB.TemplateConstraint cDbConstraint in dbConstraint.ChildConstraints.OrderBy(y => y.Order))
            {
                PublishConstraint nextNewConstraint 
                    = BuildConstraint(tdb, baseLink, igSettings, igTypePlugin, cDbConstraint, ++nextConstraintCount, dbConstraint.Id);
                newConstraint.ChildConstraints.Add(nextNewConstraint);
            }

            return newConstraint;
        }

        private void UpdateTemplateConstraints(PublishModel aModel, DB.Template aTemplate)
        {
            foreach (PublishConstraint lConstraintView in aModel.Constraints)
            {
                DB.TemplateConstraint lConstraint = _tdb.TemplateConstraints.Single(tc => tc.Id == lConstraintView.Id && tc.TemplateId == aTemplate.Id);
                this.UpdateConstraint(lConstraint, lConstraintView);
            }
        }

        private void UpdateConstraint(DB.TemplateConstraint aConstraint, PublishConstraint aConstraintView)
        {
            aConstraint.IsHeading = aConstraintView.IsHeading;
            aConstraint.HeadingDescription = aConstraintView.HeadingDescription;
            aConstraint.Description = aConstraintView.ConstraintDescription;
            aConstraint.Label = aConstraintView.ConstraintLabel;
            aConstraint.PrimitiveText = aConstraintView.PrimitiveText;

            // Remove all samples and the heading description if the constraint is no longer a heading
            if (!aConstraint.IsHeading)
            {
                aConstraintView.HeadingDescription = null;

                foreach (var sampleView in aConstraintView.Samples)
                {
                    sampleView.IsDeleted = true;
                }
            }

            foreach (ConstraintSample lSample in aConstraintView.Samples.Where(s => s.Id.HasValue && s.IsDeleted))
            {
                DB.TemplateConstraintSample lDeletedSample = _tdb.TemplateConstraintSamples.Single(tcs => tcs.Id == lSample.Id);
                _tdb.TemplateConstraintSamples.Remove(lDeletedSample);
            }

            foreach (ConstraintSample lSample in aConstraintView.Samples.Where(s => s.Id.HasValue && s.IsDeleted == false))
            {
                DB.TemplateConstraintSample lUpdatedSample = _tdb.TemplateConstraintSamples.Single(tcs => tcs.Id == lSample.Id);

                if (!string.Equals(lUpdatedSample.Name, lSample.Name)) lUpdatedSample.Name = lSample.Name;
                if (!string.Equals(lUpdatedSample.SampleText, lSample.SampleText)) lUpdatedSample.SampleText = lSample.SampleText;
            }

            foreach (ConstraintSample lSample in aConstraintView.Samples.Where(s => s.Id.HasValue == false && s.IsDeleted == false))
            {
                DB.TemplateConstraintSample lNewSample = new DB.TemplateConstraintSample();
                lNewSample.Name = lSample.Name;
                lNewSample.SampleText = lSample.SampleText;
                aConstraint.Samples.Add(lNewSample);
            }

            foreach (PublishConstraint lChildConstraint in aConstraintView.ChildConstraints)
            {
                DB.TemplateConstraint lConstraint = _tdb.TemplateConstraints.Single(tc => tc.Id == lChildConstraint.Id);
                this.UpdateConstraint(lConstraint, lChildConstraint);
            }
        }

        private void UpdateTemplateSamples(PublishModel aModel, DB.Template aTemplate)
        {
            IEnumerable<XmlSample> lDeletedSamples = from x in aModel.XmlSamples
                                                     where x.IsDeleted
                                                     && x.Id.HasValue
                                                     select x;

            foreach (XmlSample lDeletedSample in lDeletedSamples)
            {
                DB.TemplateSample lSample = aTemplate.TemplateSamples.Single(s => s.Id == lDeletedSample.Id);
                _tdb.TemplateSamples.Remove(lSample);
            }

            IEnumerable<XmlSample> lNewSamples = from x in aModel.XmlSamples
                                                 where !x.Id.HasValue
                                                 select x;

            foreach (XmlSample lNewSample in lNewSamples)
            {
                DB.TemplateSample lSample = new DB.TemplateSample() { Name = lNewSample.Name, XmlSample = lNewSample.SampleText };
                aTemplate.TemplateSamples.Add(lSample);
            }

            IEnumerable<XmlSample> lUpdatedSamples = from x in aModel.XmlSamples
                                                     where !x.IsDeleted
                                                     && x.Id.HasValue
                                                     select x;

            foreach (XmlSample lUpdatedSample in lUpdatedSamples)
            {
                DB.TemplateSample lSample = _tdb.TemplateSamples.Single(ts => ts.Id == lUpdatedSample.Id);
                lSample.Name = lUpdatedSample.Name;
                lSample.XmlSample = lUpdatedSample.SampleText;
            }
        }

        #endregion
    }
}