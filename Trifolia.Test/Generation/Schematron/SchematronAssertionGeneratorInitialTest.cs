using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Trifolia.Export.Schematron;
using Trifolia.Export.Schematron.Model;
using Trifolia.Shared;
using Trifolia.DB;

namespace Trifolia.Test.Generation.Schematron
{
    [TestClass]
    [DeploymentItem("Trifolia.Plugins.dll")]
	public class SchematronAssertionGeneratorInitialTest
    {
        private const string templateIdentifierXpath = "{0}templateId[@root='{1}']";
        private const string templateVersionIdentifierXpath = "{0}templateId[@root='{1}' and @extension = '{2}']";

        /// <summary>
		/// Generates schematron assert for single element within the cda doc
		/// </summary>
		[TestMethod, TestCategory("Schematron")]
		public void GenerateSchematronAssertion_Element_ConformanceSHALL_CardinalityOneToOne()
		{
			//create cda doc
			var cdaDocumentTemplate = new DocumentTemplate("urn:hl7-org:v3");
			//create code element
			var element = new DocumentTemplateElement("code");
			//add element to doc
			cdaDocumentTemplate.AddElement(element);
			//create schematron assertion line builder
			var builder = new AssertionLineBuilder(element, templateIdentifierXpath, templateVersionIdentifierXpath, "cda");
			//define cardinality and conformance
			var cardinality = CardinalityParser.Parse("1..1");
			var conformance = ConformanceParser.Parse("SHALL");
			//add element (context comes from doc), conformance, cardinality through fluent interface
			builder.ConformsTo(conformance).WithCardinality(cardinality);
			//generate string
			string assertion = builder.ToString();
			//did we generate the correct string?
			Assert.IsTrue(assertion == @"count(cda:code)=1", "The generated assertion was not correct. Expected 'count(cda:code)=1', Actual '{0}'.", assertion);
		}

        /// <summary>
        /// Generates schematron assert for single element within the cda doc
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void GenerateSchematronAssertion_Element_With_Value_ConformanceSHALL_CardinalityOneToOne()
        {
            //create cda doc
            var cdaDocumentTemplate = new DocumentTemplate("urn:hl7-org:v3");
            //create code element
            var element = new DocumentTemplateElement("code", "Test");
            //add element to doc
            cdaDocumentTemplate.AddElement(element);
            //create schematron assertion line builder
            var builder = new AssertionLineBuilder(element, templateIdentifierXpath, templateVersionIdentifierXpath, "cda");
            //define cardinality and conformance
            var cardinality = CardinalityParser.Parse("1..1");
            var conformance = ConformanceParser.Parse("SHALL");
            //add element (context comes from doc), conformance, cardinality through fluent interface
            builder.ConformsTo(conformance).WithCardinality(cardinality);
            //generate string
            string assertion = builder.ToString();
            //did we generate the correct string?
            Assert.IsTrue(assertion == @"count(cda:code[translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='test'])=1", "The generated assertion was not correct. Expected 'count(cda:code[text()='Test'])=1', Actual '{0}'.", assertion);
        }

        /// <summary>
        /// Generates schematron assert for single element within the cda doc, but with an override element name with attribute
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void GenerateSchematronAssertion_Element_With_Value_And_AttributeOverride_ConformanceSHALL_CardinalityOneToOne()
        {
            //create cda doc
            var cdaDocumentTemplate = new DocumentTemplate("urn:hl7-org:v3");
            //create code element
            var element = new DocumentTemplateElement("statusCode", "Test");
            element.ElementToAttributeOverrideMapping.Add("statusCode", "code");
            element.ElementToAttributeOverrideMapping.Add("code", "code");
            //add element to doc
            cdaDocumentTemplate.AddElement(element);
            //create schematron assertion line builder
            var builder = new AssertionLineBuilder(element, templateIdentifierXpath, templateVersionIdentifierXpath, "cda");
            //define cardinality and conformance
            var cardinality = CardinalityParser.Parse("1..1");
            var conformance = ConformanceParser.Parse("SHALL");
            //add element (context comes from doc), conformance, cardinality through fluent interface
            builder.ConformsTo(conformance).WithCardinality(cardinality);
            //generate string
            string assertion = builder.ToString();
            //did we generate the correct string?
            Assert.IsTrue(assertion == @"count(cda:statusCode[@code='Test'])=1", "The generated assertion was not correct. Expected 'count(cda:statusCode[@code='Test'])=1', Actual '{0}'.", assertion);
        }

