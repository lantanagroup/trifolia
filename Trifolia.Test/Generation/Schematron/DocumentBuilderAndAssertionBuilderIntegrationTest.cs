using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Trifolia.Export.Schematron;
using Trifolia.Export.Schematron.Model;
using Trifolia.Shared;
using Trifolia.DB;

namespace Schematron.Test.Generation.Schematron
{
    [TestClass]
    public class DocumentBuilderAndAssertionBuilderIntegrationTest
    {
        private const string templateIdentifierXpath = "{0}templateId[@root='{1}']";
        private const string templateVersionIdentifierXpath = "{0}templateId[@root='{1}' and @extension = '{2}']";

        [TestMethod]
        public void BuildAdvanceDirectiveObservationDocument_1stLevelOnly()
        {
            var sectionCount = 1;
            var phase = new Phase();
            phase.ID = "error";
            var document = new SchematronDocument();
            document.Phases.Add(phase);


            var doc = new DocumentTemplate("cda");
            doc.AddElement(new DocumentTemplateElement("observation"));
            doc.ChildElements[0].AddAttribute(new DocumentTemplateElementAttribute("classCode", "OBS"));
            doc.ChildElements[0].AddAttribute(new DocumentTemplateElementAttribute("moodCode", "EVN"));
            doc.AddElement(new DocumentTemplateElement("templateId"));
            doc.ChildElements[1].AddAttribute(new DocumentTemplateElementAttribute("root", "2.16.840.1.113883.10.20.22.4.48"));
            doc.AddElement(new DocumentTemplateElement("id"));
            doc.AddElement(new DocumentTemplateElement("code"));
            doc.ChildElements[doc.ChildElements.Count - 1].AddAttribute(new DocumentTemplateElementAttribute("xsi-type", "CE", "2.16.840.1.113883.1.11.20.2"));
            doc.AddElement(new DocumentTemplateElement("statusCode"));
            doc.ChildElements[doc.ChildElements.Count - 1].AddAttribute(new DocumentTemplateElementAttribute("code", "completed", "2.16.840.1.113883.5.14"));
            var participantElement = new DocumentTemplateElement("participant");
            doc.ChildElements[0].AddElement(participantElement);
            participantElement.AddAttribute(new DocumentTemplateElementAttribute("typeCode", "VRF"));
            var templateIdElement = new DocumentTemplateElement("templateId");
            templateIdElement.AddAttribute(new DocumentTemplateElementAttribute("root", "2.16.840.1.113883.10.20.1.58"));
            participantElement.AddElement(templateIdElement);
            var timeElement = new DocumentTemplateElement("time");
            timeElement.AddAttribute(new DocumentTemplateElementAttribute("xsi:type", "TS"));
            participantElement.AddElement(timeElement);
            var participantRoleElement = new DocumentTemplateElement("participantRole");
            participantElement.AddElement(participantRoleElement);


            var contextBuilder = new ContextBuilder(doc.ChildElements[0], "cda");
            var rule = new Rule();
            rule.Context = contextBuilder.GetFullyQualifiedContextString();

            var assertionBuilder = new AssertionLineBuilder(doc.ChildElements[0].Attributes[0], templateIdentifierXpath, templateVersionIdentifierXpath);  //"OBS"            
            rule.Assertions.Add(new Assertion()
            {
                AssertionMessage = "SHALL contain 1..1 @classCode='OBS' Observation (CodeSystem: HL7ActClass 2.16.840.1.113883.5.6) (CONF:8648).",
                Test             = assertionBuilder.WithCardinality(CardinalityParser.Parse("1..1")).WithinContext(contextBuilder.GetRelativeContextString()).ConformsTo(Conformance.SHALL).ToString()
            });

            assertionBuilder = new AssertionLineBuilder(doc.ChildElements[0].Attributes[1], templateIdentifierXpath, templateVersionIdentifierXpath);  //"EVN"
            rule.Assertions.Add(new Assertion()
            {
                AssertionMessage = "SHALL contain 1..1 @moodCode='EVN' Event (CodeSystem: ActMood 2.16.840.1.113883.5.1001) (CONF:8649).",
                Test             = assertionBuilder.WithCardinality(CardinalityParser.Parse("1..1")).WithinContext(contextBuilder.GetRelativeContextString()).ConformsTo(Conformance.SHALL).ToString()
            });
            var pattern = new Pattern();
            pattern.ID = sectionCount.ToString();
            pattern.Name = string.Format("pattern-{0}-errors", pattern.ID);
            pattern.Rules.Add(rule);
            phase.ActivePatterns.Add(pattern);

            rule = new Rule();
            contextBuilder = new ContextBuilder(doc.ChildElements[1], "cda");
            rule.Context = contextBuilder.GetFullyQualifiedContextString();
            assertionBuilder = new AssertionLineBuilder(doc.ChildElements[1], templateIdentifierXpath, templateVersionIdentifierXpath);  //"templateId[@rootCode]"
            rule.Assertions.Add(new Assertion()
            {
                AssertionMessage = "SHALL contain 1..1 @root='2.16.840.1.113883.10.20.22.4.48' (CONF:10485).",
                Test             = assertionBuilder.WithCardinality(CardinalityParser.Parse("1..1")).WithinContext(contextBuilder.GetRelativeContextString()).ConformsTo(Conformance.SHALL).ToString()
            });

            sectionCount++;
            pattern = new Pattern();
            pattern.ID = sectionCount.ToString();
            pattern.Name = string.Format("pattern-{0}-errors", pattern.ID);
            pattern.Rules.Add(rule);
            phase.ActivePatterns.Add(pattern);

            rule = new Rule();
            contextBuilder = new ContextBuilder(doc.ChildElements[2], "cda");
            rule.Context = contextBuilder.GetFullyQualifiedContextString();
            assertionBuilder = new AssertionLineBuilder(doc.ChildElements[2], templateIdentifierXpath, templateVersionIdentifierXpath);  //"1..* id"
            rule.Assertions.Add(new Assertion()
            {
                AssertionMessage = "SHALL contain 1..* id (CONF:8654)",
                Test = assertionBuilder.WithCardinality(CardinalityParser.Parse("1..*")).WithinContext(contextBuilder.GetRelativeContextString()).ConformsTo(Conformance.SHALL).ToString()
            });

            sectionCount++;
            pattern = new Pattern();
            pattern.ID = sectionCount.ToString();
            pattern.Name = string.Format("pattern-{0}-errors", pattern.ID);
            pattern.Rules.Add(rule);
            phase.ActivePatterns.Add(pattern);

            rule = new Rule();
            contextBuilder = new ContextBuilder(doc.ChildElements[3], "cda");
            rule.Context = contextBuilder.GetFullyQualifiedContextString();
            assertionBuilder = new AssertionLineBuilder(doc.ChildElements[3], templateIdentifierXpath, templateVersionIdentifierXpath);  //"1..1 code @xsi:type='CE' valueset = 2.16.840.1.113883.1.11.20.2"
            rule.Assertions.Add(new Assertion()
            {
                AssertionMessage = "SHALL contain 1..1 code with @xsi:type='CE', where the @code SHOULD be selected from ValueSet AdvanceDirectiveTypeCode 2.16.840.1.113883.1.11.20.2 STATIC 2006-10-17 (CONF:8651).",
                Test = assertionBuilder.WithCardinality(CardinalityParser.Parse("1..1")).WithinContext(contextBuilder.GetRelativeContextString()).ConformsTo(Conformance.SHALL).ToString()
            });

            sectionCount++;
            pattern = new Pattern();
            pattern.ID = sectionCount.ToString();
            pattern.Name = string.Format("pattern-{0}-errors", pattern.ID);
            pattern.Rules.Add(rule);
            phase.ActivePatterns.Add(pattern);

            rule = new Rule();
            contextBuilder = new ContextBuilder(doc.ChildElements[3], "cda");
            rule.Context = contextBuilder.GetFullyQualifiedContextString();
            assertionBuilder = new AssertionLineBuilder(doc.ChildElements[3], templateIdentifierXpath, templateVersionIdentifierXpath);  //"1..1 statusCode @code='completed' valueset = 2.16.840.1.113883.1.11.20.2"
            rule.Assertions.Add(new Assertion()
            {
                AssertionMessage = "SHALL contain 1..1 code with @xsi:type='CE', where the @code SHOULD be selected from ValueSet AdvanceDirectiveTypeCode 2.16.840.1.113883.1.11.20.2 STATIC 2006-10-17 (CONF:8651).",
                Test = assertionBuilder.WithCardinality(CardinalityParser.Parse("1..1")).WithinContext(contextBuilder.GetRelativeContextString()).ConformsTo(Conformance.SHALL).ToString()
            });

            sectionCount++;
            pattern = new Pattern();
            pattern.ID = sectionCount.ToString();
            pattern.Name = string.Format("pattern-{0}-errors", pattern.ID);
            pattern.Rules.Add(rule);
            phase.ActivePatterns.Add(pattern);

            rule = new Rule();
            contextBuilder = new ContextBuilder(doc.ChildElements[1].Attributes[0], "cda");
            rule.Context = contextBuilder.GetFullyQualifiedContextString();
            var childtemplateIdElementAssertionBuilder = new AssertionLineBuilder(templateIdElement.Attributes[0], templateIdentifierXpath, templateVersionIdentifierXpath)  //templateId/@root
                .WithCardinality(CardinalityParser.Parse("1..1"))
                .ConformsTo(Conformance.SHALL)
                .WithinContext("cda:");
            var childParticipantElementAssertionBuilder = new AssertionLineBuilder(participantRoleElement, templateIdentifierXpath, templateVersionIdentifierXpath)
                .WithCardinality(CardinalityParser.Parse("1..*"))
                .ConformsTo(Conformance.SHALL)
                .WithinContext("cda:");
            var childTimeElementAssertionBuilder = new AssertionLineBuilder(timeElement, templateIdentifierXpath, templateVersionIdentifierXpath)
                .WithCardinality(CardinalityParser.Parse("0..1"))
                .ConformsTo(Conformance.SHOULD)
                .WithinContext("cda:");
            assertionBuilder = new AssertionLineBuilder(participantElement, templateIdentifierXpath, templateVersionIdentifierXpath);  //participant
            rule.Assertions.Add(new Assertion()
            {
                AssertionMessage = "should contain 1..* participant (CONF:8662), participant should contain 0..1 time (CONF:8665), the data type of Observation/participant/time in a verification SHALL be TS (time stamp) (CONF:8666), participant shall contain 1..1 participantRole (CONF:8825), participant shall contain 1..1 @typeCode=VRF 'Verifier' (CodeSystem: 2.16.840.1.113883.5.90) (CONF:8663), participant shall contain 1..1 templateId (CONF:8664), templateId shall contain 1..1 @root=2.16.840.1.113883.10.20.1.58 (CONF:10486)",
                Test = assertionBuilder
                        .WithCardinality(CardinalityParser.Parse("1..*"))
                        .WithinContext("cda:")
                        .ConformsTo(Conformance.SHALL)
                        .WithChildElementBuilder(childTimeElementAssertionBuilder)
                        .WithChildElementBuilder(childParticipantElementAssertionBuilder)
                        .WithChildElementBuilder(childtemplateIdElementAssertionBuilder)
                        .ToString()
            });

            sectionCount++;
            pattern = new Pattern();
            pattern.ID = sectionCount.ToString();
            pattern.Name = string.Format("pattern-{0}-errors", pattern.ID);
            pattern.Rules.Add(rule);
            phase.ActivePatterns.Add(pattern);

            var builder = new SchematronDocumentSerializer();
            string serializedModel = builder.SerializeDocument(document);
            Assert.IsFalse(string.IsNullOrEmpty(serializedModel), "No string returned from serialize document");

            string[] lModelLines = serializedModel.Split('\n');
            Assert.IsNotNull(lModelLines, "The generated string was not split on lines");
            Assert.IsTrue(lModelLines.Length > 1, "The generated string was not split on lines");
        }
    }
}
