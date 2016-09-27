using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Trifolia.DB;
using Trifolia.Authorization;
using Trifolia.Shared;
using ImportModel = Trifolia.Shared.ImportExport.Model.Trifolia;
using ImportImplementationGuide = Trifolia.Shared.ImportExport.Model.TrifoliaImplementationGuide;
using ImportImplementationGuideSection = Trifolia.Shared.ImportExport.Model.TrifoliaImplementationGuideVolume1Section;

namespace Trifolia.Import.Native
{
    public class ImplementationGuideImporter
    {
        private IObjectRepository tdb;
        private TrifoliaDatabase dataSource;

        public List<string> Errors { get; set; }

        public ImplementationGuideImporter(IObjectRepository tdb)
        {
            this.tdb = tdb;
            this.dataSource = tdb as TrifoliaDatabase;
        }

        private PublishStatus GetImportStatus(ImportImplementationGuide importIg)
        {
            switch (importIg.status)
            {
                case Shared.ImportExport.Model.ImplementationGuideStatus.Ballot:
                    return PublishStatus.GetBallotStatus(this.tdb);
                case Shared.ImportExport.Model.ImplementationGuideStatus.Published:
                    return PublishStatus.GetPublishedStatus(this.tdb);
                case Shared.ImportExport.Model.ImplementationGuideStatus.Deprecated:
                    return PublishStatus.GetDeprecatedStatus(this.tdb);
                case Shared.ImportExport.Model.ImplementationGuideStatus.Retired:
                    return PublishStatus.GetRetiredStatus(this.tdb);
                default:
                    return PublishStatus.GetDraftStatus(this.tdb);
            }
        }

        private ImplementationGuide FindImplementationGuide(string name, int version)
        {
            var foundImplementationGuide = this.tdb.ImplementationGuides.SingleOrDefault(y =>
                y.Name.ToLower() == name.ToLower() &&
                (
                    (y.Version == null && version == 1) ||
                    (y.Version != null && y.Version.Value == version)
                ));

            return foundImplementationGuide;
        }

        private void RemoveVolume1Sections(ImplementationGuide implementationGuide)
        {
            var allSections = implementationGuide.Sections.ToList();

            foreach (var section in allSections)
            {
                this.tdb.ImplementationGuideSections.DeleteObject(section);
            }
        }

        private void UpdateVolume1(ImplementationGuide implementationGuide, IGSettingsManager igSettings, ImportImplementationGuide importImplementationGuide)
        {
            if (importImplementationGuide.Volume1 == null)
            {
                igSettings.SaveSetting(IGSettingsManager.SettingProperty.Volume1Html, string.Empty);
                this.RemoveVolume1Sections(implementationGuide);
                return;
            }

            if (importImplementationGuide.Volume1.Items.Count == 1 && importImplementationGuide.Volume1.Items.First() is string)
            {
                this.RemoveVolume1Sections(implementationGuide);
                igSettings.SaveSetting(IGSettingsManager.SettingProperty.Volume1Html, importImplementationGuide.Volume1.Items.First() as string);
            }
            else if (importImplementationGuide.Volume1.Items.Count > 0)
            {
                igSettings.SaveSetting(IGSettingsManager.SettingProperty.Volume1Html, string.Empty);

                foreach (ImportImplementationGuideSection importSection in importImplementationGuide.Volume1.Items)
                {
                    if (importSection.Heading == null)
                        throw new ArgumentException("All implementation guide sections must have a heading");

                    ImplementationGuideSection newSection = new ImplementationGuideSection()
                    {
                        Heading = importSection.Heading.Title,
                        Level = importSection.Heading.Level,
                        Order = implementationGuide.Sections.Count + 1
                    };
                    implementationGuide.Sections.Add(newSection);
                }
            }
        }

        private void UpdateCustomSchematron(ImplementationGuide implementationGuide, ImportImplementationGuide importImplementationGuide)
        {
            var allCustomSchematrons = implementationGuide.SchematronPatterns.ToList();

            // Remove all first
            foreach (var customSchematron in allCustomSchematrons)
            {
                this.tdb.ImplementationGuideSchematronPatterns.DeleteObject(customSchematron);
            }

            // Add all in import as new
            foreach (var importCustomSchematron in importImplementationGuide.CustomSchematron)
            {
                var foundSchematronPattern = new ImplementationGuideSchematronPattern();
                foundSchematronPattern.PatternId = importCustomSchematron.patternId;
                implementationGuide.SchematronPatterns.Add(foundSchematronPattern);

                if (foundSchematronPattern.Phase != importCustomSchematron.phase)
                    foundSchematronPattern.Phase = importCustomSchematron.phase;

                if (foundSchematronPattern.PatternContent != importCustomSchematron.Rule)
                    foundSchematronPattern.PatternContent = importCustomSchematron.Rule;
            }
        }

