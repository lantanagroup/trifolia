using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Trifolia.Generation.Schematron;
using Trifolia.Generation.Schematron.Model;

namespace Schematron.Test.Generation
{
    [TestClass]
    public class ContextBuilderTest
    {
        [TestMethod]
        public void GenerateContext_SingleElement()
        {
            var element = new DocumentTemplateElement("code");
            var cdaDocumentTemplate = new DocumentTemplate("urn:hl7-org:v3");
            cdaDocumentTemplate.AddElement(element);
            var contextBuilder = new ContextBuilder(element, "cda");
            var context = contextBuilder.GetRelativeContextString();
            var expected = "cda:code";
            Assert.IsTrue(context == expected, "Single element did not generate proper context, expected '{0}', actual '{0}'", expected, context);
        }

        [TestMethod]
        public void GenerateContext_SingleElement_Namespace()
        {
            var element = new DocumentTemplateElement("code");
            var cdaDocumentTemplate = new DocumentTemplate("urn:hl7-org:v3");
            cdaDocumentTemplate.AddElement(element);
            var contextBuilder = new ContextBuilder(element, "ems");
            var context = contextBuilder.GetRelativeContextString();
            var expected = "ems:code";
            Assert.IsTrue(context == expected, "Single element did not generate proper context, expected '{0}', actual '{0}'", expected, context);
        }

        [TestMethod]
        public void GenerateContext_SingleChildElement_2Levels()
        {
            var administrativeCodeElement = new DocumentTemplateElement("administrativeGenderCode");            
            administrativeCodeElement.AddAttribute(new DocumentTemplateElementAttribute("code", "20", string.Empty, "MMG-GENDER-CODE-OID"));
            var cdaDocumentTemplate = new DocumentTemplate("urn:hl7-org:v3");
            cdaDocumentTemplate.AddElement(new DocumentTemplateElement("recordTarget")
                               .AddElement(administrativeCodeElement));
            var contextBuilder = new ContextBuilder(administrativeCodeElement, "cda");
            var context = contextBuilder.GetFullyQualifiedContextString();
            var expected = "cda:recordTarget/cda:administrativeGenderCode[@code='20']";
            Assert.IsFalse(string.IsNullOrEmpty(context), "Null or empty string returned by context builder");
            Assert.IsTrue(context == expected, "Context string was not correct, expected '{0}', actual '{1}'", expected, context);
        }

        [TestMethod]
        public void GenerateContext_SingleChildElement_4Levels()
        {
            var administrativeCodeElement = new DocumentTemplateElement("administrativeGenderCode");
            administrativeCodeElement.AddAttribute(new DocumentTemplateElementAttribute("code", "20", string.Empty, "MMG-GENDER-CODE-OID"));
            var cdaDocumentTemplate = new DocumentTemplate("urn:hl7-org:v3");
            cdaDocumentTemplate.AddElement(new DocumentTemplateElement("recordTarget")
                               .AddElement(new DocumentTemplateElement("patientRole")
                               .AddElement(new DocumentTemplateElement("patient")
                               .AddElement(administrativeCodeElement))));
            var contextBuilder = new ContextBuilder(administrativeCodeElement, "cda");
            var context = contextBuilder.GetFullyQualifiedContextString();
            var expected = "cda:recordTarget/cda:patientRole/cda:patient/cda:administrativeGenderCode[@code='20']";
            Assert.IsFalse(string.IsNullOrEmpty(context), "Null or empty string returned by context builder");
            Assert.IsTrue(context == expected, "Context string was not correct, expected '{0}', actual '{1}'", expected, context);
        }

        [TestMethod]
        public void GenerateContext_SingleChildElement_4Levels_GenerateContextOn3rdLevel()
        {
            var administrativeCodeElement = new DocumentTemplateElement("administrativeGenderCode");
            administrativeCodeElement.AddAttribute(new DocumentTemplateElementAttribute("code", "20", string.Empty, "MMG-GENDER-CODE-OID"));
            var patientCodeElement = new DocumentTemplateElement("patient");
            var cdaDocumentTemplate = new DocumentTemplate("urn:hl7-org:v3");
            cdaDocumentTemplate.AddElement(new DocumentTemplateElement("recordTarget")
                               .AddElement(new DocumentTemplateElement("patientRole")
                               .AddElement(patientCodeElement
                               .AddElement(administrativeCodeElement))));
            var contextBuilder = new ContextBuilder(patientCodeElement, "cda");
            var context = contextBuilder.GetFullyQualifiedContextString();
            var expected = "cda:recordTarget/cda:patientRole/cda:patient";
            Assert.IsFalse(string.IsNullOrEmpty(context), "Null or empty string returned by context builder");
            Assert.IsTrue(context == expected, "Context string was not correct, expected '{0}', actual '{1}'", expected, context);
        }

        [TestMethod]
        public void GenerateContext_SingleChildElement_4Levels_GenerateContextOn3rdLevel_Namespace()
        {
            var administrativeCodeElement = new DocumentTemplateElement("administrativeGenderCode");
            administrativeCodeElement.AddAttribute(new DocumentTemplateElementAttribute("code", "20", string.Empty, "MMG-GENDER-CODE-OID"));
            var patientCodeElement = new DocumentTemplateElement("patient");
            var cdaDocumentTemplate = new DocumentTemplate("urn:hl7-org:v3");
            cdaDocumentTemplate.AddElement(new DocumentTemplateElement("recordTarget")
                               .AddElement(new DocumentTemplateElement("patientRole")
                               .AddElement(patientCodeElement
                               .AddElement(administrativeCodeElement))));
            var contextBuilder = new ContextBuilder(patientCodeElement, "ems");
            var context = contextBuilder.GetFullyQualifiedContextString();
            var expected = "ems:recordTarget/ems:patientRole/ems:patient";
            Assert.IsFalse(string.IsNullOrEmpty(context), "Null or empty string returned by context builder");
            Assert.IsTrue(context == expected, "Context string was not correct, expected '{0}', actual '{1}'", expected, context);
        }

        [TestMethod]
        public void GenerateContext_SingleElementChildElement_2Levels_SingleAttribute()
        {
            var element = new DocumentTemplateElement("assignedPerson");
            var attribute = new DocumentTemplateElementAttribute("classCode", "ASSIGNED");
            element.AddAttribute(attribute);
            var cdaDocumentTemplate = new DocumentTemplate("urn:hl7-org:v3");
            cdaDocumentTemplate.AddElement(new DocumentTemplateElement("custodian")
                               .AddElement(element));
            var contextBuilder = new ContextBuilder(attribute, "cda");
            var context = contextBuilder.GetRelativeContextString();
            var expected = "cda:assignedPerson[@classCode='ASSIGNED']";
            Assert.IsFalse(string.IsNullOrEmpty(context), "Null or empty string returned by context builder");
            Assert.IsTrue(context == expected, "Context string was not correct, expected '{0}', actual '{1}'", expected, context);
        }


    }
}
