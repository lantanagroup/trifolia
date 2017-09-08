package trifolia.pkg;


import java.util.regex.Pattern;
import java.util.concurrent.TimeUnit;

import org.junit.*;
import org.junit.rules.TemporaryFolder;

import static org.junit.Assert.*;

import org.openqa.selenium.*;
import org.openqa.selenium.firefox.FirefoxDriver;
import org.openqa.selenium.firefox.FirefoxProfile;
import org.openqa.selenium.firefox.internal.ProfilesIni;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.Select;
import org.openqa.selenium.support.ui.WebDriverWait;

import java.io.IOException;
import java.nio.file.*;

@Ignore
  public class H_ExportFunctions<File> {
  private WebDriver driver;
  private String baseUrl;
  private String myElementList;
  private boolean acceptNextAlert = true;
  private StringBuffer verificationErrors = new StringBuffer();
  public String File;
  public TemporaryFolder tempFolder = new TemporaryFolder();
  public java.io.File downloadFolder = null;
  
  public H_ExportFunctions() throws IOException {
	  this.tempFolder.create();
	  this.downloadFolder = this.tempFolder.newFolder("Trifolia");
  }
  
  public void setDriver(WebDriver driver){
	  this.driver = driver;
  }
  
  @Before
  public void initialize(){
	    ProfilesIni allProfiles = new ProfilesIni();
		FirefoxProfile profile = new FirefoxProfile();
		driver = new FirefoxDriver(profile);
		profile.setPreference("browser.download.manager.showWhenStarting", false);		
		profile.setPreference("browser.helperapps.alwaysAsk.force", false);	
//		profile.setPreference("browser,helperapps.neverAsk.SaveToDisk", "application/msword, application/xml");
//	    profile.setPreference("browser,helperapps.neverAsk.openFile", "application/msword, application/xml");
	    profile.setPreference("browser,helperapps.neverAsk.SaveToDisk", "*.*");
	    profile.setPreference("browser,helperapps.neverAsk.openFile", "*.*");
	    profile.setPreference("browser.download.folderList", 2);
	    profile.setPreference("browser.download.useDownloadDir", false);
	    profile.setPreference("browser.download.downloadDir",  this.downloadFolder.getPath());
		profile.setPreference("browser.download.dir", this.downloadFolder.getPath());

		if(this.driver == null){
	 		  //initialize a driver since one has not been provided already
		  this.driver = new FirefoxDriver(profile);
	  }
  }

// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
//                           PART I - EXPORT TEMPLATES to MS. WORD
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

  public void waitForPageLoad() 
  {
	WebDriverWait wait = new WebDriverWait(driver, 60);
	     wait.until(ExpectedConditions.jsReturnsValue("return document.readyState ==\"complete\";"));		
  }
  
  public void waitForBindings(String waitForBinding) 
  {
        JavascriptExecutor js = (JavascriptExecutor)driver;	
	  	WebDriverWait wait = new WebDriverWait(driver, 60);
	  	wait.until(ExpectedConditions.jsReturnsValue("return !!ko.dataFor(document.getElementById('"+waitForBinding+"'))"));  
  }
  public void waitForAngluarBindings(String waitForBinding) 
  {
        JavascriptExecutor js = (JavascriptExecutor)driver;	
	  	WebDriverWait wait = new WebDriverWait(driver, 60);
	  	wait.until(ExpectedConditions.jsReturnsValue("return !!$('"+waitForBinding+"').scope();"));  
  }
  public void OpenTerminologyBrowser()
  {
	  // Wait for the page to fully load
 	     waitForPageLoad();
 	  
		// Confirm Welcome Message is present
		WebDriverWait wait = new WebDriverWait(driver, 60);
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/h2"),"Welcome to Trifolia Workbench!"));
		assertTrue("Unable to confirm Login",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Welcome to Trifolia Workbench![\\s\\S][\\s\\S]*$"));

	    //Open the Export to Terminology Browser        
		driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[3]/a")).click();

	     // Wait for the page to fully load
	 	    waitForPageLoad();
	 	  
	 	 // Wait for the bindings to complete
	 	    waitForAngluarBindings("#BrowseTerminology");
	    
	 	// Confirm the Export page appears
	 	  	
		    WebDriverWait wait2 = new WebDriverWait(driver, 60);
			wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/h2"),"Export"));
			assertTrue("Export form does not appear",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Export[\\s\\S][\\s\\S]*$"));

	 	  	WebDriverWait wait1 = new WebDriverWait(driver, 60);
	 	    WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/form/div[2]/div/div/button")));
	 	    assertTrue("Could not find \"Select an implementation guide\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Select an implementation guide[\\s\\S][\\s\\S]*$"));
	    
	     // Wait for the page to fully re-load
	 	  waitForPageLoad();
  } 
  
  public void FindImplementationGuide(String implementationGuideName) throws Exception {
		
		// Wait for the page to fully load
		  waitForPageLoad();
		  
		  // Wait for the bindings to complete
		    waitForBindings("BrowseImplementationGuides");
		    
		// Confirm the Search options are available
			 WebDriverWait wait = new WebDriverWait(driver, 60);
		 	 WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[1]/div/div[2]/ul/li[2]/a")));
		 	    
	    // Search for the Implementation Guide
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/input")).sendKeys(implementationGuideName);
	    
	      // Wait for the page to fully load
	 	  waitForPageLoad();
	 	  
//	    //Confirm the search is complete
//	    WebDriverWait wait3 = new WebDriverWait(driver, 60);                    
//	    wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[2]/td[4]")));    
	    
	    //Confirm the correct IG is found
	    WebDriverWait wait4 = new WebDriverWait(driver, 120);                    
	    wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[1]"), implementationGuideName));
	    assertTrue("Could not find \"Template Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(implementationGuideName) >= 0);
	 }
  
//  public boolean isFileDownloaded(String downloadPath, String filename)
//  {
//	  boolean flag = false;
//	  Object dir;
//	File[] dir_contents = dir.listFiles();
//	  
//	return flag=true;
//  }
  
  public void ReturnHome(String welcomeMessage) throws Exception {
	  
	   // Return to the Trifolia Home Page
	    driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[1]/a")).click();
	    WebDriverWait wait = new WebDriverWait(driver, 60);  
	    WebElement element = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("appnav")));
	    
	    // Wait for page to fully load
		   waitForPageLoad();
		   
	   // Wait for Bindings to complete
	      waitForBindings("appnav");
	     
	    //Confirm the Welcome Message appears
		WebDriverWait wait0 = new WebDriverWait(driver, 60);                    
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/h2"), welcomeMessage));
		assertTrue("Could not find \"Value Set OID\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(welcomeMessage) >= 0);  

}

//TEST 1:  Complete the Export for Templates to MS. Word
  @Test 
  public void CompleteMSWordExport()throws Exception{
	        
			  // Wait for the page to fully load
			     waitForPageLoad();
      
			  // Wait for the bindings to complete
			     waitForBindings("ExportMSWord");
	    
			  // Confirm the Export Templates To Word form appears correctly
		  	  WebDriverWait wait5 = new WebDriverWait(driver, 60);
		      WebElement element5 = wait5.until(ExpectedConditions.visibilityOfElementLocated(By.id("ExportMSWord")));
		      assertTrue("Could not find \"Export Template to Word\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Export Templates/Profiles to MS Word[\\s\\S][\\s\\S]*$"));
			  
		      // Select the Export Options
			     // Sort Order
		      WebDriverWait wait = new WebDriverWait(driver, 60);
			  WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div[1]/select")));
			  driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div[1]/select")).click();
			  driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div[1]/select")).sendKeys(Keys.ARROW_DOWN);
			  driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div[1]/select")).click();
			  driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div[1]/select")).sendKeys(Keys.TAB);
			
			    // Document Tables - Select "List"
			  WebDriverWait wait1 = new WebDriverWait(driver, 60);
			  WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div[2]/select")));
			  driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div[2]/select")).click();
			  driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			  driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			  driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div[2]/select")).click();
			  driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.TAB);

			    // Template Tables - Select "Context"
			  WebDriverWait wait2 = new WebDriverWait(driver, 60);
			  WebElement element2 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div[3]/select")));
			  driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div[3]/select")).click();
			  driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div[3]/select")).sendKeys(Keys.ARROW_DOWN);
			  driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div[3]/select")).click();
			  driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div[3]/select")).sendKeys(Keys.TAB);
			  
		    // Notes - Select "Include"
			  
		      if (driver.findElement(By.cssSelector("tbody")).getText().matches("^[\\s\\S]*Notes[\\s\\S]*$"))
		         {
		    	  // Click the option to Add Notes to the Export
		    	  WebDriverWait wait3 = new WebDriverWait(driver, 60);
				  WebElement element3 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div[7]/input")));
		    	  driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div[7]/input")).click();
		         }                             

    	  //Export and Download the File to the Downloads folder
		    driver.manage().timeouts().pageLoadTimeout(120, TimeUnit.SECONDS);
			driver.findElement(By.xpath("/html/body/div[2]/div/form/div/button")).click();
			                             
		// Wait for the page to fully load
		   waitForPageLoad();
			  
	     // Wait for Export to Complete
	       WebDriverWait wait4 = new WebDriverWait(driver, 60);
	       WebElement element4 = wait2.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/form/div/button")));
	     
	     // Return to the Trifolia Home Page
	        ReturnHome("Welcome to Trifolia Workbench!");    
	     
  }
  
  @Test 
  public void CompleteHL7LicenceAgreement()throws Exception{	
	  
    // get current window handle
	String parentHandle = driver.getWindowHandle(); 
	for (String winHandle : driver.getWindowHandles()) { 
       // switch focus of WebDriver to HL7 QCommerce login page
      driver.switchTo().window(winHandle);
      Thread.sleep(1000);
      driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*SPECIFICATION LICENSE AGREEMENT [\\s\\S]*$");
      
      // Enter Required Fields in the License Agreement
      
      // User Name
      driver.findElement(By.xpath("//*[@id=\"license_agreement_block\"]/form/p[1]/input[4]")).clear();
      driver.findElement(By.xpath("//*[@id=\"license_agreement_block\"]/form/p[1]/input[4]")).sendKeys("Trifolia HL7 Tester");
      // Company Name
      driver.findElement(By.xpath("//*[@id=\"license_agreement_block\"]/form/p[1]/input[5]")).clear();
      driver.findElement(By.xpath("//*[@id=\"license_agreement_block\"]/form/p[1]/input[5]")).sendKeys("Lantana Consulting Group");
      // State
      driver.findElement(By.xpath("//*[@id=\"license_agreement_block\"]/form/p[1]/textarea")).clear();
      driver.findElement(By.xpath("//*[@id=\"license_agreement_block\"]/form/p[1]/textarea")).sendKeys("Vermont");
      // Phone
      driver.findElement(By.xpath("//*[@id=\"license_agreement_block\"]/form/p[1]/input[7]")).clear();
      driver.findElement(By.xpath("//*[@id=\"license_agreement_block\"]/form/p[1]/input[7]")).sendKeys("(714) 232-8679");
      // E-mail
      driver.findElement(By.xpath("//*[@id=\"license_agreement_block\"]/form/p[1]/input[8]")).clear();
      driver.findElement(By.xpath("//*[@id=\"license_agreement_block\"]/form/p[1]/input[8]")).sendKeys("hl7.user@trifoliatesting.com");
      // Name
      driver.findElement(By.xpath("/html/body/div/div/form/p[1]/span[1]/input[9]")).click();
      driver.findElement(By.xpath("/html/body/div/div/form/p[1]/span[1]/input[9]")).sendKeys("Lantana Trifolia Testing");
      // Purpose
      driver.findElement(By.xpath("/html/body/div/div/form/p[1]/span[2]/input[6]")).click();
      driver.findElement(By.xpath("/html/body/div/div/form/p[1]/span[2]/input[7]")).sendKeys("Lantana Trifolia Testing - Export functions");
      
      // Desired Products and Services
      driver.findElement(By.xpath("//*[@id=\"compliance_DesiredProductsServices_6\"]")).click();
	  // driver.findElement(By.xpath("//*[@id=\"compliance_DesiredProductsServices_6\"]")).click();  
             
      Thread.sleep(500);
      driver.findElement(By.xpath("//*[@id=\"license_agreement_block\"]/form/p[1]/span[2]/input[7]")).clear();
      driver.findElement(By.xpath("//*[@id=\"license_agreement_block\"]/form/p[1]/span[2]/input[7]")).sendKeys("None - Lantana HL7 Trifolia Testing");
      Thread.sleep(500);
      driver.findElement(By.xpath("//*[@id=\"license_agreement_block\"]/form/div[1]/input")).click();
      }
}
  
  // Test Exporting Templates to Word
  @Test
    public void ExportTemplateToWord(String implementationGuideName, String baseURL, String permissionUserName) throws Exception {  
    
	  OpenTerminologyBrowser();

	  if (permissionUserName == "lcg.admin" ) 
      {
		  FindImplementationGuide("QRDA Category III");
      }
     if (permissionUserName == "lcg.user" )
      {
	   FindImplementationGuide("ASCO-BCR Sample Report Release 1");
      }
	  	   
  	//launch the Export Options form
  	    if (permissionUserName == "lcg.admin" ) 
           {
  		      driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[1]/td[5]/div/button")).click();
           }
        if (permissionUserName == "lcg.user" )
           {
    	       driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[1]/td[4]/div/button")).click();
           }
        
   
  	// Wait for the page to fully load
	     waitForPageLoad();

	  // Wait for the bindings to complete
	     waitForBindings("ExportMSWord");
	     
  	 // Confirm the Export Templates to Word form appears
	  	WebDriverWait wait = new WebDriverWait(driver, 60);                   
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/form/div/h2"), "Export Templates/Profiles to MS Word"));
  	  
  	if ((driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Export Templates/Profiles to MS Word[\\s\\S][\\s\\S]*$")))
  	   {
  		     CompleteMSWordExport();   
  	   }
           
  }
  		
  	
    
// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
//                           PART II - EXPORT TEMPLATE XML
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
  
//  Export the Template XML for an Implementation Guide
  @Test
    public void ExportTemplateXML(String implementationGuideName, String baseURL, String permissionUserName) throws Exception {
    
	  // Wait for the page to fully load
	    waitForPageLoad();

       // Wait for Bindings to complete
      waitForBindings("appnav");
	      
	      //Open the Export Browser and find the IG
	  	driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[3]/a/b")).click();
	  	driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[3]/ul/li[2]/a")).click();

	 // Wait for the page to fully load
	    waitForPageLoad();
	
	    // Wait for the bindings to complete
	    waitForBindings("BrowseImplementationGuides");
    
	  // Confirm the Export Templates XML form appears
 	 WebDriverWait wait = new WebDriverWait(driver, 60);                                    
     WebElement element = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/h2")));
     assertTrue("Could not find \"Export XML\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Export XML[\\s\\S][\\s\\S]*$"));
	
    // Clear the Existing Search Criteria
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/button")).click();
    
    // Enter the new Search Criteria
  	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/input")).sendKeys(implementationGuideName);

  	// Confirm the correct IG is returned
	WebDriverWait wait1 = new WebDriverWait(driver, 60);                   
	wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[1]/td[1]"), implementationGuideName));
  	assertTrue("Could not find \"Implementation Guide Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(implementationGuideName) >= 0);
  	
  	//Launch the Export Options form
  	 if (permissionUserName == "lcg.admin") 
     {
  		driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[1]/td[5]/div/button")).click();
     }
  	 
     if (permissionUserName == "lcg.user")
     {
         driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[4]/div/button")).click();
     }
     
  	 if (permissionUserName == "hl7.member")
  	 {
  		  driver.findElement(By.xpath("//*[@id=\"BrowseImplementationGuides\"]/table/tbody/tr/td[4]/div/button")).click();
	 }
  	 if (permissionUserName == "hl7.user")
  	 {
  		  driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[1]/td[4]/div/button")).click();
	 }
 	 
		  // Wait for the page to fully load
		     waitForPageLoad();

		  // Wait for the bindings to complete
		     waitForBindings("ExportXML");
  	 
		  // Confirm the Export Templates XML form appears
		  	 WebDriverWait wait2 = new WebDriverWait(driver, 60);
		     WebElement element2 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("ExportXML")));
		     assertTrue("Could not find \"Export XML\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Export XML[\\s\\S][\\s\\S]*$"));
		    
         //Export and Download the File to the Downloads folder
		    WebDriverWait wait3 = new WebDriverWait(driver, 60);
		    WebElement element3 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/form/div/div[2]/button[1]")));
		    driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[2]/button[1]")).click();
		    // Thread.sleep(10000);
		 
		 // Confirm the file is downloaded
		    java.io.File[] downloadedFiles = this.downloadFolder.listFiles();
		    
		    int waitCount = 0;
		    while (downloadedFiles.length == 0) {
			    downloadedFiles = this.downloadFolder.listFiles();
			    Thread.sleep(500);
			    waitCount++;
			    
			    if (waitCount > 20) {
			    	throw new Exception("Waited too long to download the export");
			    }
		    }
		   
		 // Confirm the user is returned to the form   
			WebDriverWait wait4 = new WebDriverWait(driver, 60);                   
			wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div/label"), "Type"));

		    // Return to the Trifolia Home Page
		     ReturnHome("Welcome to Trifolia Workbench");    
					     
  }
// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
//                           PART III - EXPORT IG SCHEMATRON
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------


//Export the Schematron for an Implementation Guide
  @Test
    public void ExportSchematron(String implementationGuideName, String permissionUserName) throws Exception {
	     
   // Wait for the page to fully load
      waitForPageLoad();
	  
	// Wait for Bindings to complete
      waitForBindings("appnav");
      
	// Open the Export Browser and find the IG
	driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[3]/a")).click();
	driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[3]/ul/li[3]/a")).click();
	
	// Wait for the page to fully load
       waitForPageLoad();

   // Wait for the bindings to complete
      waitForBindings("BrowseImplementationGuides");

	// Confirm the Export Schematron form appears
	WebDriverWait wait = new WebDriverWait(driver, 60);
    WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/p/a")));
    assertTrue("Could not find \"Export Schematron\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Export Schematron[\\s\\S][\\s\\S]*$"));
	
	// Clear the Existing Search Criteria
	    WebDriverWait wait1 = new WebDriverWait(driver, 60);
	    WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/button")));
       driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/button")).click();
    
	 // Wait for the page to fully load
	    waitForPageLoad();
   
    // Enter the new Search Criteria
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/input")).sendKeys(implementationGuideName);
	
	// Wait for the page to fully load
       waitForPageLoad();
    
	// Confirm the correct IG appears
	WebDriverWait wait21 = new WebDriverWait(driver, 60);                   
	wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[1]"), implementationGuideName));
  	assertTrue("Could not find \"Implementation Guide Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(implementationGuideName) >= 0);
  	
	// Launch the Export Options form
	 if (permissionUserName == "lcg.admin") 
	     {
			 driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[5]/div/button")).click();
	     }
	 
     if (permissionUserName == "lcg.user")
	     {
	         driver.findElement(By.xpath("//*[@id=\"BrowseImplementationGuides\"]/table/tbody/tr/td[4]/div/button")).click();
	     }
     
  	 if (permissionUserName == "hl7.member")
	    {
  		      driver.findElement(By.xpath("//*[@id=\"BrowseImplementationGuides\"]/table/tbody/tr/td[4]/div/button")).click();
  	    }
  	 
     // If in Trifolia Staging, then Accept Alert
//     if (permissionUserName == "hl7.member" ||permissionUserName == "hl7.user" )
//     {
//     	  //Confirm the Alert appears
//      	   WebDriverWait wait0 = new WebDriverWait(driver, 60);
//      	   wait0.until(ExpectedConditions.alertIsPresent());
//
//     	// Switch the driver context to the "" alert
//			  Alert alertDialog1 = driver.switchTo().alert();
//			  // Get the alert text
//			  Thread.sleep(1000);
//			  String alertText1 = alertDialog1.getText();
//			  Thread.sleep(1000);
//			  alertDialog1.accept();
//			  Thread.sleep(1000);
//     }

	  // Wait for the page to fully load
	     waitForPageLoad();
	
	 // Wait for the bindings to complete
	    waitForBindings("ExportSchematronDiv");
    
	// Confirm the Export Schematron form appears
	 WebDriverWait wait2 = new WebDriverWait(driver, 60);
     WebElement element2 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("ExportSchematronDiv")));
	 assertTrue("Could not find \"Export Schematron\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Export Schematron[\\s\\S][\\s\\S]*$"));
	
	 // Wait for form to load
	 WebDriverWait wait3 = new WebDriverWait(driver, 60);
     WebElement element3 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div[4]/input")));
	 
	 // Change the Default Schematron value
	 driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div[5]/input")).clear();
	 driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div[5]/input")).sendKeys("not(tested)");
	 driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div[5]/input")).sendKeys(Keys.TAB);
	 
	 // Export and Download the File to the Downloads folder
	 WebDriverWait wait4 = new WebDriverWait(driver, 60);
	 WebElement element4 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/form/div/div[2]/button[1]")));
     driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[2]/button[1]")).click();
    
     // Return to the Trifolia Home Page
     ReturnHome("Welcome to Trifolia Workbench");    	
	
    }

