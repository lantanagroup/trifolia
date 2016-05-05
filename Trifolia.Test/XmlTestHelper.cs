using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Trifolia.Test
{
    public class XmlTestHelper
    {
        public static void AssertXmlSingleNode(XmlNode node, XmlNamespaceManager nsManager, string xpath, string message = null)
        {
            XmlNode foundNode = node.SelectSingleNode(xpath, nsManager);

            if (!string.IsNullOrEmpty(message))
                Assert.IsNotNull(foundNode, message);
            else
                Assert.IsNotNull(foundNode);
        }
    }
}
