using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;

namespace Trifolia.Test
{
    public static class AssertXML
    {
        public static void XPathExists(XmlNode context, XmlNamespaceManager nsManager, string xpath, string message = null)
        {
            XmlNodeList nodes = context.SelectNodes(xpath, nsManager);
            Assert.AreNotEqual(0, nodes.Count, !string.IsNullOrEmpty(message) ? message : "Expected to find one or more nodes for xpath: " + xpath);
        }

        public static void XPathExists(XmlNode context, string xpath, string message = null)
        {
            XmlNodeList nodes = context.SelectNodes(xpath);
            Assert.AreNotEqual(0, nodes.Count, !string.IsNullOrEmpty(message) ? message : "Expected to find one or more nodes for xpath: " + xpath);
        }

        public static void XPathNotExists(XmlNode context, XmlNamespaceManager nsManager, string xpath, string message = null)
        {
            XmlNodeList nodes = context.SelectNodes(xpath, nsManager);
            Assert.AreEqual(0, nodes.Count, !string.IsNullOrEmpty(message) ? message : "Expected to find one or more nodes for xpath: " + xpath);
        }

        public static void XPathNotExists(XmlNode context, string xpath, string message = null)
        {
            XmlNodeList nodes = context.SelectNodes(xpath);
            Assert.AreEqual(0, nodes.Count, !string.IsNullOrEmpty(message) ? message : "Expected to find one or more nodes for xpath: " + xpath);
        }
    }
}