//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
//                           PART IV - EXPORT VOCABULARY
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

//Export Vocabulary 
  @Test
    public void ExportVocabulary(String implementationGuideName, String permissionUserName) throws Exception {
	
	// Wait for the page to fully load
       waitForPageLoad();
	  
	// Wait for Bindings to complete
       waitForBindings("appnav");
      
	   //Open the Export Browser and find the IG
		driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[3]/a")).click();
		driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[3]/ul/li[4]/a")).click();

		// Wait for the page to fully load
	       waitForPageLoad();

	   // Wait for the bindings to complete
	      waitForBindings("BrowseImplementationGuides");
	      
		// Confirm the Export Vocabulary form appears
		WebDriverWait wait = new WebDriverWait(driver, 60);
	    WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/p/a")));
	    assertTrue("Could not find \"Export Schematron\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Export Vocabulary[\\s\\S][\\s\\S]*$"));
		
		// Clear the Existing Search Criteria
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/button")).click();
	    
	    // Enter the new Search Criteria
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/input")).sendKeys(implementationGuideName);

		// Confirm the correct IG is returned
		WebDriverWait wait1 = new WebDriverWait(driver, 60);                   
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[1]"), implementationGuideName));
		assertTrue("Could not find \"Implementation Guide Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(implementationGuideName) >= 0);
		 
	//Launch the Export Vocabulary Options form
	if (permissionUserName == "lcg.admin") 
	    {
			 driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[5]/div/button")).click();
	    }
	
	if (permissionUserName == "lcg.user")
	    {
	         driver.findElement(By.xpath("//*[@id=\"BrowseImplementationGuides\"]/table/tbody/tr/td[4]/div/button")).click();
	    }
	
	if (permissionUserName == "hl7.member"|| permissionUserName == "hl7.user")
	    {
			    driver.findElement(By.xpath("//*[@id=\"BrowseImplementationGuides\"]/table/tbody/tr/td[4]/div/button")).click();
	    }
	
	   // If in Trifolia Staging,  Accept Alert
//    if (permissionUserName == "hl7.member" ||permissionUserName == "hl7.user" )
//    {
//    	  //Confirm the Alert appears
//     	   WebDriverWait wait0 = new WebDriverWait(driver, 60);
//     	   wait0.until(ExpectedConditions.alertIsPresent());
//
//    	// Switch the driver context to the "" alert
//			  Alert alertDialog1 = driver.switchTo().alert();
//			  // Get the alert text
//			  Thread.sleep(1000);
//			  String alertText1 = alertDialog1.getText();
//			  Thread.sleep(1000);
//			  alertDialog1.accept();
//			  Thread.sleep(1000);
//    }

		// Wait for the page to fully load
	       waitForPageLoad();
	
	     // Wait for the bindings to complete
	        waitForBindings("ExportVocabulary");
	        
		//Confirm the Export Vocabulary form appears
		WebDriverWait wait2 = new WebDriverWait(driver, 60);
	    WebElement element2 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("ExportVocabulary")));
		assertTrue("Could not find \"Export Vocabulary\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Export Vocabulary[\\s\\S][\\s\\S]*$"));
		
		//Change the Export Settings
		driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div[2]/div/input")).sendKeys("20");
		driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div[2]/div/input")).sendKeys(Keys.TAB);
	
		//Export and Download the File to the Downloads folder
	    driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[2]/button[1]")).click();
	
	    // Confirm the export is complete
	    WebDriverWait wait3 = new WebDriverWait(driver, 60);
	    WebElement element3 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("//*[@id=\"ExportButton\"]")));

	    // Return to the Trifolia Home Page
	     ReturnHome("Welcome to Trifolia Workbench");    	
    }
  
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
//                                                          PART V - EXPORT FHIR XML
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