		/// <summary>
		/// Generates schematron assert for single element within the cda doc
		/// </summary>
		[TestMethod, TestCategory("Schematron")]
		public void GenerateSchematronAssertion_Element_ConformanceSHALL_CardinalityZeroToMany()
		{
			//create cda doc
			var cdaDocumentTemplate = new DocumentTemplate("urn:hl7-org:v3");
			//create code element
			var element = new DocumentTemplateElement("code");
			//add element to doc
			cdaDocumentTemplate.AddElement(element);
			//create schematron assertion line builder
            var builder = new AssertionLineBuilder(element, templateIdentifierXpath, templateVersionIdentifierXpath);
			//define cardinality and conformance
			var cardinality = CardinalityParser.Parse("0..*");
			var conformance = ConformanceParser.Parse("SHALL");
			//add element (context comes from doc), conformance, cardinality through fluent interface
			builder.ConformsTo(conformance).WithCardinality(cardinality);
			//generate string
			string assertion = builder.ToString();
            string expected = "not(code) or code";
			//did we generate the correct string?
			Assert.IsTrue(assertion == expected, "The generated assertion was not correct. Expected '{0}', Actual '{1}'.", expected, assertion);
		}


        /// <summary>
        /// Generates schematron for an element with specific attributes such as: <code code="57024-2" codeSystem="2.16.840.1.113883.6.1"/>
        /// </summary>
        /// <summary>
        /// Generates schematron specific attributes that have no value such as: @code/>
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
		public void GenerateSchematronAssertion_Attribute_NoValue_ConformanceSHALL_CardinalityOneToOne()
		{
			var attr = new DocumentTemplateElementAttribute("code");
			//create schematron assertion line builder, build one at a time (regular interface, see above for fluent interface)
            var builder = new AssertionLineBuilder(attr, templateIdentifierXpath, templateVersionIdentifierXpath);
			//define cardinality 
			var cardinality = CardinalityParser.Parse("1..1");
			//add cardinality for the element
			builder.WithCardinality(cardinality);
			//define conformance
			var conformance = ConformanceParser.Parse("SHALL");
			//add conformance for the element
			builder.ConformsTo(conformance);
			//generate string
			string assertion = builder.ToString();
			//did we generate the correct string? 
			string expected = "@code";
			Assert.IsTrue(assertion == expected, "Assertion string was not correct. Expected '{0}', Actual '{1}'.", expected, assertion);
		}

        /// <summary>
        /// Generates schematron specific attributes such as: @code="57024-2"/>
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void GenerateSchematronAssertion_Attribute_ConformanceSHALL_CardinalityOneToOne()
        {
            var attr = new DocumentTemplateElementAttribute("code", "57024-2");
            //create schematron assertion line builder, build one at a time (regular interface, see above for fluent interface)
            var builder = new AssertionLineBuilder(attr, templateIdentifierXpath, templateVersionIdentifierXpath);
            //define cardinality 
            var cardinality = CardinalityParser.Parse("1..1");
            //add cardinality for the element
            builder.WithCardinality(cardinality);
            //define conformance
            var conformance = ConformanceParser.Parse("SHALL");
            //add conformance for the element
            builder.ConformsTo(conformance);
            //generate string
            string assertion = builder.ToString();
            //did we generate the correct string? 
            string expected = "@code='57024-2'";
            Assert.IsTrue(assertion == expected, "Assertion string was not correct. Expected '{0}', Actual '{1}'.", expected, assertion);
        }

        /// <summary>
		/// Generates schematron specific attributes such as: @code="57024-2" and data type @xsi-type="CE"/>
		/// </summary>
		[TestMethod, TestCategory("Schematron")]
		public void GenerateSchematronAssertion_Attribute_Value_DataType_ConformanceSHALL_CardinalityOneToOne()
		{
            var attr = new DocumentTemplateElementAttribute("code", "57024-2") { DataType = "CE" };
			//create schematron assertion line builder, build one at a time (regular interface, see above for fluent interface)
			var builder = new AssertionLineBuilder(attr, templateIdentifierXpath, templateVersionIdentifierXpath);
			//define cardinality 
			var cardinality = CardinalityParser.Parse("1..1");
			//add cardinality for the element
			builder.WithCardinality(cardinality);
			//define conformance
			var conformance = ConformanceParser.Parse("SHALL");
			//add conformance for the element
			builder.ConformsTo(conformance);
			//generate string
			string assertion = builder.ToString();
			//did we generate the correct string? 
			string expected = @"count(self::node()[@code='57024-2'])=1";
			Assert.IsTrue(assertion == expected, "Assertion string was not correct. Expected '{0}', Actual '{1}'.", expected, assertion);
		}


