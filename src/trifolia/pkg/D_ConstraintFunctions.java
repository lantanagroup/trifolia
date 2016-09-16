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
import org.openqa.selenium.Keys;
import org.openqa.selenium.NoAlertPresentException;
import org.openqa.selenium.NoSuchElementException;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.JavascriptExecutor;
import org.openqa.selenium.firefox.FirefoxDriver;
import org.openqa.selenium.firefox.FirefoxProfile;
import org.openqa.selenium.firefox.internal.ProfilesIni;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.WebDriverWait;

@Ignore
public class D_ConstraintFunctions {
	private WebDriver driver;
	private String baseUrl;
	private boolean acceptNextAlert = true;
	private StringBuffer verificationErrors = new StringBuffer();
	public D_ConstraintFunctions() {}
	
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
  
@Test

public void waitForPageLoad() 
{
	WebDriverWait wait = new WebDriverWait(driver, 30);
	     wait.until(ExpectedConditions.jsReturnsValue("return document.readyState ==\"complete\";"));		
}

public void OpenTemplateBrowser() throws Exception 
{
	 // Confirm page completely loads
        waitForPageLoad();
    
     //Confirm the Welcome Message appears
		WebDriverWait wait = new WebDriverWait(driver, 60);
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/h2"), "Welcome to Trifolia Workbench!"));  	

    // Open the Template Browser
	  WebDriverWait wait1 = new WebDriverWait(driver, 60);                               
      WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[1]/div/div[2]/ul/li[2]/a"))); 
	  driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[2]/a")).click();
	  WebDriverWait wait2 = new WebDriverWait(driver, 60);
	  WebElement element2 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[1]/div/div[2]/ul/li[2]/ul/li[2]/a"))); 
	  driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[2]/ul/li[2]/a")).click();

	  // Confirm page completely loads
	     waitForPageLoad();
	  
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
	  
	  // Confirm the Template Listing appears
	  WebDriverWait wait = new WebDriverWait(driver, 60);                    
	  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[2]"), "Identifier"));
	
	  // Clear existing Search Criteria 
	   driver.manage().timeouts().pageLoadTimeout(30, TimeUnit.SECONDS);
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")).click();
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")).click();
	 
	   // Clear existing Search Filters
		   if(driver.findElements(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[6]")).size() != 0)
			  {
		 	    driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[2]/td[6]/button")).click();
			  }
		   if(driver.findElements(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[6]")).size() == 0)
			  {
		 	    driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[2]/td[5]/button")).click();
			  }

		// Wait for the page to fully load
		     waitForPageLoad();
		     
	   // Confirm search criteria is cleared
		  WebDriverWait wait1 = new WebDriverWait(driver, 60);                    
		  wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[2]/td[5]")));        
}
public void FindTemplate(String templateName, String templateOID, String templateIG) throws Exception 
	{
	  // Wait for page to fully load
	     waitForPageLoad();
	  
	  // Clear the existing search criteria
	      ClearExistingSearch();
	     
	  // Confirm the user is able to click in the Search box
	     WebDriverWait wait = new WebDriverWait(driver, 60);
		 WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")));
	  	 
	   //Search for the Template 
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")).click();
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/input")).sendKeys(templateName);
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/input")).sendKeys(Keys.TAB);
	  
	   //Confirm the search is complete
	    WebDriverWait wait1 = new WebDriverWait(driver, 60);                    
	    wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[2]/td[5]")));       
	   
	   // Confirm the correct Template Name is returned
	    WebDriverWait wait4 = new WebDriverWait(driver, 60);                     
	  	wait1.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[1]"), templateName));
	    assertTrue("Could not find \"Template Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateName) >= 0);
	    
	    // Confirm the correct Template IG is returned
	    WebDriverWait wait5 = new WebDriverWait(driver, 60);                    
	  	wait1.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath(" /html/body/div[2]/div/div/table/tbody/tr/td[3]"), templateIG));
	    assertTrue("Could not find \"Template IG\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateIG) >= 0);
	 }	

public void ConfirmTemplateViewer(String templateName, String templateOID) throws Exception 
{	
		// Wait for page to fully load
		   waitForPageLoad();
			  
		 WebDriverWait wait = new WebDriverWait(driver, 60);		
		 wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[8]"))); 
 
	  // Confirm the Template Viewer appears 
	     WebDriverWait wait1 = new WebDriverWait(driver, 60);
		 WebElement element1 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("ViewTemplate")));
		    
	  // Confirm the Template Viewer text appears
		 WebDriverWait wait2 = new WebDriverWait(driver, 60);                    
	     wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/ul/li[1]/a"), "Constraints"));
		 
	  // Confirm the correct template appears in the Template Viewer  
	     WebDriverWait wait3 = new WebDriverWait(driver, 60);                    
	     wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/a"), templateName));
	     assertTrue("Could not find \"Template Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateName) >= 0);
}
public void ConfirmTemplateEditor(String templateName, String templateOID) throws Exception 
	{
		  // Wait for page to fully load
	       waitForPageLoad();
		   WebDriverWait wait = new WebDriverWait(driver, 5);		
		   wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[5]"))); 
	    
	      // Confirm the Template Editor opens
	       driver.manage().timeouts().pageLoadTimeout(30, TimeUnit.SECONDS);
		   WebDriverWait wait1 = new WebDriverWait(driver, 60);
		   WebElement element= wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("TemplateEditor")));
		   assertTrue("Could not find \"Template Editor\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Template/Profile[\\s\\S]*$"));
		   
		   // Confirm the the correct Template Name appears in the Editor
		   driver.manage().timeouts().pageLoadTimeout(30, TimeUnit.SECONDS);
		   WebDriverWait wait2 = new WebDriverWait(driver, 60);
		   wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/h3/span[2]"), templateName));
		   assertTrue("Could not find \"Template Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateName) >= 0);
		   
		   // Confirm the the correct Template OID appears in the Editor
		   driver.manage().timeouts().pageLoadTimeout(30, TimeUnit.SECONDS);
		   WebDriverWait wait3 = new WebDriverWait(driver, 60);                                  
		   wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/h4/span"), templateOID));
		   assertTrue("Could not find \"Template OID\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateOID) >= 0);	
	}	

public void OpenConstraintsEditor() throws Exception
	{
	// Wait for page to fully load
       waitForPageLoad();
	   WebDriverWait wait = new WebDriverWait(driver, 60);		
	   wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[5]"))); 
    
      // Confirm the Template Editor appears
         WebDriverWait wait1 = new WebDriverWait(driver, 60);
		 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[1]/a"), "Template/Profile"));
         
	  // Open the Constraints Listing
	   WebDriverWait wait2 = new WebDriverWait(driver, 60);
	   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[2]/a")));
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[2]/a")).click();
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[2]/a")).click();
	   
	   // Confirm page completely loads
	      waitForPageLoad();
	       
	   // Confirm the Constraints Listing opens      
	      WebDriverWait wait3 = new WebDriverWait(driver, 60);                                  
		  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[5]"), "Conformance"));
         assertTrue("Could not find \"Conformance\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Conformance[\\s\\S]*$"));
	}

public void ConfirmConstraintEditor() throws Exception
	{
		// Confirm form is fully loaded
	    waitForPageLoad();
	    
	    // Confirm the constraint form appears
	    WebDriverWait wait = new WebDriverWait(driver, 60);
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/div[1]"), "Conf/Card:"));
	}
public void SetConformance() throws Exception 
	{
	
	// Confirm page completely re-loads
	  waitForPageLoad();
	  
	// Set the Conformance        
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/select")).click();
	   WebDriverWait wait3 = new WebDriverWait(driver, 60);
	   WebElement element3 = wait3.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/select/option[1]")));
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/select/option[1]")).sendKeys(Keys.ARROW_UP);
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/select/option[1]")).sendKeys(Keys.ARROW_UP);
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/select/option[1]")).click();
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/select/option[1]")).sendKeys(Keys.RETURN);
	   
	// Wait for page to refresh
	 	  waitForPageLoad();
	 	  
	   // Confirm the selected Conformance appears in the field
	     WebDriverWait wait = new WebDriverWait(driver, 60);
		 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/select"), "SHALL"));

		// Confirm page completely re-loads
	 	  waitForPageLoad();
	}	  

public void SetCardinality() throws Exception 
	{
	     // Set the Cardinality    
	
		// Confirm page completely re-loads
		  waitForPageLoad();
	  
	   WebDriverWait wait3 = new WebDriverWait(driver, 60);
	   WebElement element3 = wait3.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/div[2]/input")));
	   // driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/div[2]/input")).clear();
	   // driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/div[2]/input")).click();
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/div[2]/input")).sendKeys(Keys.ARROW_DOWN);
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/div[2]/input")).sendKeys(Keys.ARROW_DOWN);
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/div[2]/input")).sendKeys(Keys.ARROW_DOWN);
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/div[2]/input")).sendKeys(Keys.ARROW_DOWN);
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/div[2]/input")).click();
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/div[2]/input")).click();
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/div[2]/input")).sendKeys(Keys.TAB);
	
		// Wait for page to refresh
	 	  waitForPageLoad();
	 	  
	   // Confirm the selected Cardinality appears in the field
	     WebDriverWait wait = new WebDriverWait(driver, 60);                   
		 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/div[2]/input"), "1..1"));

		// Confirm page completely re-loads
	 	  waitForPageLoad();
	}  

public void SetBindingType(String bindingType) throws Exception 
	
		{
	  // Confirm page completely re-loads
    	  waitForPageLoad();
    
	 //Set Binding Type 
	
	   WebDriverWait wait = new WebDriverWait(driver, 60);
	   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[5]/select")));
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[5]/select")).click();	  
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[5]/select")).sendKeys(bindingType);
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[5]/select")).click();
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[5]/select")).sendKeys(Keys.TAB);
	  
		// Confirm page completely re-loads
	 	  waitForPageLoad();
 	  
	   //Confirm Binding Type appears in the field
	   WebDriverWait wait3 = new WebDriverWait(driver, 60);                                  
	   wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[5]/select"), bindingType));
		
		}	 
	
public void SaveTemplate() throws Exception 
{
	 // Confirm page completely loads
	    waitForPageLoad();
	  
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[1]/div[1]/button")).click();
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[1]/div[1]/ul/li[1]/a")).click();
	      
	   //Confirm template was updated successfully
	   WebDriverWait wait = new WebDriverWait(driver, 60);
	   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("//*[@id=\"footer\"]/div/div[2]/span")));
	   assertTrue("Could not find \"Done Saving\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Done saving.[\\s\\S]*$"));
	   
	// Confirm page completely loads
	    waitForPageLoad();
	    
	 // Return to the Template Browser
	    OpenTemplateBrowser();
	     
	 // Confirm page completely loads
	    waitForPageLoad();
	    
	 // Clear existing Search Criteria 
	    ClearExistingSearch();
 }	