//Export the FHIR XML for the "CDA on FHIR" Implementation Guide
@Test
public void ExportFHIRXML(String implementationGuideName, String permissionUserName) throws Exception 
{
	// Wait for the page to fully load
       waitForPageLoad();
	  
	// Wait for Bindings to complete
       waitForBindings("appnav");
    
		//Open the Export Browser and find the IG
		driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[3]/a")).click();
		driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[3]/ul/li[2]/a")).click();
		
		// Wait for the page to fully load
	       waitForPageLoad();

	   // Wait for the bindings to complete
	      waitForBindings("BrowseImplementationGuides");
	      
		// Confirm the Export XML page appears
	  	WebDriverWait wait = new WebDriverWait(driver, 60);
	    WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/p/a")));
	    assertTrue("Could not find \"Export Template XML\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Export XML[\\s\\S][\\s\\S]*$"));
		
		// Clear the Existing Search Criteria
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/button")).click();
		
		// Wait for the page to fully load
	       waitForPageLoad();
      
		// Enter the new Search Criteria
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/input")).sendKeys(implementationGuideName);
		
		// Wait for the page to fully load
	       waitForPageLoad();
	       
		// Confirm the correct IG appears  
			WebDriverWait wait4 = new WebDriverWait(driver, 60);                   
			wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[1]/td[1]"), implementationGuideName));
			assertTrue("Could not find \"Implementation Guide Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(implementationGuideName) >= 0);
			
			//Launch the ExportXML Options form
		if (permissionUserName == "lcg.admin") 
		{
		driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[5]/div/button")).click();
		
		}
		
		if (permissionUserName == "lcg.user")
		{
		driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[4]/div/button")).click();
		}
		
		if (permissionUserName == "hl7.member"|| permissionUserName == "hl7.user")
		{	
		driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[4]/div/button")).click();
		}

		   //  Accept Alert
	     if (permissionUserName == "hl7.member" ||permissionUserName == "hl7.user" )
	     {
	     	  //Confirm the Alert appears
	      	   WebDriverWait wait0 = new WebDriverWait(driver, 60);
	      	   wait0.until(ExpectedConditions.alertIsPresent());

	     	// Switch the driver context to the "" alert
				  Alert alertDialog1 = driver.switchTo().alert();
				  // Get the alert text
				  Thread.sleep(1000);
				  String alertText1 = alertDialog1.getText();
				  Thread.sleep(1000);
				  alertDialog1.accept();
				  Thread.sleep(1000);
	     }

	 	// Wait for the page to fully load
	       waitForPageLoad();

	   // Wait for the bindings to complete
	      waitForBindings("ExportXML");
	      
		//Confirm the Export FHIR XML form appears
		WebDriverWait wait1 = new WebDriverWait(driver, 60);
		WebElement element1 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div[2]/div/form/div/h2")));
		assertTrue("Could not find \"Export FHIR XML\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Export XML/JSON[\\s\\S][\\s\\S]*$"));
		
	// Change the Export Settings
		driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div/select")).click();
		WebDriverWait wait2 = new WebDriverWait(driver, 60);
		WebElement element2 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div[2]/div/form/div/h2")));
		driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div/select")).sendKeys(Keys.ARROW_DOWN);
		driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div/select")).click();
		driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div/select")).sendKeys(Keys.TAB);
	
		// Select the Export Option
		WebDriverWait wait3 = new WebDriverWait(driver, 60);
		WebElement element3 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/form/div/div[2]/button[1]")));
		driver.findElement(By.xpath("/html/body/div[2]/div/form/div/div[2]/button[1]")).click();
		
		// Confirm the user is returned to the form   
		WebDriverWait wait5 = new WebDriverWait(driver, 60);                   
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/form/div/div[1]/div[1]/div/label"), "Type"));

		 // Return to the Trifolia Home Page
	    ReturnHome("Welcome to Trifolia Workbench");    
    
	}
}