		/// <summary>
		///test to look for state element with parent of addr where use = "HP" within the context of recordTarget/patientRole
		/// 	<recordTarget>
		///        <patientRole>
			///         <id nullFlavor="NI"/>
			///         <addr use="HP">
				///         <streetAddressLine>4301 West Boy Scout Boulevard, Suite 800</streetAddressLine>
				///         <city>Tampa</city>
				///         <state>FL</state>
				///         <postalCode>33607</postalCode>
				///         <country>USA</country>
			///         </addr>
		/// The context will be : <sch:rule context="cda:addr[@use='HP']">
		/// The assert will be <sch:assert test="count(cda:state)=1">shall...</sch:assert>
		/// </summary>
		[TestMethod, TestCategory("Schematron")]
		public void GenerateSchematronAssertion_NestedElement_WithAttribute_ConformanceSHALL_CardinalityOneToOne()
		{
			var addrElement = new DocumentTemplateElement("addr");
			var useAttr = new DocumentTemplateElementAttribute("use", "HP");
			addrElement.AddAttribute(useAttr);
			var element = new DocumentTemplateElement("state");
			var cdaDocumentTemplate = new DocumentTemplate("urn:hl7-org:v3")
							   .AddElement(new DocumentTemplateElement("recordTarget")
							   .AddElement(new DocumentTemplateElement("patientRole")
							   .AddElement(addrElement)
							   .AddElement(element)));
			//need to have cardinality on both parent and child elements?
			//how do we tell it to generate cda: on context as in below? 
			var cardinality = CardinalityParser.Parse("1..1");
            var builder = new AssertionLineBuilder(element, templateIdentifierXpath, templateVersionIdentifierXpath).WithCardinality(cardinality).ConformsTo(Conformance.SHALL).WithinContext(@"cda:");
			var assertion = builder.ToString();
			var expectedAssertion = @"count(cda:state)=1";
			Assert.IsTrue(assertion == expectedAssertion, 
				"Assertion string was not correct. Expected '{0}', Actual '{1}'.", expectedAssertion, assertion);
            var contextBuilder = new ContextBuilder(useAttr, "cda");
			var context = contextBuilder.GetRelativeContextString();
			var expectedContext = "cda:addr[@use='HP']";
			Assert.IsTrue(context == expectedContext, "Context is not correct. Expected '{0}', Actual '{1}'", expectedContext, context);
		}


		/// <summary>
		/// test to look for NULL Flavor (uses AND)
		/// </summary>
		[TestMethod, TestCategory("Schematron")]
		public void GenerateSchematronAssertion_NestedElement_WithAttribute_NULLFlavor_ConformanceSHALL_CardinalityOneToOne()
		{
			var idElement = new DocumentTemplateElement("id");
			var nullFlavorAttribute = new DocumentTemplateElementAttribute("nullFlavor", "NI");
			idElement.AddAttribute(nullFlavorAttribute);
			var cdaDocumentTemplate = new DocumentTemplate("urn:hl7-org:v3");
			cdaDocumentTemplate.AddElement(new DocumentTemplateElement("component")
							   .AddElement(new DocumentTemplateElement("section")
							   .AddElement(new DocumentTemplateElement("entry")
							   .AddElement(new DocumentTemplateElement("organizer")
							   .AddElement(new DocumentTemplateElement("performer")
							   .AddElement(new DocumentTemplateElement("assignedEntity")
							   .AddElement(idElement)))))));
            var builder = new AssertionLineBuilder(nullFlavorAttribute, templateIdentifierXpath, templateVersionIdentifierXpath);
			builder.ConformsTo(ConformanceParser.Parse("SHALL")).WithCardinality(CardinalityParser.Parse("1..1")).WithinContext("cda:");
			var assertion = builder.ToString();
			var expectedAssertion = @"cda:id[@nullFlavor='NI']";
			Assert.IsTrue(assertion == expectedAssertion,
				"Assertion string was not correct. Expected '{0}', Actual '{1}'.", expectedAssertion, assertion);
            var contextBuilder = new ContextBuilder(idElement.ParentElement, "cda");
			var context = contextBuilder.GetRelativeContextString();
			var expectedContext = "cda:assignedEntity";
			Assert.IsTrue(context == expectedContext, "Context is not correct. Expected '{0}', Actual '{1}'", expectedContext, context);
		}

