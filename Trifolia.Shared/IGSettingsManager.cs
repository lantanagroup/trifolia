using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trifolia.Config;
using Trifolia.DB;

namespace Trifolia.Shared
{
    [Serializable]
    public class IGSettingsManager
    {
        public enum SettingProperty
        {
            CardinalityOneToOne,
            CardinalityZeroToOne,
            CardinalityAtLeastOne,
            CardinalityZeroOrMore,
            CardinalityZero,
            UseConsolidatedConstraintFormat,
            Volume1Html,
            Categories
        }

        private List<ImplementationGuideSetting> settings = null;
        private List<IGTemplateType> templateTypes = null;
        private IObjectRepository tdb;

        public bool IsPublished { get; set; }
        public DateTime? PublishDate { get; set; }
        public int ImplementationGuideId { get; set; }

        public List<IGTemplateType> TemplateTypes
        {
            get { return templateTypes; }
            set { templateTypes = value; }
        }

        public IGSettingsManager(IObjectRepository tdb, int implementationGuideId)
        {
            this.ImplementationGuideId = implementationGuideId;
            this.tdb = tdb;

            ImplementationGuide ig = this.tdb.ImplementationGuides.Single(y => y.Id == implementationGuideId);

            this.IsPublished = ig.IsPublished();
            this.PublishDate = ig.PublishDate;

            this.settings = tdb.ImplementationGuideSettings
                .Where(y => y.ImplementationGuideId == implementationGuideId)
                .ToList();

            LoadImplementationGuideType(ig.ImplementationGuideTypeId);

            foreach (ImplementationGuideTemplateType cIgTemplateType in this.tdb.ImplementationGuideTemplateTypes.Where(y =>
                y.ImplementationGuideId == this.ImplementationGuideId))
            {
                IGTemplateType cItem = this.templateTypes.Single(y => y.TemplateTypeId == cIgTemplateType.TemplateTypeId);
                cItem.Name = cIgTemplateType.Name;
                cItem.DetailsText = cIgTemplateType.DetailsText;
            }
        }

        public IGSettingsManager(IObjectRepository tdb)
        {
            this.settings = new List<ImplementationGuideSetting>();
            this.templateTypes = new List<IGTemplateType>();
            this.tdb = tdb;
        }

        public void LoadImplementationGuideType(int implementationGuideTypeId)
        {
            LoadImplementationGuideType(this.tdb, implementationGuideTypeId);
        }

        public void LoadImplementationGuideType(IObjectRepository tdb, int implementationGuideTypeId)
        {
            this.templateTypes = (from tt in tdb.TemplateTypes
                                  where tt.ImplementationGuideTypeId == implementationGuideTypeId
                                  select new IGTemplateType()
                                  {
                                      TemplateTypeId = tt.Id,
                                      Name = tt.Name,
                                      IGTypeName = tt.Name,
                                      OutputOrder = tt.OutputOrder
                                  }).ToList();
        }

        public string GetSetting(string property)
        {
            ImplementationGuideSetting setting = this.settings.SingleOrDefault(y => y.PropertyName == property);

            if (setting == null)
            {
                SettingProperty predefinedProperty = SettingProperty.CardinalityZero;

                if (Enum.TryParse<SettingProperty>(property, out predefinedProperty))
                {
                    return GetDefaultSetting(predefinedProperty);
                }

                return string.Empty;
            }

            return setting.PropertyValue;
        }

        public string GetSetting(SettingProperty property)
        {
            return GetSetting(property.ToString());
        }

        public bool GetBoolSetting(SettingProperty property)
        {
            string value = GetSetting(property.ToString());
            return bool.Parse(value);
        }

        public void SaveSetting(string property, string value)
        {
            ImplementationGuideSetting setting = this.tdb.ImplementationGuideSettings.SingleOrDefault(y => y.ImplementationGuideId == this.ImplementationGuideId && y.PropertyName == property);
            SettingProperty predefinedProperty = SettingProperty.CardinalityZero;
            string predefinedValue = string.Empty;
            bool removed = false;

            if (Enum.TryParse<SettingProperty>(property, out predefinedProperty))
            {
                predefinedValue = GetDefaultSetting(predefinedProperty);
            }

            if (setting == null)
            {
                if (predefinedValue == value)
                    return;

                setting = new ImplementationGuideSetting();
                setting.ImplementationGuideId = this.ImplementationGuideId;
                this.tdb.ImplementationGuideSettings.Add(setting);
            }
            else if (predefinedValue == value)
            {
                this.tdb.ImplementationGuideSettings.Remove(setting);
                removed = true;
            }

            if (!removed)
            {
                if (setting.PropertyName != property)
                    setting.PropertyName = property;

                if (setting.PropertyValue != value)
                    setting.PropertyValue = value;
            }

            this.tdb.SaveChanges();
        }

