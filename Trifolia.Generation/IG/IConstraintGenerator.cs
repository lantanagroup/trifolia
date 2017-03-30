using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;

using Trifolia.Shared;
using Trifolia.DB;
using Trifolia.Shared.Plugins;

namespace Trifolia.Generation.IG
{
    public interface IConstraintGenerator
    {
        IIGTypePlugin IGTypePlugin { get; set; }
        IGSettingsManager IGSettings { get; set; }
        Body DocumentBody { get; set; }
        FigureCollection Figures { get; set; }
        WIKIParser WikiParser { get; set; }
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

        void GenerateConstraints(bool aCreateHyperlinksForValueSetNames = false, bool includeNotes = false);
    }
}
