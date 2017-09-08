package trifolia.pkg;

import static org.junit.Assert.assertTrue;
import static org.junit.Assert.fail;

import java.util.concurrent.TimeUnit;

import org.junit.After;
import org.junit.Before;
import org.junit.Ignore;
import org.junit.Test;
import org.openqa.selenium.Alert;
import org.openqa.selenium.By;
import org.openqa.selenium.JavascriptExecutor;
import org.openqa.selenium.Keys;
import org.openqa.selenium.NoAlertPresentException;
import org.openqa.selenium.NoSuchElementException;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.firefox.FirefoxDriver;
import org.openqa.selenium.firefox.FirefoxProfile;
import org.openqa.selenium.firefox.internal.ProfilesIni;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.WebDriverWait;

@Ignore
public class F_PublishingFunctions {
	private WebDriver driver;
	private String baseUrl;
	private boolean acceptNextAlert = true;
	private StringBuffer verificationErrors = new StringBuffer();
	public F_PublishingFunctions() {}
	
	  public void setDriver(WebDriver driver){
		  this.driver = driver;
	  }
	  
	  @Before
	  public void initialize(){
		  ProfilesIni allProfiles = new ProfilesIni();
		  FirefoxProfile profile = allProfiles.getProfile("default");
		  if(this.driver == null){
			  //initialize a driver since one has not been provided already
			  this.driver = new FirefoxDriver(profile);
		  }
	  }

// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
//                               PART I - CREATE, EDIT and DELETE TEMPLATE XML SAMPLE
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
  public void OpenTemplateBrowser() throws Exception 
	  {	  
		  // Wait for page to fully load
		     waitForPageLoad();
		     
	     // Open the Template Browser
		   WebDriverWait wait = new WebDriverWait(driver, 60);                               
	       WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[1]/div/div[2]/ul/li[2]/a"))); 
		   driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[2]/a")).click();
		   WebDriverWait wait2 = new WebDriverWait(driver, 60);
		   WebElement element2 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[1]/div/div[2]/ul/li[2]/ul/li[2]/a"))); 
		   driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[2]/ul/li[2]/a")).click();

		  // Confirm page completely loads
		  waitForPageLoad();
		  
		  // Wait for Bindings to complete
		     waitForBindings("BrowseTemplates");
		// Confirm Template Browser opens
		 WebDriverWait wait3 = new WebDriverWait(driver, 60);
		 WebElement element3 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("BrowseTemplates")));
		 assertTrue("Could not find \"Browse Templates\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Browse Templates[\\s\\S]*$"));
	 
		 // Confirm Correct Page Title appears
		 WebDriverWait wait4 = new WebDriverWait(driver, 60);                    
		 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/h3"), "Browse Templates/Profiles"));
}
	  