		[TestMethod, TestCategory("Schematron")]
		public void GenerateSchematronAssertion_Element_WithSiblingAttributes_ConformanceSHALL_CardinalityOneToOne()
		{
			var doc = new DocumentTemplate("cda");
			doc.AddElement(new DocumentTemplateElement("observation"));
			doc.ChildElements[0].AddAttribute(new DocumentTemplateElementAttribute("classCode", "OBS"));
			doc.ChildElements[0].AddAttribute(new DocumentTemplateElementAttribute("moodCode", "EVN"));

			var contextBuilder = new ContextBuilder(doc.ChildElements[0], "cda");
			var assertionBuilder = new AssertionLineBuilder(doc.ChildElements[0].Attributes[0], templateIdentifierXpath, templateVersionIdentifierXpath, "cda");  //"EVN/@moodCode"
			var assertion = assertionBuilder.WithCardinality(CardinalityParser.Parse("1..1")).ConformsTo(Conformance.SHALL).ToString();
			var expected = "cda:observation[@classCode='OBS']";
			Assert.IsTrue(assertion == expected, "Assertion string was not correct. Expected '{0}', Actual '{1}'", expected, assertion);
		}


		[TestMethod, TestCategory("Schematron")]
		public void GenerateSchematronAssertion_Element_ConformanceSHALL_CardinalityOneToMany()
		{
			var doc = new DocumentTemplate("cda");
			doc.AddElement(new DocumentTemplateElement("observation"));
			doc.ChildElements[0].AddElement(new DocumentTemplateElement("id"));

            var contextBuilder = new ContextBuilder(doc.ChildElements[0].ChildElements[0], "cda");
			var assertionBuilder = new AssertionLineBuilder(doc.ChildElements[0].ChildElements[0], templateIdentifierXpath, templateVersionIdentifierXpath);  //"id"
			var assertion = assertionBuilder.WithCardinality(CardinalityParser.Parse("1..*")).WithinContext("cda:").ConformsTo(Conformance.SHALL).ToString();
			var expected = "count(cda:id) > 0";
			Assert.IsTrue(assertion == expected, "Assertion string was not correct. Expected '{0}', Actual '{1}'", expected, assertion);
		}

        [TestMethod, TestCategory("Schematron")]
        public void GenerateSchematronAssertion_Attribute_Valueset_ConformanceMAY_CardinalityOneToOne()
        {
            var attr = new DocumentTemplateElementAttribute("moodCode", "EVN");
            //create schematron assertion line builder, build one at a time (regular interface, see above for fluent interface)
            var builder = new AssertionLineBuilder(attr, templateIdentifierXpath, templateVersionIdentifierXpath);
            //define cardinality 
            var cardinality = CardinalityParser.Parse("1..1");
            //add cardinality for the element
            builder.WithCardinality(cardinality);
            //define conformance
            var conformance = ConformanceParser.Parse("MAY");
            //add conformance for the element
            builder.ConformsTo(conformance);
            //add valueset
            builder.WithinValueSet("2.16.840.1.113883.1.11.20.2");
            //generate string
            string assertion = builder.ToString();
            //did we generate the correct string? 
            string expected = "@moodCode='EVN' and @moodCode=document('the_voc.xml')/voc:systems/voc:system[@valueSetOid='2.16.840.1.113883.1.11.20.2']/voc:code/@value";
            Assert.IsTrue(assertion == expected, "Assertion string was not correct. Expected empty string, Actual '{1}'.", expected, assertion);
        }

