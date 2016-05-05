using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Trifolia.Config
{
    public class ToolTipSection : ConfigurationSection
    {
        public const string TemplateEditorConstraintContainedTemplate = "TemplateEditor_Constraint_ContainedTemplate";
        public const string TemplateEditorConstraintDescription = "TemplateEditor_Constraint_Description";
        public const string TemplateEditorConstraintLabel = "TemplateEditor_Constraint_Label";
        public const string TemplateEditorConstraintIsInheritable = "TemplateEditor_Constraint_IsInheritable";
        public const string TemplateEditorConstraintTemplateContext = "TemplateEditor_Constraint_TemplateContext";
        public const string TemplateEditorConstraintValueSetDate = "TemplateEditor_Constraint_ValueSetDate";
        public const string TemplateEditorConstraintNotes = "TemplateEditor_Constraint_Notes";
        public const string TemplateEditorSearchConformance = "TemplateEditor_SearchConformance";
        public const string TemplateEditorValidationButton = "TemplateEditor_ValidationButton";

        private const string TOOLTIP_COLLECTION_PROP = "toolTips";
        private const string CONFIG_SECTION_NAME = "toolTip";

        private static ToolTipSection _configSection;

        public static ToolTipSection GetSection()
        {
            return _configSection ?? (_configSection = ConfigurationManager.GetSection(CONFIG_SECTION_NAME) as ToolTipSection);
        }

        public static string GetToolTipText(string toolTipId)
        {
            ToolTipSection section = GetSection();
            return section.ToolTips[toolTipId].Text;
        }

        [ConfigurationProperty(TOOLTIP_COLLECTION_PROP, IsDefaultCollection = true)]
        public ToolTipCollection ToolTips
        {
            get { return (ToolTipCollection)base[TOOLTIP_COLLECTION_PROP]; }
        }
    }
}
