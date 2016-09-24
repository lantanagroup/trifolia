using System;
using System.Collections.Generic;
using Trifolia.DB;
using Trifolia.Shared;

namespace Trifolia.Shared.Plugins
{
    public interface IIGTypePlugin
    {
        /// <summary>
        /// The sub-xpath for schematron's closed template assertion, which is used on each template that should NOT be found within context.
        /// This xpath assumes that only a root is available for the template, not the extension
        /// </summary>
        /// <remarks>
        /// {0} = schema prefix
        /// {1} = template identifier root
        /// </remarks>
        string ClosedTemplateIdentifierXpath { get; }

        /// <summary>
        /// The sub-xpath for schematron's closed template assertion, which is used on each template that should NOT be found within context
        /// This xpath assumes that root AND extension are available for the template
        /// </summary>
        /// <remarks>
        /// {0} = schema prefix
        /// {1} = template identifier root
        /// {2} = template identifier extension
        /// </remarks>
        string ClosedTemplateVersionIdentifierXpath { get; }

        /// <summary>
        /// The format string for schematron's closed template assertion
        /// </summary>
        /// <remarks>
        /// {0} = schema prefix (including :)
        /// {1} = sub-xpath for version identifier checks. ex: @root != 'XXX' and not(@root = 'XXX' and @extension = 'YYY')
        /// </remarks>
        string ClosedTemplateXpath { get; }

        /// <summary>
        /// The format string for schematron to identify an unversioned template.
        /// </summary>
        /// <remarks>
        /// {0} = schema prefix (including :)
        /// {1} = template identifier root
        /// </remarks>
        string TemplateIdentifierXpath { get; }

        /// <summary>
        /// The format string for schematron to identify a versioned template.
        /// </summary>
        /// <remarks>
        /// {0} = schema prefix (including :)
        /// {1} = template identifier root
        /// {2} = template identifier extension
        /// </remarks>
        string TemplateVersionIdentifierXpath { get; }

        /// <summary>
        /// Fills the sample element with data commonly used by the implementation guide type, if the data doesn't already exist
        /// </summary>
        /// <param name="element">The XML sample element that needs to be filled with sample data</param>
        void FillSampleData(System.Xml.XmlElement element);

        /// <summary>
        /// Adds the template identifier to the generated sample.
        /// </summary>
        /// <param name="templateElement">The XML sample element that represents the template as a whole (ex: observation)</param>
        /// <param name="template">The template in question</param>
        /// <remarks>
        /// In the case of CDA, adds a "templateId" element with at least a @root attribute, and optionally an @extension
        /// </remarks>
        void AddTemplateIdentifierToSample(System.Xml.XmlElement templateElement, Trifolia.DB.Template template);

        string ParseIdentifier(string identifier);

        string Export(IObjectRepository tdb, SimpleSchema schema, ExportFormats format, IGSettingsManager igSettings, List<string> categories, List<Template> templates, bool includeVocabulary, bool returnJson = true);

        string GenerateSample(IObjectRepository tdb, Template template);
    }

    public enum ExportFormats
    {
        Proprietary,
        FHIR,
        TemplatesDSTU,
        Snapshot,
        FHIRBuild
    }
}
