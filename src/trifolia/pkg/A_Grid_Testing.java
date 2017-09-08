package trifolia.pkg;

import java.util.regex.Pattern;
import java.util.concurrent.TimeUnit;

import org.junit.*;

import static org.junit.Assert.*;
import static org.hamcrest.CoreMatchers.*;

import org.openqa.selenium.*;
import org.openqa.selenium.remote.DesiredCapabilities;
import java.net.MalformedURLException;
import org.openqa.selenium.remote.RemoteWebDriver;
import java.net.URL;
import org.openqa.selenium.firefox.FirefoxDriver;
import org.openqa.selenium.firefox.FirefoxProfile;
import org.openqa.selenium.chrome.ChromeDriver;
import org.openqa.selenium.firefox.internal.ProfilesIni;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.Select;
import org.openqa.selenium.support.ui.WebDriverWait;
import org.openqa.selenium.WebDriver;

public class A_Grid_Testing {
  String baseURL; 
  String nodeURL;
  String platform_name;
  String browser_name;	
  String firefox_binary;
  private boolean acceptNextAlert = true;
  private StringBuffer verificationErrors = new StringBuffer();
  private A_TrifoliaLogin LoginFunctions;
  private C_ImplementationGuideFunctions IGFunctions;
  private static Boolean createdImplementationGuide = false;
  private D_TemplateFunctions TemplateFunctions;
  private E_ConstraintFunctions ConstraintFunctions;
  private F_PublishingFunctions PublishingFunctions;
  private G_TerminologyFunctions TerminologyFunctions;
  private H_ExportFunctions ExportFunctions;
  // private String implementationGuideName;
  
  public static RemoteWebDriver driver;
  
  @Before
  public void setUp() throws MalformedURLException {
	  // this.implementationGuideName = "Automated Test IG " + Math.random();
	   
	    baseURL = "http://dev.trifolia.lantanagroup.com/";
        nodeURL = "http://10.0.10.65:5555/wd/hub";
             	    
    	DesiredCapabilities firefoxcapability = DesiredCapabilities.firefox();
    	// firefoxcapability.setBrowserName("firefox");
    	firefoxcapability.setCapability("marionette", true);
    	firefoxcapability.setVersion("48.0.2");
    	firefoxcapability.setPlatform(Platform.VISTA);
    	// firefoxcapability.setCapability(firefox_binary, "C:\\Program Files (x86)\\Mozilla Firefox\\firefox.exe");
    	driver = new RemoteWebDriver(new URL(nodeURL), firefoxcapability);

//    	System.setProperty("webdriver.chrome.driver","C:\\Program Files\\Chrome\\chromedriver_win32\\chromedriver.exe");
//    	driver = new ChromeDriver(); 
//    	DesiredCapabilities chromecapability = DesiredCapabilities.chrome();
//	    chromecapability.setBrowserName("chrome");
//	    chromecapability.setPlatform(Platform.VISTA);
//    	baseURL = "http://staging.lantanagroup.com:1234/";
//    	driver = new RemoteWebDriver(new URL(nodeURL), chromecapability);
  
      	driver.manage().window().maximize();
	
	this.LoginFunctions = new A_TrifoliaLogin();
	this.LoginFunctions.setDriver(this.driver);
	this.IGFunctions = new C_ImplementationGuideFunctions();
	this.IGFunctions.setDriver(this.driver);
	this.TemplateFunctions = new D_TemplateFunctions();
	this.TemplateFunctions.setDriver(this.driver);
	this.ConstraintFunctions = new E_ConstraintFunctions();
	this.ConstraintFunctions.setDriver(this.driver);
	this.PublishingFunctions = new F_PublishingFunctions();
	this.PublishingFunctions.setDriver(this.driver);
	this.TerminologyFunctions = new G_TerminologyFunctions();
	this.TerminologyFunctions.setDriver(this.driver);
	this.ExportFunctions = new H_ExportFunctions();
	this.ExportFunctions.setDriver(this.driver);
	
  }
@Test