        private void UpdateTemplateTypes(ImplementationGuide implementationGuide, ImportImplementationGuide importImplementationGuide)
        {
            foreach (var importTemplateType in importImplementationGuide.CustomTemplateType)
            {
                var foundTemplateType = implementationGuide.ImplementationGuideType.TemplateTypes.SingleOrDefault(y => y.Name.ToLower() == importTemplateType.templateTypeName.ToLower());

                if (foundTemplateType == null)
                    throw new Exception("Could not find template type " + importTemplateType.templateTypeName + " associated with implementation guide type");

                var foundIgTemplateType = implementationGuide.TemplateTypes.SingleOrDefault(y => y.TemplateTypeId == foundTemplateType.Id);

                if (foundIgTemplateType == null)
                {
                    foundIgTemplateType = new ImplementationGuideTemplateType()
                    {
                        ImplementationGuide = implementationGuide,
                        TemplateType = foundTemplateType
                    };

                    implementationGuide.TemplateTypes.Add(foundIgTemplateType);
                    this.tdb.ImplementationGuideTemplateTypes.AddObject(foundIgTemplateType);
                }

                if (foundIgTemplateType.Name != importTemplateType.CustomName)
                    foundIgTemplateType.Name = importTemplateType.CustomName;

                if (foundIgTemplateType.DetailsText != importTemplateType.Description)
                    foundIgTemplateType.DetailsText = importTemplateType.Description;
            }
        }

        private void UpdateCardinalities(IGSettingsManager igSettings, ImportImplementationGuide importImplementationGuide)
        {
            foreach (var importCardinality in importImplementationGuide.CardinalityDisplay)
            {
                switch (importCardinality.cardinality)
                {
                    case Shared.ImportExport.Model.CardinalityTypes.Zero:
                        igSettings.SaveSetting(IGSettingsManager.SettingProperty.CardinalityZero, importCardinality.display);
                        break;
                    case Shared.ImportExport.Model.CardinalityTypes.ZeroOrOne:
                        igSettings.SaveSetting(IGSettingsManager.SettingProperty.CardinalityZeroToOne, importCardinality.display);
                        break;
                    case Shared.ImportExport.Model.CardinalityTypes.ZeroToMany:
                        igSettings.SaveSetting(IGSettingsManager.SettingProperty.CardinalityZeroOrMore, importCardinality.display);
                        break;
                    case Shared.ImportExport.Model.CardinalityTypes.One:
                        igSettings.SaveSetting(IGSettingsManager.SettingProperty.CardinalityOneToOne, importCardinality.display);
                        break;
                    case Shared.ImportExport.Model.CardinalityTypes.OneToMany:
                        igSettings.SaveSetting(IGSettingsManager.SettingProperty.CardinalityAtLeastOne, importCardinality.display);
                        break;
                    default:
                        throw new ArgumentException("Unexpected cardinality value " + importCardinality.cardinality);
                }
            }
        }

        private void UpdateProperties(ImplementationGuide implementationGuide, ImplementationGuideType igType, ImportImplementationGuide importImplementationGuide)
        {
            var importIgStatus = GetImportStatus(importImplementationGuide);
            var organization = !string.IsNullOrEmpty(importImplementationGuide.organizationName) ? this.tdb.Organizations.SingleOrDefault(y => y.Name.ToLower() == importImplementationGuide.organizationName.ToLower()) : null;

            if (implementationGuide.Organization != organization)
                implementationGuide.Organization = organization;

            if (implementationGuide.Name != importImplementationGuide.name)
                implementationGuide.Name = importImplementationGuide.name;

            if (implementationGuide.ImplementationGuideType != igType)
                implementationGuide.ImplementationGuideType = igType;

            if (implementationGuide.Version != importImplementationGuide.version)
                implementationGuide.Version = importImplementationGuide.version;

            if (implementationGuide.DisplayName != importImplementationGuide.displayName)
                implementationGuide.DisplayName = importImplementationGuide.displayName;

            if (implementationGuide.WebReadmeOverview != importImplementationGuide.WebReadmeOverview)
                implementationGuide.WebReadmeOverview = importImplementationGuide.WebReadmeOverview;

            if (implementationGuide.WebDisplayName != importImplementationGuide.webDisplayName)
                implementationGuide.WebDisplayName = importImplementationGuide.webDisplayName;

            if (implementationGuide.WebDescription != importImplementationGuide.WebDescription)
                implementationGuide.WebDescription = importImplementationGuide.WebDescription;

            if (implementationGuide.PublishStatus != importIgStatus)
                implementationGuide.PublishStatus = importIgStatus;
        }

        private void UpdateCategories(ImplementationGuide implementationGuide, IGSettingsManager igSettings, ImportImplementationGuide importImplementationGuide)
        {
            var currentCategories = igSettings.GetSetting(IGSettingsManager.SettingProperty.Categories);

            if (importImplementationGuide.Category == null || importImplementationGuide.Category.Count == 0)
            {
                if (!string.IsNullOrEmpty(currentCategories))
                    igSettings.SaveSetting(IGSettingsManager.SettingProperty.Categories, string.Empty);

                return;
            }

            var categories = importImplementationGuide.Category.Select(y => y.name.Replace(',', '-'));
            var categoriesString = String.Join(",", categories);

            if (currentCategories != categoriesString)
                igSettings.SaveSetting(IGSettingsManager.SettingProperty.Categories, categoriesString);
        }

