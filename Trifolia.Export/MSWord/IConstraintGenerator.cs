using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Collections.Generic;
using Trifolia.DB;
using Trifolia.Plugins;
using Trifolia.Shared;

namespace Trifolia.Export.MSWord
{
    public interface IConstraintGenerator
    {
        IIGTypePlugin IGTypePlugin { get; set; }
        IGSettingsManager IGSettings { get; set; }
        MainDocumentPart MainPart { get; set; }
        Body DocumentBody { get; set; }
        FigureCollection Figures { get; set; }
        bool IncludeSamples { get; set; }
        IObjectRepository DataSource { get; set; }
        List<TemplateConstraint> RootConstraints { get; set; }
        List<TemplateConstraint> AllConstraints { get; set; }
        Template CurrentTemplate { get; set; }
        List<Template> AllTemplates { get; set; }
        string ConstraintHeadingStyle { get; set; }
        CommentManager CommentManager { get; set; }
        bool IncludeCategory { get; set; }
        List<string> SelectedCategories { get; set; }
        List<ConstraintReference> ConstraintReferences { get; set; }
        HyperlinkTracker HyperlinkTracker { get; set; }

        void GenerateConstraints(bool aCreateHyperlinksForValueSetNames = false, bool includeNotes = false);
    }
}