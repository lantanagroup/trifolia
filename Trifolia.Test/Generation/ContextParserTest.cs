using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trifolia.Export.Schematron;
using Trifolia.Export.Schematron.Model;

namespace Trifolia.Test.Generation
{
    [TestClass]
    public class ContextParserTest
    {
        [TestMethod]
        public void ParseElementAndAttribute()
        {
            string context = "code/@code";
            DocumentTemplateElement element = null;
            DocumentTemplateElementAttribute attribute = null;
            var parser = new ContextParser(context);
            parser.Parse(out element, out attribute);
            Assert.IsNotNull(element, "No element was passed back from the parser.");
            Assert.IsTrue(element.ElementName == "code", "Element name was incorrect. Expected 'code', Actual '{0}'", element.ElementName);
            Assert.IsNotNull(attribute, "No attribute was passed back from the parser.");
            Assert.IsTrue(attribute.AttributeName == "code", "Element name was incorrect. Expected 'code', Actual '{0}'", attribute.AttributeName);
        }

        [TestMethod]
        public void ParseMultipleElements()
        {
            string context = "entry/observation";
            DocumentTemplateElement element = null;
            DocumentTemplateElementAttribute attribute = null;
            var parser = new ContextParser(context);
            parser.Parse(out element, out attribute);
            Assert.IsNotNull(element, "No element was passed back from the parser.");
            Assert.IsTrue(element.ElementName == "observation", "Element name was incorrect. Expected 'observation', Actual '{0}'", element.ElementName);

            Assert.IsNotNull(element.ParentElement, "No parent element was passed back from the parser.");
            Assert.IsTrue(element.ParentElement.ElementName == "entry", "Element name was incorrect. Expected 'entry', Actual '{0}'", element.ElementName);

            Assert.IsNull(attribute, "An attribute was passed back from the parser. Exected null.");
        }        
    }
}
