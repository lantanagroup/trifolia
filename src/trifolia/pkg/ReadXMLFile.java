package trifolia.pkg;

import java.io.File;
import java.io.IOException;

import javax.xml.xpath.XPath;
import javax.xml.xpath.XPathConstants;
import javax.xml.xpath.XPathExpressionException;
import javax.xml.xpath.XPathFactory;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;

import org.openqa.selenium.WebDriver;
import org.openqa.selenium.firefox.FirefoxDriver;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;
import org.xml.sax.SAXException;

public class ReadXMLFile{
	 public static void main(String args[]) throws ParserConfigurationException, SAXException, IOException{
	 
	 File xmlFile=new File("C:\\C-CDA on FHIR.xml");
	 
	 //Creating object for DocumentBuilderFactory
	 DocumentBuilderFactory dbFactory=DocumentBuilderFactory.newInstance();
	 DocumentBuilder dBuilder=dbFactory.newDocumentBuilder();
	 Document doc=dBuilder.parse(xmlFile);
	 
	 // Get node lists for xml
	 NodeList lxml=doc.getChildNodes();
	 
	 // Get first child node
	 Node nxml=lxml.item(0);
	 
	 // Assign Node Element
	 Element element=(Element)nxml;
	 
	 //Get element by tagname
	 
	 Element bundle=(Element)doc.getChildNodes().item(0);
	 Element entry=(Element)bundle.getElementsByTagName("entry").item(0);
	 Element resource=(Element)entry.getElementsByTagName("resource").item(0);
	 Element implementationGuide=(Element)resource.getElementsByTagName("ImplementationGuide").item(0);
	 Element implementationGuideId=(Element)implementationGuide.getElementsByTagName("id").item(0);
	 Element implementationGuideUrl=(Element)implementationGuide.getElementsByTagName("url").item(0);
	 Element implementationGuideName=(Element)implementationGuide.getElementsByTagName("name").item(0);
	 //
//	 String iGValue = implementationGuide.getAttribute("value");
//	 String idValue = implementationGuideId.getAttribute("value");
//	 String urlValue = implementationGuideUrl.getAttribute("value");
	 

     System.out.println(bundle.getAttribute("xmlns"));
     System.out.println(entry.getAttribute("resource"));
	 System.out.println(implementationGuideId.getAttribute("value"));
	 System.out.println(implementationGuideUrl.getAttribute("value"));
	 System.out.println(implementationGuideName.getAttribute("value"));   }
}

