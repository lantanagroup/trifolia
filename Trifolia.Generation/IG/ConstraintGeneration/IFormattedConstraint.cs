using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

using Trifolia.DB;
using Trifolia.Shared;
using Trifolia.Shared.Plugins;

namespace Trifolia.Generation.IG.ConstraintGeneration
{
    public interface IFormattedConstraint
    {
        IGSettingsManager IgSettings { get; set; }
        IObjectRepository Tdb { get; set; }
        bool IncludeCategory { get; set; }
        bool LinkContainedTemplate { get; set; }
        bool LinkIsBookmark { get; set; }
        bool CreateLinkForValueSets { get; set; }

        string Category { get; set; }
        string Number { get; set; }
        string Context { get; set; }
        bool IsPrimitive { get; set; }
        bool IsBranch { get; set; }
        bool HasChildren { get; set; }
        string Narrative { get; set; }
        bool ParentIsBranch { get; set; }
        string ParentContext { get; set; }
        string ParentCardinality { get; set; }
        string Conformance { get; set; }
        string Cardinality { get; set; }
        string DataType { get; set; }
        int ContainedTemplateId { get; set; }
        string ContainedTemplateTitle { get; set; }
        string ContainedTemplateLink { get; set; }
        string ContainedTemplateOid { get; set; }
        string ValueConformance { get; set; }
        string StaticDynamic { get; set; }
        string ValueSetName { get; set; }
        string ValueSetOid { get; set; }
        DateTime? ValueSetVersion { get; set; }
        string CodeSystemName { get; set; }
        string CodeSystemOid { get; set; }
        string Value { get; set; }
        string DisplayName { get; set; }
        string Description { get; set; }
        string Label { get; set; }
        bool IsHeading { get; set; }
        string HeadingDescription { get; set; }
        string TemplateLinkBase { get; set; }
        string ValueSetLinkBase { get; set; }

        void ParseConstraint(IIGTypePlugin igTypePlugin, IConstraint constraint, Template containedTemplate = null, ValueSet valueSet = null, CodeSystem codeSystem = null);
        void ParseFormattedConstraint();
        Paragraph AddToDocParagraph(WIKIParser wikiParser, OpenXmlElement parent, int level, int id, string headingStyle);
        string GetPlainText(bool includeHeading = true, bool includeDescription = true, bool includeLabel = true);
        string GetHtml(WIKIParser parser, string linkBase, int constraintCount, bool includeLabel);
    }
}