        [TestMethod, TestCategory("Schematron")]
        public void GenerateSchematronAssertion_Attribute_Valueset_ConformanceMAYNOT_CardinalityOneToOne()
        {
            var attr = new DocumentTemplateElementAttribute("moodCode", "EVN");
            //create schematron assertion line builder, build one at a time (regular interface, see above for fluent interface)
            var builder = new AssertionLineBuilder(attr, templateIdentifierXpath, templateVersionIdentifierXpath);
            //define cardinality 
            var cardinality = CardinalityParser.Parse("1..1");
            //add cardinality for the element
            builder.WithCardinality(cardinality);
            //define conformance
            var conformance = Conformance.MAY_NOT;
            //add conformance for the element
            builder.ConformsTo(conformance);
            //add valueset
            builder.WithinValueSet("2.16.840.1.113883.1.11.20.2");
            //generate string
            string assertion = builder.ToString();
            //did we generate the correct string? 
            string expected = "not(@moodCode='EVN' and @moodCode=document('the_voc.xml')/voc:systems/voc:system[@valueSetOid='2.16.840.1.113883.1.11.20.2']/voc:code/@value)";
            Assert.IsTrue(assertion == expected, "Assertion string was not correct. Expected empty string, Actual '{1}'.", expected, assertion);
        }

        /// <summary>
		/// check for <observation><templateid root='2.16.840.1.113883.10.20.22.4.48' /><effectiveTime><high></high></effectiveTime></observation>
		/// SHALL contain 1..1 effectiveTime (CONF:8656).
			// This effectiveTime SHALL contain 1..1 high (CONF:15521).
		///this should come out as count(cda:effectiveTime[count(cda:high)=1])=1 within context of cda:observation[cda:templateId/@root='2.16.840.1.113883.10.20.22.4.48']
		/// </summary>
		[TestMethod, TestCategory("Schematron")]
		public void GenerateSchematronAssertion_NestedElement_ConformanceSHALL_CardinalityOneToOne()
		{
			var doc = new DocumentTemplate("cda");
			doc.AddElement(new DocumentTemplateElement("observation"));
			doc.ChildElements[0].AddElement(new DocumentTemplateElement("templateId"));
			doc.ChildElements[0].ChildElements[0].AddAttribute(new DocumentTemplateElementAttribute("root", "2.16.840.1.113883.10.20.22.4.48"));
			doc.ChildElements[0].AddElement(new DocumentTemplateElement("effectiveTime"));
			doc.ChildElements[0].ChildElements[1].AddElement(new DocumentTemplateElement("high"));

            var contextBuilder = new ContextBuilder(doc.ChildElements[0].ChildElements[0].Attributes[0], "cda"); //observation/templateid[@root]
			var relativeContext = contextBuilder.GetRelativeContextString();
			var fqContext = contextBuilder.GetFullyQualifiedContextString();
			var expectedRelativeContext = "cda:templateId[@root='2.16.840.1.113883.10.20.22.4.48']";
			var expectedFqContext = "cda:observation[templateId/@root='2.16.840.1.113883.10.20.22.4.48']";
			Assert.IsTrue(relativeContext == expectedRelativeContext, "Relative context was not correct. Expected '{0}', Actual '{1}'", expectedRelativeContext, relativeContext);
			Assert.IsTrue(fqContext == expectedFqContext, "Fully Qualified context was not correct. Expected '{0}', Actual '{1}'", expectedFqContext, fqContext);
            var childAssertionBuilder = new AssertionLineBuilder(doc.ChildElements[0].ChildElements[1].ChildElements[0], templateIdentifierXpath, templateVersionIdentifierXpath);  //high
			childAssertionBuilder.WithCardinality(CardinalityParser.Parse("1..1")).ConformsTo(Conformance.SHALL).WithinContext("cda:");
            var assertionBuilder = new AssertionLineBuilder(doc.ChildElements[0].ChildElements[1], templateIdentifierXpath, templateVersionIdentifierXpath);  //effectiveTime
			var assertion = assertionBuilder.WithCardinality(CardinalityParser.Parse("1..1")).WithinContext("cda:").ConformsTo(Conformance.SHALL).WithChildElementBuilder(childAssertionBuilder).ToString();
			var expected = "count(cda:effectiveTime[count(cda:high)=1])=1";
			Assert.IsTrue(assertion == expected, "Assertion string was not correct. Expected '{0}', Actual '{1}'", expected, assertion);
		}