public void ClearExistingSearch() throws Exception
	  {
		  // Wait for the page to fully load
		  waitForPageLoad();
		  
		  // Wait for Bindings to complete
		     waitForBindings("BrowseTemplates");
		     
		  // Confirm the Template Listing appears
		  WebDriverWait wait = new WebDriverWait(driver, 60);                    
		  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[2]"), "Identifier"));
		
	 	  // Clear existing Search Criteria 
	  	   //driver.manage().timeouts().pageLoadTimeout(30, TimeUnit.SECONDS);
	  	   driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")).click();
	  	   driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")).click();
		 
	  		// Wait for page to fully load
	 	       waitForPageLoad();  
	 	  
		   // Clear existing Search Filters
		  driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[2]/td[6]/button")).click();
		  driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[2]/td[6]/button")).click();
		 
			// Wait for page to fully load
		      waitForPageLoad();  
		  
		   // Confirm search criteria is cleared
		  WebDriverWait wait1 = new WebDriverWait(driver, 60);                    
		  wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[2]/td[5]")));        
	  }
 public void FindTemplate(String templateName, String templateOID, String templateIG, String permissionUserName) throws Exception 
	{	 
	  // Wait for page to fully load
		  waitForPageLoad();
		  
		  // Wait for Bindings to complete
		     waitForBindings("BrowseTemplates");
		  
		  // Clear the existing search criteria
		      ClearExistingSearch();
		      
		  	// Wait for page to fully load
			  waitForPageLoad();  
		      
	  // Confirm the search parameters are cleared
	     
	     if (permissionUserName == "lcg.admin")
	    {
	     WebDriverWait wait = new WebDriverWait(driver, 60);
		 wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[4]/td[6]/div/a[1]")));
	    }   
	     if (permissionUserName == "hl7.member")
	    {
	     WebDriverWait wait = new WebDriverWait(driver, 60);
		 wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[4]/td[5]/div/a[1]")));
	    }   
	     
	   //Search for the Template 
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")).click();
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/input")).sendKeys(templateName);
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/span/button[2]")).click();
	  
		// Wait for page to fully load
		  waitForPageLoad();  
		  
	   // Confirm the search is complete
	   WebDriverWait wait2 = new WebDriverWait(driver, 60);
	   wait2.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]"),"Page 1 of 1, 1 templates/profiles"));
	   
	   // Confirm the correct Template is returned
	    
	    WebDriverWait wait3 = new WebDriverWait(driver, 60);
	  	wait3.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[2]"), templateOID));
	    assertTrue("Could not find \"Template OID\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateOID) >= 0);
	 }	
 
 public void ConfirmTemplateViewer(String templateName, String templateOID) throws Exception 
	{	
	 
			 // Wait for page to fully load
	            waitForPageLoad();  
	         
	      // Wait for Bindings to complete
		     waitForBindings("ViewTemplate");
	         
			 WebDriverWait wait = new WebDriverWait(driver, 5);		
			 wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[8]"))); 
	 
		  // Confirm the Template Viewer appears 
		     WebDriverWait wait1 = new WebDriverWait(driver, 60);
			 WebElement element1 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("ViewTemplate")));
			    
		  // Confirm the form is available.
			 WebDriverWait wait2 = new WebDriverWait(driver, 60);                    
		     wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/ul/li[1]/a"), "Constraints"));
			 	   
		     WebDriverWait wait3 = new WebDriverWait(driver, 60);                    
		     wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/a"), templateName));
		     assertTrue("Could not find \"Template Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateName) >= 0);
   }
 
 public void ConfirmTemplateEditor(String templateName, String templateOID) throws Exception 
	{
	  
		// Wait for page to fully load
		   waitForPageLoad();
		  
		// Wait for Bindings to complete
		   waitForBindings("TemplateEditor");
		  
	   // Confirm the Template Editor opens
	       //driver.manage().timeouts().pageLoadTimeout(30, TimeUnit.SECONDS);
		   WebDriverWait wait = new WebDriverWait(driver, 60);
		   wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[1]/a"), "Template/Profile"));
	       assertTrue("Could not find \"Template/Profile\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Template/Profile[\\s\\S]*$"));
		   
		   // Wait for page to fully load
		   WebDriverWait wait1 = new WebDriverWait(driver, 60);		
		   wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[5]"))); 
	    
		   // Confirm the the correct Template Name appears in the Editor
		   //driver.manage().timeouts().pageLoadTimeout(30, TimeUnit.SECONDS);
		   WebDriverWait wait2 = new WebDriverWait(driver, 60);
		   wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/h3/span[2]"), templateName));
		   assertTrue("Could not find \"Template Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateName) >= 0);
		   
		   // Confirm the the correct Template OID appears in the Editor
		   //driver.manage().timeouts().pageLoadTimeout(30, TimeUnit.SECONDS);
		   WebDriverWait wait3 = new WebDriverWait(driver, 60);                                  
		   wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/h4/span"), templateOID));
		   assertTrue("Could not find \"Template OID\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateOID) >= 0);	
	}	
 public void SaveTemplateSample(String templateName, String templateOID) throws Exception 
	{
	 
	// Wait for page to fully load
	  waitForPageLoad();
	  
	// Wait for Bindings to complete
	   waitForBindings("mainBody");
	   
   // Confirm the OK option is enabled, and click OK for the Template Sample
	WebDriverWait wait = new WebDriverWait(driver, 60);
	WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[5]/div/div/div[3]/button[1]"))); 	
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[5]/div/div/div[3]/button[1]")).click();
  
  // Confirm the user is returned to the Edit Publish Settings page
     
     WebDriverWait wait1 = new WebDriverWait(driver, 60);
     WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[2]/a"))); 	
     assertTrue(driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Edit Publish Settings[\\s\\S]*$"));

  // Wait for page to fully load
  	  waitForPageLoad(); 
  	  
  //Confirm the template sample appears in the Edit Publish Settings page
     
     WebDriverWait wait2 = new WebDriverWait(driver, 60);
	 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/div[1]/div[2]/div[1]"), templateName));
	 assertTrue("Could not find \"Template Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateName) >= 0);             
}
 
 public void SavePublishSettings(String templateName, String templateSample) throws Exception 
	{		
	 // Wait for page to fully load
	    waitForPageLoad(); 
	    
	 // Wait for Bindings to complete
		waitForBindings("mainBody");
	  
	  //Confirm the Save option is available and Save the Publish Settings form
        WebDriverWait wait = new WebDriverWait(driver, 60);
        WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[2]/button[1]"))); 	
        driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/button[1]")).click();
 
	  //Confirm the Alert appears
	   WebDriverWait wait0 = new WebDriverWait(driver, 60);
	   wait0.until(ExpectedConditions.alertIsPresent());
	   
	    // 3.1 Switch the driver context to the "Successfully saved publish settings" alert
	    Alert alertDialog = driver.switchTo().alert();
	    // 3.2 Get the alert text
	    String alertText = alertDialog.getText();
	    // 3.3 Click the OK button on the alert.
	    alertDialog.accept();
	    // assertTrue(driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Successfully saved publish settings[\\s\\S]*$"));

		// Wait for page to fully load
		  waitForPageLoad();  
		  
	   // Confirm the Page is re-loaded
	    
	    WebDriverWait wait1 = new WebDriverWait(driver, 60);
	    WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[2]/button[1]"))); 
   }	
 
 public void ReturnHome(String welcomeMessage) throws Exception {
	  
		// Wait for page to fully load
	       waitForPageLoad(); 
	       
	    // Wait for Bindings to complete
			waitForBindings("appnav");
	  
	   // Return to the Trifolia Home Page
	    driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[1]/a")).click();
	    WebDriverWait wait4 = new WebDriverWait(driver, 60);
	    WebElement element4 = wait4.until(ExpectedConditions.visibilityOfElementLocated(By.id("appnav")));
	    
		// Wait for page to fully load
		  waitForPageLoad();  
		  
	    //Confirm the Welcome Message appears
		WebDriverWait wait = new WebDriverWait(driver, 60);
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/h2"), welcomeMessage));
		assertTrue("Could not find \"Welcome Message\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(welcomeMessage) >= 0);   	
}
 
//TEST 1: Create Sample XML for the Template
@Test
   public void CreateTemplateXMLSample(String templateName, String templateOID, String constraint1, String permissionUserName) throws Exception {
   
	// Open the Template Browser
	   OpenTemplateBrowser();
	   
	   // Find the Template
	 	if (permissionUserName == "lcg.admin")
	 		{
	 		FindTemplate("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "1Automation Test IG", "lcg.admin");
	 		}
	 	else if (permissionUserName == "hl7.member")
		 	{
	 		FindTemplate("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2", "1HL7 Member Test IG", "hl7.member");
		 	}
	  	 	
	      // Open the Template Viewer
	 	  if (permissionUserName == "lcg.admin") 
	 	    {
	 	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[6]/div/a[1]")).click();
	 	    }
	 	   
	 	    else if (permissionUserName == "hl7.member")
	 	    {                                
	 	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[5]/div/a[1]")).click();
	 	    }
	 	  	  
	 	// Wait for page to fully load
		   waitForPageLoad();  
		  
	 	 // Confirm the Template Viewer opens and the correct template appears
		 	 if (permissionUserName == "lcg.admin")
		 		{
		 		ConfirmTemplateViewer("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10");
		 		}
		 	 if (permissionUserName == "hl7.member")
			 	{
		 		ConfirmTemplateViewer("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2");
			 	}
		
	 	  
      // Launch the Public Settings form
 		driver.findElement(By.xpath("//*[@id=\"bs-example-navbar-collapse-1\"]/ul/li[3]/a")).click();

	 	// Wait for page to fully load
		   waitForPageLoad(); 
		   
		// Wait for Bindings to complete
		   waitForBindings("mainBody");
		  
      // Confirm the 'Edit Publish Settings' form opens
		  WebDriverWait wait = new WebDriverWait(driver, 60);
		  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/btn/span[1]"), templateName));
		  assertTrue(driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Edit Publish Settings[\\s\\S]*$"));
	     
       // Create a new Template Sample
	      driver.findElement(By.xpath("//*[@id=\"templateSamples\"]/div/div[2]/div/button")).click();
	  
	   // Wait for page to fully load
	       waitForPageLoad();  
	       
	   //Confirm the 'Edit Template Sample' form opens   
	      WebDriverWait wait1= new WebDriverWait(driver, 60);
		  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[5]/div/div/div[1]/h4"), "Edit Template/Profile Sample"));
		  assertTrue("Could not find \"Edit Template Sample\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Edit Template/Profile Sample[\\s\\S]*$"));
		  
	   //Generate the Template Sample
	   	   // Add the name
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[5]/div/div/div[2]/div[1]/input")).sendKeys(templateName);
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[5]/div/div/div[2]/div[1]/input")).sendKeys(Keys.RETURN);
		   
		   // Generate the Sample
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[5]/div/div/div[2]/div[2]/textarea")).click();
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[5]/div/div/div[3]/div/button[1]")).click();

		   //Format and Indent the Template Sample
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[5]/div/div/div[3]/div/button[2]")).click();
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[5]/div/div/div[3]/div/button[2]")).click();
	   
		// Save the Template Sample form
		   if (permissionUserName == "lcg.admin")
	 		{
	 		SaveTemplateSample("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10");
	 		}
	 	   if (permissionUserName == "hl7.member")
		 	{
	 		SaveTemplateSample("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2");
		 	}  
	 	   
	 	// Save the Publish Settings Form
		   if (permissionUserName == "lcg.admin")
	 		{
	 		SavePublishSettings("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10");
	 		}
	 	   if (permissionUserName == "hl7.member")
		 	{
	 		SavePublishSettings("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2");
		 	}  
		 
	 	// Open the Template Browser
		   OpenTemplateBrowser();
		 
		   // Find the Template
		 	if (permissionUserName == "lcg.admin")
		 		{
		 		FindTemplate("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "1Automation Test IG", "lcg.admin");
		 		}
		 	else if (permissionUserName == "hl7.member")
			 	{
		 		FindTemplate("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2", "1HL7 Member Test IG", "hl7.member");
			 	}
		 	  
		 	// Open the Template Viewer
		 	  if (permissionUserName == "lcg.admin") 
		 	    {
		 	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[6]/div/a[1]")).click();
		 	    }
		 	   
		 	    else if (permissionUserName == "hl7.member")
		 	    {                                
		 	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[5]/div/a[1]")).click();
		 	    }
		   
       // Confirm the Template Viewer opens and the correct template appears
		 	 
	 	 if (permissionUserName == "lcg.admin")
	 		{
	 		ConfirmTemplateViewer("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10");
	 		}
	 	 if (permissionUserName == "hl7.member")
		 	{
	 		ConfirmTemplateViewer("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2");
		 	}
	 	 
		// Confirm the Samples Tab appears and click to open
		   WebDriverWait wait4 = new WebDriverWait(driver, 60);
		   WebElement element4 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/ul/li[2]/a"))); 	
	       driver.findElement(By.xpath("/html/body/div[2]/div/div/ul/li[2]/a")).click();
	
	    // Wait for page to fully load
	       waitForPageLoad();  
	  
	     // Confirm the correct template appears
	     WebDriverWait wait5 = new WebDriverWait(driver, 60);
		 WebElement element5 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("ViewTemplate")));
	     assertTrue("Could not find \"Template Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateName) >= 0);
	     
	     // Return to the Trifolia Home Page
	     ReturnHome("Welcome to Trifolia Workbench");     
}
     
//TEST 2: Edit the Sample XML for the Template
 @Test
     public void EditTemplateXMLSample(String templateName, String templateOID, String permissionUserName) throws Exception {
	 
	    // Open the Template Browser
	       OpenTemplateBrowser();
	   
	    // Find the Template
	 	if (permissionUserName == "lcg.admin")
	 		{
	 		FindTemplate("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "1Automation Test IG", "lcg.admin");
	 		}
	 	
	 	else if (permissionUserName == "hl7.member")
		 	{
	 		FindTemplate("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2", "1HL7 Member Test IG", "hl7.member");
		 	}
		
	 	  // Open the Template Viewer
	 	  if (permissionUserName == "lcg.admin") 
	 	    {
	 	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[6]/div/a[1]")).click();
	 	    }
	 	   
	 	    else if (permissionUserName == "hl7.member")
	 	    {                                
	 	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[5]/div/a[1]")).click();
	 	    }
	 	   
		    
	 	 // Confirm the Template Viewer opens and the correct template appears
		 	 if (permissionUserName == "lcg.admin")
		 		{
		 		ConfirmTemplateViewer("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10");
		 		}
		 	 if (permissionUserName == "hl7.member")
			 	{
		 		ConfirmTemplateViewer("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2");
			 	}
		 	 
	 	  // Confirm the correct template appears in the Template Viewer  
	     WebDriverWait wait = new WebDriverWait(driver, 60);                    
	     wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[2]/div/h4[1]"), templateOID));

       //Open the Publish Settings form
		 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[3]/a")).click();

		// Wait for page to fully load
	       waitForPageLoad();  
	       
	    // Wait for Bindings to complete
		   waitForBindings("mainBody");
	  
        //Confirm the 'Edit Publish Settings' form opens and the correct template appears
	    WebDriverWait wait1 = new WebDriverWait(driver, 60);
	    wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/btn/span[1]"), templateName));
	    assertTrue(driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Edit Publish Settings[\\s\\S]*$"));
	    
       //Select and Edit the Template Sample 
       driver.findElement(By.xpath("//*[@id=\"templateSamples\"]/div[2]/div[2]/div/div/button[1]")).click();
    
	    // Wait for page to fully load
	       waitForPageLoad();  
  
       //Confirm the 'Edit Template Sample' form opens   
	      WebDriverWait wait2 = new WebDriverWait(driver, 60);
		  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[5]/div/div/div[1]/h4"), "Edit Template/Profile Sample"));
		  assertTrue("Could not find \"Edit Template Sample\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Edit Template/Profile Sample[\\s\\S]*$"));
		  
	   //Append the word 'Sample' to the Template Sample Name
       driver.findElement(By.xpath("//*[@id=\"templateSampleEditorDialog\"]/div/div/div[2]/div[1]/input")).sendKeys(" Sample");
       driver.findElement(By.xpath("//*[@id=\"templateSampleEditorDialog\"]/div/div/div[2]/div[1]/input")).sendKeys(Keys.TAB);
       
    // Save the Template Sample form
	   if (permissionUserName == "lcg.admin")
 		{
 		SaveTemplateSample("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10");
 		}
 	   if (permissionUserName == "hl7.member")
	 	{
 		SaveTemplateSample("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2");
	 	}  
 	   
 	// Save the Publish Settings Form
	   if (permissionUserName == "lcg.admin")
 		{
 		SavePublishSettings("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10");
 		}
 	   if (permissionUserName == "hl7.member")
	 	{
 		SavePublishSettings("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2");
	 	}  
 	   
		 	// Open the Template Browser
			   OpenTemplateBrowser();
		   
	       // Find the Template
		 	if (permissionUserName == "lcg.admin")
		 		{
		 		FindTemplate("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "1Automation Test IG", "lcg.admin");
		 		}
		 	else if (permissionUserName == "hl7.member")
			 	{
		 		FindTemplate("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2", "1HL7 Member Test IG", "hl7.member");
			 	}
		 	  
		 	// Open the Template Viewer
		 	  if (permissionUserName == "lcg.admin") 
		 	    {
		 	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[6]/div/a[1]")).click();
		 	    }
		 	   
		 	    else if (permissionUserName == "hl7.member")
		 	    {                                
		 	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[5]/div/a[1]")).click();
		 	    }
	
	       // Confirm the Template Viewer opens and the correct template appears
		 	 if (permissionUserName == "lcg.admin")
		 		{
		 		ConfirmTemplateViewer("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10");
		 		}
		 	 if (permissionUserName == "hl7.member")
			 	{
		 		ConfirmTemplateViewer("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2");
			 	} 	    
	
		 	// Wait for page to fully load
		       waitForPageLoad();  
		  
		   // Confirm the Samples Tab appears and click to open
		   WebDriverWait wait4 = new WebDriverWait(driver, 60);
		   WebElement element4 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/ul/li[2]/a"))); 	
	       driver.findElement(By.xpath("/html/body/div[2]/div/div/ul/li[2]/a")).click();
	
	    // Wait for page to fully load
	       waitForPageLoad();  
	  
	    // Confirm the correct template appears
	       WebDriverWait wait5 = new WebDriverWait(driver, 60);
		   WebElement element5 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("ViewTemplate")));
	       assertTrue("Could not find \"Template Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateName) >= 0);
	    
	    // Return to Trifolia Home Page
	       ReturnHome("Welcome to Trifolia Workbench!");
 }
       
//TEST 3: Delete the Template Sample for the "Automation Document Template"
 @Test
       public void DeleteTemplateXMLSample(String templateName, String templateOID, String permissionUserName) throws Exception {
	 
	// Open the Template Browser
	   OpenTemplateBrowser();
	 
	 // Find the Template
	 	if (permissionUserName == "lcg.admin")
	 		{
	 		FindTemplate("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "1Automation Test IG", "lcg.admin");
	 		}
	 	else if (permissionUserName == "hl7.member")
		 	{
	 		FindTemplate("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2", "1HL7 Member Test IG", "hl7.member");
		 	}
	 
	 	// Open the Template Viewer
	 	  if (permissionUserName == "lcg.admin") 
	 	    {
	 	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[6]/div/a[1]")).click();
	 	    }
	 	   
	 	    else if (permissionUserName == "hl7.member")
	 	    {                                
	 	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[5]/div/a[1]")).click();
	 	    }
	 	 	   
	 	 // Confirm the Template Viewer opens and the correct template appears
		 	 if (permissionUserName == "lcg.admin")
		 		{
		 		ConfirmTemplateViewer("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10");
		 		}
		 	 if (permissionUserName == "hl7.member")
			 	{
		 		ConfirmTemplateViewer("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2");
			 	}
		 	 
	      // Open the Publish Settings Form
	    	 driver.findElement(By.xpath("//*[@id=\"bs-example-navbar-collapse-1\"]/ul/li[3]/a")).click();

    	// Wait for page to fully load
	       waitForPageLoad();  
	       
	    // Wait for Bindings to complete
		   waitForBindings("mainBody");
	       
	       //Confirm the 'Edit Publish Settings' form opens and the correct template appears
		    WebDriverWait wait = new WebDriverWait(driver, 60);
		    wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/btn/span[1]"), templateName));
		    assertTrue(driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Edit Publish Settings[\\s\\S]*$"));
		  
	       //Select and Delete the Template Sample 
	         driver.findElement(By.xpath("//*[@id=\"templateSamples\"]/div[2]/div[2]/div/div/button[2]")).click();

	       //Confirm the Alert appears
		   WebDriverWait wait20 = new WebDriverWait(driver, 60);
		   wait.until(ExpectedConditions.alertIsPresent());
	  
	       // 3.1 Switch the driver context to the "Are you sure you want to delete this template sample" alert
	       Alert alertDialog2 = driver.switchTo().alert();
	       // 3.2 Get the alert text
	       String alertText2 = alertDialog2.getText();
	       // 3.3 Click the OK button on the alert.
	       alertDialog2.accept();
	       // assertTrue(driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Successfully deleted template sample[\\s\\S]*$"));
	         
	       // Wait for page to fully load
	       waitForPageLoad();  
	       //Confirm the user is returned to the 'Edit Publish Settings' form 
	 	     WebDriverWait wait1 = new WebDriverWait(driver, 60);
	         WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("//*[@id=\"mainBody\"]/div[2]/button[1]"))); 	
	 	     assertTrue(driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Edit Publish Settings[\\s\\S]*$"));
	  	 	   
	 	// Save the Publish Settings Form
		   if (permissionUserName == "lcg.admin")
	 		{
	 		SavePublishSettings("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10");
	 		}
	 	   if (permissionUserName == "hl7.member")
		 	{
	 		SavePublishSettings("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2");
		 	}  
		       	
	 	  // Wait for page to fully load
	       waitForPageLoad();  
	       
	 	 // Return to the Trifolia Home Page
	 	    ReturnHome("Welcome to Trifolia Workbench");
	}
 
// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
//                     PART II - BALLOT, PUBLISH and VERSION IMPLEMENTATION GUIDE and TEMPLATE
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

 public void OpenIGBrowser()
 {
	    //Open the IG Browser 
		 WebDriverWait wait = new WebDriverWait(driver, 60);
	 	 WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[1]/div/div[2]/ul/li[2]/a")));
		 driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[2]/a")).click();
		 WebDriverWait wait2 = new WebDriverWait(driver, 60);
	     wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[1]/div/div[2]/ul/li[2]/ul/li[1]/a"),"Implementation Guides"));
		 driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[2]/ul/li[1]/a")).click();
		 
		 // Wait for page to fully load
		    waitForPageLoad();
		    
	     // Wait for the bindings to complete
		    waitForBindings("BrowseImplementationGuides");
	    
	    //Confirm the IG Browser appears
	    WebDriverWait wait3 = new WebDriverWait(driver, 60);
	    WebElement element3 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("BrowseImplementationGuides")));
	    assertTrue("Could not find \"Browse Implementation Guides\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Browse Implementation Guides[\\s\\S]*$"));
	       
	    // Clear existing Search Criteria
	       driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/button")).click();
	    
	    // Confirm existing Search Criteria is cleared
	       WebDriverWait wait4 = new WebDriverWait(driver, 60);                    
	       wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[2]/td[4]")));    
	    	
	       // Wait for page to fully load
		     waitForPageLoad(); 
		     
	       // Wait for the bindings to complete
	          waitForBindings("BrowseImplementationGuides");
 } 
 
 public void FindImplementationGuide(String implementationGuideName) throws Exception {
		
	  // Wait for page to fully load
	     waitForPageLoad();
	     
	  // Wait for the bindings to complete
	     waitForBindings("BrowseImplementationGuides");
	  
	// Confirm the Search options are available
		 WebDriverWait wait = new WebDriverWait(driver, 60);
	 	 WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[1]/div/div[2]/ul/li[2]/a")));
	 	    
   // Search for the Implementation Guide
      driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/input")).sendKeys(implementationGuideName);
   
      // Wait for page to fully load
	     waitForPageLoad();
	     
      // Wait for the bindings to complete
	      waitForBindings("BrowseImplementationGuides");
	  
   //Confirm the search is complete
     WebDriverWait wait3 = new WebDriverWait(driver, 60);                    
     wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[2]/td[4]")));    
   
   //Confirm the correct IG is found
     WebDriverWait wait4 = new WebDriverWait(driver, 120);                    
     wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[1]"), implementationGuideName));
     assertTrue("Could not find \"Implementation Guide Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(implementationGuideName) >= 0);
}
 
 public void ConfirmIGViewer(String implementationGuideName) throws Exception
 {
	  // Wait for page to fully load
	    waitForPageLoad();
	    
	  // Wait for the bindings to complete
	     waitForBindings("ViewImplementationGuide");
	
	 // Confirm the IG Viewer appears.
	  WebDriverWait wait = new WebDriverWait(driver, 60);                     
 	  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/ul/li[1]/a"), "Templates/Profiles"));
    
	// Confirm the correct IG appears in the IG Vewer.
       WebDriverWait wait4 = new WebDriverWait(driver, 60);                     
	   wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/a"), implementationGuideName));
      assertTrue("Could not find \"Implementation Guide Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(implementationGuideName) >= 0);
 }
//TEST 1: Ballot and Publish the Implementation Guide
 @Test
     public void BallotAndPublishIG(String implementationGuideName, String permissionUserName, String igStatus1) throws Exception {
 	
	 // Open the Implementation Guide Browser
	 
	    OpenIGBrowser();
     
     //Find the Implementation Guide
	    
	    FindImplementationGuide("1Automation Test IG");
	        
        //Open the IG Viewer
	     if (permissionUserName == "lcg.admin") 
	     {
	     	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[5]/div/button")).click();
	     }
	     else
	     {
	     	driver.findElement(By.xpath("//*[@id=\"BrowseImplementationGuides\"]/table/tbody/tr/td[4]/div/button")).click();
	     }
     		       
         //Confirm the IG Viewer opens with correct IG 
	       ConfirmIGViewer("1Automation Test IG");
	       
	     //Select the "Workflow" option 
	       WebDriverWait wait = new WebDriverWait(driver, 60); 
	       WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[4]/a")));
	       driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[4]/a")).click();
	       
	       // Click "Ballot" the IG
	       WebDriverWait wait1 = new WebDriverWait(driver, 60); 
	       WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[4]/ul/li[1]/a")));
	       driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[4]/ul/li[1]/a")).click();
          
	     // Wait for page to fully load
	       waitForPageLoad(); 
	       
	     // Wait for screen to refresh
        
        WebDriverWait wait2 = new WebDriverWait(driver, 60);                    
	    wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/a"), implementationGuideName));
	    assertTrue("Could not find \"Implementation Guide Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(implementationGuideName) >= 0);
     
	     // Confirm the status is set to "Ballot"
	     WebDriverWait wait3 = new WebDriverWait(driver, 60);                    
		 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[2]/div/span[3]/span"), igStatus1));
	     assertTrue("Could not find \"Implementation Guide Status\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Ballot[\\s\\S]*$"));
	     
	     //Select the "Workflow" option 
	       WebDriverWait wait4 = new WebDriverWait(driver, 60); 
	       WebElement element4 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[4]/a")));   
	       driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[4]/a")).click();
	     
	       // Click "Publish" the IG
	       WebDriverWait wait5 = new WebDriverWait(driver, 60); 
	       WebElement element5 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[4]/ul/li[2]/a")));   
	        driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[4]/ul/li[2]/a")).click();
	     
     // Wait for page to fully load
     	waitForPageLoad(); 
     	
       // Confirm the Publish Date Selector form opens    
     	
     	 WebDriverWait wait6 = new WebDriverWait(driver, 60);                    
 	     wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[4]/div/div/div[1]/h4"), "Select publish date"));
 	 
	     WebDriverWait wait7 = new WebDriverWait(driver, 60);
	     WebElement element7 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("publishDateSelector")));
	     assertTrue("Could not find \"Select publish date\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Select publish date[\\s\\S]*$"));
	     
       //Select the Publish Date
	     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[4]/div/div/div[2]/div/div[2]/button[2]")).click();
	     driver.findElement(By.xpath("/html/body/div[9]/div[1]/table/tfoot/tr[1]")).click();
	     
	     //Save the Date
	     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[4]/div/div/div[3]/button[1]")).click();
	     
       // Wait for page to fully load
          waitForPageLoad(); 
        
        // Wait for the bindings to complete
	       waitForBindings("ViewImplementationGuide");

     // Confirm the IG is Published
	     WebDriverWait wait8 = new WebDriverWait(driver, 60);                    
		 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[2]/div/span[3]/span[1]"), "Published"));
		      
     //Confirm the user is Returned to the IG Viewer
     WebDriverWait wait9 = new WebDriverWait(driver, 60);
     WebElement element9 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("//*[@id=\"ViewImplementationGuide\"]/ul/li[1]/a")));
     assertTrue("Could not find \"Implementation Guide Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(implementationGuideName) >= 0);
       
     //Confirm the IG Status has been updated
     // Wait for screen to refresh
     
     WebDriverWait wait10 = new WebDriverWait(driver, 60); 
     WebElement element10 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[3]/div[1]/div[1]/div[2]/button")));
     assertTrue("Could not find \"Implementation Guide Status\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Published :[\\s\\S]*$")); 
    
     // Return to the Trifolia Home Page
     ReturnHome("Welcome to Trifolia Workbench");
  }
         
//TEST 2: Version the Implementation Guide
 @Test
     public void VersionIG(String implementationGuideName, String implementationGuideVersioned, String permissionUserName) throws Exception {
 	 
	 // Open the IG Browser
	    OpenIGBrowser();
   
     //Find the Implementation Guide
	    FindImplementationGuide("1Automation Test IG");
 
       //Open the IG Viewer
     if (permissionUserName == "lcg.admin") 
     {
     	driver.findElement(By.xpath("//*[@id=\"BrowseImplementationGuides\"]/table/tbody/tr/td[5]/div/button")).click();
     }
     else
     {
     	driver.findElement(By.xpath("//*[@id=\"BrowseImplementationGuides\"]/table/tbody/tr/td[4]/div/button")).click();
     }
     
     // Wait for page to fully load
     waitForPageLoad(); 
     
     //Confirm the IG Viewer opens with correct IG 
       ConfirmIGViewer("1Automation Test IG");
     
     //Version the Implementation Guide and accept the confirmation
       driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[5]/a")).click();

     //Confirm the Alert appears
	   WebDriverWait wait10 = new WebDriverWait(driver, 60);
	   wait10.until(ExpectedConditions.alertIsPresent());

     //Switch the driver context to the "Are you sure you want to create a new version of this IG?" alert
     Alert alertDialog4 = driver.switchTo().alert();
     //Get the alert text
     String alertText4 = alertDialog4.getText();
     //Click the OK button on the alert.
     alertDialog4.accept();
     
     // Wait for page to fully load
     waitForPageLoad(); 
     
     // Confirm the user is returned to the IG Viewer
     WebDriverWait wait = new WebDriverWait(driver, 60);
     WebElement element = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("//*[@id=\"ViewImplementationGuide\"]/ul/li[1]/a")));
     
     //Confirm the correct IG appears in the IG Viewer
        WebDriverWait wait1 = new WebDriverWait(driver, 60);
	  	wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[2]/div/h2"), implementationGuideVersioned));
        assertTrue("Could not find \"Versioned Implementation Guide Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(implementationGuideVersioned) >= 0);
     
     //Open the IG Browser
       OpenIGBrowser();
       
     //Find the Versioned Implementation Guide
	    FindImplementationGuide("1Automation Test IG V2");
           
       //Confirm the versioned IG appears in the Browser
         assertTrue("Could not find \"Versioned Implementation Guide Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(implementationGuideVersioned) >= 0);
     
       // Return to the Trifolia Home Page
         ReturnHome("Welcome to Trifolia Workbench");     
         
 }
         
//TEST 3: Version the Template
 @Test
     public void VersionTemplate(String templateName, String templateOID, String versionedTemplate, String versionedTemplateNotes, String permissionUserName) throws Exception {
 	 
	// Open the Template Browser
	   OpenTemplateBrowser(); 
     
     // Find the Template
	 
	 	if (permissionUserName == "lcg.admin")
	 		{
	 		FindTemplate("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "1Automation Test IG", "lcg.admin");
	 		}
	 	else if (permissionUserName == "hl7.member")
		 	{
	 		FindTemplate("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2", "1HL7 Member Test IG", "hl7.member");
		 	}
  	    
	    //Open the Template Viewer
	    if (permissionUserName == "lcg.admin") 
	   {
	   	driver.findElement(By.xpath("//*[@id=\"BrowseTemplates\"]/table/tbody/tr/td[6]/div/a[1]")).click();
	   }
	    else
	   {
	   	driver.findElement(By.xpath("//*[@id=\"BrowseTemplates\"]/table/tbody/tr/td[5]/div/a[1]")).click();
	   }
           
	 
	    // Confirm the page is fully loaded
	    if (permissionUserName == "lcg.admin") 
	     {
		       WebDriverWait wait = new WebDriverWait(driver, 60);
			   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[9]/a")));
	     }
	     else
	     {
	    	   WebDriverWait wait = new WebDriverWait(driver, 60);
			   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[6]/a")));
	     }
	    
	    // Wait for page to fully load
	       waitForPageLoad(); 
	       
	       ConfirmTemplateViewer("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10");
	       
           //Open the Template Version Form
		     if (permissionUserName == "lcg.admin") 
		     {
		    	 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[8]/a")).click();
		     }
		     else
		     {
		     	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[6]/a")).click();
		     }
		     
		     // Wait for page to fully load
		       waitForPageLoad(); 
		       
		       // Wait for Bindings to complete
			     waitForBindings("CopyTemplate");
		     
       //Confirm the Version Step 1 form appears
		 //driver.manage().timeouts().pageLoadTimeout(30, TimeUnit.SECONDS);
	     WebDriverWait wait = new WebDriverWait(driver, 60);
	     WebElement element = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("Step1")));
	     assertTrue(driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Version Step 1:[\\s\\S]*$"));
	     
	     // Confirm the correct template appears in the Versioning Form
	     WebDriverWait wait1 = new WebDriverWait(driver, 60);
	     wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/h2/span[2]"), templateName));
         assertTrue("Could not find \"Versioned Implementation Guide Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateName) >= 0);
	     
         // Wait for Bindings to complete
		   waitForBindings("CopyTemplate");
		   
	     //Add Version and click Next
         driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/input")).click();
	     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/input")).sendKeys(" V2");
	     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/input")).sendKeys(Keys.TAB);
	     
	     // Click Next   
	     WebDriverWait wait2 = new WebDriverWait(driver, 60);
		 WebElement elemen2 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div[6]/button[1]")));
	     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[6]/button[1]")).click();
	    
	     // Wait for page to fully load
	        waitForPageLoad(); 
	       
	    // Wait for Bindings to complete
		   waitForBindings("CopyTemplate");
	       
	     //Version Step 2 - Constraint setup
     
       // Generate New Constraints and Finish
	     WebDriverWait wait6 = new WebDriverWait(driver, 60);
	     WebElement element6 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("Step2")));
	     assertTrue(driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Version Step 2:[\\s\\S]*$"));
	     
	  // Confirm the form is loaded
	     WebDriverWait wait7 = new WebDriverWait(driver, 60);
	     WebElement element7 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[2]/div[2]/button[1]")));
	   
	     // Click Finish
	     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div[2]/button[1]")).click();
	      
	      // Wait for page to fully load
	       waitForPageLoad();
		   WebDriverWait wait3 = new WebDriverWait(driver, 5);		           
		   wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[5]"))); 
	    
	      // Confirm the user is returned to the Template Editor
		       //driver.manage().timeouts().pageLoadTimeout(30, TimeUnit.SECONDS);
			   WebDriverWait wait11 = new WebDriverWait(driver, 60);
			   wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[1]/a"), "Template/Profile"));
		       assertTrue("Could not find \"Template/Profile\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Template/Profile[\\s\\S]*$"));
			   
			   // Wait for page to fully load
			   WebDriverWait wait12 = new WebDriverWait(driver, 60);		
			   wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[5]"))); 
		    
			   // Confirm the the correct Template Name appears in the Editor
			   //driver.manage().timeouts().pageLoadTimeout(30, TimeUnit.SECONDS);
			   WebDriverWait wait13 = new WebDriverWait(driver, 60);
			   wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/h3/span[2]"), templateName));
			   assertTrue("Could not find \"Template Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateName) >= 0);
			       
		  // Add Versioned Template Notes
		    if (permissionUserName == "lcg.admin")
				{
		    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[8]/textarea")).clear();
		    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[8]/textarea")).sendKeys(versionedTemplateNotes);
				}
		    else
				{
		    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[7]/textarea")).clear();
		    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[7]/textarea")).sendKeys(versionedTemplateNotes);
				}
	     
	     // Click Save and return to the Template Viewer
	     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[1]/div[1]/button")).click();
	     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[1]/div[1]/ul/li[4]/a")).click();
	     
	     // Wait for page to fully load
	       waitForPageLoad(); 
	       
	     // Confirm template was created 
	     WebDriverWait wait4 = new WebDriverWait(driver, 60);
	     WebElement element4 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("ViewTemplate")));
	     assertTrue("Could not find \"Versioned Template\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(versionedTemplate) >= 0);
	  
	     // Return to the Trifolia Home Page
	     ReturnHome("Welcome to Trifolia Workbench");	     
    }
}
