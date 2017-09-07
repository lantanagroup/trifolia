using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Trifolia.DB;
using Trifolia.Shared;
using Trifolia.Shared.Plugins;

namespace Trifolia.Generation.IG.ConstraintGeneration
{
    public class FormattedConstraintFactory
    {
        /// <summary>
        /// Different versions of FormattedConstraint classes that should be used based on date.
        /// Selection is based on date of publication (or current date if not published) being 
        /// greater than or equal to the date.
        /// </summary>
        private static Dictionary<DateTime, Type> versions = new Dictionary<DateTime, Type>()
        {
            { new DateTime(2014, 4, 21), typeof(FormattedConstraint20150421) },
            { new DateTime(2014, 4, 15), typeof(FormattedConstraint20140415) },
            { new DateTime(2016, 11, 28), typeof(FormattedConstraint20161128) }
        };

        public static IFormattedConstraint NewFormattedConstraint(
            IObjectRepository tdb,
            IGSettingsManager igSettings,
            IIGTypePlugin igTypePlugin,
            TemplateConstraint constraint,
            List<ConstraintReference> references = null,
            string templateLinkBase = null,
            string valueSetLinkBase = null,
            bool linkContainedTemplate = false,
            bool linkIsBookmark = false,
            bool createLinksForValueSets = false,
            bool includeCategory = true)
        {
            if (references == null)
            {
                references = (from tcr in tdb.TemplateConstraintReferences
                              join t in tdb.Templates on tcr.ReferenceIdentifier equals t.Oid
                              where tcr.TemplateConstraintId == constraint.Id
                              select new ConstraintReference()
                              {
                                  Bookmark = t.Bookmark,
                                  Identifier = t.Oid,
                                  Name = t.Name,
                                  TemplateConstraintId = tcr.TemplateConstraintId
                              }).ToList();
            }
            else
            {
                references = references.Where(y => y.TemplateConstraintId == constraint.Id).ToList();
            }

            return NewFormattedConstraint(
                tdb,
                igSettings,
                igTypePlugin,
                (IConstraint)constraint,
                references,
                templateLinkBase,
                valueSetLinkBase,
                linkContainedTemplate,
                linkIsBookmark,
                createLinksForValueSets,
                includeCategory,
                constraint.ValueSet,
                constraint.CodeSystem);
        }

        public static IFormattedConstraint NewFormattedConstraint(
            IObjectRepository tdb, 
            IGSettingsManager igSettings,
            IIGTypePlugin igTypePlugin,
            IConstraint constraint,
            List<ConstraintReference> references,
            string templateLinkBase = null,
            string valueSetLinkBase = null,
            bool linkContainedTemplate = false, 
            bool linkIsBookmark = false, 
            bool createLinksForValueSets = false,
            bool includeCategory = true,
            ValueSet valueSet = null,
            CodeSystem codeSystem = null)
        {
            Type selectedType = null;

            if (igSettings == null || !igSettings.IsPublished)
            {
                var latestVersion = versions.Keys.OrderByDescending(y => y).First();
                selectedType = versions[latestVersion];
            }
            else
            {
                foreach (var versionDate in versions.Keys.OrderByDescending(y => y))
                {
                    if (igSettings.PublishDate > versionDate)
                    {
                        selectedType = versions[versionDate];
                        break;
                    }
                }

                if (selectedType == null)
                {
                    var latestVersion = versions.Keys.OrderByDescending(y => y).Last();
                    selectedType = versions[latestVersion];
                }
            }

            IFormattedConstraint formattedConstraint = (IFormattedConstraint)Activator.CreateInstance(selectedType);

            formattedConstraint.Tdb = tdb;
            formattedConstraint.IgSettings = igSettings;
            formattedConstraint.IncludeCategory = includeCategory;
            formattedConstraint.TemplateLinkBase = templateLinkBase;
            formattedConstraint.ValueSetLinkBase = valueSetLinkBase;
            formattedConstraint.LinkContainedTemplate = linkContainedTemplate;
            formattedConstraint.LinkIsBookmark = linkIsBookmark;
            formattedConstraint.CreateLinkForValueSets = createLinksForValueSets;
            formattedConstraint.ConstraintReferences = references;

            // Set the properties in the FormattedConstraint based on the IConstraint
            formattedConstraint.ParseConstraint(igTypePlugin, constraint, valueSet, codeSystem);

            // Pre-process the constraint so that calls to GetHtml(), GetPlainText(), etc. returns something
            formattedConstraint.ParseFormattedConstraint();

            return formattedConstraint;
        }
    }
}