		/// <summary>        
		//SHOULD contain 1..1 participant (CONF:8667) such that it
			//SHALL contain 1..1 @typeCode="CST" Custodian (CodeSystem: HL7ParticipationType 2.16.840.1.113883.5.90) (CONF:8668).
			//SHALL contain 1..1 participantRole (CONF:8669).
				//This participantRole SHALL contain 1..1 @classCode="AGNT" Agent (CodeSystem: RoleClass 2.16.840.1.113883.5.110) (CONF:8670).
				//This participantRole SHOULD contain 0..1 addr (CONF:8671).
				//This participantRole SHOULD contain 0..1 telecom (CONF:8672).
				//This participantRole SHALL contain 1..1 playingEntity (CONF:8824).
					//This playingEntity SHALL contain 1..1 name (CONF:8673).
						//The name of the agent who can provide a copy of the Advance Directive SHALL be recorded in the name element inside the playingEntity element (CONF:8674).
		/// </summary>
		[TestMethod, TestCategory("Schematron")]
		public void GenerateSchematronAssertion_MultipleNestedElement_ConformanceSHALL_AND_SHOULD_CardinalityOneToOne_AND_ZeroToOne_For_Advance_Directive_Number9Constraint()
		{
			var doc = new DocumentTemplate("cda");
			doc.AddElement(new DocumentTemplateElement("observation"));
			var participantElement = new DocumentTemplateElement("participant");
			participantElement.AddAttribute(new DocumentTemplateElementAttribute("typeCode", "CST"));
			var participantRoleElement = new DocumentTemplateElement("participantRole");
			participantRoleElement.AddAttribute(new DocumentTemplateElementAttribute("classCode", "AGNT"));
			participantElement.AddElement(participantRoleElement);
			var addrElement = new DocumentTemplateElement("addr");
			participantRoleElement.AddElement(addrElement);
			var telecomElement = new DocumentTemplateElement("telecom");
			participantRoleElement.AddElement(telecomElement);
			var playingEntityElement = new DocumentTemplateElement("playingEntity");
			participantRoleElement.AddElement(playingEntityElement);
			var nameElement = new DocumentTemplateElement("name");
			playingEntityElement.AddElement(nameElement);


            var participantRoleChildAssertionBuilder = new AssertionLineBuilder(participantRoleElement, templateIdentifierXpath, templateVersionIdentifierXpath)
														   .WithinContext("cda:")
														   .WithCardinality(CardinalityParser.Parse("1..1"))
														   .ConformsTo(Conformance.SHALL);
            var addrChildAssertionBuilder = new AssertionLineBuilder(addrElement, templateIdentifierXpath, templateVersionIdentifierXpath)
														   .WithinContext("cda:")
														   .WithCardinality(CardinalityParser.Parse("0..1"))
														   .ConformsTo(Conformance.SHALL);
            var telecomChildAssertionBuilder = new AssertionLineBuilder(telecomElement, templateIdentifierXpath, templateVersionIdentifierXpath)
														   .WithinContext("cda:")
														   .WithCardinality(CardinalityParser.Parse("0..1"))
														   .ConformsTo(Conformance.SHALL);
            var nameChildAssertionBuilder = new AssertionLineBuilder(nameElement, templateIdentifierXpath, templateVersionIdentifierXpath)
														   .WithinContext("cda:")
														   .WithCardinality(CardinalityParser.Parse("1..1"))
														   .ConformsTo(Conformance.SHALL);
            var playingEntityChildAssertionBuilder = new AssertionLineBuilder(playingEntityElement, templateIdentifierXpath, templateVersionIdentifierXpath)
														   .WithinContext("cda:")
														   .WithCardinality(CardinalityParser.Parse("1..1"))
														   .WithChildElementBuilder(nameChildAssertionBuilder)   //nested child assertion builder
														   .ConformsTo(Conformance.SHALL);

            var assertionBuilder = new AssertionLineBuilder(participantElement, templateIdentifierXpath, templateVersionIdentifierXpath);  //participant
			var assertion = assertionBuilder
								.WithCardinality(CardinalityParser.Parse("1..1"))
								.WithinContext("cda:")
								.ConformsTo(Conformance.SHALL)
								.WithChildElementBuilder(participantRoleChildAssertionBuilder)
								.WithChildElementBuilder(addrChildAssertionBuilder)
								.WithChildElementBuilder(telecomChildAssertionBuilder)
								.WithChildElementBuilder(playingEntityChildAssertionBuilder)
								.ToString();
			var expected = "count(cda:participant[@typeCode='CST'][count(cda:participantRole[@classCode='AGNT'])=1][count(cda:addr) < 2][count(cda:telecom) < 2][count(cda:playingEntity[count(cda:name)=1])=1])=1";
			Assert.IsTrue(assertion == expected, "Assertion string was not correct. Expected '{0}', Actual '{1}'", expected, assertion);
		}

