using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Trifolia.Generation.Schematron;
using Trifolia.Generation.Schematron.Model;

namespace Schematron.Test.Generation.Schematron
{
    [TestClass]
    public class TemplateDatabaseCDADocumentTemplateTest
    {
        [TestMethod, TestCategory("Schematron")]
        public void TestCDADocumentTemplate_WithValidNamespace_SingleRootElement_RootElementCountIsOne()
        {
            var cdaDocumentTemplate = new DocumentTemplate("urn:hl7-org:v3");
            var element = new DocumentTemplateElement("code");
            cdaDocumentTemplate.AddElement(element);
            Assert.IsNotNull(cdaDocumentTemplate.ChildElements, "cdaDocumentTemplate.RootElements is null, expected instance.");
            Assert.AreEqual(cdaDocumentTemplate, element.Template, "CDA Document Template was not set on the element properly.");
            Assert.IsTrue(cdaDocumentTemplate.ChildElements.Count == 1, "Root element count failed, expected 1, actual {0}", cdaDocumentTemplate.ChildElements.Count);
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestCDADocumentTemplate_WithValidNamespace_MulitpleRootElements_RootElementCountIsGreaterThanOne()
        {
            var cdaDocumentTemplate = new DocumentTemplate("urn:hl7-org:v3");
            cdaDocumentTemplate
                .AddElement(new DocumentTemplateElement("code"))
                .AddElement(new DocumentTemplateElement("realmCode"));
            Assert.IsNotNull(cdaDocumentTemplate.ChildElements, "cdaDocumentTemplate.RootElements is null, expected instance.");
            Assert.IsTrue(cdaDocumentTemplate.ChildElements.Count > 1, "Root element count failed, expected 2, actual {0}", cdaDocumentTemplate.ChildElements.Count);
            Assert.AreEqual(cdaDocumentTemplate, cdaDocumentTemplate.ChildElements[0].Template, "CDA Document Template was not set on the element properly.");
        }

        [TestMethod, TestCategory("Schematron")]
        public void TestCDADocumentTemplate_WithValidNamespace_SingleChildElement_SingleChildElement_ChildCountIsOne()
        {
            var cdaDocumentTemplate = new DocumentTemplate("urn:hl7-org:v3");
            var element = new DocumentTemplateElement("author");
            Assert.IsNotNull(element.ChildElements, "element.ChildElements is null, expected instance");
            element.AddElement(new DocumentTemplateElement("assignedPerson"));
            cdaDocumentTemplate.AddElement(element);
            Assert.IsNotNull(cdaDocumentTemplate.ChildElements, "cdaDocumentTemplate.RootElements is null, expected instance.");
            Assert.IsTrue(cdaDocumentTemplate.ChildElements.Count == 1, "Root element count failed, expected 1, actual {0}", cdaDocumentTemplate.ChildElements.Count);
            Assert.IsTrue(cdaDocumentTemplate.ChildElements[0].ChildElements.Count == 1, "Child element count failed, expected 1, actual {0}", cdaDocumentTemplate.ChildElements[0].ChildElements.Count);
            Assert.AreEqual(cdaDocumentTemplate.ChildElements[0].ChildElements[0].ParentElement, cdaDocumentTemplate.ChildElements[0], "Child element parent was not set properly.");
        }
    }
}