        public void SaveTemplateTypes()
        {
            List<ImplementationGuideTemplateType> existingItems = this.tdb.ImplementationGuideTemplateTypes.Where(y =>
                y.ImplementationGuideId == this.ImplementationGuideId)
                .ToList();
            List<ImplementationGuideTemplateType> updatedItems = (from e in existingItems
                                                                    join it in this.TemplateTypes on e.TemplateTypeId equals it.TemplateTypeId
                                                                    select e).ToList();

            // Remove template types that don't exist for this IG any more
            var deletedTemplateTypes = existingItems.Where(y => !this.TemplateTypes.Exists(x => x.TemplateTypeId == y.TemplateTypeId));

            foreach (ImplementationGuideTemplateType cIgTemplateType in deletedTemplateTypes)
                this.tdb.ImplementationGuideTemplateTypes.Remove(cIgTemplateType);

            // Update existing template types for the IG
            foreach (ImplementationGuideTemplateType cItem in updatedItems)
            {
                IGTemplateType cIgTemplateType = this.TemplateTypes.Single(y => y.TemplateTypeId == cItem.TemplateTypeId);
                cItem.Name = cIgTemplateType.Name;
                cItem.DetailsText = cIgTemplateType.DetailsText;

                if (string.IsNullOrEmpty(cItem.Name))
                {
                    TemplateType originalTemplateType = this.tdb.TemplateTypes.Single(y => y.Id == cItem.TemplateTypeId);
                    cItem.Name = originalTemplateType.Name;
                }
            }

            // Add new template types for the IG
            foreach (IGTemplateType cItem in this.TemplateTypes.Where(y => !existingItems.Exists(x => x.TemplateTypeId == y.TemplateTypeId)))
            {
                ImplementationGuideTemplateType newIgTemplateType = new ImplementationGuideTemplateType()
                {
                    TemplateTypeId = cItem.TemplateTypeId,
                    ImplementationGuideId = this.ImplementationGuideId,
                    Name = cItem.Name,
                    DetailsText = cItem.DetailsText
                };


                if (string.IsNullOrEmpty(newIgTemplateType.Name))
                {
                    TemplateType originalTemplateType = this.tdb.TemplateTypes.Single(y => y.Id == newIgTemplateType.TemplateTypeId);
                    newIgTemplateType.Name = originalTemplateType.Name;
                }

                this.tdb.ImplementationGuideTemplateTypes.Add(newIgTemplateType);
            }

            this.tdb.SaveChanges();
        }

        public void SaveSetting(SettingProperty property, string value)
        {
            SaveSetting(property.ToString(), value);
        }

        public void SaveBoolSetting(SettingProperty property, bool value)
        {
            SaveSetting(property.ToString(), value.ToString());
        }

        private static string GetDefaultSetting(SettingProperty property)
        {
            switch (property)
            {
                case SettingProperty.CardinalityOneToOne:
                    return AppSettings.CardinalityOneToOne;
                case SettingProperty.CardinalityZeroToOne:
                    return AppSettings.CardinalityZeroToOne;
                case SettingProperty.CardinalityAtLeastOne:
                    return AppSettings.CardinalityAtLeastOne;
                case SettingProperty.CardinalityZeroOrMore:
                    return AppSettings.CardinalityZeroOrMore;
                case SettingProperty.CardinalityZero:
                    return AppSettings.CardinalityZero;
                case SettingProperty.UseConsolidatedConstraintFormat:
                    return "true";
                default:
                    return string.Empty;
            }
        }

        [Serializable]
        public class IGTemplateType
        {
            public int TemplateTypeId { get; set; }
            public string IGTypeName { get; set; }
            public string Name { get; set; }
            public string DetailsText { get; set; }
            public int OutputOrder { get; set; }

            public IGTemplateType()
            { }
        }
    }
}