        /// <summary>
        /// Generates schematron assert for single element with a template id specified but inside the ema namespace
        /// This test was generated to test for Bug # TDBMGT-472
        /// </summary>
        [TestMethod, TestCategory("Schematron")]
        public void GenerateSchematronAssertion_NamespaceEMA_Element_Template_ConformanceSHALL_CardinalityOneToOne()
        {
            //create cda doc
            var cdaDocumentTemplate = new DocumentTemplate("urn:hl7-org:v3");
            //create code element
            var element = new DocumentTemplateElement("encounter");
            //add element to doc
            cdaDocumentTemplate.AddElement(element);
            //create schematron assertion line builder
            var builder = new AssertionLineBuilder(element, templateIdentifierXpath, templateVersionIdentifierXpath, "ema");
            //define cardinality and conformance
            var cardinality = CardinalityParser.Parse("1..1");
            var conformance = ConformanceParser.Parse("SHALL");
            //add element (context comes from doc), conformance, cardinality through fluent interface
            builder.ConformsTo(conformance).WithCardinality(cardinality).ContainsTemplate("urn:oid:4.3.2.1");
            //generate string
            string assertion = builder.ToString();
            string expected = @"count(ema:encounter[ema:templateId[@root='4.3.2.1']])=1";
            //did we generate the correct string?
            Assert.IsTrue(assertion == expected, "The generated assertion was not correct. Expected '{0}', Actual '{1}'.", expected, assertion);
        }

        [TestMethod, TestCategory("Schematron")]
        public void GenerateSchematronAssertion_Attribute_TwoAttributes_No_Element_ConformanceSHALL_CardinalityOneToOne()
        {
            var attr = new DocumentTemplateElementAttribute("code");
            attr.SingleValue = "OBS";
            attr.DataType = "CD";
            //create schematron assertion line builder, build one at a time (regular interface, see above for fluent interface)
            var builder = new AssertionLineBuilder(attr, templateIdentifierXpath, templateVersionIdentifierXpath);
            //define cardinality 
            var cardinality = CardinalityParser.Parse("1..1");
            //add cardinality for the element
            builder.WithCardinality(cardinality);
            //define conformance
            var conformance = ConformanceParser.Parse("SHALL");
            //add conformance for the element
            builder.ConformsTo(conformance);
            //generate string
            string assertion = builder.ToString();
            //did we generate the correct string? 
            string expected = @"count(self::node()[@code='OBS'])=1";
            Assert.IsTrue(assertion == expected, "Assertion string was not correct. Expected '{0}', Actual '{1}'.", expected, assertion);
        }

        [TestMethod, TestCategory("Schematron")]
        public void GenerateSchematronAssertion_Element_ValueSet_SVS_ConformanceSHALL_CardinalityOneToOne()
        {
            var element = new DocumentTemplateElement("administrativeGenderCode");
            //create schematron assertion line builder, build one at a time (regular interface, see above for fluent interface)
            var builder = new AssertionLineBuilder(element, templateIdentifierXpath, templateVersionIdentifierXpath);
            //define cardinality 
            var cardinality = CardinalityParser.Parse("1..1");
            //add cardinality for the element
            builder.WithCardinality(cardinality);
            //define conformance
            var conformance = ConformanceParser.Parse("SHALL");
            //add conformance for the element
            builder.ConformsTo(conformance);
            builder.WithinValueSet("4.3.2.1", VocabularyOutputType.SVS_SingleValueSet);
            //generate string
            string assertion = builder.ToString();
            //did we generate the correct string? 
            string expected = @"count(administrativeGenderCode[@code=document('4.3.2.1.xml')/svs:RetrieveValueSetResponse/svs:ValueSet[@id='4.3.2.1']/svs:ConceptList/svs:Concept/@code])=1";
            Assert.IsTrue(assertion == expected, "Assertion string was not correct. Expected '{0}', Actual '{1}'.", expected, assertion);
        }
    }
}
