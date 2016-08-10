package trifolia.pkg;

import java.util.regex.Pattern;
import java.util.concurrent.TimeUnit;

import org.junit.*;

import static org.junit.Assert.*;
import static org.hamcrest.CoreMatchers.*;
import org.openqa.selenium.remote.DesiredCapabilities;
import java.net.MalformedURLException;
import java.net.URL;
import org.openqa.selenium.remote.RemoteWebDriver;

import org.openqa.selenium.*;
import org.openqa.selenium.firefox.FirefoxDriver;
import org.openqa.selenium.firefox.FirefoxProfile;
import org.openqa.selenium.firefox.internal.ProfilesIni;
import org.openqa.selenium.support.ui.Select;
import org.openqa.selenium.WebDriver;

public class AllUsersSpecificFunctions {
  private static final String String = null;
  WebDriver driver;
  String baseUrl,nodeURL;
  private boolean acceptNextAlert = true;
  private StringBuffer verificationErrors = new StringBuffer();
  private A_TrifoliaLogin LoginFunctions;
  private B_ImplementationGuideFunctions IGFunctions;
  private static Boolean createdImplementationGuide = false;
  private C_TemplateFunctions TemplateFunctions;
  private D_ConstraintFunctions ConstraintFunctions;
  private E_ExportFunctions ExportFunctions;
  private F_PublishingFunctions PublishingFunctions;
  private G_TerminologyFunctions TerminologyFunctions;

  

  
  @Before
  public void setUp() throws Exception {
	  
	String platform_name = null;
    String browser_name = "firefox";
	
	           DesiredCapabilities capability = new DesiredCapabilities();
	           ProfilesIni allProfiles = new ProfilesIni();
			   FirefoxProfile ffprofile = allProfiles.getProfile("default");
			   driver = new FirefoxDriver(ffprofile);
			   driver.manage().window().maximize();
			   driver.manage().timeouts().implicitlyWait(30, TimeUnit.SECONDS);
			   ffprofile.setPreference("browser.helperapps.alwaysAsk.force", false);
			   ffprofile.setPreference("browser,helperapps.neverAsk.SaveToDisk","*.*");
			   ffprofile.setPreference("browser.download.folderList",1);
			   
			   capability = DesiredCapabilities.firefox();
			   capability.setCapability(FirefoxDriver.PROFILE, ffprofile);
			   capability.setBrowserName("firefox");
			   capability.setPlatform(Platform.VISTA);
			   capability.setCapability("nodeIp", "10.0.10.58");
			  
			   URL driverHub = new URL("http://dev5:4444/wd/hub");
			   driver = new RemoteWebDriver(driverHub, capability);
			   driver.navigate().to("http://dev.trifolia.lantanagroup.com/");
	   
//	   if (browser_name == "chrome")
//	   		{
//		       baseUrl = "http://staging.lantanagroup.com:1234/";
//			   DesiredCapabilities capability = DesiredCapabilities.firefox();
//			   capability.setBrowserName("chrome");
//			   capability.setPlatform(Platform.VISTA);
//			   capability.setCapability("nodeIp", "10.0.10.54");
//			   WebDriver driver = new RemoteWebDriver(new URL("http://dev5:4444/wd/hub"), capability);
//	   		}
//	   
//	   if (browser_name == "internet explorer")
//  		{
//		   baseUrl = "http://fhir_profiling.trifolia.lantanagroup.com/";
//		   DesiredCapabilities capability = DesiredCapabilities.firefox();
//		   capability.setBrowserName("internet explorer");
//		   capability.setPlatform(Platform.VISTA);
//		   WebDriver driver = new RemoteWebDriver(new URL("http://dev5:4444/wd/hub"), capability);
//  		}
	
	this.LoginFunctions = new A_TrifoliaLogin();
	this.LoginFunctions.setDriver(this.driver);
	this.IGFunctions = new B_ImplementationGuideFunctions();
	this.IGFunctions.setDriver(this.driver);
	this.TemplateFunctions = new C_TemplateFunctions();
	this.TemplateFunctions.setDriver(this.driver);
	this.ConstraintFunctions = new D_ConstraintFunctions();
	this.ConstraintFunctions.setDriver(this.driver);
	this.ExportFunctions = new E_ExportFunctions();
	this.ExportFunctions.setDriver(this.driver);
	this.PublishingFunctions = new F_PublishingFunctions();
	this.PublishingFunctions.setDriver(this.driver);
	this.TerminologyFunctions = new G_TerminologyFunctions();
	this.TerminologyFunctions.setDriver(this.driver);
  }
@Test