        public ImplementationGuide Import(ImportImplementationGuide importImplementationGuide)
        {
            var implementationGuide = FindImplementationGuide(importImplementationGuide.name, importImplementationGuide.version);
            ImplementationGuide foundPreviousVersionIg = null;
            IGSettingsManager igSettings = null;
            var igType = this.tdb.ImplementationGuideTypes.SingleOrDefault(y => y.Name.ToLower() == importImplementationGuide.type.ToLower());

            this.Errors = new List<string>();

            if (igType == null)
            {
                this.Errors.Add("Implementation guide type \"" + importImplementationGuide.type + "\" is not found");
                return null;
            }

            // Make sure IG's previous version is set
            // May not have an "id" for the previous version, so using the entity model to ensure the reference saves
            if (importImplementationGuide.PreviousVersion != null && !string.IsNullOrEmpty(importImplementationGuide.PreviousVersion.name) && importImplementationGuide.PreviousVersion.number > 0)
            {
                foundPreviousVersionIg = FindImplementationGuide(importImplementationGuide.PreviousVersion.name, importImplementationGuide.PreviousVersion.number);

                if (foundPreviousVersionIg != null && !foundPreviousVersionIg.PreviousVersion.Contains(implementationGuide))
                    foundPreviousVersionIg.PreviousVersion.Add(implementationGuide);
            }

            if (implementationGuide == null && foundPreviousVersionIg == null && this.tdb.ImplementationGuides.Count(y => y.Name.ToLower() == importImplementationGuide.name.ToLower()) > 0)
            {
                this.Errors.Add("Implementation guide with the same name already exists, and this is not a new version of that implementation guide.");
                return null;
            }

            if (implementationGuide == null)
            {
                implementationGuide = new ImplementationGuide();
                implementationGuide.ImplementationGuideType = igType;
                implementationGuide.ImplementationGuideTypeId = igType.Id;

                this.tdb.ImplementationGuides.AddObject(implementationGuide);
                igSettings = new IGSettingsManager(this.tdb);

                // Add default template types to new implementation guide
                foreach (var templateType in igType.TemplateTypes)
                {
                    var igTemplateType = new ImplementationGuideTemplateType()
                    {
                        TemplateType = templateType,
                        Name = templateType.Name
                    };
                    implementationGuide.TemplateTypes.Add(igTemplateType);
                }
            }
            else
            {
                igSettings = new IGSettingsManager(this.tdb, implementationGuide.Id);
            }

            // Set changed/new properties of implementation guide
            this.UpdateProperties(implementationGuide, igType, importImplementationGuide);

            // Update cardinalities
            this.UpdateCardinalities(igSettings, importImplementationGuide);

            // Update custom template type names
            this.UpdateTemplateTypes(implementationGuide, importImplementationGuide);

            // Update custom schematron rules
            this.UpdateCustomSchematron(implementationGuide, importImplementationGuide);

            // Update volume1 HTML and/or Sections
            this.UpdateVolume1(implementationGuide, igSettings, importImplementationGuide);

            // Update categories
            this.UpdateCategories(implementationGuide, igSettings, importImplementationGuide);

            if (foundPreviousVersionIg != null && foundPreviousVersionIg.PublishStatus != PublishStatus.GetPublishedStatus(this.tdb))
            {
                this.Errors.Add("Previous version of implementation guide is not published, and cannot hav a new version associated with it");
                return null;
            }

            var existingNextVersion = foundPreviousVersionIg != null ?
                this.tdb.ImplementationGuides.SingleOrDefault(y => y.PreviousVersionImplementationGuideId == foundPreviousVersionIg.Id) :
                null;

            // Validate versioning of the implementation guide
            if (foundPreviousVersionIg == null && implementationGuide.Version != 1)
            {
                this.Errors.Add("Implementation guide cannot have a version greater than one when no previous version is specified");
                return null;
            }

            if (foundPreviousVersionIg != null && existingNextVersion != null && existingNextVersion != implementationGuide)
            {
                this.Errors.Add("Previous version of implementation guide already has a new version created for it");
                return null;
            }

            // If the object is changed, make sure the user has permissions to the implementation guide
            if (this.dataSource != null)
            {
                var igState = this.dataSource.ObjectStateManager.GetObjectStateEntry(implementationGuide);

                if (igState.State == System.Data.Entity.EntityState.Modified && !CheckPoint.Instance.GrantEditImplementationGuide(implementationGuide.Id) && !CheckPoint.Instance.IsDataAdmin)
                {
                    this.Errors.Add("You do not have permission to modify implementation guide \"" + implementationGuide.Name + "\" version " + implementationGuide.Version);
                    return null;
                }
            }

            return implementationGuide;
        }
    }
}