public void ReturnHome(String welcomeMessage) throws Exception {
	  
	   // Return to the Trifolia Home Page
	    driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[1]/a")).click();
	    WebDriverWait wait4 = new WebDriverWait(driver, 60);
	    WebElement element4 = wait4.until(ExpectedConditions.visibilityOfElementLocated(By.id("appnav")));
	    
	    //Confirm the Welcome Message appears
		WebDriverWait wait = new WebDriverWait(driver, 60);
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/h2"), welcomeMessage));
		assertTrue("Could not find \"Welcome Message\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(welcomeMessage) >= 0);   	
}

// Test 1: Create Computed Constraints for the template
 @Test
   public void CreateComputedConstraint(String templateName, String templateOID, String templateType, String bindingType, String templateIdRoot, String templateIdExtension, String permissionUserName) throws Exception {
   
	 // Find the Template
	 
		// Confirm the page is fully loaded
	       waitForPageLoad();
	    
		// Open the Template Browser
		   OpenTemplateBrowser();
		   
		// Confirm the page is fully loaded
	       waitForPageLoad();
	       
		// Find the template
		if (permissionUserName == "lcg.admin") 
		{
		FindTemplate("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "1Automation Test IG");	 
		}
		
		if (permissionUserName == "hl7.member") 
		{
		FindTemplate("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2", "1HL7 Member Test IG");	 
		}
		
		// Open the Template Editor
		
	    if (permissionUserName == "lcg.admin")
	    	{
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[6]/div/a[2]")).click();
	    	}
	    else
	    	{
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[5]/div/a[2]")).click();
	    	}
	    
		 // Confirm the page is fully loaded
		     waitForPageLoad();
	    
	     // Confirm the correct template appears in the Editor
		   
	 		if (permissionUserName == "lcg.admin")
	 	 		{
	 				ConfirmTemplateEditor("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10");
	 	 		}
	  	    if (permissionUserName == "hl7.member")
	 		 	{
	 	 		ConfirmTemplateEditor("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2");
	 		 	}
	 
 	   // Confirm page completely loads
	       waitForPageLoad();
	    
	   // Open the Constraints Editor
	 	   OpenConstraintsEditor();
 	   
   // Add "realmCode" "typeId" Constraints 
	   assertTrue("Could not find \"Template OID\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateOID) >= 0);
	   if (templateType == "CDA: Document")
			{
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[5]/div[2]/span")).click();
			}
	   if (templateType == "eMeasure: Document")
			{
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[4]/div[2]/span")).click();
			}
   
	   // Open the realmCode constraint
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[4]/div/div/button[1]")).click();
	   
	  // Confirm the Constraints Editor opens for the selected constraint
	  ConfirmConstraintEditor();
	  
	   SetConformance();
	   
	   SetCardinality();
	   
	   SetBindingType("Other");
	   
	   // Confirm constraint form re-loads
	      waitForPageLoad();
	    
	    // Confirm the code field appears
	    WebDriverWait wait = new WebDriverWait(driver, 60);                    
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[9]/div/div[2]/div"), "Code:"));
	
	   // Add Code Value
	   WebDriverWait wait2 = new WebDriverWait(driver, 60);                               
	   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[9]/div/div[2]/input[1]")));
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[9]/div/div[2]/input[1]")).sendKeys("US");

     // Select the "typeId" constraint
	   if (templateType == "CDA: Document")
			{
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[6]/div[2]/span")).click();
			}
	    else if (templateType == "eMeasure: Document")
			{
			driver.findElement(By.xpath("//*[@id=\"constraintsTree\"]/div[2]/div[4]/div[2]/span")).click();
			}
   
   // Open the "typeId" element     
   	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[4]/div/div/button[1]")).click();
	   
     // Confirm the Constraints Editor opens for the selected constraint
 	  ConfirmConstraintEditor();
 	  
   	   // Select the conformance for the constraint
	   SetConformance();
	   
	   // Set the cardinality for the constraint
	   SetCardinality();
	   
    //Add Template ID Parent Branched and Child Identifier Constraints
   if (templateType == "CDA: Document")
		{
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[7]/div[2]/span")).click();
		}
   else if (templateType == "eMeasure: Document")
		{
		driver.findElement(By.xpath("//*[@id=\"constraintsTree\"]/div[2]/div[5]/div[2]/span")).click();
		}
   // Open the Constraint Editor
   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[4]/div/div/button[1]")).click();
   
   // Confirm the Constraints Editor opens for the selected constraint
	  ConfirmConstraintEditor();
	  
	  // Set the Confirmance for the constraint
	   SetConformance();
	   
	   // Set the Cardinality for the constraint
	   SetCardinality();
   
   //Setup templateId as 'Branch Root'
   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[3]/div[2]/input[1]")).click();
   
   // Expand the Template ID Element
   if (templateType == "CDA: Document")
		{
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[7]/div[1]/span[1]/div")).click();	 
		}
   else if (templateType == "eMeasure: Document")
		{
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[5]/div[1]/span[1]/div")).click();
		}  
   
   // Confirm the Template ID is expanded
	   if (templateType == "CDA: Document")
		{
	      WebDriverWait wait13 = new WebDriverWait(driver, 60);
	  	  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[1]/div[2]"), "@nullFlavor"));
		} 
	   if (templateType == "eMeasure: Document")
		{
		   WebDriverWait wait13 = new WebDriverWait(driver, 60);
		   wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[7]/div[1]/div[2]/span[3]"), "@nullFlavor"));
		}   
   //Add 'root' child constraint
   if (templateType == "CDA: Document")
		{
	   WebDriverWait wait3 = new WebDriverWait(driver, 60);                               
	   WebElement element3 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[2]/div[2]")));
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[2]/div[2]")).click();
		}
   if (templateType == "eMeasure: Document")
		{
	     WebDriverWait wait3 = new WebDriverWait(driver, 60);                               
	     WebElement element3 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[6]/div[5]/div[2]/span[3]")));
	     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[6]/div[5]/div[2]/span[3]")).click();
		}   

   // Confirm the Root element constraints editor appears and open 
      WebDriverWait wait13 = new WebDriverWait(driver, 60);
	  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[2]/span"), "@root"));
      
	//   Open the constraint editor for the constraint
	  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[4]/div/div/button[1]")).click();
   
	// Confirm the Constraints Editor opens for the selected constraint
	ConfirmConstraintEditor();
	  
   // Select the Conformance
   SetConformance();
   
   //Confirm the Alert appears
	   WebDriverWait wait0 = new WebDriverWait(driver, 60);
	   wait0.until(ExpectedConditions.alertIsPresent());
   
	     // Set the "root" element as an 'Identifier'
	      Alert alertDialog = driver.switchTo().alert();
	      // Get the alert text
	      String alertText = alertDialog.getText();
	      // Click the OK button on the alert.
	      alertDialog.accept();
	      // assertTrue(driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*The parent constraint is a branch root. Do you want to mark this constraint as a branch identifier?[\\s\\S]*$"));
	      
	  //Select the Cardinality
      SetCardinality();
      
      // Confirm constraint form re-loads
      waitForPageLoad();
      
      // Select the Binding Type
      SetBindingType("Single Value");
      
      // Confirm constraint form re-loads
      waitForPageLoad();
    
    // Confirm the code field appears
    WebDriverWait wait5 = new WebDriverWait(driver, 60);
	wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[6]/div/div[1]/div"), "Code:"));
      
	   // Set Code Value  
       WebDriverWait wait6 = new WebDriverWait(driver, 60);                               
	   WebElement element6 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[6]/div/div[1]/input[1]")));
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[6]/div/div[1]/input[1]")).sendKeys(templateIdRoot);
	   
     // Add "Extension" Child Constraint
   if (templateType == "CDA: Document")
		{
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[3]/div[2]/span[3]")).click();
		}
   else if (templateType == "eMeasure: Document")
		{
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[7]/div[3]/div[2]/span[3]")).click();
		}
   // Open the constraint editor
   driver.findElement(By.xpath("//*[@id=\"constraints\"]/div[2]/div[1]/table/tbody/tr/td[4]/div/div/button[1]/i")).click();
   
    // Confirm the Constraints Editor opens for the selected constraint
	ConfirmConstraintEditor();
	
   // Select the Conformance
   SetConformance();
   
   //Confirm the Alert appears
   WebDriverWait wait7 = new WebDriverWait(driver, 60);
   wait7.until(ExpectedConditions.alertIsPresent());
   
     // Set the "extension" element as an 'Identifier'
      Alert alertDialog1 = driver.switchTo().alert();
      // Get the alert text
      String alertText1 = alertDialog1.getText();
      // Click the OK button on the alert.
      alertDialog1.accept();
      // assertTrue(driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*The parent constraint is a branch root. Do you want to mark this constraint as a branch identifier?[\\s\\S]*$"));
         
      //Select the Cardinality
      SetCardinality();
      
      SetBindingType("Single Value");
      
      // Confirm constraint form re-loads
      waitForPageLoad();
    
	    // Confirm the code field appears
	    WebDriverWait wait8 = new WebDriverWait(driver, 60);
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[6]/div/div[1]/div"), "Code:"));
      
	   // Set Code Value
       WebDriverWait wait9 = new WebDriverWait(driver, 60);                             
	   WebElement element9 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[6]/div/div[1]/input[1]")));
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[6]/div/div[1]/input[1]")).sendKeys(templateIdExtension);
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[6]/div/div[1]/input[1]")).sendKeys(Keys.TAB);
	   
	   // Save the Template 
	   SaveTemplate();
	   
	   // Return to the Trifolia Home Page
	   ReturnHome("Welcome to Trifolia Workbench");  
      
}

//Add a Primitive Constraint containing schematron 
 @Test   
   public void AddPrimitiveConstraint(String templateName, String templateType, String primitiveProse, String bindingType, 
		       String viewMode, String schematronRule, String permissionUserName) throws Exception {
     
	 // Open the Template Browser
	    OpenTemplateBrowser();
	    
	 // Find the Template
	 if (permissionUserName == "lcg.admin")
		{
		FindTemplate("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "1Automation Test IG");
		}
	 if (permissionUserName == "hl7.member")
	 	{
		FindTemplate("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2", "1HL7 Member Test IG");
	 	}
	
	 //Open the Template Editor
	   if (permissionUserName == "lcg.admin")
			{
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[6]/div/a[2]")).click();
			}
	   else
			{
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[5]/div/a[2]")).click();       
			}		
     
	   // Confirm the correct template appears in the Editor
	   
	   if (permissionUserName == "lcg.admin")
			{
			ConfirmTemplateEditor("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10");
			}
       if (permissionUserName == "hl7.member")
		 	{
			ConfirmTemplateEditor("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2");
		 	}
       
       // Confirm page completely loads
	      // waitForPageLoad();
       
	   // Open the Constraints Editor
	 	   OpenConstraintsEditor();
	    
	 //Select the "id" constraint
   if (templateType == "CDA: Document")
		{
	    WebDriverWait wait = new WebDriverWait(driver, 60);                             
	    WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[2]")));
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[2]")).click();
		}                         
   
   if (templateType == "eMeasure: Document")
		{  
	    WebDriverWait wait = new WebDriverWait(driver, 60);                             
	    WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[6]/div[2]/span")));
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[6]/div[2]/span")).click();
		}
   
   // Confirm page completely loads
       waitForPageLoad();
   
     // Open the 'id' element       
       driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[4]/div/div/button[1]")).click();
       WebDriverWait wait = new WebDriverWait(driver, 60);                             
	   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[4]/div/div/button[1]")));
       
	   //Confirm the Constraint Editor opens
	     ConfirmConstraintEditor();
	     
	   // Confirm page completely loads
	       waitForPageLoad();
	    
       //Create the "primitive" constraint     
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[4]/div/div/button[1]")).click();
	   
	   // Confirm the primitive constraint dialog box appears  
	   WebDriverWait wait0 = new WebDriverWait(driver, 60);               
	  	wait0.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[3]/div"), "MISSING NARRATIVE FOR PRIMITIVE (CONF:X-X)."));
	   
	 //Add Constraint Prose 
	   WebDriverWait wait1 = new WebDriverWait(driver, 60);                             
	   WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[2]/div/textarea")));
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[2]/div/textarea")).sendKeys(primitiveProse);      
        
     //Set Binding Type 
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[2]/div/div[4]/select")).click();
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[2]/div/div[4]/select")).sendKeys(bindingType);
	   
	   //Confirm Binding Type appears in field
	    WebDriverWait wait2 = new WebDriverWait(driver, 60);               
	  	wait2.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[2]/div/div[4]/select"), bindingType));
	
	   // Confirm page completely loads
	      waitForPageLoad();
	    
	   // Add custom schematron
	      //Change View Mode to "Engineer"
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[4]/div/select")).click();
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[4]/div/select")).sendKeys(Keys.ARROW_DOWN);
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[4]/div/select")).sendKeys(Keys.ARROW_DOWN);
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[4]/div/select")).click();

	   // Confirm the view mode has been changed
	    WebDriverWait wait4 = new WebDriverWait(driver, 60);               
	  	wait4.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[4]/div/select"), "Engineer"));
	  	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[4]/div/select")).sendKeys(Keys.TAB);
		
	   // Confirm page is re-loaded
	      waitForPageLoad();
	    
	   // Confirm the schematron form opened 
	      WebDriverWait wait5 = new WebDriverWait(driver, 60);               
		  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath(" /html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[4]/div[1]/span[1]"), "Auto Generate:"));
	      
	   // Uncheck the Auto-Generate option and confirm the schematron default rule appears                            
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[4]/div[1]/input")).click();
	   WebDriverWait wait6 = new WebDriverWait(driver, 60);                                
	   WebElement element6 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[4]/textarea")));
   	     
	   // Confirm page completely loads
	      waitForPageLoad();
	    
	   // Save the template 
	   SaveTemplate();			   
 }
 
// Edit Primitive Constraint, then Duplicate and Delete the Primitive Constraint 
 @Test  
   public void EditPrimitiveConstraint(String templateName, String templateOID, String templateType, String viewMode, 
		   String conformance, String permissionUserName) throws Exception {
	 
	 // Open Template Browser
	    OpenTemplateBrowser();
	    
	 // Find the Template
	 if (permissionUserName == "lcg.admin")
		{
		FindTemplate("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "1Automation Test IG");
		}
	 if (permissionUserName == "hl7.member")
	 	{
		FindTemplate("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2", "1HL7 Member Test IG");
	 	}
	
	 //Open the Template Editor
	   if (permissionUserName == "lcg.admin")
			{
		   WebDriverWait wait = new WebDriverWait(driver, 60);                                
		   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[6]/div/a[2]")));   	  
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[6]/div/a[2]")).click();
			}
	   else
			{
		   WebDriverWait wait = new WebDriverWait(driver, 60);                                
		   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[5]/div/a[2]")));   	  
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[5]/div/a[2]")).click();       
			}		
     
	   // Confirm page completely loads
	    // waitForPageLoad();
	    
	   // Confirm the correct template appears in the Editor
	   
	   if (permissionUserName == "lcg.admin")
			{
			ConfirmTemplateEditor("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10");
			}
       if (permissionUserName == "hl7.member")
		 	{
			ConfirmTemplateEditor("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2");
		 	}
  
       OpenConstraintsEditor();
       
	   // Confirm page completely loads
	       waitForPageLoad();
	    
	 //Select the "id" constraint
   if (templateType == "CDA: Document")
		{
	    WebDriverWait wait = new WebDriverWait(driver, 60);                             
	    WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[2]/span")));
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[2]")).click();
		}                         
   
   if (templateType == "eMeasure: Document")
		{  
	    WebDriverWait wait = new WebDriverWait(driver, 60);                             
	    WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[6]/div[2]/span")));
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[6]/div[2]/span")).click();
		}
   
	   // Change View Mode back to 'Analyst' 
	   WebDriverWait wait = new WebDriverWait(driver, 60);                                     
	   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[4]/div/select")));
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[4]/div/select")).sendKeys(viewMode);
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[4]/div/select/option[1]")).sendKeys(Keys.RETURN);
	     
	   // Confirm the view mode has been changed
	   WebDriverWait wait0 = new WebDriverWait(driver, 60);               
	  	wait0.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[4]/div/select"), "Analyst"));

     // Expand the 'id' element       
       driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[4]/div/div/button[1]")).click();
       WebDriverWait wait1 = new WebDriverWait(driver, 60);                             
	   WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[4]/div/div/button[1]")));
	   
	   if (templateType == "CDA: Document")
			{
		   // Confirm the primitive constraint is selectable
		   WebDriverWait wait3 = new WebDriverWait(driver, 60);                             
		   WebElement element3 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[9]/div[6]/div[3]")));
		  
		 //Select the Primitive Constraint
		    WebDriverWait wait4 = new WebDriverWait(driver, 60);                                     
		    WebElement element4 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[9]/div[6]/div[3]")));
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[9]/div[6]/div[3]")).click();
			}
	   if (templateType == "eMeasure: Document")
			{  
			// Confirm the primitive constraint is selectable
			   WebDriverWait wait3 = new WebDriverWait(driver, 60);                             
			   WebElement element3 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[11]/div[6]")));
			
			// Confirm the primitive constraint appears in the Grid View
				 WebDriverWait wait5 = new WebDriverWait(driver, 60);                    
		  	     wait5.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[11]/div[6]"), "SHALL"));
			   
		  	// Click the other constraints
		  	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[1]/div[2]/span")).click();                                 
		  	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[2]/div[2]/span")).click();   
			 
		  	//Scroll to the bottom of the page
		  	   ((JavascriptExecutor)driver).executeScript("scroll(0,400)");
		  	   
		  	 //Select the Primitive Constraint
		     WebDriverWait wait4 = new WebDriverWait(driver, 60);                                                           
		     WebElement element4 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[7]/div[11]/div[6]")));	
		     driver.findElement(By.cssSelector("html body#ctl00_mainBody div.container-fluid div.main div#TemplateEditor div div.tab-content div#constraints.tab-pane.active div#constraintsTree.constraintTree div div div.constraintRow.constrained.primitive div.constraintColumn.constraintBranch")).click();                                 
			}
	   
	// Confirm page completely loads
       waitForPageLoad();
	   
	   //Confirm the constraint editor opens
         WebDriverWait wait5 = new WebDriverWait(driver, 60);                    
	     wait5.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[2]/div/div[1]/div"), "Conformance"));
	 
	     	
      // Update the Conformance
      
	   WebDriverWait wait6 = new WebDriverWait(driver, 60);
	   WebElement element6 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[2]/div/div[1]/select")));
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[2]/div/div[1]/select")).sendKeys(conformance);
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[2]/div/div[1]/select")).sendKeys(Keys.TAB);
		  
	   //Duplicate the 'id' primitive constraint and confirm the new element appears
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[4]/div/div/button[2]")).click();
	    
      //Confirm the duplicated element appears and Select the Duplicated primitive constraint
	   if (templateType == "CDA: Document")
			{
		   WebDriverWait wait7 = new WebDriverWait(driver, 60);                               
		   WebElement element7 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[9]/div[7]/div[6]")));
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[9]/div[7]/div[6]")).click();
			}
	   else if (templateType == "eMeasure: Document")
			{
		   WebDriverWait wait8 = new WebDriverWait(driver, 60);
		   WebElement element8 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[12]/div[6]")));
		    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[12]/div[6]")).click();
			}
   
	      // Delete the Duplicated constraint
	   	  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[4]/div/div/button[4]")).click();
	   	  
	   	  // Save the Template 
		   SaveTemplate();
		   
		   // Return to the Trifolia Home Page
		   ReturnHome("Welcome to Trifolia Workbench"); 
   }
   
// TEST 4: Bind Value Sets and Templates to Constraints 
 @Test
   public void constraintBinding(String templateName, String templateType, String valueSetOID, String valueSetName, String templateOID, String permissionUserName) throws Exception {
	
	 // Open the Template Browser
	    OpenTemplateBrowser();
	    
	 // Find the Template
	 if (permissionUserName == "lcg.admin")
		{
		FindTemplate("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "1Automation Test IG");
		}
	 if (permissionUserName == "hl7.member")
	 	{
		FindTemplate("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2", "1HL7 Member Test IG");
	 	}
	
	 //Open the Template Editor
	   if (permissionUserName == "lcg.admin")
			{
		   WebDriverWait wait = new WebDriverWait(driver, 60);
		   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[6]/div/a[2]")));
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[6]/div/a[2]")).click();
			}
	  
	   if (permissionUserName == "hl7.member")
			{
		   WebDriverWait wait = new WebDriverWait(driver, 60);
		   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[5]/div/a[2]")));
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[5]/div/a[2]")).click();       
			}		
     
	   // Confirm the correct template appears in the Editor
	   
	   if (permissionUserName == "lcg.admin")
			{
			ConfirmTemplateEditor("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10");
			}
       if (permissionUserName == "hl7.member")
		 	{
			ConfirmTemplateEditor("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2");
		 	}
       
       // Confirm page completely loads
	    // waitForPageLoad();
	    
	    // Open the Constraints Editor
	    OpenConstraintsEditor();
	    
   // Select and open the 'code' element 
	 if (templateType == "CDA: Document")
		{
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[2]/span")).click();
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[4]/div/div/button[1]")).click();
		}
	 else if (templateType == "eMeasure: Document")
	 	{
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[7]/div[2]/span")).click();
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[4]/div/div/button[1]")).click();
	 	} 
	 
	   //Confirm the Constraint Editor box opens
	     ConfirmConstraintEditor();
	     
	// Set Binding Type and Select the Value Set
	 
		 // Confirm page completely loads
		    waitForPageLoad();
		    
	 SetBindingType("Value Set");
	    
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[7]/div/div[2]/div[1]/div/div/input")).sendKeys(valueSetOID);

	// Confirm Value Set is located
	WebDriverWait wait = new WebDriverWait(driver, 60);                                      
    WebElement element = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("//*[@id=\"constraints\"]/div[2]/div[2]/div/div[1]/div[7]/div/div[2]/div[2]")));
	assertTrue("Could not find \"Value Set Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(valueSetName) >= 0);
	     
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[7]/div/div[2]/div[1]/div/div/input")).sendKeys(Keys.RETURN);
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[7]/div/div[2]/div[1]/div/div/input")).sendKeys(Keys.ARROW_DOWN);
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[7]/div/div[2]/div[1]/div/div/input")).sendKeys(Keys.RETURN);
	
	// Navigate to the Section constraint
	if (templateType == "CDA: Document")
	{  
			
    //Confirm the Component element is available and expand the element 
      WebDriverWait wait2 = new WebDriverWait(driver, 60);                               
	  WebElement element2 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[31]/div[1]/span[1]/div")));
	  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[31]/div[1]/span[1]/div")).click();  
	
	  // Confirm the Structure Document element is available, and expand the element
	  WebDriverWait wait3 = new WebDriverWait(driver, 60);
	  WebElement element3 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[32]/div[8]/div[1]/span[1]/div")));
	  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[32]/div[8]/div[1]/span[1]/div")).click(); 
	
	// Confirm the Component element is available, expand the 'component' element
	 WebDriverWait wait4 = new WebDriverWait(driver, 60);
	 WebElement element4 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[32]/div[9]/div[9]/div[1]/span[1]/div")));
	 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[32]/div[9]/div[9]/div[1]/span[1]/div")).click(); 
	
	// Select and open 'section' element
	   WebDriverWait wait5 = new WebDriverWait(driver, 60);
	   WebElement element5 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[32]/div[9]/div[10]/div[7]/div[2]/span[7]")));
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[32]/div[9]/div[10]/div[7]/div[2]/span[7]")).click();
	
	// Open the 'section' element
	   WebDriverWait wait6 = new WebDriverWait(driver, 60);
	   WebElement element6 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[4]/div/div/button[1]")));
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[4]/div/div/button[1]")).click();
	
	}
	
	else if (templateType == "eMeasure: Document")
 	{
		 //Expand the 'component' element 
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[23]/div[1]/span[1]/div")).click();  
	    WebDriverWait wait2 = new WebDriverWait(driver, 60);
		WebElement element2 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[24]/div[7]/div[2]/span[3]")));
					
		// Select and open 'section' element
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[24]/div[7]/div[2]/span[3]")).click();
		WebDriverWait wait5 = new WebDriverWait(driver, 60);
		WebElement element5 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[4]/div/div/button[1]")));
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[4]/div/div/button[1]")).click();
		
 	}
	
	// Bind the Template 
	WebDriverWait wait6 = new WebDriverWait(driver, 60);                               
	WebElement element6 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[4]/div[1]/div/div/input")));	
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[4]/div[1]/div/div/input")).click();
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[4]/div[1]/div/div/input")).sendKeys(templateOID);
	
	// Confirm the binding template is located
	   WebDriverWait wait4 = new WebDriverWait(driver, 60);                                
	   WebElement element4 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("//*[@id=\"constraints\"]/div[2]/div[2]/div/div[1]/div[4]/div[2]")));
	   assertTrue("Could not find \"Template Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateName) >= 0);
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[4]/div[1]/div/div/input")).sendKeys(Keys.ARROW_DOWN);
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[4]/div[1]/div/div/input")).sendKeys(Keys.RETURN);
		
		  // Save the Template 
		   SaveTemplate();
		   
		   // Return to the Trifolia Home Page
		   ReturnHome("Welcome to Trifolia Workbench"); 
   }
   
// TEST 5: Preview and Validate Template Constraints
 @Test
   public void PreviewAndValidateConstraint(String templateName, String templateOID, String constraint1, String constraint2, String constraint3, String constraint4, 
		   String constraint5, String validationText, String permissionUserName) throws Exception {
	   
	 // Open the Template Browser
	    OpenTemplateBrowser();
	    
	 // Find the Template
	 
	 if (permissionUserName == "lcg.admin")
		{
		FindTemplate("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "1Automation Test IG");
		}
	 if (permissionUserName == "hl7.member")
	 	{
		FindTemplate("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2", "1HL7 Member Test IG");
	 	}
	
	 
	 //Open the Template Editor
	   if (permissionUserName == "lcg.admin")
			{
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[6]/div/a[2]")).click();
			}
	   else
			{
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[5]/div/a[2]")).click();       
			}		
     
	   // Confirm the correct template appears in the Editor
	   
	   if (permissionUserName == "lcg.admin")
			{
				ConfirmTemplateEditor("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10");
			}
	   if (permissionUserName == "hl7.member")
		 	{
			ConfirmTemplateEditor("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2");
		 	}
    
	 // Open the Preview Pane
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[4]/a")).click();
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[3]/a")).click();
	    WebDriverWait wait = new WebDriverWait(driver, 60);
	    WebElement element = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[3]/div/div/p[1]/span[2]")));
	    assertTrue("Could not find \"SHALL\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*SHALL[\\s\\S]*$"));
    	   
	   // Confirm all 5 constraints appear correctly
	   assertTrue("Could not find \"Conformance\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*SHALL contain[\\s\\S]*$"));
	   assertTrue("Could not find \"Constraint 1\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(constraint1) >= 0);
	   assertTrue("Could not find \"Constraint 2\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(constraint2) >= 0);
	   assertTrue("Could not find \"Constraint 3\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(constraint3) >= 0);
	   assertTrue("Could not find \"Constraint 4\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(constraint4) >= 0);
	   assertTrue("Could not find \"Constraint 5\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(constraint5) >= 0);
   
	   // Open the Validation pane 
	   driver.findElement(By.xpath("//*[@id=\"templateEditorTabs\"]/li[4]/a")).click();
	   WebDriverWait wait8 = new WebDriverWait(driver, 60);
	   WebElement element8 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("validation")));
	   assertTrue("Could not find \"Validation\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Warnings[\\s\\S]*$"));
	   
	   //Confirm the validation text appears    
	   WebDriverWait wait9 = new WebDriverWait(driver, 60);               
	   wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[4]/div[1]/div[1]/h3"), "Errors"));
	   assertTrue("Could not find \"Validation Text\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(validationText) >= 0);
   
	   // Cancel and View the Template
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[1]/div[2]/button")).click();
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[1]/div[2]/ul/li[2]/a")).click();

	   ConfirmTemplateViewer("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10");
	   	   	   	
	   // Confirm the Back to List option is available, and Return to the Template Browser 
	   WebDriverWait wait10 = new WebDriverWait(driver, 60);
	   WebElement element10 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[1]/a")));
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[1]/a")).click();                         
	 
	   //Confirm the Template Browser appears
	   WebDriverWait wait11 = new WebDriverWait(driver, 60);
	   WebElement element11 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("BrowseTemplates")));
	   assertTrue("Could not find \"Browse Templates\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Browse Templates[\\s\\S]*$"));
	     
	   // Confirm the Browser is loaded
	    WebDriverWait wait12 = new WebDriverWait(driver, 60);               
	   	wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]"), "Page 1 of 1, 1 templates/profiles"));
	  
	   //Clear the existing Search Criteria
	     ClearExistingSearch();
	    
	     // Return to the Trifolia Home Page
	     ReturnHome("Welcome to Trifolia Workbench");  
 }
 
}