    public void testAllTrifoliaFunctions() throws Exception {
    driver.get(baseUrl + "/");
    
// ========================================================================================================================================================================================================
//                            PART I - LANTANA ADMINISTRATOR TESTS
// ========================================================================================================================================================================================================

	    // Testing Logging into Trifolia with Lantana Admin account
		this.LoginFunctions.LCGAdminLogin("lcg.admin","http://dev.trifolia.lantanagroup.com/" );

	
                        //    *** IMPLEMENTATION GUIDE TESTS ***
                        //------------------------------------		
 
		// Test Browsing the HAI R9 Implementation Guide
	    this.IGFunctions.BrowseImplementationGuide("Healthcare Associated Infection Reports Release 9", "15-19677", "The value of the id", "No audit entries", "hai_voc.xml", "Published : Tuesday, June 18, 2013", "section", "[1..1]", "Phase", "Entire Organization (HL7)", "HTML Content", "Notify", "lcg.admin");
		
	    // Test WebViewing the ASCO-BCR Sample Report Release 1
	    // this.IGFunctions.WebViewImplementationGuide("ASCO-BCR Sample Report Release 1", "ASCO-BCR Sample Report Release 1 - Draft", "lcg.admin");
		
		// Test Adding Permissions to the "IHE PCC"
		this.IGFunctions.PermissionImplementationGuide("Test IHE PCC", "CDA", "lcg.admin");
	
		// Test Creating the "Automation Test IG"
		this.IGFunctions.CreateImplementationGuide("Automation Test IG", "Automation Test IG Display Name", "Automation Test Web IG", "Automation Test Web IG for testing", "CDA", "lcg.admin");
		
		// Test Editing the "Automation Test IG"
		this.IGFunctions.EditImplementationGuide("Automation Test IG", "lcg.admin", 
		"<sch:rule context=\"cda:ClinicalDocument\"> <sch:assert test=\"count(cda:templateId[@root='1.2.3.4.5.6.7.8.9.10'])=1\">This document SHALL conform to template \"Automation Test Template\" (templateId: 1.2.3.4.5.6.7.8.9.10).</sch:assert> </sch:rule>");
			
                               //  *** TEMPLATE TESTS ***
                               //------------------------------
		
		// Test Viewing the "Acuity Score Data Section" Template
		   this.TemplateFunctions.ViewTemplate("Acuity score data section", "urn:oid:2.16.840.1.113883.10.20.17.2.3","Patient data section - NCR (urn:oid:2.16.840.1.113883.10.20.17.2.5)","urn:oid:2.16.840.1.113883.10.20.17.2.5",
		   "This section contains the templates that represent data elements whose variants are thought to be significant indicators of the severity of illness and of subsequent infant outcome", "lcg.admin");
		
		// Test Creating the "Automation Test Template"
		   this.TemplateFunctions.CreateTemplate("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "Automation Test IG", "CDA: document", "Test Template for Automation", "lcg.admin");
		
		// Test Editing the "Automation Test Template"
		   this.TemplateFunctions.EditTemplate("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "Add Notes to the Automation Test Template", "lcg.admin");
		
		// Test Copying the "Automation Test Template"
		   this.TemplateFunctions.CopyTemplate("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "Automation Test IG", "Automation Test Template Copy", "lcg.admin");
	
     	// Test Moving the "Automation Test Template Copy"
    	   this.TemplateFunctions.MoveTemplate("Automation Test Template Copy", "urn:hl7ii:1.2.3.4.5.6.7.8.9.10", "Consolidation V2", "Section", "lcg.admin");
	    
		// Test Deleting the "Automation Test Template Copy"
		   this.TemplateFunctions.DeleteTemplate("Automation Test Template Copy", "urn:hl7ii:1.2.3.4.5.6.7.8.9.10", "lcg.admin");
		   

                        //  *** CONSTRAINT TESTS ***
                        //------------------------------

		// Test Creating Computed Constraints for the "Automation Test Template"
	       this.ConstraintFunctions.CreateComputedConstraint("Automation Test Template","urn:oid:1.2.3.4.5.6.7.8.9.10", "CDA: Document", "Other", "1.2.3.4.5.6.7.8", 
	    		      "POCD_HD000040", "lcg.admin");
		
	    // Test Adding the "id" element with a Primitive Constraint containing schematron and "code" element with Value Set Binding
	       this.ConstraintFunctions.AddPrimitiveConstraint("Automation Test Template", "CDA: Document", "This id SHALL be a globally unique identifier for the document", 
	    		       "STATIC", "Engineer", "not(tested)", "lcg.admin");
		
		// Test Editing a Primitive Constraint in the "Automation Test Template"
	       this.ConstraintFunctions.EditPrimitiveConstraint("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "CDA: Document", "Analyst", 
	    		    "SHOULD", "lcg.admin");
		
	    // Test Binding Value Sets and Templates to the "Automation Test Template" constraints
		   this.ConstraintFunctions.constraintBinding("Automation Test Template", "CDA: Document", "2.16.840.1.113883.3.2898.11.22", "Cause of Injury", "ACCF Family History", "lcg.admin");
		
		// Test Previewing and Validating Constraints within the "Automation Test Template"
	       this.ConstraintFunctions.PreviewAndValidateConstraint("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10",
	        "SHALL contain exactly one [1..1] realmCode=\"US\"", 
	       	"SHALL contain exactly one [1..1] @root=\"1.2.3.4.5.6.7.8\"", 
	       	"SHALL contain exactly one [1..1] @extension=\"POCD_HD000040\"", 
	       	"SHALL contain exactly one [1..1] code (ValueSet: Cause of Injury urn:oid:2.16.840.1.113883.3.2898.11.22)", 
	       	"SHALL contain exactly one [1..1] ACCF Family History (identifier: urn:oid:1.3.6.1.4.1.19376.1.4.1.6.4.99.6)"
	       	, "Schema allows multiple for", "lcg.admin");

	       
                    //        *** PUBLISHING TESTS ***
                    //    ----------------------------------

	    // Test Creating Sample XML for the "Automation Test Template"
		   this.PublishingFunctions.CreateTemplateXMLSample("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "1. SHALL contain exactly one [1..1] realmCode= \"US\" (CONF:2234-3).", "lcg.admin" );
		
		// Test Editing the Sample XML for the "Automation Test Template"
		this.PublishingFunctions.EditTemplateXMLSample("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "lcg.admin");
		
		// Test Deleting the Template Sample for the "Automation Test Template"
		this.PublishingFunctions.DeleteTemplateXMLSample("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "lcg.admin");
		
		// Test Balloting and Publishing the "Automation Test Implementation Guide"
		this.PublishingFunctions.BallotAndPublishIG("Automation Test IG", "lcg.admin", "ballot");
		
		// Test Versioning the "Automation Test Implementation Guide"
		this.PublishingFunctions.VersionIG("Automation Test IG", "Automation Test IG V2", "lcg.admin");
		
		// Test Versioning the "Automation Test Template"
		this.PublishingFunctions.VersionTemplate("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "Automation Test Template V2", "lcg.admin");
		
		// Test Deleting the "Automation Test IG V2" and "Automation Test IG"
        this.IGFunctions.DeleteImplementationGuide("Automation Test IG V2", "Automation Test IG", "lcg.admin");

                       //     *** EXPORT TESTS ***
                       //   ----------------------------

     // Test Exporting the Templates for the "QRDA Category III" Implementation Guide
		   this.ExportFunctions.ExportTemplateToWord("QRDA Category III", "", "lcg.admin");
		
		// Test Exporting the Template XML for the "National Health Care Surveys" Implementation Guide
		   this.ExportFunctions.ExportTemplateXML("National Health Care Surveys", "", "lcg.admin");
			
		// Test Exporting the Schematron for the "Cath/PCI Registry Reporting Implementation Guide" Implementation Guide
		this.ExportFunctions.ExportSchematron("Cath/PCI Registry Reporting Implementation Guide", "lcg.admin");
		
		// Test Exporting the Vocabulary for the "Consolidation V2" Implementation Guide
     	this.ExportFunctions.ExportVocabulary("Consolidation V2", "lcg.admin");

                   //        TERMINOLOGY TESTS
                   //  ------------------------------


		// Test Browsing the "CA Realm Header languageCode" Value Set
		   this.TerminologyFunctions.BrowseValueSet("LanguageCode", "urn:oid:2.16.840.1.113883.2.20.3.190", "fra-CA", "lcg.admin");
		
		// Test Creating the "Automation Test Value Set"
		this.TerminologyFunctions.CreateValueSet("Automation Test Gender Value Set", "urn:oid:2.2.2.2.2.2.2.2", "TEST_AdministrativeGender", 
				"Administrative Gender based upon TEST vocabulary. This value set contains only male, female and undifferentiated concepts", "www.automationtesting.com");
		
		// Test Editing the "Automation Test Value Set"
		   this.TerminologyFunctions.EditValueSet("Automation Test Gender Value Set", "urn:oid:2.2.2.2.2.2.2.2", "F", "Female", "AdministrativeGender", 
				"Active", "M", "Male", "AdministrativeGender", "Active", "UN", "Undifferentiated", "AdministrativeGender", "Active", "lcg.admin");
		
		// Test Deleting the "Automation Test Value Set"
		   this.TerminologyFunctions.DeleteValueSet("Automation Test Gender Value Set", "urn:oid:2.2.2.2.2.2.2.2", "lcg.admin");
		
		
		// Test Browsing the "FIPS 5-2 (State)" Code System
		this.TerminologyFunctions.BrowseCodeSystem("FIPS 5-2 (State)", "urn:oid:2.16.840.1.113883.6.92", "lcg.admin");
		
		// Test Creating the "Automation Test Vaccine Code System"
		this.TerminologyFunctions.CreateCodeSystem("Automation Test Vaccine Code System", "urn:oid:2.4.4.4.4.4.4.4", "Also known as CVX codes.");
		
		// Test Editing the "Automation Test Vaccine Code System"
		   this.TerminologyFunctions.EditCodeSystem("Automation Test Vaccine Code System", "urn:oid:2.4.4.4.4.4.4.4", "Used by the CDC", "lcg.admin");
		
		// Test Deleting the "Automation Test Vaccine Code System"
		   this.TerminologyFunctions.DeleteCodeSystem("Automation Test Vaccine Code System", "urn:oid:2.4.4.4.4.4.4.4", "lcg.admin");

                     //  ***  CLOSING ***
                     //---------------------

		// Lantana Administrator Logout
		this.LoginFunctions.LCGLogout("lcg.admin");
//=======================================================================================================================================================================================================
//                           PART II - LANTANA USER TESTS
//========================================================================================================================================================================================================

		// Testing Logging into Trifolia Dev as Lantana User
		   this.LoginFunctions.LCGUserLogin("lcg.user", "http://dev.trifolia.lantanagroup.com/");
			
		                 //  *** IMPLEMENTATION GUIDE TESTS ***
                         // ----------------------------------		

		// Test Browsing the "PHARM HIT Demo" Implementation Guide
			this.IGFunctions.BrowseImplementationGuide("PHARM HIT Demo", "","", "", "", "PHARM HIT Demo", "document", "[0..1]","Pattern ID", "","","", "lcg.user" );


						//	*** TEMPLATE TESTS ***
    					// ------------------------------

		// Test Viewing the "IHE PCC EDD Observation" Template
		this.TemplateFunctions.ViewTemplate ("IHE PCC EDD Observation", "urn:oid:1.3.6.1.4.1.19376.1.5.3.1.1.11.2.3.1", "", "",
	     "The EDD observation reflects the clinician’s best judgment about the estimated delivery date of the patient", "lcg.user");

    					//	  *** TERMINOLOGY TESTS ***
   						// ------------------------------


		// Test Browsing the "Family Member Value Set" Value Set
	    this.TerminologyFunctions.BrowseValueSet("Family Member Value Set", "urn:oid:2.16.840.1.113883.1.11.19579", "FAMMEMB", "lcg.user");
	
		// Test Browsing the "FIPS 5-2 (State)" Code System
		this.TerminologyFunctions.BrowseCodeSystem("EntityNameUse", "urn:oid:2.16.840.1.113883.5.45", "lcg.user");

		
                          //  *** EXPORT TESTS ***
                          // ----------------------------

		//Test Exporting the Templates for the "Test IHE PCC" Implementation Guide
				this.ExportFunctions.ExportTemplateToWord("C-CDA on FHIR", "", "lcg.user");

				// Test Exporting the Template XML for the "TEST_CONF_IG V2 V2" Implementation Guide
				this.ExportFunctions.ExportTemplateXML("PHARM HIT Demo", "", "lcg.user");
					


                          //***  CLOSING ***
                          // -------------------
		
		// Lantana User Logout
		this.LoginFunctions.LCGLogout("lcg.user");
    
// ========================================================================================================================================================================================================
//                            PART IV - HL7 MEMBER TESTS
// ========================================================================================================================================================================================================
//
//        // Testing Logging into Trifolia as HL7 Member "student_test"
//        this.LoginFunctions.HL7MemberLogin();
//       
//                        //   *** IMPLEMENTATION GUIDE TESTS ***
//                        //-----------------------------------------
//
//		// Test Browsing the "Public Health Case Report Release 1" 
//		this.IGFunctions.BrowseImplementationGuide("Public Health Case Report Release 1", "Published : Thursday, October 01, 2009", "entry", "[1..*]", "Rule(s) Definition", "hl7.member");
//
//		// Test Creating the "HL7 Member Test IG"
//		this.IGFunctions.CreateImplementationGuide("HL7 Member Test IG", "HL7 Member Test IG", "eMeasure", "hl7.member");
//		
//		// Test Editing the "HL7 Member Test IG"
//		this.IGFunctions.EditImplementationGuide("HL7 Member Test IG","hl7.member",
//	    "<sch:rule context=\"cda:ClinicalDocument\"> <sch:assert test=\"count(cda:templateId[@root='2.2.2.2.2.2.2.2'])=1\">This document SHALL conform to template \"HL7 Member Test Template\" (templateId: 2.2.2.2.2.2.2.2).</sch:assert> </sch:rule>");
//	
//                       //   *** TEMPLATE TESTS ***
//                       //-----------------------------
//
//	   // Test Viewing the "LTPAC Home Health Summary" Template
//     	  this.TemplateFunctions.ViewTemplate("LTPAC Home Health Summary", "urn:oid:2.16.840.1.113883.10.20.22.1.11.2", "", "",
//        	   "This template describes constraints that apply to the OASIS LTPAC Home Health Summary.", "hl7.member");
//     	
//		// Test Creating the "HL7 Member Test Template"
//		   this.TemplateFunctions.CreateTemplate("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2", "HL7 Member Test IG", "eMeasure: document", "Test Template HL7 Member", "hl7.member");
//		
//		// Test Editing the "HL7 Member Test Template"
//		   this.TemplateFunctions.EditTemplate("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2", "Add Notes to the HL7 Member Test Template" ,"hl7.member");
//		
//		// Test Copying the "HL7 Member Test Template"
//		   this.TemplateFunctions.CopyTemplate("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2", "HL7 Member Test IG", "HL7 Member Test Template Copy", "hl7.member");
//		
//		// Test Moving the "HL7 Member Test Template Copy"
//		   //this.TemplateFunctions.MoveTemplate("HL7 Member Test Template Copy", "Consolidation V2", "Document", "hl7.member");
//		
//		// Test Deleting the "HL7 Member Test Template Copy"
//		   this.TemplateFunctions.DeleteTemplate("HL7 Member Test Template Copy", "hl7.member");
//
//       				    //	*** CONSTRAINT TESTS ***
//     					//------------------------------
//
//		// Test Creating Computed Constraints for the "HL7 Member Test Template"
//		this.ConstraintFunctions.CreateComputedConstraint("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2", "eMeasure: Document", "Other", "2.2.2.2.2.2.2.2", "POCD_HD000040", "hl7.member");
//
//		// Test Adding the "id" element with a Primitive Constraint containing schematron
//		this.ConstraintFunctions.AddPrimitiveConstraint("HL7 Member Test Template", "eMeasure: Document", "This id SHALL be a globally unique identifier for the document", "STATIC", "Engineer", "tested)");
//
//		// Test Editing, Duplicating and Deleting a Primitive Constraint in the "HL7 Member Test Template"
//		this.ConstraintFunctions.EditPrimitiveConstraint("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2", "eMeasure: Document", "Analyst", "SHOULD", "hl7.member");
//
//		// Test Binding Value Sets and Templates to constraints in the "HL7 Member Test Template"
//		this.ConstraintFunctions.constraintBinding("HL7 Member Test Template", "eMeasure: Document", "2.16.840.1.113883.11.20.11.2", "Admission Diagnosis Section (V2)");
//
//		// Test Previewing and Validating Constraints within the "HL7 Member Test Template"
//		this.ConstraintFunctions.PreviewAndValidateConstraint("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2",
//				"SHALL contain exactly one [1..1] realmCode=\"US\"",
//				"SHALL contain exactly one [1..1] typeId", 
//				"SHALL contain exactly one [1..1] @root=\"2.2.2.2.2.2.2.2\"", 
//				"SHALL contain exactly one [1..1] @extension=\"POCD_HD000040\"",
//				"", "Schema allows multiple for", "hl7.member");
//
//                      //     *** PUBLISHING TESTS ***
//                      //----------------------------------
//		
//		// Test Creating Sample XML for the "HL7 Member Test Template"
//		this.PublishingFunctions.CreateTemplateXMLSample("HL7 Member Test Template",  "urn:oid:2.2.2.2.2.2.2.2", "hl7.member");
//		
//		// Test Editing the Sample XML for the "HL7 Member Test Template"
//		this.PublishingFunctions.EditTemplateXMLSample("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2", "hl7.member");
//		
//		// Test Deleting the Template Sample for the "HL7 Member Test Template"
//		this.PublishingFunctions.DeleteTemplateXMLSample("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2", "hl7.member");
//		
//		// Test Balloting and Publishing the "HL7 Member Test Implementation Guide"
//		this.PublishingFunctions.BallotAndPublishIG("HL7 Member Test IG", "hl7.member", "Ballot");
//		
//		// Test Versioning the "HL7 Member Test Implementation Guide"
//		this.PublishingFunctions.VersionIG("HL7 Member Test IG", "HL7 Member Test IG V2", "hl7.member");
//		
//		// Test Versioning the "HL7 Member Test Template"
//		this.PublishingFunctions.VersionTemplate("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2", "HL7 Member Test Template V2", "hl7.member");
//		
//		// Test Deleting the "HL7 Member Test IG V2" and "HL7 Member Test IG"
//		this.IGFunctions.DeleteImplementationGuide("HL7 Member Test IG V2", "HL7 Member Test IG", "hl7.member");
//    
//                         //  *** TERMINOLOGY TESTS ***
//                         //------------------------------
//
//		// Test Browsing the "Observation Interpretation (HL7)" Value Set
//    	this.TerminologyFunctions.BrowseValueSet("Observation Interpretation (HL7)", "urn:oid:2.16.840.1.113883.1.11.78", "above high threshold");
//
//      	// Test Creating the "HL7 Member Test Value Set"
//      	this.TerminologyFunctions.CreateValueSet("HL7 Member Test Gender Value Set", "urn:oid:1.2.3.4.5.6.7.8", "TEST_AdministrativeGender", 
//			"Administrative Gender based upon TEST vocabulary. This value set contains only male, female and undifferentiated concepts", "www.automationtesting.com");
//	
//		// Test Editing the "HL7 Member Test Value Set"
//		this.TerminologyFunctions.EditValueSet("HL7 Member Test Gender Value Set", "urn:oid:1.2.3.4.5.6.7.8", 
//				"F", "Female", "AdministrativeGender", "Active", "M", "Male", "AdministrativeGender", "Active", "UN", "Undifferentiated", "AdministrativeGender", "Active");
//		
//		// Test Deleting the "HL7 Member Value Set"
//		this.TerminologyFunctions.DeleteValueSet("HL7 Member Test Gender Value Set", "urn:oid:1.2.3.4.5.6.7.8");
//		
//		
//		// Test Browsing the "HL7 V2.5 Route of Administration Codes" Code System
//      	this.TerminologyFunctions.BrowseCodeSystem("HL7 V2.5 Route of Administration codes", "2.16.840.1.113883.12.162", "hl7.member");
//    
//		// Test Creating the "HL7 Member Test Vaccine Code System"
//		this.TerminologyFunctions.CreateCodeSystem("HL7 Member Test Vaccine Code System", "urn:oid:1.5.5.5.5.5.5.5", "Also known as CVX codes.");
//		
//		// Test Editing the "HL7 Member Test Vaccine Code System"
//		this.TerminologyFunctions.EditCodeSystem("HL7 Member Test Vaccine Code System", "urn:oid:1.5.5.5.5.5.5.5", "Used by the CDC");
//		
//		// Test Deleting the "HL7 Member Test Vaccine Code System"
//		this.TerminologyFunctions.DeleteCodeSystem("HL7 Member Test Vaccine Code System", "urn:oid:1.5.5.5.5.5.5.5");	
//
//                         //  *** EXPORT TESTS ***
//                         //-------------------------
//
//		// Test Exporting the Templates for the "HIV/AIDS Services Report" Implementation Guide
//		this.ExportFunctions.ExportTemplateToWord("HIV/AIDS Services Report", "hl7.member");
//	
//		// Test Exporting the Template XML for the "PC Review of Allergies and Intolerances" Implementation Guide
//		this.ExportFunctions.ExportTemplateXML("PC Review of Allergies and Intolerances", "hl7.member");
//		
//		// Test Exporting the Schematron for the "Healthcare Associated Infection Reports Normative Release 1" Implementation Guide
//		this.ExportFunctions.ExportSchematron("Healthcare Associated Infection Reports Normative Release 1", "hl7.member");
//		
//		// Test Exporting the Vocabulary for the "Transfer Summary" Implementation Guide
//     	this.ExportFunctions.ExportVocabulary("Transfer Summary", "hl7.member");
//
//                           //  ***  CLOSING ***
//                           //-------------------------
//
//		// Test HL7 Member Logout
//		this.LoginFunctions.HL7Logout("hl7.member");
//		  
//          
//// ========================================================================================================================================================================================================
////                            PART V - HL7 USER TESTS
//// ========================================================================================================================================================================================================
//
//    	// Testing Logging into Trifolia as HL7 User "trifolia_test"
//       this.LoginFunctions.HL7UserLogin();
//
//          
//                        //   *** IMPLEMENTATION GUIDE TESTS ***
//                        //-----------------------------------------
//
//       // Test Browsing the "Cath/PCI Registry Reporting Implementation Guide" 
//		this.IGFunctions.BrowseImplementationGuide("Cath/PCI Registry Reporting Implementation Guide", "Published : Wednesday, October 16, 2013", "subentry", "[0..*]", "", "hl7.user");
//
//
//                         //  *** TEMPLATE TESTS ***
//                         //-----------------------------
//
//		// Test Viewing the "eMeasure Reference QDM" Template
//		this.TemplateFunctions.ViewTemplate("eMeasure Reference QDM", "urn:oid:2.16.840.1.113883.10.20.24.3.97","", "", 
//		  "This template defines the way that a QDM eMeasure should be referenced in a QDM-Based QRDA", "hl7.user");
//
//                      //     *** TERMINOLOGY TESTS ***
//                      // ------------------------------
//
//		// Test Browsing the "HL7 BasicConfidentialityKind" Value Set
//		this.TerminologyFunctions.BrowseValueSet("HL7 BasicConfidentialityKind", "2.16.840.1.113883.1.11.16926", "very restricted");
//
//		// Test Browsing the "HL7 Realm" Code System
//		this.TerminologyFunctions.BrowseCodeSystem("HL7Realm", "2.16.840.1.113883.5.1124", "hl7.user");
//
//                         //  *** EXPORT TESTS ***
//                         //-------------------------
//
//		// Test Exporting the Templates for the "Neonatal Care Report Release 1" Implementation Guide
//		this.ExportFunctions.ExportTemplateToWord("Neonatal Care Report Release 1", "hl7.user");
//
//		// Test Exporting the Template XML for the "Cath/PCI Registry Reporting Implementation Guide" Implementation Guide
//		this.ExportFunctions.ExportTemplateXML("Cath/PCI Registry Reporting Implementation Guide", "hl7.user");
//
//
//                          //   ***  CLOSING ***
//                          //-------------------------
//
//		// Test HL7 User Logout
//		this.LoginFunctions.HL7Logout("hl7.user");
//		
}

@After
public void tearDown() throws Exception {
  driver.quit();
  String verificationErrorString = verificationErrors.toString();
  if (!"".equals(verificationErrorString)) {
    fail(verificationErrorString);
  }
}
private boolean isElementPresent(By by) {
  try {
    driver.findElement(by);
    return true;
  } catch (NoSuchElementException e) {
    return false;
  }
}

private boolean isAlertPresent() {
  try {
    driver.switchTo().alert();
    return true;
  } catch (NoAlertPresentException e) {
    return false;
  }
}

private String closeAlertAndGetItsText() {
  try {
    Alert alert = driver.switchTo().alert();
    String alertText = alert.getText();
    if (acceptNextAlert) {
      alert.accept();
    } else {
      alert.dismiss();
    }
    return alertText;
  } finally {
    acceptNextAlert = true;
  }
 }
}