    public void simpleTest() throws Exception {
    driver.get(baseURL);

// ========================================================================================================================================================================================================
//  PART I - LANTANA ADMINISTRATOR TESTS
//========================================================================================================================================================================================================

       // Testing Logging into Trifolia Dev environment with Lantana Admin account 
	   this.LoginFunctions.LCGAdminLogin("lcg.admin", "http://dev.trifolia.lantanagroup.com/");

	    //  Testing Logging into Trifolia Staging environment with Lantana Admin account  
        //	    this.LoginFunctions.LCGAdminLogin("lcg.admin", "http://staging.lantanagroup.com:1234/");


	//    *** IMPLEMENTATION GUIDE TESTS ***
	//------------------------------------		

	// Test Browsing the HAI R9 Implementation Guide
	this.IGFunctions.BrowseImplementationGuide("Healthcare Associated Infection Reports Release 9", "15-19677", "The value of the id", "No audit entries", "hai_voc.xml", "Published : Tuesday, June 18, 2013", "section", "[1..1]", "Phase", "Entire Organization (HL7)", "HTML Content", "Notify", "lcg.admin");
	
	// Test Adding Permissions to the "IHE PCC"
	this.IGFunctions.PermissionImplementationGuide("Test IHE PCC", "CDA", "lcg.admin", "Lantana Admin");
	
	// Test Creating the "Automation Test IG"
	this.IGFunctions.CreateImplementationGuide("Automation Test IG", "automation.test@lcg.org", "LCG" ,"Automation Test IG Display Name", "Automation Test Web IG", "Automation Test Web IG for testing", "CDA", "lcg.admin");
	
	// Test Editing the "Automation Test IG"
	this.IGFunctions.EditImplementationGuide("Automation Test IG", "lcg.admin", 
	"<sch:rule context=\"cda:ClinicalDocument\"> <sch:assert test=\"count(cda:templateId[@root='1.2.3.4.5.6.7.8.9.10'])=1\">This document SHALL conform to template \"Automation Test Template\" (templateId: 1.2.3.4.5.6.7.8.9.10).</sch:assert> </sch:rule>");	
	
	// Test WebViewing the Healthcare Associated Infection Reports Release 2 DSTU 1 V8 IG
			 this.IGFunctions.WebViewImplementationGuide("http://dev.trifolia.lantanagroup.com/", "Healthcare Associated Infection Reports Release 2 DSTU 1 V8",
			 "NHSN Healthcare Associated", "Structure of This Guide", "Antimicrobial Resistance Option (ARO) Summary Report (V2)",
			 "Administrative Gender (HL7 V3)", "SNOMED CT", "lcg.admin");
      //  *** TEMPLATE TESTS ***
      //------------------------------

	// Test Viewing the "Acuity Score Data Section" Template
	this.TemplateFunctions.ViewTemplate("Acuity score data section", "urn:oid:2.16.840.1.113883.10.20.17.2.3","Patient data section - NCR (urn:oid:2.16.840.1.113883.10.20.17.2.5)","urn:oid:2.16.840.1.113883.10.20.17.2.5",
	"This section contains the templates that represent data elements whose variants are thought to be significant indicators of the severity of illness and of subsequent infant outcome", "lcg.admin");
	
	// Test Creating the "Automation Test Template"
	this.TemplateFunctions.CreateTemplate("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "Automation Test IG", "CDA: document", "Test Template for Automation", "lcg.admin");
	
	// Test Editing the "Automation Test Template"
	this.TemplateFunctions.EditTemplate("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "Allergies - Test", "Add Notes to the Automation Test Template", "lcg.admin");
	
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
	this.ConstraintFunctions.EditPrimitiveConstraint("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "CDA: Document", "Analyst", "SHOULD", "lcg.admin");
	
	// Test Binding Value Sets to the "Automation Test Template" constraints
	this.ConstraintFunctions.constraintValueSetBinding("Automation Test Template",
			"CDA: Document", "2.16.840.1.113883.3.2898.11.22",
			"Cause of Injury", "lcg.admin");

	// Test Binding Templates to the "Automation Test Template" constraints
	this.ConstraintFunctions.constraintTemplateBinding("Automation Test Template",
			"CDA: Document", "ACCF Family History", "lcg.admin");
	
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
	this.PublishingFunctions.BallotAndPublishIG("Automation Test IG", "lcg.admin", "Ballot");
	
	// Test Versioning the "Automation Test Implementation Guide"
	this.PublishingFunctions.VersionIG("Automation Test IG", "Automation Test IG V2", "lcg.admin");
	
	// Test Versioning the "Automation Test Template"
	this.PublishingFunctions.VersionTemplate("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "Automation Test Template V2", "Adding Versioned Template Notes", "lcg.admin");
	
	// Test Deleting the "Automation Test IG V2" and "Automation Test IG"
	this.IGFunctions.DeleteImplementationGuide("Automation Test IG V2", "Automation Test IG", "lcg.admin");
	
	//        TERMINOLOGY TESTS
	//  ------------------------------
	
	// Test Browsing the "CA Realm Header languageCode" Value Set
	this.TerminologyFunctions.BrowseValueSet("LanguageCode", "urn:oid:2.16.840.1.113883.2.20.3.190", "fra-CA", "lcg.admin");
	
	// Test Creating the "Automation Test Value Set"
	this.TerminologyFunctions.CreateValueSet("Automation Test Gender Value Set", "urn:oid:2.2.2.2.2.2.2.2", "TEST_AdministrativeGender", 
	"Administrative Gender based upon TEST vocabulary. This value set contains only male, female and undifferentiated concepts", "www.automationtesting.com", "lcg.admin");
	
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
	
	//     *** EXPORT TESTS ***
	//   ----------------------------
	
	// Test Exporting the Templates for the "QRDA Category III" Implementation Guide
	this.ExportFunctions.ExportTemplateToWord("QRDA Category III", "", "lcg.admin");
	
	// Test Exporting the Template XML for the "National Health Care Surveys" Implementation Guide
	this.ExportFunctions.ExportTemplateXML("National Health Care Surveys", "", "lcg.admin");
	
	// Test Exporting the Schematron for the "Cath/PCI Registry Reporting Implementation Guide" Implementation Guide
	this.ExportFunctions.ExportSchematron("Cath/PCI Registry Reporting Implementation Guide", "lcg.admin");
	
	// Test Exporting the Vocabulary for the "Consolidation V2" Implementation Guide
	this.ExportFunctions.ExportVocabulary("ASCO-BCR Sample Report Release 1", "lcg.admin");
	
	// Test Exporting the XML for the "C-CDA on FHIR" Implementation Guide
	this.ExportFunctions.ExportFHIRXML("C-CDA on FHIR", "lcg.admin");
	
	//  ***  CLOSING ***
	//---------------------
	
	// Lantana Administrator Logout
	this.LoginFunctions.LCGLogout("lcg.admin");
//=======================================================================================================================================================================================================
// PART II - LANTANA USER TESTS
//========================================================================================================================================================================================================

	// Testing Logging into Trifolia Dev as Lantana User
	this.LoginFunctions.LCGUserLogin("lcg.user", "http://dev.trifolia.lantanagroup.com/");
	
	
	// Testing Logging into Trifolia Staging as Lantana User
	// this.LoginFunctions.LCGUserLogin("lcg.user", "http://staging.lantanagroup.com:1234/");
	
	
	//  *** IMPLEMENTATION GUIDE TESTS ***
	// ----------------------------------		
	
	// Test Browsing the "PHARM HIT Demo" Implementation Guide
	this.IGFunctions.BrowseImplementationGuide("PHARM HIT Demo", "","", "", "", "PHARM HIT Demo", "document", "[0..1]","Pattern ID", "","","", "lcg.user" );
	
	//	*** TEMPLATE TESTS ***
	// ------------------------------
	
	// Test Viewing the "MAP Topic" Template
	this.TemplateFunctions.ViewTemplate ("MAP Topic", "http://www.lantanagroup.com/camara/fhir/map-topic", "", "",
	"SHALL contain exactly one [1..1] extension (CONF:1206-1034) such that it", "lcg.user");
	
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

}

//========================================================================================================================================================================================================
//       CLOSING
//========================================================================================================================================================================================================

    
@After
    
    public void afterTest() {
         driver.quit();
         }
  } 
