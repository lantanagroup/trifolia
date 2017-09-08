package trifolia.pkg;

import org.junit.*;

import static org.junit.Assert.*;
import org.openqa.selenium.*;
import org.openqa.selenium.firefox.FirefoxDriver;
import org.openqa.selenium.firefox.FirefoxProfile;
import org.openqa.selenium.firefox.internal.ProfilesIni;
import org.openqa.selenium.WebDriver;

public class AllUsersSpecificFunctions {
	private static final String String = null;
	public static String downloadPath = "E:\\Selenium\\TrifoliaDownloads\\";
	private WebDriver driver;
	private String baseUrl;
	private boolean acceptNextAlert = true;
	private StringBuffer verificationErrors = new StringBuffer();
	private A_TrifoliaLogin LoginFunctions;
	private B_SecurityFunctions SecurityFunctions;
	private C_ImplementationGuideFunctions IGFunctions;
	private static Boolean createdImplementationGuide = false;
	private D_TemplateFunctions TemplateFunctions;
	private E_ConstraintFunctions ConstraintFunctions;
	private F_PublishingFunctions PublishingFunctions;
	private G_TerminologyFunctions TerminologyFunctions;
	private H_ExportFunctions ExportFunctions;

	@Before
	public void setUp() throws Exception {
		ProfilesIni allProfiles = new ProfilesIni();
		FirefoxProfile profile = allProfiles.getProfile("default");
		// FirefoxProfile profile = new FirefoxProfile();
		driver = new FirefoxDriver(profile);
		profile.setPreference("browser.download.manager.showWhenStarting", false);		
		profile.setPreference("browser.helperapps.alwaysAsk.force", false);	
//		profile.setPreference("browser,helperapps.neverAsk.SaveToDisk", "application/msword, application/xml");
//	    profile.setPreference("browser,helperapps.neverAsk.openFile", "application/msword, application/xml");
	    profile.setPreference("browser,helperapps.neverAsk.SaveToDisk", "*.*");
	    profile.setPreference("browser,helperapps.neverAsk.openFile", "*.*");
	    profile.setPreference("browser.download.folderList", 2);
		profile.setPreference("browser.download.dir", "E:\\Selenium\\TrifoliaDownloads\\");
		driver.manage().window().maximize();

	   baseUrl = "http://trifolia-dev.lantanagroup.com/";
       // baseUrl = "https://trifolia-staging.lantanagroup.com";

		this.LoginFunctions = new A_TrifoliaLogin();
		this.LoginFunctions.setDriver(this.driver);
		this.SecurityFunctions = new B_SecurityFunctions();
		this.SecurityFunctions.setDriver(this.driver);
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
	public void testAllTrifoliaFunctions() throws Exception {
		driver.get(baseUrl + "/");

//		// ========================================================================================================================================================================================================
//		//                                        SECURITY AND PERMISSIONS TESTING
//		// ========================================================================================================================================================================================================
//
//		// Testing Logging into Trifolia Dev environment and create the Lantana Admin account
//		   this.LoginFunctions.LCGAdminLogin("lcg.admin", "http://dev.trifolia.lantanagroup.com/");
//
//		// Testing Logging into Trifolia Staging environment with Lantana Admin account
//		   this.LoginFunctions.LCGAdminLogin("lcg.admin","https://trifolia-staging.lantanagroup.com");
//
//		// Logout the Lantana Admin Account
//		   this.LoginFunctions.LCGLogout("lcg.admin");
//		
//		// Testing adding the "Administrator" role to the Lantana Admin account
//		   this.LoginFunctions.LCGAdminLogin("lcg.admin", "http://dev.trifolia.lantanagroup.com/");	
//
//		// Testing Logging into Trifolia Dev environment with Lantana Admin account
//		   this.LoginFunctions.LCGAdminLogin("lcg.admin", "http://dev.trifolia.lantanagroup.com/");
//
//		// Testing Logging into Trifolia Staging environment with Lantana Admin account
//		   // this.LoginFunctions.LCGAdminLogin("lcg.admin","https://trifolia-staging.lantanagroup.com");
//

// ========================================================================================================================================================================================================
//                                                   CDA TESTING
// ========================================================================================================================================================================================================
		// Testing Logging into Trifolia Dev environment and create the Lantana Admin account
		   this.LoginFunctions.LCGAdminLogin("lcg.admin", "http://dev.trifolia.lantanagroup.com/");
	
		// Testing Logging into Trifolia Staging environment with Lantana Admin account
		   //this.LoginFunctions.LCGAdminLogin("lcg.admin","https://trifolia-staging.lantanagroup.com");

		// *** CDA Implementation Guide Tests ***
		// ------------------------------------

		// Test Browsing the HAI R9 Implementation Guide 
		        this.IGFunctions.BrowseImplementationGuide("Healthcare Associated Infection Reports Release 9",
				"15-19677", "The value of the id", "No audit entries",
				"hai_voc.xml", "Published : Tuesday, June 18, 2013", "document",
				"[1..1]", "Phase", "Internal Analysts, Group", "HTML Content",
				"Notify", "lcg.admin");

		// Test Adding Permissions to the "IHE PCC"
		this.IGFunctions.PermissionImplementationGuide("Test IHE PCC", "CDA","lcg.admin", "Admin");

		// Test Creating the "1Automation Test IG"
		 this.IGFunctions.CreateImplementationGuide("1Automation Test IG","automation.test@lcg.org","LCG", "1Automation Test IG Display Name", "Automation Test Web IG","Automation Test Web IG for testing", "CDA", "lcg.admin");

		// Test Editing the "1Automation Test IG"
		 this.IGFunctions.EditImplementationGuide("1Automation Test IG","lcg.admin",
						"<sch:rule context=\"cda:ClinicalDocument\"> <sch:assert test=\"count(cda:templateId[@root='1.2.3.4.5.6.7.8.9.10'])=1\">This document SHALL conform to template \"Automation Document Template\" (templateId: 1.2.3.4.5.6.7.8.9.10).</sch:assert> </sch:rule>");

		 // Test WebViewing the Healthcare Associated Infection Reports Release 2 DSTU 1 V8 IG
		 this.IGFunctions.WebViewImplementationGuide("http://trifolia-staging.lantanagroup.com/", 
		 "Healthcare Associated Infection Reports Release 2 DSTU 1 V8",
		 "NHSN Healthcare Associated", 
		 "Structure of This Guide", 
		 "Antimicrobial Resistance Option (ARO) Summary Report (V2)",
		 "Administrative Gender (HL7 V3)", "SNOMED CT", "lcg.admin");

		 
         // ** Entry Template Tests**		   
         // _______________________________
		  
			// Test Creating the ""Automation Entry Observation Template"
		       this.TemplateFunctions.CreateEntryObsTemplate("Automation Entry Observation Template", "urn:oid:1.2.3.4.5.6.7", "1Automation Test IG", "CDA: entry", "Test Entry Template for Regression Testing", "lcg.admin");	

			// Test Creating Computed Constraints for the "Automation Entry Observation Template" 
		       this.ConstraintFunctions.AddEntryObsCompConstraints("Automation Entry Observation Template", "urn:oid:1.2.3.4.5.6.7", "1Automation Test IG","CDA: Entry", "lcg.admin");

         // ** Organizer Template Tests **		   
         // _______________________________

			// Test Creating the ""Automation Entry Organizer Template"
		       this.TemplateFunctions.CreateEntryOrgTemplate("Automation Entry Organizer Template", "urn:oid:1.2.3.4.5.6.7.8","1Automation Test IG", "CDA: Organizer", "Test Organizer Template for Regression Testing", "lcg.admin");	

			// Test Creating Computed Constraints for the "Automation Entry Organizer Template"
		       this.ConstraintFunctions.AddEntryOrgCompConstraints("Automation Entry Organizer Template", "urn:oid:1.2.3.4.5.6.7.8",  "1Automation Test IG", "CDA: Organizer", "lcg.admin");

         // ** Section Template Tests **		   
         //  _______________________________		   
		   	   
			// Test Creating the ""Automation Section Template"	   
		       this.TemplateFunctions.CreateSectionTemplate("Automation Section Template", "urn:oid:1.2.3.4.5.6.7.8.9", "1Automation Test IG", "CDA: section", "Test Section Template for Regression Testing", "lcg.admin");	
	
			// Test Creating Computed Constraints for the "Automation Section Template"
		       //this.ConstraintFunctions.AddSectionCompConstraints("Automation Section Template", "urn:oid:1.2.3.4.5.6.7","CDA: Section", "lcg.admin");

         // ** Document Template Tests **		   
         // _______________________________			   
		 
		// Test Viewing the "Acuity Score Data Section" Template
			this.TemplateFunctions.ViewTemplate("Acuity score data section", "urn:oid:2.16.840.1.113883.10.20.17.2.3","Neonatal Care Report Release 1", "Patient data section - NCR (urn:oid:2.16.840.1.113883.10.20.17.2.5)","urn:oid:2.16.840.1.113883.10.20.17.2.5",
			"This section contains the templates that represent data elements whose variants are thought to be significant indicators of the severity of illness and of subsequent infant outcome", "", "lcg.admin");
			   
		// Test Creating the ""Automation Document Template"
		   this.TemplateFunctions.CreateDocumentTemplate("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "1Automation Test IG", "CDA: document", "Test Document Template for Regression Testing", "lcg.admin");		
	   
		// Test Editing the ""Automation Document Template"
		   this.TemplateFunctions.EditTemplate("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "Allergies - Test", "Add Notes to the Automation Document Template", "lcg.admin");

		// Test Copying the "Automation Document Template"
		   this.TemplateFunctions.CopyTemplate("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "1Automation Test IG", "Automation Document Template Copy", "lcg.admin");

		// Test Moving the "Automation Document Template Copy"
		   this.TemplateFunctions.MoveTemplate("Automation Document Template Copy",  "urn:hl7ii:1.2.3.4.5.6.7.8.9.10", "1Automation Test IG", "document", "entry", "lcg.admin");

		// Test Deleting the "Automation Document Template Copy" 
		   this.TemplateFunctions.DeleteTemplate("Automation Document Template Copy", "urn:hl7ii:1.2.3.4.5.6.7.8.9.10", "1Automation Test IG", "lcg.admin");
		   		   
         // ** Document Constraint Tests **		   
         // _______________________________	

		// Test Creating Computed Constraints for the "Automation Document Template"
		   this.ConstraintFunctions.AddDocCompConstraints("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "1Automation Test IG", "CDA: Document", "Other", "1.2.3.4.5.6.7.8", "POCD_HD000040", "lcg.admin");

		// Test Adding and Editing the "id" element with a Primitive Constraint containing schematron
		   this.ConstraintFunctions.AddEditPrimConstraint("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "1Automation Test IG","CDA: Document","This id SHALL be a globally unique identifier for the document", 
		   "STATIC", "Engineer", "not(tested)", "SHOULD", "lcg.admin");

		// Test Binding Value Sets to the "Automation Document Template" constraints
		   this.ConstraintFunctions.DocumentValueSetBinding("Automation Document Template","urn:oid:1.2.3.4.5.6.7.8.9.10", "1Automation Test IG","CDA: Document", "2.16.840.1.113883.3.2898.11.22","Cause of Injury", "lcg.admin");

		// Test Binding Templates to the "Automation Document Template" Section constraint
		   this.ConstraintFunctions.DocumentTemplateBinding("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "1Automation Test IG", "CDA: Document", "urn:oid:1.3.6.1.4.1.19376.1.4.1.6.4.99.6", "ACCF Family History", "lcg.admin");
		
		// Test Previewing and Validating Constraints within the "Automation Document Template"
		   this.ConstraintFunctions.PreviewAndValidateConstraint(
						"Automation Document Template",	"urn:oid:1.2.3.4.5.6.7.8.9.10","1Automation Test IG", "CDA: Document",
						"SHALL contain exactly one [1..1] realmCode=\"US\"",
						"SHALL contain exactly one [1..1] @root=\"1.2.3.4.5.6.7.8\"",
						"SHALL contain exactly one [1..1] @extension=\"POCD_HD000040\"",
						"SHALL contain exactly one [1..1] code (ValueSet: Cause of Injury urn:oid:2.16.840.1.113883.3.2898.11.22)",
						"SHALL contain exactly one [1..1] ACCF Family History (identifier: urn:oid:1.3.6.1.4.1.19376.1.4.1.6.4.99.6)",
						"There are no validation errors/warnings.", "lcg.admin");

		//  Document Publishing Testing 
		// ----------------------------------

		// Test Creating Sample XML for the "Automation Document Template"
		this.PublishingFunctions.CreateTemplateXMLSample("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "1. SHALL contain exactly one [1..1] realmCode= \"US\" (CONF:2234-3).", "lcg.admin");

		// Test Editing the Sample XML for the "Automation Document Template"
		this.PublishingFunctions.EditTemplateXMLSample("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "lcg.admin");

		// Test Deleting the Template Sample for the "Automation Document Template"
		this.PublishingFunctions.DeleteTemplateXMLSample("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10","lcg.admin");

		// Test Balloting and Publishing the "Automation Test Implementation Guide"
		this.PublishingFunctions.BallotAndPublishIG("1Automation Test IG","lcg.admin", "Ballot");

		// Test Versioning the "Automation Test Implementation Guide"
		   this.PublishingFunctions.VersionIG("1Automation Test IG", "1Automation Test IG V2", "lcg.admin");

		// Test Versioning the "Automation Document Template"
		   this.PublishingFunctions.VersionTemplate("Automation Document Template",	"urn:oid:1.2.3.4.5.6.7.8.9.10", "Automation Document Template V2", "Adding Notes to Versioned Template", "lcg.admin");

		// Test Deleting the "1Automation Test IG V2" 
		   this.IGFunctions.DeleteVersionedImplementationGuide("1Automation Test IG V2", "CDA", "lcg.admin");
		   
		// Test Deleting the "1Automation Test IG"
		   this.IGFunctions.DeleteImplementationGuide("1Automation Test IG", "CDA", "lcg.admin");
		   
		// Terminology Tests
		// ------------------------------

		// Test Browsing the "CA Realm Header languageCode" Value Set
		   this.TerminologyFunctions.BrowseValueSet("LanguageCode", "urn:oid:2.16.840.1.113883.2.20.3.190", "fra-CA", "lcg.admin");

		// Test Importing the VSAC " Encounter Planned Value Set"
		   this.TerminologyFunctions.ImportValueSet("Enounter Planned Value Set", "2.16.840.1.113883.11.20.9.52", "TEST_AdministrativeGender",
				"Successfully imported value set!", "VSAC", "lcg.admin");
   
		// Test Creating the "Automation Test Value Set"
		   this.TerminologyFunctions.CreateValueSet("Automation Test Gender Value Set", "urn:oid:2.2.2.2.2.2.2.2", "TEST_AdministrativeGender",
				"Administrative Gender based upon TEST vocabulary. This value set contains only male, female and undifferentiated concepts",
			    "http://www.lantanagroup.com.com", "lcg.admin");

		// Test Editing the "Automation Test Value Set"
		   this.TerminologyFunctions.EditValueSet("Automation Test Gender Value Set", "urn:oid:2.2.2.2.2.2.2.2", "F", "Female", "AdministrativeGender", "Active", "M", "Male",
				"AdministrativeGender", "Active", "UN", "Undifferentiated", "AdministrativeGender", "Active", "lcg.admin");

		// Test Deleting the "Automation Test Value Set"
		   this.TerminologyFunctions.DeleteValueSet("Automation Test Gender Value Set", "urn:oid:2.2.2.2.2.2.2.2", "lcg.admin");

		// Test Browsing the "FIPS 5-2 (State)" Code System
		   this.TerminologyFunctions.BrowseCodeSystem("FIPS 5-2 (State)","urn:oid:2.16.840.1.113883.6.92", "lcg.admin");

		// Test Creating the "Automation Test Vaccine Code System"
		   this.TerminologyFunctions.CreateCodeSystem("Automation Test Vaccine Code System", "https://www2a.cdc.gov/vaccines/iis/iisstandards/vaccines.asp?rpt=vg", "Also known as CVX codes.");

		// Test Editing the "Automation Test Vaccine Code System"
		   this.TerminologyFunctions.EditCodeSystem("Automation Test Vaccine Code System",	"https://www2a.cdc.gov/vaccines/iis/iisstandards/vaccines.asp?rpt=vg", "Used by the CDC", "lcg.admin");

		// Test Deleting the "Automation Test Vaccine Code System"
		   this.TerminologyFunctions.DeleteCodeSystem("Automation Test Vaccine Code System","https://www2a.cdc.gov/vaccines/iis/iisstandards/vaccines.asp?rpt=vg", "lcg.admin");

		// *** EXPORT TESTS ***
		// ----------------------------

		// Test Exporting the Templates for the "QRDA Category III" Implementation Guide
		   this.ExportFunctions.ExportTemplateToWord("QRDA Category III", "","lcg.admin");

		// Test Exporting the Template XML for the "National Health Care Surveys" Implementation Guide
		    this.ExportFunctions.ExportTemplateXML("ACCF DEMO","", "lcg.admin");

		// Test Exporting the Schematron for the "Cath/PCI Registry Reporting Implementation Guide" Implementation  Guide
		    this.ExportFunctions.ExportSchematron("Cath/PCI Registry Reporting Implementation Guide","lcg.admin");

		// Test Exporting the Vocabulary for the "Consolidation V2" Implementation Guide
		    this.ExportFunctions.ExportVocabulary("ASCO-BCR Sample Report Release 1", "lcg.admin");

		// Test Exporting the XML for the "C-CDA on FHIR" Implementation Guide
		    this.ExportFunctions.ExportFHIRXML("C-CDA on FHIR", "lcg.admin");

		// *** CLOSING ***
		// ---------------------

		// Lantana Administrator Logout
		this.LoginFunctions.LCGLogout("lcg.admin");
		
	////==============================================================================================================================================================================================================
//      // *** FHIR TESTING ***
////==============================================================================================================================================================================================================		   

	// Testing Logging into Trifolia Dev environment and create the Lantana Admin account
	   this.LoginFunctions.LCGAdminLogin("lcg.admin", "http://dev.trifolia.lantanagroup.com/");   
	   
	// Testing Logging into Trifolia Staging environment with Lantana Admin account
	   //this.LoginFunctions.LCGAdminLogin("lcg.admin","https://trifolia-staging.lantanagroup.com");
	   
	// ** Implementation Guide Tests **
	// ------------------------------------
	
	// Test Browsing the C-CDA on FHIR Implementation Guide
	   this.IGFunctions.BrowseImplementationGuide("C-CDA on FHIR","2219-148", "No primitives", "No audit entries",
		"allergy-intolerance.xml", "C-CDA on FHIR", "Extension","[0..1]", "Pattern ID", "", "HTML Content", "Notify", "lcg.admin");		   
	
	// Test Creating the "1FHIR Test IG"
	   this.IGFunctions.CreateImplementationGuide("1FHIR Test IG", "fhir.test@lcg.org","LCG","1FHIR Test IG Display Name", "FHIR Test Web IG", "FHIR Test Web IG for testing", "FHIR STU3", "lcg.admin");
	
	// ** FHIR Profile Tests**		   
	//  _______________________________
	
	// Test Viewing the "C-CDA on FHIR Care Plan" Profile
	    this.TemplateFunctions.ViewTemplate("C-CDA on FHIR Care Plan", "http://hl7.org/fhir/ccda/StructureDefinition/CCDA_on_FHIR_Care_Plan", "C-CDA on FHIR", "FHIR",
	    "C-CDA on FHIR US Realm Header", "urn:oid:2.16.840.1.113883.10.20.17.2.5", "A Care Plan (including Home Health Plan of Care (HHPoC)) is a consensus-driven dynamic plan", "lcg.admin");	
	
	// Test Creating the "FHIR Test Care Plan Profile"
	    this.TemplateFunctions.CreateEntryObsTemplate("FHIR Test Care Plan", "http://hl7.org/fhir/ccda/StructureDefinition/FHIR_Test_Care_Plan", "1FHIR Test IG", "Composition", "FHIR Test Profile for Regression Testing", "lcg.admin");	
	
	// Test Creating Computed Constraints for the "FHIR Test Care Plan Profile"
	   this.ConstraintFunctions.AddFHIRCompConstraints("FHIR Test Care Plan", "http://hl7.org/fhir/ccda/StructureDefinition/FHIR_Test_Care_Plan",  "1FHIR Test IG", "Composition", "lcg.admin");
	
	// Test Adding Text Only Section to the "FHIR Test Care Plan" Profile
	   this.ConstraintFunctions.AddFHIRSectionConstraint("FHIR Test Care Plan", "http://hl7.org/fhir/ccda/StructureDefinition/FHIR_Test_Care_Plan", "1FHIR Test IG", "Composition", "lcg.admin");

	// Test Copying the "FHIR Test Care Plan" Profile
	   this.TemplateFunctions.CopyTemplate("FHIR Test Care Plan", "http://hl7.org/fhir/ccda/StructureDefinition/FHIR_Test_Care_Plan",  "1FHIR Test IG","FHIR Test Care Plan Copy", "lcg.admin");

	// Test Moving the "FHIR Test Care Plan Copy" Profile to an Observation Template Type
	   this.TemplateFunctions.MoveTemplate("FHIR Test Care Plan Copy", "http://hl7.org/fhir/ccda/StructureDefinition/FHIR_Test_Care_Plan_Copy",  "1FHIR Test IG", "Composition", "Account", "lcg.admin");
	
	// Test Deleting the "FHIR Test Care Plan Copy" Profile 
	   this.TemplateFunctions.DeleteTemplate("FHIR Test Care Plan Copy", "http://hl7.org/fhir/ccda/StructureDefinition/FHIR_Test_Care_Plan_Copy", "1FHIR Test IG", "lcg.admin");
		
	// Test Deleting the "1Automation Test IG V2" 
	   this.IGFunctions.DeleteImplementationGuide("1FHIR Test IG", "FHIR STU3", "lcg.admin");
	   
	// Lantana Administrator Logout
	   this.LoginFunctions.LCGLogout("lcg.admin");
			
//=======================================================================================================================================================================================================
// PART III - LANTANA USER TESTS
//
//  ========================================================================================================================================================================================================

		// Testing Logging into Trifolia Dev as Lantana User
		   // this.LoginFunctions.LCGUserLogin("lcg.user","http://dev.trifolia.lantanagroup.com/");

		//  Testing Logging into Trifolia Staging as Lantana User
		    //this.LoginFunctions.LCGUserLogin("lcg.user","http://trifolia-staging.lantanagroup.com/");

		// *** IMPLEMENTATION GUIDE TESTS ***
		// ----------------------------------

		// Test Browsing the "ASCO Example Document R1" Implementation Guide
		this.IGFunctions.BrowseImplementationGuide("ASCO-BCR Sample Report Release 1", "", "","", "", "ASCO-BCR Sample Report Release 1", "document", "[0..1]", "Pattern ID","", "", "", "lcg.user");

		// *** TEMPLATE TESTS ***
		// ------------------------------

		// Test Viewing the "ASCO Example Document R1" Template
		this.TemplateFunctions.ViewTemplate("ASCO Example Document R1",	"urn:oid:2.16.840.1.113883.3.117.1.5.3", "ASCO-BCR Sample Report Release 1", "CDA", "", "", "SHALL contain exactly one [1..1]", "lcg.user");

		// *** TERMINOLOGY TESTS ***
		// ------------------------------

		// Test Browsing the "Family Member Value Set" Value Set
		this.TerminologyFunctions.BrowseValueSet("Family Member Value Set","urn:oid:2.16.840.1.113883.1.11.19579", "FAMMEMB", "lcg.user");

		// Test Browsing the "FIPS 5-2 (State)" Code System
		this.TerminologyFunctions.BrowseCodeSystem("EntityNameUse", "urn:oid:2.16.840.1.113883.5.45", "lcg.user");

		// *** EXPORT TESTS ***
		// ----------------------------

		// Test Exporting the Templates for the "C-CDA on FHIR" Implementation Guide
		this.ExportFunctions.ExportTemplateToWord("ASCO-BCR Sample Report Release 1", "", "lcg.user");

		// Test Exporting the Template XML for the "National Health Care Surveys V2" Implementation Guide
		this.ExportFunctions.ExportTemplateXML("National Health Care Surveys", "", "lcg.user");

		// *** CLOSING ***
		// -------------------

		// Lantana User Logout
		   this.LoginFunctions.LCGLogout("lcg.user");
		   
		   
}

	// ========================================================================================================================================================================================================
	// CLOSING
	// ========================================================================================================================================================================================================

	@After
	public void tearDown()  {
		try {
			driver.quit();
			String verificationErrorString = verificationErrors.toString();
			if (!"".equals(verificationErrorString)) {
				fail(verificationErrorString);
			}
		} catch (Exception ex) {
			
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