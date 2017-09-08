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
public class E_ConstraintFunctions {
	private WebDriver driver;
	private String baseUrl;
	private boolean acceptNextAlert = true;
	private StringBuffer verificationErrors = new StringBuffer();
	public E_ConstraintFunctions() {}
	
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
	 // Confirm page completely loads
        waitForPageLoad();
        
        // Wait for Bindings to complete
	      waitForBindings("appnav");
    
    // Open the Template Browser
	  WebDriverWait wait = new WebDriverWait(driver, 60);                               
      WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[1]/div/div[2]/ul/li[2]/a"))); 
	  driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[2]/a")).click();
	  WebDriverWait wait1 = new WebDriverWait(driver, 60);
	  WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[1]/div/div[2]/ul/li[2]/ul/li[2]/a"))); 
	  driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[2]/ul/li[2]/a")).click();

	  // Confirm page completely loads
	     waitForPageLoad();
	  
	  // Wait for Bindings to complete
	     waitForBindings("BrowseTemplates");
	  
	// Confirm Template Browser opens
	 WebDriverWait wait2 = new WebDriverWait(driver, 60);
	 WebElement element2 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("BrowseTemplates")));
	 assertTrue("Could not find \"Browse Templates\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Browse Templates[\\s\\S]*$"));

	 // Confirm Correct Page Title appears
	 WebDriverWait wait3 = new WebDriverWait(driver, 60);                    
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
	   ////driver.manage().timeouts().pageLoadTimeout(30, TimeUnit.SECONDS);
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")).click();
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")).click();
	 
	   // Wait for the page to fully load
	     waitForPageLoad();
	     
	     // Wait for Bindings to complete
	     waitForBindings("BrowseTemplates");
	     
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
		     
		     // Wait for Bindings to complete
		     waitForBindings("BrowseTemplates");
		     
	   // Confirm search criteria is cleared
		  WebDriverWait wait1 = new WebDriverWait(driver, 60);                    
		  wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[2]/td[5]")));        
}
public void FindTemplate(String templateName, String templateOID, String templateIG, String templateType) throws Exception 
	{
	  // Wait for page to fully load
	     waitForPageLoad();
	  
     // Wait for Bindings to complete
        waitForBindings("BrowseTemplates");
	     
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
	   
	    // Wait for Bindings to complete
	     waitForBindings("BrowseTemplates");
	     
	   //Confirm the search is complete
	    WebDriverWait wait2 = new WebDriverWait(driver, 60);                    
	    wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[2]/td[5]")));       
	   
	   // Confirm the correct Template Name is returned
	    WebDriverWait wait3 = new WebDriverWait(driver, 60);                    
	  	wait1.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[1]"), templateName));
	    assertTrue("Could not find \"Template Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateName) >= 0);
	    
	    // Confirm the correct Template IG is returned
	    WebDriverWait wait4 = new WebDriverWait(driver, 60);                     
	  	wait1.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[3]"), templateIG));
	    assertTrue("Could not find \"Template IG\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateIG) >= 0);
	 }	

public void ConfirmTemplateViewer(String templateName, String templateOID) throws Exception 
{	
		// Wait for page to fully load
		   waitForPageLoad();
			  
	   // Wait for Bindings to complete
	     waitForBindings("ViewTemplate");
		     
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
		   
		   // Wait for Bindings to complete
		     waitForBindings("TemplateEditor");
	    
	      // Confirm the Template Editor opens
	       ////driver.manage().timeouts().pageLoadTimeout(30, TimeUnit.SECONDS);
		   WebDriverWait wait1 = new WebDriverWait(driver, 60);
		   WebElement element= wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("TemplateEditor")));
		   assertTrue("Could not find \"Template Editor\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Template/Profile[\\s\\S]*$"));
		   
		   // Confirm the the correct Template Name appears in the Editor
		   ////driver.manage().timeouts().pageLoadTimeout(30, TimeUnit.SECONDS);
		   WebDriverWait wait2 = new WebDriverWait(driver, 60);
		   wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/h3/span[2]"), templateName));
		   assertTrue("Could not find \"Template Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateName) >= 0);
		   
		   // Confirm the the correct Template OID appears in the Editor
		   //driver.manage().timeouts().pageLoadTimeout(30, TimeUnit.SECONDS);
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
	   // driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[2]/a")).click();
	   
	   // Confirm page completely loads
	      waitForPageLoad();
	       
	   // Confirm the Constraints Listing opens      
	      WebDriverWait wait3 = new WebDriverWait(driver, 60);                                  
		  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[1]/div[5]"), "Conformance"));
         assertTrue("Could not find \"Conformance\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Conformance[\\s\\S]*$"));
         
       // Click Anywhere
         WebDriverWait wait4 = new WebDriverWait(driver, 60);
  	     WebElement element4 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[2]/a")));
  	     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[2]/a")).click();
  	   
  	   // Wait for Bindings to complete
	     waitForBindings("TemplateEditor");
	}

public void ConfirmConstraintEditor() throws Exception
	{
		// Confirm form is fully loaded
	    waitForPageLoad();
	    
	    // Confirm the constraint form appears
	    WebDriverWait wait = new WebDriverWait(driver, 60);
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/div[1]"), "Conf/Card:"));
	}
public void SetConformance(String conformance) throws Exception 
	{
	
	// Confirm page completely re-loads
	  waitForPageLoad();
	  
	// Set the Conformance   
	     //Wait for the Conformance Field to appear
	     WebDriverWait wait = new WebDriverWait(driver, 60);
		 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/div[1]"), "Conf/Card:"));
	   
	   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/select")));
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/select")).click();
	   
	   WebDriverWait wait1 = new WebDriverWait(driver, 60);
	   WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/select")));
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/select")).click();
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/select")).sendKeys(Keys.ARROW_UP);
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/select")).sendKeys(Keys.ARROW_UP);
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/select")).sendKeys(Keys.TAB);
	   
	// Wait for page to refresh
	 	  waitForPageLoad();
	 	  
	   // Confirm the selected Conformance appears in the field
	     WebDriverWait wait2 = new WebDriverWait(driver, 60);
		 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/select"), "SHALL"));

		// Confirm page completely re-loads
	 	  waitForPageLoad();
	}	  

public void SetCardinality(String cardinality) throws Exception 
	{
	     // Set the Cardinality    
	
		// Confirm page completely re-loads
		  waitForPageLoad();
	  
	   WebDriverWait wait3 = new WebDriverWait(driver, 60);
	   WebElement element3 = wait3.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/div[2]/input")));
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/div[2]/input")).click();
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/div[2]/input")).clear();
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/div[2]/input")).sendKeys(cardinality);
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/div[2]/input")).sendKeys(Keys.TAB);
	   //driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[4]/div[1]/div/div/input")).click();
 
	   // Confirm the selected Cardinality appears in the field                 
	     WebDriverWait wait = new WebDriverWait(driver, 60);                     
		 wait.until(ExpectedConditions.invisibilityOfElementWithText(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[1]/div[2]/input"), "0..1"));

		// Confirm page completely re-loads
	 	  waitForPageLoad();
	}  

public void SetDataType(String dataType) throws Exception {

// Confirm page completely re-loads
   waitForPageLoad();
  
// Set the DataType 
   
   // Confirm the Data Type Field appears
     WebDriverWait wait = new WebDriverWait(driver, 60);
 	 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[2]/div/span"), "Data Type:"));
     
   // Open the drop-down for the Data Type
 	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[2]/select")).click();
 	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[2]/select")).sendKeys(dataType);
 	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
 	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[2]/select")).click();
   
//	   WebDriverWait wait3 = new WebDriverWait(driver, 60);
//	   WebElement element3 = wait3.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[2]/select/option[62]")));
//	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[2]/select/option[62]")).click();
   
   // Wait for page to refresh
 	  waitForPageLoad();
 	  
   // Confirm the selected Data Type appears in the field
     WebDriverWait wait1 = new WebDriverWait(driver, 60);
	 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[2]/select"),dataType ));

	// Confirm page completely re-loads
 	  waitForPageLoad();
}	 

public void SetBindingType(String bindingType) throws Exception 
	
		{
	  // Confirm page completely re-loads
    	  waitForPageLoad();
    
     // Confirm the Binding Type field default appears
 	     WebDriverWait wait = new WebDriverWait(driver, 60);
 		 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[5]/select"), "None"));

	 //Set Binding Type 
	   if (bindingType == "Single")
	   {
		   WebDriverWait wait1 = new WebDriverWait(driver, 60);                              
		   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[5]/select")));
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[5]/select")).click();	 
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[5]/select")).sendKeys(Keys.ARROW_DOWN);
//		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[5]/select")).sendKeys(Keys.ARROW_DOWN);
//		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[5]/select")).sendKeys(Keys.ARROW_DOWN);
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[5]/select")).click();	
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[5]/select")).sendKeys(Keys.TAB);	   
	   }
	   else
	   {
		   WebDriverWait wait1 = new WebDriverWait(driver, 60);                              
		   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[5]/select")));
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[5]/select")).click();	  
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[5]/select")).sendKeys(bindingType);
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[5]/select")).click();	
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[5]/select")).sendKeys(Keys.TAB);
	   }
		  
		// Confirm page completely re-loads
	 	  waitForPageLoad();
  
 	   // Confirm the Binding Type default no longer appears              
	     WebDriverWait wait2 = new WebDriverWait(driver, 60);                     
		 wait.until(ExpectedConditions.invisibilityOfElementWithText(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[5]/select"), "None"));

	   //Confirm Binding Type appears in the field
	   WebDriverWait wait3 = new WebDriverWait(driver, 60);                                  
	   wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[5]/select"), bindingType));	
		}	 
public void BindValueSet(String valueSetOID, String valueSetName) throws Exception {
	
		// Confirm the Value Set field appear
		WebDriverWait wait = new WebDriverWait(driver, 60);                    
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[7]/div/div[1]/div"), "Value Conf.:"));
		
		WebDriverWait wait1 = new WebDriverWait(driver, 60);                    
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[7]/div/div[2]/div[1]/div/div/span[1]"), "Value Set:"));
		
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[7]/div/div[2]/div[1]/div/div/input")).sendKeys(valueSetOID);
		
		// Confirm Value Set is located
		WebDriverWait wait2 = new WebDriverWait(driver, 60);                                      
		WebElement element2 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("//*[@id=\"constraints\"]/div[2]/div[2]/div/div[1]/div[7]/div/div[2]/div[2]")));
		assertTrue("Could not find \"Value Set Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(valueSetName) >= 0);
		
		// Bind the Value Set
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[7]/div/div[2]/div[1]/div/div/input")).sendKeys(Keys.RETURN);
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[7]/div/div[2]/div[1]/div/div/input")).sendKeys(Keys.ARROW_DOWN);
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[7]/div/div[2]/div[1]/div/div/input")).sendKeys(Keys.RETURN);
}

public void AddClassCode(String templateType, String conformance, String cardinality, String bindingType) throws Exception{
	
	   // Select the "classCode" Constraint
	    if (templateType == "CDA: Document")
			{
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[2]/div[2]/span")).click();
			}
	   if (templateType == "eMeasure: Document")
			{
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[4]/div[2]")).click();
			}
	   if (templateType == "CDA: Entry" || templateType == "CDA: Organizer")
		{
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[2]/div[2]/span")).click();
		}

	   // Open the classCode constraint 

	      driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")).click();

	   // Confirm the Constraints Editor opens for the selected constraint
	      ConfirmConstraintEditor();
	  
	   // Set the conformance 
	      SetConformance(conformance);
	   
	   // Confirm constraint form re-loads
	      waitForPageLoad();
	      
	   // Set the Cardinality 
	      SetCardinality(cardinality);
	   
	   // Confirm constraint form re-loads
	      waitForPageLoad();
	   
	   // Set the Binding Type
	      SetBindingType(bindingType);	
	   
	   // Confirm constraint form re-loads
	      waitForPageLoad();   
}

public void AddMoodCode(String templateType, String conformance, String cardinality, String bindingType) throws Exception{
	
	   // Select the "moodCode" Constraint
	    if (templateType == "CDA: Document")
			{
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[3]/div[3]")).click();
			}
	    if (templateType == "CDA: Entry" || templateType == "CDA: Organizer" )
		{
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[3]/div[2]/span")).click();
		}
	   if (templateType == "eMeasure: Document")
			{
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[4]/div[2]")).click();
			}

	   // Open the moodCode constraint
	      driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")).click();
	   
	   // Confirm the Constraints Editor opens for the selected constraint
	      ConfirmConstraintEditor();
	  
	   // Set the conformance 
	      SetConformance(conformance);
	   
	   // Confirm constraint form re-loads
	      waitForPageLoad();
	      
	   // Set the Cardinality 
	      SetCardinality(cardinality);
	   
	   // Confirm constraint form re-loads
	      waitForPageLoad();
	   
	   // Set the Binding Type
	      SetBindingType(bindingType);	
	      
	   // Add the "Code" value for the element
	      
	   // Confirm constraint form re-loads
	      waitForPageLoad();   
}

public void AddRealmCode(String templateType, String conformance, String cardinality, String bindingType) throws Exception{
	
	   // Add "realmCode" Constraint
	    if (templateType == "CDA: Document")
			{     
	    	WebDriverWait wait = new WebDriverWait(driver, 60);                              
			WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[4]/div[2]/span")));
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[4]/div[2]/span")).click();
			}
	   if (templateType == "eMeasure: Document")
			{
		    WebDriverWait wait = new WebDriverWait(driver, 60);                              
			WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[4]/div[2]")));
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[4]/div[2]")).click();
			}
	   
	      driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")).click();
	   
	   // Confirm the Constraints Editor opens for the selected constraint
	      ConfirmConstraintEditor();
	  
	   // Set the conformance for the selected constraint
	      SetConformance(conformance);
	   
	   // Confirm constraint form re-loads
	      waitForPageLoad();
	      
	   SetCardinality(cardinality);
	   
	   // Confirm constraint form re-loads
	      waitForPageLoad();
	   
	   SetBindingType(bindingType);	
}

public void AddCodeValueAndDisplay(String code, String display)throws Exception{
	
	// Confirm the code field appears
    WebDriverWait wait = new WebDriverWait(driver, 60);                     
	wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[6]/div/div[1]/div"), "Code:"));

   // Add Code Value
   WebDriverWait wait1 = new WebDriverWait(driver, 60);                              
   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[6]/div/div[1]/input[1]")));
   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[6]/div/div[1]/input[1]")).sendKeys(code);	
   
   // Add Display Value
   WebDriverWait wait2 = new WebDriverWait(driver, 60);                              
   WebElement element2 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[6]/div/div[1]/input[2]")));
   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[6]/div/div[1]/input[2]")).sendKeys(display);	
}

public void AddCodeSystem(String codeSystemOID, String codeSystemName)throws Exception{
	
	// Confirm the Code System field appears
    WebDriverWait wait = new WebDriverWait(driver, 60);                     
	wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[6]/div/div[2]/div"), "Code System:"));

   // Add Code System Value
   WebDriverWait wait1 = new WebDriverWait(driver, 60);                              
   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[6]/div/div[2]/select")));
   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[6]/div/div[2]/select")).sendKeys(codeSystemOID);
		
		// Confirm Code System is located
		WebDriverWait wait2 = new WebDriverWait(driver, 60);                                      
		WebElement element2 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[6]/div/div[2]/select")));
		assertTrue("Could not find \"Code System Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(codeSystemName) >= 0);
		
//		// Bind the Code System
//		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[6]/div/div[2]/select")).sendKeys(Keys.RETURN);
//		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[6]/div/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
//		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[6]/div/div[2]/select")).sendKeys(Keys.RETURN);
   
 }

public void AddRealmCodeValue(String value) throws Exception{
	
    // Confirm the code field appears
    WebDriverWait wait = new WebDriverWait(driver, 60);                     
	wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[9]/div/div[2]/div"), "Code:"));

   // Add Code Value
   WebDriverWait wait2 = new WebDriverWait(driver, 60);                              
   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[9]/div/div[2]/input[1]")));
   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[9]/div/div[2]/input[1]")).sendKeys(value);
}

public void AddtypeId(String templateType, String conformance, String cardinality) throws Exception{
	
	// Select the "typeId" constraint
    if (templateType == "CDA: Document")
 			{
 			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[5]/div[2]/span")).click();
 			}
 	    else if (templateType == "eMeasure: Document")
 			{
 			driver.findElement(By.xpath("//*[@id=\"constraintsTree\"]/div[2]/div[5]/div[2]/span")).click();
 			}
    
      // Open the "typeId" element     
    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")).click();
 	   
      // Confirm the Constraints Editor opens for the selected constraint
  	      ConfirmConstraintEditor();
  	  
       // Set the conformance for the constraint
 	      SetConformance(conformance);
 	   
 	   // Set the cardinality for the constraint
 	   SetCardinality(cardinality);
}

public void AddtemplateId(String templateType, String conformance, String cardinality)throws Exception{
	
	//Add Template ID Parent Branched and Child Identifier Constraints
	  if (templateType == "CDA: Document" || templateType == "CDA: Organizer")
		{
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[6]/div[2]/span")).click();
		}
	  else if (templateType == "CDA: Entry")
		{                            
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[7]/div[2]/span")).click();
		}
	
	  // Open the Constraint Editor
	     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")).click();
	
      // Confirm the Constraints Editor opens for the selected constraint
        ConfirmConstraintEditor();
	  
	   // Set the conformance for the selected constraint
	      SetConformance(conformance);
	  
	  // Set the Cardinality for the constraint
	      SetCardinality(cardinality);

	  //Setup templateId as 'Branch Root'
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[3]/div[2]/input[1]")).click();

	   // Expand the Template ID Element
	   if (templateType == "CDA: Document" || templateType == "CDA: Organizer")
			{
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[6]/div[1]/span[1]/div")).click();	 
			}
	   else if (templateType == "CDA: Entry")
			{
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[7]/div[1]/span[1]/div")).click();
			}  
	   
	   // Confirm the Template ID is expanded
		   if (templateType == "CDA: Document" || templateType == "CDA: Organizer")
			{
		      WebDriverWait wait = new WebDriverWait(driver, 60);
		  	  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[7]/div[1]/div[2]/span[3]"), "@nullFlavor"));
			} 
		   if (templateType == "CDA: Entry")
			{
			   WebDriverWait wait1 = new WebDriverWait(driver, 60);
			   wait1.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[1]/div[2]/span[3]"), "@nullFlavor"));
			}  
}

public void AddtemplateIdRoot(String templateType, String conformance, String cardinality, String bindingType, String templateIdRoot) throws Exception{
	
	   //Select the 'root' child constraint and confirm
	   if (templateType == "CDA: Document" || templateType == "CDA: Organizer")
			{
		   WebDriverWait wait = new WebDriverWait(driver, 60);                               
		   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[7]/div[2]/div[2]/span[3]")));
		    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[7]/div[2]/div[2]/span[3]")).click();
			}
	   if (templateType == "CDA: Entry")
			{
		     WebDriverWait wait = new WebDriverWait(driver, 60);                               
		     WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[2]/div[2]/span[3]")));
		     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[2]/div[2]/span[3]")).click();
			}   

//	   // Confirm the Root element constraints editor appears and open 
//	     
//	      WebDriverWait wait = new WebDriverWait(driver, 60);                   
//		  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[2]/div[2]/span[3]"), "@root"));
//	      
		//   Open the constraint editor for the constraint
		  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")).click();
	   
		// Confirm the Constraints Editor opens for the selected constraint
		   ConfirmConstraintEditor();
		  
	   // Select the Conformance
	      SetConformance(conformance);
	   
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
	        SetCardinality(cardinality);
	        //Thread.sleep(2000);
	      
	      // Confirm the Identifier Flag is checked
	         driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[3]/div[2]/input[2]")).isSelected();
	      
	      // Confirm constraint form re-loads
	         waitForPageLoad();
	      
	      // Select the Binding Type
	         SetBindingType(bindingType);
	      
	      // Confirm constraint form re-loads
	         waitForPageLoad();
	    
	         WebDriverWait wait = new WebDriverWait(driver, 60);
	     	wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[5]/select"), "Single Value"));
	        
		    // Confirm the code field appears
		    WebDriverWait wait60 = new WebDriverWait(driver, 60);
			wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[6]/div/div[1]/div"), "Code:"));
		      
		   // Set Code Value  
	       WebDriverWait wait6 = new WebDriverWait(driver, 60);                               
		   WebElement element6 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[6]/div/div[1]/input[1]")));
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[6]/div/div[1]/input[1]")).sendKeys(templateIdRoot);
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[6]/div/div[1]/input[2]")).click();
}

public void CheckFHIRisModifier() throws Exception{

		//   Confirm the isModifier field appears 
			WebDriverWait wait = new WebDriverWait(driver, 60);
			wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[10]/div[1]/span"), "Is Modifier?"));
		    
		// Check the Flag	
			WebDriverWait wait1 = new WebDriverWait(driver, 60);                               
			WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[10]/div[2]/input")));
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[10]/div[2]/input")).click();			   	            	     	 
}

public void CheckFHIRmustSupport() throws Exception{

		//   Confirm Must Support field appears
			WebDriverWait wait = new WebDriverWait(driver, 60);
			wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[11]/div[1]/span"), "Must Support?"));
		    
		// Un-Check the Flag	
			WebDriverWait wait1 = new WebDriverWait(driver, 60);                               
			WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[11]/div[2]/input")));
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[11]/div[2]/input")).click();			   	            	     	 
}

public void AddFHIRProfileType(String templateType, String conformance, String cardinality, String bindingType) throws Exception{
	
	   //Select the 'type' child constraint and confirm
	   if (templateType == "Composition")
			{
		   WebDriverWait wait = new WebDriverWait(driver, 60);                               
		   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[11]/div[2]/span")));
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[11]/div[2]/span")).click();
			}
	   if (templateType == "CDA: Entry")
			{
		     WebDriverWait wait = new WebDriverWait(driver, 60);                               
		     WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[2]/div[2]/span[3]")));
		     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[2]/div[2]/span[3]")).click();
			}   

		//   Open the constraint editor for the constraint
	         WebDriverWait wait = new WebDriverWait(driver, 60);                               
	         WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")));
		      driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")).click();
	   
		// Confirm the Constraints Editor opens for the selected constraint
		   ConfirmConstraintEditor();
		  
	   // Select the Conformance
	      SetConformance(conformance);
	      
		  //Select the Cardinality
	        SetCardinality(cardinality);
	        //Thread.sleep(2000);
	            
	      // Confirm constraint form re-loads
	         waitForPageLoad();
	      
	      // Select the Binding Type
	         SetBindingType(bindingType);
	      
	      // Confirm constraint form re-loads
	         waitForPageLoad();
	    
	         WebDriverWait wait1 = new WebDriverWait(driver, 60);
	     	 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[5]/select"), "Single Value"));
	        
	      // Add Code and Display Values
	     	 AddCodeValueAndDisplay("52521-2", "Care Plan");
	     	 
	      // Add Code System
	     	 AddCodeSystem("2.16.840.1.113883.6.1", "LOINC");
	     	 
		  }
public void AddFHIRSection(String conformance, String cardinality)throws Exception{

	// Select the 'section' element
	WebDriverWait wait = new WebDriverWait(driver, 60);
	WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[22]/div[2]/span")));
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[22]/div[2]/span")).click();
	
	// Open the Section element
	WebDriverWait wait1 = new WebDriverWait(driver, 60);
	WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")));
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")).click();	
	
	// Confirm the Constraints Editor opens for the selected constraint
	  ConfirmConstraintEditor();
	 
	// Select the Conformance
	   SetConformance(conformance);
	 
	 //Select the Cardinality
	   SetCardinality(cardinality);
	   
	 //Setup templateId as 'Branch Root'
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[3]/div[2]/input[1]")).click();
	   
	 // Check the Is Modifier Flag
	    CheckFHIRisModifier();
	   
	 // Check the Must Support Flag
	    CheckFHIRmustSupport();
	    
	}

public void AddtemplateIdExtension(String templateType, String conformance, String cardinality, String bindingType, String templateIdExtension)throws Exception{
	
	    // Add the "Extension" Child Constraint
	  if (templateType == "CDA: Document")
			{
			   WebDriverWait wait = new WebDriverWait(driver, 60);                               
			   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[7]/div[3]/div[2]/span[3]")));
				driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[7]/div[3]/div[2]/span[3]")).click();
			}
	  else if (templateType == "CDA: Entry")
			{
			   WebDriverWait wait = new WebDriverWait(driver, 60);                               
			   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[3]/div[2]/span[3]")));
				driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[3]/div[2]/span[3]")).click();
			}
	  else if (templateType == "CDA: Organizer")
		{
		   WebDriverWait wait = new WebDriverWait(driver, 60);                               
		   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[7]/div[3]/div[2]")));
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[7]/div[3]/div[2]")).click();
		}
	  
	  // Open the constraint editor
	     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")).click();
	  
	   // Confirm the Constraints Editor opens for the selected constraint
		ConfirmConstraintEditor();
		
	  // Select the Conformance
	    SetConformance(conformance);
	  
	  //Confirm the Alert appears
	  WebDriverWait wait = new WebDriverWait(driver, 60);
	  wait.until(ExpectedConditions.alertIsPresent());
	  
	    // Set the "extension" element as an 'Identifier'
	     Alert alertDialog1 = driver.switchTo().alert();
	     // Get the alert text
	     String alertText1 = alertDialog1.getText();
	     // Click the OK button on the alert.
	     alertDialog1.accept();
	     // assertTrue(driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*The parent constraint is a branch root. Do you want to mark this constraint as a branch identifier?[\\s\\S]*$"));
	        
	     //Select the Cardinality
	     SetCardinality(cardinality);
	     //Thread.sleep(2000);
	     
	     // Set the Binding Type
	     SetBindingType(bindingType);
	     
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
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[6]/div/div[1]/input[2]")).click();	
}

public void AddiD(String templateType, String conformance, String cardinality) throws Exception{
	
	   // Add "iD" Constraint
	    if (templateType == "CDA: Document")
			{
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[2]/span")).click();
			}
	   if (templateType == "CDA: Entry")
			{                           
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[9]/div[2]/span")).click();
			}
	   if (templateType == "CDA: Organizer")
		{                           
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[2]/span")).click();
		}

	   // Open the iD constraint
	      driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")).click();
	   
	   // Confirm the Constraints Editor opens for the selected constraint
	      ConfirmConstraintEditor();
	  
	   // Set the conformance for the selected constraint
	      SetConformance(conformance);
	   
	   // Confirm constraint form re-loads
	      waitForPageLoad();
	      
	      SetCardinality(cardinality);
	   
	   // Confirm constraint form re-loads
	      waitForPageLoad();
}

public void AddCode(String templateType, String conformance, String cardinality, String bindingType) throws Exception{
	
	   // Add "Code" Constraint
	    if (templateType == "CDA: Document")
			{
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[9]/div[2]/span")).click();
			}
	   if (templateType == "CDA: Entry")
			{                            
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[10]/div[2]/span")).click();
			}
	   if (templateType == "CDA: Organizer")
		{                             
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[9]/div[2]")).click();
		}

	   // Open the Code constraint
	      driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")).click();
	   
	   // Confirm the Constraints Editor opens for the selected constraint
	      ConfirmConstraintEditor();
	  
	   // Set the conformance for the selected constraint
	      SetConformance(conformance);
	   
	   // Confirm constraint form re-loads
	      waitForPageLoad();
	      
	      SetCardinality(cardinality);
	   
	   // Confirm constraint form re-loads
	      waitForPageLoad();
	      
	   // Set Binding Type
	      SetBindingType(bindingType);
	      
      // Confirm constraint form re-loads
	      waitForPageLoad();
}

public void AddCodeCode(String templateType, String conformance, String cardinality, String bindingType) throws Exception{
	
	// Select the "Code" constraint
    if (templateType == "CDA: Document")
 			{
 			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[12]/div[2]/span")).click();
 			}
 	    else if (templateType == "CDA: Organizer")
 			{
 			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[2]/span")).click();
 			}
    
      // Expand the "Code" element     
    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[9]/div[1]/span[1]/div")).click();
    	
 	  // Confirm the Code Element is expanded 
    	WebDriverWait wait = new WebDriverWait(driver, 60);                    
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[10]/div[2]/div[2]/span[3]"), "@code"));
     
      // Select the @code element
		 WebDriverWait wait1 = new WebDriverWait(driver, 60); 
	     WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[10]/div[2]/div[2]/span[3]")));
	     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[10]/div[2]/div[2]/span[3]")).click();
	 
	  // Open the Constraints Editor for the @code element
	      driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")).click();
		
      // Confirm the Constraints Editor opens for the selected constraint
  	      ConfirmConstraintEditor();
  	  
       // Set the conformance for the constraint
 	      SetConformance(conformance);
 	   
 	   // Set the cardinality for the constraint
 	      SetCardinality(cardinality);
 	      
	  // Set the Binding Type
		 SetBindingType(bindingType);	
		      
	 // Add Code value and Display value for the @code element
        AddCodeValueAndDisplay("74728-7", "Vital signs, weight, height, head circumference, oximetry, BMI, and BSA panel - HL7.CCDAr1.1");	       
}

public void AddCodeCodeSystem(String templateType, String conformance, String cardinality, String bindingType) throws Exception{
	    	
 	  // Confirm the Code Element is expanded 
    	WebDriverWait wait = new WebDriverWait(driver, 60);                      
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[10]/div[3]/div[2]/span[3]"), "@codeSystem"));
     
      // Select the @codeSystem element
		 WebDriverWait wait1 = new WebDriverWait(driver, 60); 
	     WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[10]/div[3]/div[2]/span[3]")));
	     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[10]/div[3]/div[2]/span[3]")).click();
	 
	  // Open the Constraints Editor for the @codeSystem element
	      driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")).click();
		
      // Confirm the Constraints Editor opens for the selected constraint
  	      ConfirmConstraintEditor();
  	  
       // Set the conformance for the constraint
 	      SetConformance(conformance);
 	   
 	   // Set the cardinality for the constraint
 	      SetCardinality(cardinality);
 	      
	  // Set the Binding Type
		 SetBindingType(bindingType);	
		      
	  // Add Code System and Add Display
         AddCodeValueAndDisplay("2.16.840.1.113883.6.1 ", "LOINC");	       
}

public void AddStatus(String templateType, String conformance, String cardinality, String bindingType) throws Exception{
	
	// Select the "Status" constraint
    if (templateType == "CDA: Document")
 			{
 			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[12]/div[2]/span")).click();
 			}
 	    else if (templateType == "CDA: Entry")
 			{
 			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[13]/div[2]/span")).click();
 			}
    if (templateType == "CDA: Organizer")
  		{                             
  		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[11]/div[2]/span")).click();
  		}
    
      // Open the "Status" element     
    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")).click();
 	   
      // Confirm the Constraints Editor opens for the selected constraint
  	      ConfirmConstraintEditor();
  	  
       // Set the conformance for the constraint
 	      SetConformance(conformance);
 	   
 	   // Set the cardinality for the constraint
 	      SetCardinality(cardinality);
 	      
 	   // Expand the StatusCode element 
 	     WebDriverWait wait = new WebDriverWait(driver, 60);                               
 	     WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[13]/div[1]/span[1]/div")));
 	     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[13]/div[1]/span[1]/div")).click();
 	     
 	   // Select the @code element
 	     WebDriverWait wait1 = new WebDriverWait(driver, 60); 
	     WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[14]/div[2]/div[2]/span[3]")));
	     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[14]/div[2]/div[2]/span[3]")).click();
	 
	  // Open the "@code" element        
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")).click();
	 
	  // Set the Binding Type
		 SetBindingType(bindingType);	
		      
	 // Add Code value and Display value for the classCode element
        AddCodeValueAndDisplay("completed", "Completed");
		       
}

public void AddEffectiveTime(String templateType, String conformance, String cardinality) throws Exception{
	
	// Select the "EffectiveTime" constraint
    if (templateType == "CDA: Document")
 			{
 			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[13]/div[2]/span")).click();
 			}
 	    else if (templateType == "CDA: Entry")
 			{                
 			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[15]/div[2]/span")).click();
 			}
    if (templateType == "CDA: Organizer")
		{                             
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[12]/div[2]/span")).click();
		}
    
      // Open the "EffectiveTime" element     
    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")).click();
 	   
      // Confirm the Constraints Editor opens for the selected constraint
  	      ConfirmConstraintEditor();
  	  
       // Set the conformance for the constraint
 	      SetConformance(conformance);
 	   
 	   // Set the cardinality for the constraint
 	      SetCardinality(cardinality);
}

public void AddValueUnit(String templateType, String conformance, String cardinality, String bindingType, String dataType) throws Exception{
	
	// Select the "Value" constraint
    if (templateType == "CDA: Document")
 			{                           
 			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[20]/div[2]/span")).click();
 			}
 	    else if (templateType == "CDA: Entry")
 			{
 			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[19]/div[2]/span")).click();
 			}
    
      // Open the "Value" element     
    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")).click();
 	   
      // Confirm the Constraints Editor opens for the selected constraint
  	      ConfirmConstraintEditor();
  	  
       // Set the conformance for the constraint
 	      SetConformance(conformance);
 	   
 	   // Set the cardinality for the constraint
 	      SetCardinality(cardinality);
 	   
 	   // Set the Data Type for the constraint
 	      SetDataType(dataType);   
 	      
 	   // Expand the Value element and confirm the child elements appear
 	     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[19]/div[1]/span[1]/div")).click();
 	    WebDriverWait wait = new WebDriverWait(driver, 60);                    
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[20]/div[1]/div[2]/span[3]"), "@nullFlavor"));
     
 	   // Confirm the Value @Unit element appears 
 	    WebDriverWait wait0 = new WebDriverWait(driver, 60);                    
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[20]/div[3]/div[2]/span[3]"), "@unit"));
     
	   // Select the @Unit Element
		 WebDriverWait wait1 = new WebDriverWait(driver, 60);                              
		 WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[20]/div[3]/div[2]/span[3]")));
		  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[20]/div[3]/div[2]/span[3]")).click();
		 	 
		 // Open the @unit element    
		 WebDriverWait wait2 = new WebDriverWait(driver, 60);                              
		 WebElement element2 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")));
    	 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")).click();
    	 
	   // Set the conformance for the constraint
	      SetConformance(conformance);
	   
	   // Set the cardinality for the constraint
	      SetCardinality(cardinality);
	      
	      // Set Binding Type
	      SetBindingType(bindingType);      
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
	    
	    // Wait for page to fully load
		   waitForPageLoad();
		   
	    // Wait for Bindings to complete
	      waitForBindings("appnav");
	      
	    //Confirm the Welcome Message appears
		WebDriverWait wait = new WebDriverWait(driver, 60);
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/h2"), welcomeMessage));
		assertTrue("Could not find \"Welcome Message\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(welcomeMessage) >= 0);   	
}

//Create Computed Constraints for the Entry Type Template
 @Test
   public void AddEntryObsCompConstraints(String templateName, String templateOID, String templateIG, String templateType, String permissionUserName) throws Exception {
   
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
		FindTemplate(templateName, templateOID, templateIG, templateType);	 
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
	 				ConfirmTemplateEditor(templateName, templateOID);
	 	 		}
	 
 	   // Confirm page completely loads
	       waitForPageLoad();
	    
	   // Open the Constraints Editor
	 	   OpenConstraintsEditor();
	 	   
	   // Confirm the template OID appears
	 	  assertTrue("Could not find \"Template OID\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateOID) >= 0);
 	   
	   // Constrain the classCode element
	      AddClassCode(templateType, "SHALL", "1..1", "Single");
	      
	   // Add Code value and Display value for the classCode element
	      AddCodeValueAndDisplay("CLUSTER", "CLUSTER");
	      
	   // Confirm constraint form re-loads
          waitForPageLoad();
      
       // Constrain the moodCode element
          AddMoodCode(templateType, "SHALL", "1..1", "Single");
         
	   // Add Code value and Display value for the moodCode element
	      AddCodeValueAndDisplay("EVN", "Event");
	      
	   // Add Template ID constraint
	      AddtemplateId("CDA: Entry", "SHALL", "1..1");
	      
	   // Constrain Template ID Root element
	      AddtemplateIdRoot("CDA: Entry", "SHALL", "1..1", "Single", "1.2.3.4.5.6.7");
	      
	   // Constrain Template ID Extension element
	      AddtemplateIdExtension("CDA: Entry", "SHALL", "1..1", "Single", "2017-01-01");
	      
	   // Confirm constraint form re-loads
          waitForPageLoad();
          
       // Constrain the iD element
          AddiD("CDA: Entry", "SHALL", "1..*");
          
   	   // Confirm constraint form re-loads
          waitForPageLoad();
       
       // Constrain the Code element
          AddCode("CDA: Entry", "SHALL", "1..1", "Value");
          
       // Bind a Value Set to the Code Element
   	      BindValueSet("urn:oid:2.16.840.1.113883.3.88.12.80.62", "Vital Sign Result");
   	      
   	   // Constrain the Status element   
   	     AddStatus("CDA: Entry", "SHALL", "1..1", "Single");
   	     
   	  // Constrain the effectiveTime element   
   	     AddEffectiveTime("CDA: Entry", "SHALL", "1..1");
   	     
   	  // Confirm constraint form re-loads
         waitForPageLoad();
         
      // Constrain the Value element   
   	     AddValueUnit("CDA: Entry", "SHALL", "1..1", "Value", "PQ"); 
   	     
   	  // Bind the Value Set to Value Unit
   	     BindValueSet("urn:oid:2.16.840.1.113883.1.11.12839", "UnitsOfMeasureCaseSensitive");
   	     
   	   // Save the Template 
   	      SaveTemplate();
   	   
   	   // Return to the Trifolia Home Page
   	      ReturnHome("Welcome to Trifolia Workbench");   
 }
 
//Create Computed Constraints for the Entry Type Template
@Test
  public void AddEntryOrgCompConstraints(String templateName, String templateOID, String templateIG, String templateType, String permissionUserName) throws Exception {
  
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
		FindTemplate(templateName, templateOID, templateIG, templateType);	 
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
	 				ConfirmTemplateEditor(templateName, templateOID);
	 	 		}
	 
	   // Confirm page completely loads
	       waitForPageLoad();
	    
	   // Open the Constraints Editor
	 	   OpenConstraintsEditor();
	 	   
	   // Confirm the template OID appears
	 	  assertTrue("Could not find \"Template OID\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateOID) >= 0);
	   
	   // Constrain the classCode element
	      AddClassCode(templateType, "SHALL", "1..1", "Single");
	      
	   // Add Code value and Display value for the classCode element
	      AddCodeValueAndDisplay("CLUSTER", "CLUSTER");
	      
	   // Confirm constraint form re-loads
         waitForPageLoad();
     
      // Constrain the moodCode element
         AddMoodCode(templateType, "SHALL", "1..1", "Single");
        
	   // Add Code value and Display value for the moodCode element
	      AddCodeValueAndDisplay("EVN", "Event");
	      
	   // Add Template ID constraint
	      AddtemplateId(templateType, "SHALL", "1..1");
	      
	   // Constrain Template ID Root element
	      AddtemplateIdRoot(templateType, "SHALL", "1..1", "Single", templateOID);
	      
	   // Constrain Template ID Extension element
	      AddtemplateIdExtension(templateType, "SHALL", "1..1", "Single", "2017-01-01");
	      
	   // Confirm constraint form re-loads
         waitForPageLoad();
         
      // Constrain the iD element
         AddiD(templateType, "SHALL", "1..*");
         
  	   // Confirm constraint form re-loads
         waitForPageLoad();
      
      // Constrain the Code element
         AddCode(templateType, "MAY", "1..1", "Value");
      

      // Constrain the Code @code element
         AddCodeCode(templateType, "SHALL", "1..1", "Single");
         
      // Constrain the CodeSystem 
         AddCodeCodeSystem(templateType, "SHALL", "1..1", "Single");
         
//      // Bind a Value Set to the Code Element
//  	     BindValueSet("urn:oid:2.16.840.1.113883.3.88.12.80.62", "Vital Sign Result");
  	      
  	   // Constrain the Status element   
  	     AddStatus(templateType, "SHALL", "1..1", "Single");
  	     
  	  // Constrain the effectiveTime element   
  	     AddEffectiveTime(templateType, "SHALL", "1..1");
  	     
  	  // Confirm constraint form re-loads
          waitForPageLoad();
        
  	   // Save the Template 
  	      SaveTemplate();
  	   
  	   // Return to the Trifolia Home Page
  	      ReturnHome("Welcome to Trifolia Workbench");   
}
 
//Create Computed Constraints for the Document Type Template
@Test
 public void AddDocCompConstraints(String templateName, String templateOID, String templateIG, String templateType, String bindingType, String templateIdRoot, String templateIdExtension, String permissionUserName) throws Exception {
 
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
		FindTemplate(templateName, templateOID, templateIG, templateType);	 
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
	 				ConfirmTemplateEditor(templateName, templateOID);
	 	 		}
	 
	   // Confirm page completely loads
	       waitForPageLoad();
	    
	   // Open the Constraints Editor
	 	   OpenConstraintsEditor();
	 	   
	   // Confirm the template OID appears
	 	  assertTrue("Could not find \"Template OID\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateOID) >= 0);
	   
	       
	   // Constrain the realmCode element
	      AddRealmCode("CDA: Document", "SHALL", "1..1", "Single");
	   
	   // Confirm constraint form re-loads
	      waitForPageLoad();
	    
	   // Constrain the code element     
	      AddCodeValueAndDisplay("US", "");
	      
	   // Confirm constraint form re-loads
        waitForPageLoad();
	      
	   // Constrain the typeId element
        AddtypeId(templateType, "SHALL", "1..1");
        
 	   // Confirm constraint form re-loads
        waitForPageLoad();   
        
     // Constrain the Template ID element 
        AddtemplateId(templateType, "SHALL", "1..1");
       
     // Constrain the Template ID Root element
        AddtemplateIdRoot(templateType, "SHALL", "1..1", "Single", templateIdRoot);

     // Constrain the Template ID Extension element
        AddtemplateIdExtension(templateType, "SHALL", "1..1", "Single", templateIdExtension);
        
     // Confirm constraint form refresh
       waitForPageLoad();
	      	   
	   // Save the Template 
	   SaveTemplate();
	   
	   // Return to the Trifolia Home Page
	   ReturnHome("Welcome to Trifolia Workbench");   
}
 
@Test  
public void AddFHIRCompConstraints(String templateName, String templateOID, String templateIG, String templateType, String permissionUserName) throws Exception {

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
		FindTemplate(templateName, templateOID, templateIG, templateType);	 
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
	 				ConfirmTemplateEditor(templateName, templateOID);
	 	 		}
	 
	   // Confirm page completely loads
	       waitForPageLoad();
	    
	   // Open the Constraints Editor
	 	   OpenConstraintsEditor();
	 	   
	   // Confirm the template OID appears
	 	  assertTrue("Could not find \"Template OID\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateOID) >= 0);
      
	    // Constrain the Type element
	       AddFHIRProfileType(templateType, "SHALL", "1..1", "Single");
	       
	    // Confirm constraint form refresh
	      waitForPageLoad();
	      	   
	   // Save the Template 
	   SaveTemplate();
	   
	   // Return to the Trifolia Home Page
	   ReturnHome("Welcome to Trifolia Workbench");   
}

@Test   
public void AddFHIRSectionConstraint(String templateName, String templateOID, String templateIG, String templateType, String permissionUserName) throws Exception {

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
		FindTemplate(templateName, templateOID, templateIG, templateType);	 
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
	 				ConfirmTemplateEditor(templateName, templateOID);
	 	 		}
	 
	   // Confirm page completely loads
	       waitForPageLoad();
	    
	   // Open the Constraints Editor
	 	   OpenConstraintsEditor();
	 	   
	   // Confirm the template OID appears
	 	  assertTrue("Could not find \"Template OID\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateOID) >= 0);
      
	    // Constrain the Section element
	       AddFHIRSection("SHALL", "1..1");
	       
	    // Confirm constraint form refresh
	      waitForPageLoad();
	      	   
	   // Save the Template 
	   SaveTemplate();
	   
	   // Return to the Trifolia Home Page
	   ReturnHome("Welcome to Trifolia Workbench");   
}

//Add a Primitive Constraint containing schematron 
 @Test   
   public void AddEditPrimConstraint(String templateName, String templateOID, String templateIG, String templateType, String primitiveProse, String bindingType, 
		       String viewMode, String schematronRule, String conformance, String permissionUserName) throws Exception {
     
	 // Open the Template Browser
	    OpenTemplateBrowser();
	    
	 // Find the Template
	 if (permissionUserName == "lcg.admin")
		{
		FindTemplate(templateName, templateOID, templateIG, templateType);
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
			ConfirmTemplateEditor(templateName, templateOID);
			}
        
       // Confirm page completely loads
	      // waitForPageLoad();
       
	   // Open the Constraints Editor
	 	   OpenConstraintsEditor();
	    
	 //Select the "id" constraint
   if (templateType == "CDA: Document")
		{
	    WebDriverWait wait = new WebDriverWait(driver, 60);                             
	    WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[7]/div[2]/span")));
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[7]/div[2]/span")).click();
		}                         
   
   if (templateType == "eMeasure: Document")
		{  
	    WebDriverWait wait = new WebDriverWait(driver, 60);                             
	    WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[7]/div[2]/span")));
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[7]/div[2]/span")).click();
		}
   
   // Confirm page completely loads
       waitForPageLoad();
   
     // Open the constraint editor for the 'id' element       
       driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")).click();
       WebDriverWait wait = new WebDriverWait(driver, 60);                             
	   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")));
       
	   //Confirm the Constraint Editor opens
	     ConfirmConstraintEditor();
	     
	   // Confirm page completely loads
	       waitForPageLoad();
	    
       //Create the "primitive" constraint     
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")).click();
	   
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
	    
	   // Confirm page completely loads
		    waitForPageLoad();
		  
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[1]/div[1]/button")).click();
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[1]/div[1]/ul/li[1]/a")).click();
		      
		   //Confirm template was updated successfully
		   WebDriverWait wait7 = new WebDriverWait(driver, 60);
		   WebElement element7 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("//*[@id=\"footer\"]/div/div[2]/span")));
		   assertTrue("Could not find \"Done Saving\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Done saving.[\\s\\S]*$"));
		   
		// Confirm page completely loads
		    waitForPageLoad();
  
		 //Select the Primitive Constraint
		    WebDriverWait wait8 = new WebDriverWait(driver, 60);                                     
		    WebElement element8 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[6]/div[2]")));
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[6]/div[2]")).click();

		      //Change View Mode to "Analyst"
			 WebDriverWait wait00 = new WebDriverWait(driver, 60);                                     
			 WebElement element00 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[4]/div/select")));
			   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[4]/div/select")).click();
			   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[4]/div/select")).sendKeys(Keys.ARROW_UP);
			   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[4]/div/select")).sendKeys(Keys.ARROW_UP);
			   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[4]/div/select")).click();

		   // Confirm the view mode has been changed
		    WebDriverWait wait9 = new WebDriverWait(driver, 60);               
		  	wait4.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[4]/div/select"), "Analyst"));
		  	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[4]/div/select")).sendKeys(Keys.TAB);    
		  	
      // Update the Conformance
      
	   WebDriverWait wait10 = new WebDriverWait(driver, 60);
	   WebElement element10 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[2]/div/div[1]/select")));
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[2]/div/div[1]/select")).sendKeys(conformance);
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[2]/div/div[1]/select")).sendKeys(Keys.TAB);
		  
	   //Duplicate the 'id' primitive constraint and confirm the new element appears
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[2]")).click();
      //Confirm the duplicated element appears and Select the Duplicated primitive constraint
	   if (templateType == "CDA: Document")
			{
		   WebDriverWait wait11 = new WebDriverWait(driver, 60);                               
		   WebElement element11= wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[7]/div[6]")));
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[7]/div[6]")).click();
			}
	   else if (templateType == "eMeasure: Document")
			{
		   WebDriverWait wait11 = new WebDriverWait(driver, 60);
		   WebElement element11 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[13]/div[6]")));
		    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[13]/div[6]")).click();
			}
   
	      // Delete the Duplicated constraint
	   	  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[4]")).click();
	   	  
	   	  // Save the Template 
		   SaveTemplate();
		   
		   // Return to the Trifolia Home Page
		   ReturnHome("Welcome to Trifolia Workbench"); 
   }
   
// TEST 4: Bind Value Sets to Constraints 
 @Test
   public void DocumentValueSetBinding(String templateName, String templateOID, String templateIG, String templateType, String valueSetOID, String valueSetName, String permissionUserName) throws Exception {
	
	 // Open the Template Browser
	    OpenTemplateBrowser();
	    
	 // Find the Template
	 if (permissionUserName == "lcg.admin")
		{
		FindTemplate(templateName, templateOID, templateIG, templateType);
		}
	
	 //Open the Template Editor
		   WebDriverWait wait = new WebDriverWait(driver, 60);
		   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[6]/div/a[2]")));
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[6]/div/a[2]")).click();

	   // Confirm the correct template appears in the Editor
			ConfirmTemplateEditor("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10");

       // Confirm page completely loads
	      waitForPageLoad();
	    
	    // Open the Constraints Editor
	      OpenConstraintsEditor();
	    
   // Select and open the 'code' element 
	 if (templateType == "CDA: Document")
		{
		   WebDriverWait wait1 = new WebDriverWait(driver, 60);
		   WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[2]")));
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[2]")).click();
		}
	 else if (templateType == "eMeasure: Document")
	 	{
		   WebDriverWait wait1 = new WebDriverWait(driver, 60);
		   WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[2]/span")));
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[8]/div[2]/span")).click();
	 	} 
	 
	 // Open the constraint editor for the selected constraint
	 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")).click();
     WebDriverWait wait1 = new WebDriverWait(driver, 60);                             
	 WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")));

	   //Confirm the Constraint Editor box opens
	     ConfirmConstraintEditor();
 
	 // Confirm page completely loads
	    waitForPageLoad();
		    
	 // Set the Binding Type	    
	   SetBindingType("Value");
	
	 // Bind the Value Set
	   BindValueSet("2.16.840.1.113883.3.2898.11.22","Cause of Injury");
	    	
 	  // Save the Template 
	   SaveTemplate();
	   
	   // Return to the Trifolia Home Page
	   ReturnHome("Welcome to Trifolia Workbench"); 
}
	
 @Test
 public void DocumentTemplateBinding(String templateName, String templateOID, String templateIG, String templateType, String boundTemplateOID, String boundTemplateName, String permissionUserName) throws Exception {
	
	 // Open the Template Browser
	    OpenTemplateBrowser();
	    
	 // Find the Template
		FindTemplate(templateName, templateOID, templateIG, templateType);
	
	 //Open the Template Editor

		   WebDriverWait wait = new WebDriverWait(driver, 60);
		   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[6]/div/a[2]")));
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[6]/div/a[2]")).click();

	   // Confirm the correct template appears in the Editor
			ConfirmTemplateEditor(templateName, templateOID);
    
       // Confirm page completely loads
	    waitForPageLoad();
	    
	   // Open the Constraints Editor
	    OpenConstraintsEditor();	
	    
	  //Scroll to the bottom of the page
	 	((JavascriptExecutor)driver).executeScript("scroll(0,400)");
	    
	if (templateType == "CDA: Document")
		{	
	// Navigate to the Section constraint and confirm the Component element is available and expand the element 
      WebDriverWait wait1 = new WebDriverWait(driver, 60);                               
	  WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[31]/div[1]/span[1]/div")));
	  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[31]/div[1]/span[1]/div")).click();  
		  
	  	//Scroll to the bottom of the page
 	   ((JavascriptExecutor)driver).executeScript("scroll(0,400)");
 	   
	// Confirm the StructuredBody element is available, expand the 'structuredBody' element
	   WebDriverWait wait2 = new WebDriverWait(driver, 60);
	   WebElement element2 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[32]/div[8]/div[1]/span[1]/div")));	
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[32]/div[8]/div[1]/span[1]/div")).click();
	   
	  	//Scroll to the bottom of the page
  	   ((JavascriptExecutor)driver).executeScript("scroll(0,400)");
  	   
	// Confirm the Component element is available, expand the 'component' element
	   WebDriverWait wait3 = new WebDriverWait(driver, 60);
       WebElement element3 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[32]/div[9]/div[9]/div[1]/span[1]/div")));	
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[32]/div[9]/div[9]/div[1]/span[1]/div")).click();

	  	//Scroll to the bottom of the page
  	   ((JavascriptExecutor)driver).executeScript("scroll(0,400)");
  	   
	 // Select and open 'section' element
	   WebDriverWait wait4 = new WebDriverWait(driver, 60);                             
	   WebElement element4 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[32]/div[9]/div[10]/div[7]/div[2]/span[7]")));
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[32]/div[9]/div[10]/div[7]/div[2]/span[7]")).click();
	
	// Open the 'section' element
	   WebDriverWait wait5 = new WebDriverWait(driver, 60); 
	   WebElement element5 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")));
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")).click();
	}
	
	else if (templateType == "eMeasure: Document")
 	{
		 //Expand the 'component' element 
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[23]/div[1]/span[1]/div")).click();  
	    WebDriverWait wait0 = new WebDriverWait(driver, 60);
		WebElement element0 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[24]/div[7]/div[2]/span[3]")));
					
		// Select the 'section' element
		WebDriverWait wait1 = new WebDriverWait(driver, 60);
		WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[24]/div[7]/div[2]/span[3]")));
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[1]/div[2]/div[24]/div[7]/div[2]/span[3]")).click();
		
		// Open the Section element
		WebDriverWait wait2 = new WebDriverWait(driver, 60);
		WebElement element2 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")));
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/table/tbody/tr/td[5]/div/div/button[1]")).click();	
 	}
	
	// Open the Template Selector
		WebDriverWait wait6 = new WebDriverWait(driver, 60);                               
		WebElement element6 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/a")));	
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/a")).click();
		
   // Confirm the Template Selector form opens
		 WebDriverWait wait7 = new WebDriverWait(driver, 60);                    
  	     wait6.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[4]/div/div/div[1]/h4"), "Select template/profile..."));
	  
	// Enter the Template OID in the Selector and click Search	
  	   WebDriverWait wait8 = new WebDriverWait(driver, 60);                               
	   WebElement element8 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[4]/div/div/div[2]/div/input")));	
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[4]/div/div/div[2]/div/input")).sendKeys(boundTemplateOID);
	   WebDriverWait wait9 = new WebDriverWait(driver, 60);                               
	   WebElement element9 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[4]/div/div/div[2]/div/div/button")));	
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[4]/div/div/div[2]/div/div/button")).click();
		
    // Confirm the correct Template appears in the Selector
		 WebDriverWait wait10 = new WebDriverWait(driver, 60);                    
  	     wait10.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[4]/div/div/div[2]/table/tbody/tr/td[1]/span"), boundTemplateName));
  	     WebDriverWait wait11 = new WebDriverWait(driver, 60);                    
	     wait11.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[4]/div/div/div[2]/table/tbody/tr/td[2]"), boundTemplateOID));
	
	// Click Select
	     WebDriverWait wait12 = new WebDriverWait(driver, 60);                               
		 WebElement element12 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[4]/div/div/div[2]/table/tbody/tr/td[4]/button")));	
	     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/div[4]/div/div/div[2]/table/tbody/tr/td[4]/button")).click();
			
	// Confirm the bound template appears in the Constraint Editor
	     WebDriverWait wait13 = new WebDriverWait(driver, 60);                    
  	     wait13.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/table/tbody/tr/td[1]/span[1]"), boundTemplateName));
  	     WebDriverWait wait14= new WebDriverWait(driver, 60);                    
	     wait14.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[2]/div/div[1]/table/tbody/tr/td[1]/span[2]"), boundTemplateOID));
		
		  // Save the Template 
		   SaveTemplate();
		   
		   // Return to the Trifolia Home Page
		   ReturnHome("Welcome to Trifolia Workbench"); 
   }
   
// TEST 5: Preview and Validate Template Constraints
 @Test
   public void PreviewAndValidateConstraint(String templateName, String templateOID, String templateIG, String templateType, String constraint1, String constraint2, String constraint3, String constraint4, 
		   String constraint5, String validationText, String permissionUserName) throws Exception {
	   
	 // Open the Template Browser
	    OpenTemplateBrowser();
	    
	 // Find the Template
		FindTemplate(templateName, templateOID, templateIG, templateType);
	
	 //Open the Template Editor

		   WebDriverWait wait = new WebDriverWait(driver, 60);
		   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[6]/div/a[2]")));
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[6]/div/a[2]")).click();

	   // Confirm the correct template appears in the Editor
			ConfirmTemplateEditor(templateName, templateOID);
 
       // Confirm page completely loads
	    waitForPageLoad();
	    
	   // Open the Constraints Editor
	    OpenConstraintsEditor();
    
	 // Open the Preview Pane 
		 WebDriverWait wait0 = new WebDriverWait(driver, 60);                               
		 WebElement element0 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[3]/a")));	
	      driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[3]/a")).click();
	    // driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[3]/a")).click();
	    
	    // Confirm the Preview form opens
	    WebDriverWait wait1 = new WebDriverWait(driver, 60);
	    WebElement element1 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[3]/div/div/p[1]/span[2]")));
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
	   WebDriverWait wait2 = new WebDriverWait(driver, 60);
	   WebElement element2 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("validation")));
	   //assertTrue("Could not find \"Validation\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Warnings[\\s\\S]*$"));
	   
	   //Confirm the validation text appears    
	   WebDriverWait wait3 = new WebDriverWait(driver, 60);               
	   wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[4]/div[1]/div/span"), validationText));
	   assertTrue("Could not find \"Validation Text\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(validationText) >= 0);
   
	   // Cancel and View the Template
	   WebDriverWait wait4 = new WebDriverWait(driver, 60);
	   WebElement element4 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[1]/div[2]/button")));
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[1]/div[2]/button")).click();
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[1]/div[2]/ul/li[2]/a")).click();

	   ConfirmTemplateViewer(templateName, templateOID);
	   	   	   	
	   // Confirm the Back to List option is available, and Return to the Template Browser 
	   WebDriverWait wait5 = new WebDriverWait(driver, 60);
	   WebElement element5 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[1]/a")));
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[1]/a")).click();                         
	 
	   //Confirm the Template Browser appears
	   WebDriverWait wait6 = new WebDriverWait(driver, 60);
	   WebElement element6 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("BrowseTemplates")));
	   assertTrue("Could not find \"Browse Templates\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Browse Templates[\\s\\S]*$"));
	     
	   // Confirm the Browser is loaded
	    WebDriverWait wait7 = new WebDriverWait(driver, 60);               
	   	wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]"), "Page 1 of 1, 1 templates/profiles"));
	  
	   //Clear the existing Search Criteria
	     ClearExistingSearch();
	    
	     // Return to the Trifolia Home Page
	     ReturnHome("Welcome to Trifolia Workbench");  
 }
 
}
