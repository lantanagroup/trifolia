package trifolia.pkg;

import java.util.regex.Pattern;
import java.util.concurrent.TimeUnit;

import org.junit.*;

import static org.junit.Assert.*;

import org.openqa.selenium.*;
import org.openqa.selenium.firefox.FirefoxDriver;
import org.openqa.selenium.firefox.FirefoxProfile;
import org.openqa.selenium.firefox.internal.ProfilesIni;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.Select;
import org.openqa.selenium.support.ui.WebDriverWait;

@Ignore
public class D_TemplateFunctions {
  private WebDriver driver;
  private String baseUrl;
  private boolean acceptNextAlert = true;
  private StringBuffer verificationErrors = new StringBuffer();
  public D_TemplateFunctions() {}

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
	  	WebDriverWait wait = new WebDriverWait(driver, 120);
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
 	   
 	  // Wait for page to fully load
 	     waitForPageLoad();
 	     
	   // Confirm search criteria is cleared
	  WebDriverWait wait1 = new WebDriverWait(driver, 60);                    
	  wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[2]/td[5]")));        
  }
  
  public void FindTemplate(String templateName, String templateOID, String templateIG) throws Exception 
	{
	  // Wait for page to fully load
	  waitForPageLoad();
	  
	  // Wait for Bindings to complete
	     waitForBindings("BrowseTemplates");
	  
	  // Clear the existing search criteria
	      ClearExistingSearch();
		 
      // Wait for page to fully load
	  waitForPageLoad();
		  
	   //Search for the Template /html/body/div[2]/div/div/form/div/span
	    WebDriverWait wait = new WebDriverWait(driver, 60);
		WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/form/div/input")));
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/input")).sendKeys(templateName);
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/input")).sendKeys(Keys.TAB);
	   
	// Cllck the Search button
	     WebDriverWait wait1 = new WebDriverWait(driver, 60);
		 WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/form/div/span/button[2]")));
		 driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/span/button[2]")).click();
	  
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
		     WebDriverWait wait1 = new WebDriverWait(driver, 120);
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
		  
		// Wait for Bindings to complete
		   waitForBindings("TemplateEditor");
		  
	   // Confirm the Template Editor opens
	       ////driver.manage().timeouts().pageLoadTimeout(30, TimeUnit.SECONDS);
		   WebDriverWait wait = new WebDriverWait(driver, 60);
		   wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[1]/a"), "Template/Profile"));
	       assertTrue("Could not find \"Template/Profile\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Template/Profile[\\s\\S]*$"));
		   
		   // Wait for page to fully load
		   WebDriverWait wait1 = new WebDriverWait(driver, 60);		
		   wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[5]"))); 
	    
		   // Confirm the the correct Template Name appears in the Editor
		   ////driver.manage().timeouts().pageLoadTimeout(30, TimeUnit.SECONDS);
		   WebDriverWait wait2 = new WebDriverWait(driver, 60);
		   wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/h3/span[2]"), templateName));
		   assertTrue("Could not find \"Template Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateName) >= 0);
		   
		   // Confirm the the correct Template OID appears in the Editor
		   ////driver.manage().timeouts().pageLoadTimeout(30, TimeUnit.SECONDS);
		   WebDriverWait wait3 = new WebDriverWait(driver, 60);                                  
		   wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/h4/span"), templateOID));
		   assertTrue("Could not find \"Template OID\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateOID) >= 0);	
	}	

  public void SaveTemplate() throws Exception 
  {
  	 // Confirm page completely loads
  	    waitForPageLoad();
  	  
  	   // Save the Template
  	   WebDriverWait wait = new WebDriverWait(driver, 60);
 	   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[1]/div[1]/button")));
  	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[1]/div[1]/button")).click();
  	   WebDriverWait wait1 = new WebDriverWait(driver, 60);
	   WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[1]/div[1]/ul/li[1]/a")));
  	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[1]/div[1]/ul/li[1]/a")).click();
  	      
  	   //Confirm template was updated successfully
  	   WebDriverWait wait2 = new WebDriverWait(driver, 60);
  	   WebElement element2 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("//*[@id=\"footer\"]/div/div[2]/span")));
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


@Test
//TEST 1: View an existing Template
    public void ViewTemplate(String templateName, String templateOID, String templateIG, String templateType, String containedTemplate, String linkedTemplateOID, 
    		String verificationText, String permissionUserName) throws Exception {
    
	// Open the Template Browser
	   OpenTemplateBrowser();
	
	// Find the Template
	if (permissionUserName == "lcg.admin") 
	{
	FindTemplate(templateName, templateOID, templateIG);	 
	}
	
	if (permissionUserName == "lcg.user") 
	{
	FindTemplate(templateName, templateOID, templateIG);	 
	}
	
	// Clear main Search box
    driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")).click();
    
     // Wait for page to re-load
	  waitForPageLoad();
	  
    if (permissionUserName == "lcg.admin") 
    	{
	      // Filter by Organization
		    driver.findElement(By.xpath("//*[@id=\"BrowseTemplates\"]/table/thead/tr[2]/td[5]/select")).sendKeys("LCG");
		    driver.findElement(By.xpath("//*[@id=\"BrowseTemplates\"]/table/thead/tr[2]/td[5]/select")).sendKeys(Keys.RETURN);
            
		    // Wait for page to re-load
			  waitForPageLoad();
			  
		    // Confirm Templates for the selected Organization appear
		    WebDriverWait wait = new WebDriverWait(driver, 60);                    
		   	wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[1]/td[5]"), "LCG"));
		    
		    // Confirm Templates for other Organizations do not appear
		    assertFalse("Found\"HL7\" on page.", driver.findElement(By.cssSelector("tbody")).getText().matches("^[\\S]*HL7[\\S]*$"));
		    
	    // Filter by Template Name 
		    // Clear the existing Filter        
		    driver.findElement(By.xpath("//*[@id=\"SearchQuery\"]")).clear();
		    driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[2]/td[6]/button")).click();
		    WebDriverWait wait8 = new WebDriverWait(driver, 60);                     
		   	wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/thead/tr[2]/td[5]/select"), "Filter..."));
	  	    
		    // Wait for page to re-load
			  waitForPageLoad();
			  
		    //Send the new filter criteria
	  	    driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[2]/td[1]/input")).click();
	  	    driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[2]/td[1]/input")).sendKeys(templateName); 
	  	    
	  	  // Wait for page to re-load
	  	     waitForPageLoad();
	  	  
	  	  if (templateType == "CDA")
	  	    {
	  	    // Confirm the correct Template OID is returned
	  	    WebDriverWait wait9 = new WebDriverWait(driver, 60);                    
		   	wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[2]"), templateOID));
	  	    assertTrue("Could not find \"Template OID\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateOID) >= 0);
	  	    }
	  	    
	  	  if (templateType == "FHIR")
	  	    {
	  	    // Confirm the correct Template IG is returned
	  	    WebDriverWait wait9 = new WebDriverWait(driver, 60);                    
		   	wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[3]"), templateIG));
	  	    assertTrue("Could not find \"Template IG\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateIG) >= 0);
	  	    }
	  	  
  	    // Filter by Template OID
	  	    // Clear Filter
	  	    driver.findElement(By.xpath("//*[@id=\"SearchQuery\"]")).clear();
	  	    driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[2]/td[6]/button")).click();
	  	    
	  	  // Wait for page to re-load
	  	     waitForPageLoad();
	  	  
	  	    // Enter new search criteria
	  	    driver.findElement(By.xpath("//*[@id=\"BrowseTemplates\"]/table/thead/tr[2]/td[2]/input")).click();
	  	    driver.findElement(By.xpath("//*[@id=\"BrowseTemplates\"]/table/thead/tr[2]/td[2]/input")).sendKeys(templateOID);

	  	  // Wait for page to re-load
	  	     waitForPageLoad();
	  	     
	  	    // Confirm the correct Template Name appears
	  	    WebDriverWait wait10 = new WebDriverWait(driver, 60);                     
		   	wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[1]/span"), templateName));
	  	    assertTrue("Could not find \"Template Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateName) >= 0);
    	}
	  else
	  	{
		    // Filter by Template Name
			driver.findElement(By.xpath("//*[@id=\"SearchQuery\"]")).clear(); 
		    driver.findElement(By.xpath("//*[@id=\"BrowseTemplates\"]/table/thead/tr[2]/td[1]/input")).click();
		    driver.findElement(By.xpath("//*[@id=\"BrowseTemplates\"]/table/thead/tr[2]/td[1]/input")).sendKeys(templateName);
		    
		    // Wait for page to re-load
			  waitForPageLoad();
			  
		    // Confirm the search is complete
			   WebDriverWait wait2 = new WebDriverWait(driver, 60);
			   wait2.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]"),"Page 1 of 1, 1 templates/profiles"));	  
		    
		    // Confirm the correct Template OID appears
		    WebDriverWait wait = new WebDriverWait(driver, 60);                     
		   	wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[2]"), templateOID));	    
		    assertTrue("Could not find \"Template OID\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateOID) >= 0);
		    
		    // Clear Filter               
		    if (permissionUserName == "lcg.admin") 
		    {
		    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[2]/td[5]/button")).click();
		    }
		    if (permissionUserName == "lcg.user") 
		    {
		    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[2]/td[5]/button")).click();
		    }
		    
		    // Wait for page to re-load
			  waitForPageLoad();
			  
		      // Filter by Template OID
		    driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[2]/td[2]/input")).click();
		    driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[2]/td[2]/input")).sendKeys(templateOID);
	
		    // Wait for page to re-load
			  waitForPageLoad();
			  
		    // Confirm the correct Template OID is returned
		    //driver.manage().timeouts().pageLoadTimeout(30, TimeUnit.SECONDS);
		    WebDriverWait wait5 = new WebDriverWait(driver, 120);                      
		   	wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[2]"), templateOID));
		    assertTrue("Could not find \"Template OID\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateOID) >= 0);
	  	}
    // Open the Template Viewer
		    if (permissionUserName == "lcg.admin") 
		    {
		    	WebDriverWait wait = new WebDriverWait(driver, 60);
		    	WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[6]/div/a[1]")));	    	  
		    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[6]/div/a[1]")).click();
		    }
		   		    
		    else if (permissionUserName == "lcg.user")
		    {
		    	WebDriverWait wait = new WebDriverWait(driver, 60);
		    	WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[1]/td[5]/div/a[1]")));	    	  
		    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[1]/td[5]/div/a[1]")).click();
		    }
    // Confirm the Template Viewer opens
		
		    // Wait for page to re-load
			  waitForPageLoad();		  

	    	ConfirmTemplateViewer(templateName, templateOID);

	    // Confirm the correct text appears for the selected template
	       assertTrue("Could not find Template Verification Text on page\"Constraint Text\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(verificationText) >= 0);
	   
	    // Wait for page to re-load
		  waitForPageLoad();
		  
    // Check Template Relationships
	
     if(templateType == "CDA")
		{
	    	 WebDriverWait wait = new WebDriverWait(driver, 60);
			 WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/ul/li[2]/a")));
	    	 driver.findElement(By.xpath("/html/body/div[2]/div/div/ul/li[2]/a")).click();  
	    	 
	    	 if (permissionUserName == "lcg.admin") 
			    {
	    		 WebDriverWait wait5 = new WebDriverWait(driver, 120);                       
				 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[3]/div[2]/div[4]/ul/li/a/span[1]"), containedTemplate));
		 	     assertTrue("Could not find \"Contained Template\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(containedTemplate) >= 0);
			    }
			   		    
		    if (permissionUserName == "lcg.user")
		    {
		    	 WebDriverWait wait5 = new WebDriverWait(driver, 120);                       
				 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[3]/div[2]/div[3]/ul/li[1]/span"), containedTemplate));
		 	     assertTrue("Could not find \"Contained Template\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(containedTemplate) >= 0);
			}
    	  
	 	    // Click on a linked Template
		   	 if (permissionUserName == "lcg.admin") 
			    {
		   		WebDriverWait wait1 = new WebDriverWait(driver, 60);
		 		 WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("//*[@id=\"references\"]/div[4]/ul/li/a/span[1]")));
		 		 driver.findElement(By.xpath("//*[@id=\"references\"]/div[4]/ul/li/a/span[1]")).click();
		 		 
		 		   // Confirm the correct template appears
			 	    WebDriverWait wait2 = new WebDriverWait(driver, 60);
			 	 	wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[2]/div/h4"), linkedTemplateOID));
			 		assertTrue("Could not find \"Linked Template OID\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(linkedTemplateOID) >= 0);
			 	    
			 		// Click on the Relationships Tab
			 		 WebDriverWait wait3 = new WebDriverWait(driver, 60);
			 		 WebElement element3 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/ul/li[2]/a")));
			 		 driver.findElement(By.xpath("/html/body/div[2]/div/div/ul/li[2]/a")).click();
			 		   
			 		// Confirm the Parent Template appears
			 		 WebDriverWait wait4 = new WebDriverWait(driver, 60);
			 		 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[3]/div[2]/div[3]/ul/li[1]/a/span[2]"), templateOID));
			 	     assertTrue("Could not find \"Parent Template OID\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateOID) >= 0);  	 
			    }
			   		    
		}
     
     if(templateType == "FHIR")
	 	{
	 	     // Click on the Relationships tab
	    	 WebDriverWait wait = new WebDriverWait(driver, 60);
			 WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/ul/li[3]/a")));
	     	 driver.findElement(By.xpath("/html/body/div[2]/div/div/ul/li[3]/a")).click();  
	     	 
	     	 // Confirm the Implied Template appears
	     	 WebDriverWait wait5 = new WebDriverWait(driver, 120);                       
			 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[3]/div[3]/div[1]/ul/li/a/span[1]"), containedTemplate));
	 	     assertTrue("Could not find \"Implied Template\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(containedTemplate) >= 0);
	 	} 

	    //Return to Template Listing and confirm the correct form opens
	     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[1]/a")).click();
	     WebDriverWait wait = new WebDriverWait(driver, 60);
	     WebElement element1 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("BrowseTemplates")));
	     
	    // Confirm the page is fully loaded
	       waitForPageLoad();
	       
	       // Wait for Bindings to complete
		     waitForBindings("BrowseTemplates");
		     
	    // Confirm the Template Listing appears
		  WebDriverWait wait2 = new WebDriverWait(driver, 60);                    
		  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[2]"), "Identifier"));
		 
		  // Wait for page to re-load
		     waitForPageLoad();
		     
		    
		 // Clear existing Search criteria
		    ClearExistingSearch();	    
    
    // Return to the Trifolia Home Page
    ReturnHome("Welcome to Trifolia Workbench!");   
}
   
// Create a New "Entry" Observation Type Template
@Test
public void CreateEntryObsTemplate(String templateName, String templateOID, String templateIG, String templateType, String templateText,
			String permissionUserName) throws Exception {
		
		// Open the Template Browser
		   OpenTemplateBrowser(); 
		   
		// Confirm the page is fully loaded
	  waitForPageLoad();
	  
		// Clear existing Search criteria
		    ClearExistingSearch();  
		    
		 // Confirm the page is re-loaded
		       waitForPageLoad();
		       
	//Open the Template Editor
	if (permissionUserName == "lcg.admin") 
	{
	   WebDriverWait wait = new WebDriverWait(driver, 60);
		    WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[6]/div/a")));   
		driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[6]/div/a")).click();       	
	}
	
	else
	{
		WebDriverWait wait = new WebDriverWait(driver, 60);
		    WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[5]/div/a")));   
		driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[5]/div/a")).click();
	}
	
		 // Confirm the page is fully loaded
		    waitForPageLoad();
		    
		    // Wait for Bindings to complete
		     waitForBindings("TemplateEditor");
		     
	  // Confirm the Template Editor Opens
	      WebDriverWait wait = new WebDriverWait(driver, 60);                    
		  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/span"), "Owning Implementation Guide is required"));
		  assertTrue("Could not find \"Owning Implementation Guide is required\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Owning Implementation Guide is required[\\s\\S]*$"));
	
		  WebDriverWait wait1 = new WebDriverWait(driver, 60);                    
		  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[3]/div/div[1]/div/div/span[1]"), "Quick Edit"));
		  assertTrue("Could not find \"Quick Edit\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Quick Edit[\\s\\S]*$"));
	
		  WebDriverWait wait2 = new WebDriverWait(driver, 60);                    
		  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[4]/div/span"), "View Mode"));
		  assertTrue("Could not find \"View Mode\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*View Mode[\\s\\S]*$"));
	
	// Add Template Meta Data
			
		// Confirm list of IG's appears
		 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select")).click();  
		 WebDriverWait wait3 = new WebDriverWait(driver, 60);                    
		 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select/option[2]"), templateIG));
	
		// Add Owning IG
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select")).click();
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select")).sendKeys(Keys.ARROW_DOWN);
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select")).click();
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select")).click();
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select")).sendKeys(Keys.TAB);
	
	
	if(templateIG == "1Automation Test IG")
		{
		
		// Add Template Type
		WebDriverWait wait4 = new WebDriverWait(driver, 60);
		WebElement element4 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")));
		assertTrue("Could not find \"Type\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Type:[\\s\\S]*$"));
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).click();
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).click();
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.TAB);
		
		// Add Applies To Template Type
		// Open Applies To Template Selector
		WebDriverWait wait5 = new WebDriverWait(driver, 60);
		WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[3]/div[2]/button")));
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[3]/div[2]/button")).click();
		
		// Confirm form opens and select an Applies To Template Type
		 WebDriverWait wait6 = new WebDriverWait(driver, 60);                    
		 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[4]/div/div/div[1]"), "Applies to..."));	 
		 WebDriverWait wait7 = new WebDriverWait(driver, 60);
	     WebElement element7 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[4]/div/div/div[2]/div/div[6]/div[2]/span")));
	     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[4]/div/div/div[2]/div/div[6]/div[2]/span")).click();

		//Click OK to add Applies To Template and return to the Template Editor
	     WebDriverWait wait8 = new WebDriverWait(driver, 60);
	     WebElement element8 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[4]/div/div/div[3]/button[1]")));
	     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[4]/div/div/div[3]/button[1]")).click();
		}
	
	if(templateIG == "1FHIR Test IG")
		{
	     // Add Template Type
			WebDriverWait wait4 = new WebDriverWait(driver, 60);
			WebElement element4 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")));
			assertTrue("Could not find \"Type\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Type:[\\s\\S]*$"));
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).click();
		    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).click();
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.TAB);	
		}
	
	// Wait for new field to load and add Template Name
	WebDriverWait wait9 = new WebDriverWait(driver, 60);
	WebElement element9 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")));
	assertTrue("Could not find \"Type\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Name:[\\s\\S]*$"));
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[1]/input")).sendKeys(templateName);
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[1]/input")).sendKeys(Keys.TAB);
	
	// Add Template OID
	WebDriverWait wait10 = new WebDriverWait(driver, 60);
	WebElement element10 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[1]/input")));
	assertTrue("Could not find \"Type\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Type:[\\s\\S]*$"));
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[2]/input[1]")).sendKeys(templateOID);
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[2]/input[1]")).sendKeys(Keys.TAB);
	  
	// Add Template Description
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[7]/textarea")).sendKeys(templateText);
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[7]/textarea")).sendKeys(Keys.TAB);
	
	// Save the Template
	SaveTemplate();
	
	// Return to the Trifolia Home Page
	ReturnHome("Welcome to Trifolia Workbench");  
}

//Create a New "Entry" Observation Type Template
@Test
public void CreateEntryOrgTemplate(String templateName, String templateOID, String templateIG, String templateType, String templateText,
			String permissionUserName) throws Exception {
		
		// Open the Template Browser
		   OpenTemplateBrowser(); 
		   
		// Confirm the page is fully loaded
	  waitForPageLoad();
	  
		// Clear existing Search criteria
		    ClearExistingSearch();  
		    
		 // Confirm the page is re-loaded
		       waitForPageLoad();
		       
	//Open the Template Editor
	if (permissionUserName == "lcg.admin") 
	{
	   WebDriverWait wait = new WebDriverWait(driver, 60);
		    WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[6]/div/a")));   
		driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[6]/div/a")).click();       	
	}
	
	else
	{
		WebDriverWait wait = new WebDriverWait(driver, 60);
		    WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[5]/div/a")));   
		driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[5]/div/a")).click();
	}
	
		 // Confirm the page is fully loaded
		    waitForPageLoad();
		    
		    // Wait for Bindings to complete
		     waitForBindings("TemplateEditor");
		     
	  // Confirm the Template Editor Opens
	      WebDriverWait wait = new WebDriverWait(driver, 60);                    
		  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/span"), "Owning Implementation Guide is required"));
		  assertTrue("Could not find \"Owning Implementation Guide is required\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Owning Implementation Guide is required[\\s\\S]*$"));
	
		  WebDriverWait wait1 = new WebDriverWait(driver, 60);                    
		  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[3]/div/div[1]/div/div/span[1]"), "Quick Edit"));
		  assertTrue("Could not find \"Quick Edit\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Quick Edit[\\s\\S]*$"));
	
		  WebDriverWait wait2 = new WebDriverWait(driver, 60);                    
		  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[4]/div/span"), "View Mode"));
		  assertTrue("Could not find \"View Mode\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*View Mode[\\s\\S]*$"));
	
	// Add Template Meta Data
			
		// Confirm list of IG's appears
		 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select")).click();  
		 WebDriverWait wait3 = new WebDriverWait(driver, 60);                    
		 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select/option[2]"), templateIG));
	
		// Add Owning IG
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select")).click();
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select")).sendKeys(Keys.ARROW_DOWN);
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select")).click();
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select")).click();
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select")).sendKeys(Keys.TAB);
	
	
	if(templateIG == "1Automation Test IG")
		{
		
		// Add Template Type
		WebDriverWait wait4 = new WebDriverWait(driver, 60);
		WebElement element4 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")));
		assertTrue("Could not find \"Type\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Type:[\\s\\S]*$"));
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).click();
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).click();
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.TAB);
		
		// Add Applies To Template Type
		// Open Applies To Template Selector
		WebDriverWait wait5 = new WebDriverWait(driver, 60);
		WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[3]/div[2]/button")));
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[3]/div[2]/button")).click();
		
		// Confirm form opens and select an Applies To Template Type
		 WebDriverWait wait6 = new WebDriverWait(driver, 60);                    
		 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[4]/div/div/div[1]"), "Applies to..."));	 
		 WebDriverWait wait7 = new WebDriverWait(driver, 60);
	     WebElement element7 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[4]/div/div/div[2]/div/div[8]/div[2]/span")));
	     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[4]/div/div/div[2]/div/div[8]/div[2]/span")).click();

		//Click OK to add Applies To Template and return to the Template Editor
	     WebDriverWait wait8 = new WebDriverWait(driver, 60);
	     WebElement element8 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[4]/div/div/div[3]/button[1]")));
	     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[4]/div/div/div[3]/button[1]")).click();
		}
	
	if(templateIG == "1FHIR Test IG")
		{
	     // Add Template Type
			WebDriverWait wait4 = new WebDriverWait(driver, 60);
			WebElement element4 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")));
			assertTrue("Could not find \"Type\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Type:[\\s\\S]*$"));
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).click();
		    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).click();
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.TAB);	
		}
	
	// Wait for new field to load and add Template Name
	WebDriverWait wait9 = new WebDriverWait(driver, 60);
	WebElement element9 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")));
	assertTrue("Could not find \"Type\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Name:[\\s\\S]*$"));
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[1]/input")).sendKeys(templateName);
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[1]/input")).sendKeys(Keys.TAB);
	
	// Add Template OID
	WebDriverWait wait10 = new WebDriverWait(driver, 60);
	WebElement element10 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[1]/input")));
	assertTrue("Could not find \"Type\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Type:[\\s\\S]*$"));
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[2]/input[1]")).sendKeys(templateOID);
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[2]/input[1]")).sendKeys(Keys.TAB);
	  
	// Add Template Description
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[7]/textarea")).sendKeys(templateText);
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[7]/textarea")).sendKeys(Keys.TAB);
	
	// Save the Template
	SaveTemplate();
	
	// Return to the Trifolia Home Page
	ReturnHome("Welcome to Trifolia Workbench");  

}
//Create a New "Section" Type Template
@Test
 public void CreateSectionTemplate(String templateName, String templateOID, String templateIG, String templateType, String templateText,
			String permissionUserName) throws Exception {
	
	// Open the Template Browser
	   OpenTemplateBrowser(); 
	   
	// Confirm the page is fully loaded
    waitForPageLoad();
    
	// Clear existing Search criteria
	    ClearExistingSearch();  
	    
	 // Confirm the page is re-loaded
	       waitForPageLoad();
	       
 //Open the Template Editor
 if (permissionUserName == "lcg.admin") 
 {
     WebDriverWait wait = new WebDriverWait(driver, 60);
	 WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[6]/div/a")));   
 	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[6]/div/a")).click();       	
 }
 
 else
 {
 	WebDriverWait wait = new WebDriverWait(driver, 60);
	    WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[5]/div/a")));   
 	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[5]/div/a")).click();
 }
 
	 // Confirm the page is fully loaded
	    waitForPageLoad();
	    
	    // Wait for Bindings to complete
	     waitForBindings("TemplateEditor");
	     
    // Confirm the Template Editor Opens
      WebDriverWait wait = new WebDriverWait(driver, 60);                    
	  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/span"), "Owning Implementation Guide is required"));
	  assertTrue("Could not find \"Owning Implementation Guide is required\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Owning Implementation Guide is required[\\s\\S]*$"));
 
	  WebDriverWait wait1 = new WebDriverWait(driver, 60);                    
	  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[3]/div/div[1]/div/div/span[1]"), "Quick Edit"));
	  assertTrue("Could not find \"Quick Edit\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Quick Edit[\\s\\S]*$"));

	  WebDriverWait wait2 = new WebDriverWait(driver, 60);                    
	  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[4]/div/span"), "View Mode"));
	  assertTrue("Could not find \"View Mode\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*View Mode[\\s\\S]*$"));

 // Add Template Meta Data
		
	// Confirm list of IG's appears
	 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select")).click();  
	 WebDriverWait wait3 = new WebDriverWait(driver, 60);                    
	 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select/option[2]"), templateIG));

	// Add Owning IG
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select")).click();
	 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select")).sendKeys(Keys.ARROW_DOWN);
	 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select")).click();
	 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select")).click();
	 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select")).sendKeys(Keys.TAB);

 // Add Template Type
 WebDriverWait wait4 = new WebDriverWait(driver, 60);
 WebElement element4 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")));
 assertTrue("Could not find \"Type\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Type:[\\s\\S]*$"));
 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).click();
 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).click();
 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.TAB);

 // Wait for new field to load and add Template Type
 WebDriverWait wait5 = new WebDriverWait(driver, 60);
 WebElement element5 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")));
 assertTrue("Could not find \"Type\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Name:[\\s\\S]*$"));
 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[1]/input")).sendKeys(templateName);
 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[1]/input")).sendKeys(Keys.TAB);

 // Add Template OID
 WebDriverWait wait6 = new WebDriverWait(driver, 60);
 WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[1]/input")));
 assertTrue("Could not find \"Type\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Type:[\\s\\S]*$"));
 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[2]/input[1]")).sendKeys(templateOID);
 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[2]/input[1]")).sendKeys(Keys.TAB);
    
 // Add Template Description
 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[7]/textarea")).sendKeys(templateText);
 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[7]/textarea")).sendKeys(Keys.TAB);
 
 // Save the Template
 SaveTemplate();
 
 // Return to the Trifolia Home Page
 ReturnHome("Welcome to Trifolia Workbench");  
 
}

// TEST 5: Create a New "Document" Type Template
@Test
    public void CreateDocumentTemplate(String templateName, String templateOID, String templateIG, String templateType, String templateText,
			String permissionUserName) throws Exception {
	
	// Open the Template Browser
	   OpenTemplateBrowser(); 
	   
	// Confirm the page is fully loaded
       waitForPageLoad();
       
	// Clear existing Search criteria
	    ClearExistingSearch();  
	    
	 // Confirm the page is re-loaded
	       waitForPageLoad();
	       
    //Open the Template Editor
    if (permissionUserName == "lcg.admin") 
    {
        WebDriverWait wait = new WebDriverWait(driver, 60);
 	    WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[6]/div/a")));   
    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[6]/div/a")).click();       	
    }
    
    else
    {
    	WebDriverWait wait = new WebDriverWait(driver, 60);
  	    WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[5]/div/a")));   
    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[5]/div/a")).click();
    }
    
	 // Confirm the page is fully loaded
	    waitForPageLoad();
	    
	    // Wait for Bindings to complete
	     waitForBindings("TemplateEditor");
	     
       // Confirm the Template Editor Opens
      WebDriverWait wait = new WebDriverWait(driver, 60);                    
	  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/span"), "Owning Implementation Guide is required"));
	  assertTrue("Could not find \"Owning Implementation Guide is required\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Owning Implementation Guide is required[\\s\\S]*$"));
    
	  WebDriverWait wait1 = new WebDriverWait(driver, 60);                    
	  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[3]/div/div[1]/div/div/span[1]"), "Quick Edit"));
	  assertTrue("Could not find \"Quick Edit\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Quick Edit[\\s\\S]*$"));
  
	  WebDriverWait wait2 = new WebDriverWait(driver, 60);                    
	  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[4]/div/span"), "View Mode"));
	  assertTrue("Could not find \"View Mode\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*View Mode[\\s\\S]*$"));
  
    // Add Template Meta Data
		
	// Confirm list of IG's appears
	 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select")).click();  
	 WebDriverWait wait3 = new WebDriverWait(driver, 60);                    
	 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select/option[2]"), templateIG));

	// Add Owning IG
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select")).click();
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select")).sendKeys(Keys.ARROW_DOWN);
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select")).click();
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select")).click();
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select")).sendKeys(Keys.TAB);

    // Add Template Type
    WebDriverWait wait4 = new WebDriverWait(driver, 60);
    WebElement element4 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")));
    assertTrue("Could not find \"Type\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Type:[\\s\\S]*$"));
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).click();
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.ARROW_DOWN);
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).click();
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.TAB);

    // Wait for new field to load and add Template Type
    WebDriverWait wait5 = new WebDriverWait(driver, 60);
    WebElement element5 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")));
    assertTrue("Could not find \"Type\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Name:[\\s\\S]*$"));
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[1]/input")).sendKeys(templateName);
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[1]/input")).sendKeys(Keys.TAB);

    // Add Template OID
    WebDriverWait wait6 = new WebDriverWait(driver, 60);
    WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[1]/input")));
    assertTrue("Could not find \"Type\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Type:[\\s\\S]*$"));
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[2]/input[1]")).sendKeys(templateOID);
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[2]/input[1]")).sendKeys(Keys.TAB);
       
    // Add Template Description
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[7]/textarea")).sendKeys(templateText);
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[7]/textarea")).sendKeys(Keys.TAB);
    
    // Save the Template
    SaveTemplate();
    
    // Return to the Trifolia Home Page
    ReturnHome("Welcome to Trifolia Workbench");  
    
}
    
// Test 6: Find and Edit a Template
@Test
    public void EditTemplate(String templateName, String templateOID, String impliedTemplate, String templateNotes, String permissionUserName) throws Exception {
	
	// Confirm the page is fully loaded
    waitForPageLoad();
    
	// Open the Template Browser
	   OpenTemplateBrowser();
	   
	// Confirm the page is fully loaded
       waitForPageLoad();
       
	// Find the template
	if (permissionUserName == "lcg.admin") 
	{
	FindTemplate("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "1Automation Test IG");	 
	}
	
	
	// Open the Template Editor
	
    if (permissionUserName == "lcg.admin")
    	{
    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[6]/div/a[2]")).click();
    	}
    else
    	{
    	driver.findElement(By.xpath("//*[@id=\"BrowseTemplates\"]/table/tbody/tr/td[5]/div/a[2]")).click();
    	}
    
	 // Confirm the page is fully loaded
	    waitForPageLoad();
    
     // Confirm the correct template appears in the Editor
	   
 		if (permissionUserName == "lcg.admin")
 	 		{
 				ConfirmTemplateEditor("Automation Document Template", "urn:oid:1.2.3.4.5.6.7.8.9.10");
 	 		}
   
  	  // Search for the Implied Template 
		WebDriverWait wait = new WebDriverWait(driver, 60);                               
		WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[4]/div[1]/div/div/input")));	
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[4]/div[1]/div/div/input")).click();
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[4]/div[1]/div/div/input")).sendKeys(impliedTemplate);
		
		// Confirm the template is located, then 
		   WebDriverWait wait1 = new WebDriverWait(driver, 60);            
		   WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[4]/div[1]/div/div/input")));
		   // assertTrue("Could not find \"Implied Template Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(impliedTemplate) >= 0);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[4]/div[1]/div/div/input")).sendKeys(Keys.ARROW_DOWN);
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[4]/div[1]/div/div/input")).sendKeys(Keys.RETURN);

  	        
    // Change the Template Status
	    if (permissionUserName == "lcg.admin")
			{
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[6]/select")).click();
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[6]/select")).sendKeys(Keys.ARROW_DOWN);
	        driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[6]/select")).click();
	        driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[6]/select")).click();
	    	
			}
	    else
			{
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[5]/select")).click();
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[5]/select")).sendKeys(Keys.ARROW_DOWN);
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[5]/select")).click();
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[5]/select")).click();
	    	
			}
	    
	    // Add Template Notes
	    if (permissionUserName == "lcg.admin")
			{
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[8]/textarea")).click();
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[8]/textarea")).clear();
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[8]/textarea")).sendKeys(templateNotes);
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[8]/textarea")).sendKeys(Keys.TAB);
			}
	    else
			{
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[7]/textarea")).click();
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[7]/textarea")).clear();
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[7]/textarea")).sendKeys(templateNotes);
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[7]/textarea")).sendKeys(Keys.TAB);
			}

	    // Click the Quick Edit area
    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[3]/div/div[1]/div/div/input")).click();
			    
	    // Save the Template
	    SaveTemplate();
	    
	    // Return to the Trifolia Home Page
	    ReturnHome("Welcome to Trifolia Workbench");  
    
}
    
// TEST 4: Copy a Template
  @Test
	  public void CopyTemplate(String templateName, String templateOID, String templateIG, String templateCopy, String permissionUserName) throws Exception {
	    
	// Open the Template Browser
	   OpenTemplateBrowser();
	   
	// Confirm the page is fully loaded
        waitForPageLoad();
       
	    //Find the Template
	    if (permissionUserName == "lcg.admin") 
		{
		FindTemplate(templateName, templateOID, templateIG);	 
		}
		
		  
		// Confirm the page is re-loaded
	       waitForPageLoad();
	       
	  // Open the Template Viewer
	  if (permissionUserName == "lcg.admin") 
	    {
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[6]/div/a[1]")).click();
	    }
	    else
	    {
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[5]/div/a[1]")).click();
	    }
	  
		// Confirm the page is fully loaded
	      waitForPageLoad();
      
	  // Confirm the Template Viewer opens and the correct template appears
	  
	 	 if (permissionUserName == "lcg.admin")
	 		{
	 		ConfirmTemplateViewer(templateName, templateOID);
	 		}
  
	 	// Confirm the page is fully loaded
	      waitForPageLoad();
	      
	  //Open the Copy Template form
	  if (permissionUserName == "lcg.admin") 
	    {
		  // Confirm the Copy option is available
		  WebDriverWait wait = new WebDriverWait(driver, 60);                     
		  wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[5]/a")));
		  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[5]/a")).click();
	    }
	  else
	    {
		  // Confirm the Copy option is available
		  WebDriverWait wait = new WebDriverWait(driver, 60);                     
		  wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[4]/a")));
	      driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[4]/a")).click();
	    }
	  
		// Confirm the page is fully loaded
	       waitForPageLoad();
       // Wait for Copy Page To Load    
	       WebDriverWait wait = new WebDriverWait(driver, 60);                     
	 	  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/h2/span[1]"), "Copy"));
	       
       // Wait for Bindings to complete
	     waitForBindings("CopyTemplate");
      
	  // Confirm the Copy Template form open 
	  WebDriverWait wait0 = new WebDriverWait(driver, 90);
	  WebElement element = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("CopyTemplate")));
	  assertTrue("Could not find \"Copy Step 1: \" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Copy Step 1:[\\s\\S]*$"));
	  
	  // Confirm the correct template appears in the Template Copy form
	  WebDriverWait wait1 = new WebDriverWait(driver, 60);                     
	  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/h2/span[2]"), templateName));
	  assertTrue("Could not find \"Template Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateName) >= 0);

	  // Confirm the input fields are available for entry
	  WebDriverWait wait2 = new WebDriverWait(driver, 60);                     
	  wait1.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/input")));
	
	  // Copy Step 1 - Add Template Name
	  WebDriverWait wait3 = new WebDriverWait(driver, 60);                     
	  wait1.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/input")));	
	  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/input")).clear();
	  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/input")).sendKeys(templateCopy);
	  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/input")).sendKeys(Keys.TAB);
	  
	  // Copy Step 1 - Update Identifier
	  WebDriverWait wait4 = new WebDriverWait(driver, 60);                     
	  wait1.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/input")));	
	  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/input")).click();
	  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/input")).sendKeys("_Copy");
	  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/input")).sendKeys(Keys.TAB);
	  
	  // IG Assignment
	// Confirm list of IG's appears
		 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[5]/select")).click();  
		 WebDriverWait wait5 = new WebDriverWait(driver, 60);                    
		 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[5]/select/option[2]"), templateIG));

		// Add Copy Template IG
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[5]/select")).click();
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[5]/select")).sendKeys(Keys.ARROW_DOWN);
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[5]/select")).click();
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[5]/select")).click();
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[5]/select")).sendKeys(Keys.TAB);
	    
	    // Confirm the page is fully loaded
	      waitForPageLoad();
	      
 
	  // Click Next
	  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[6]/button[1]")).click();
  
	   // Confirm the page is fully loaded
      waitForPageLoad();
      
	  //Copy Step 2 - Constraint Generation
	  WebDriverWait wait7 = new WebDriverWait(driver, 60);
	  WebElement element7 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("Step2")));
	  assertTrue("Could not find \"Copy Step 2\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Copy Step 2:[\\s\\S]*$"));

	  // Confirm the Regenerate Constratins Text appears
	  WebDriverWait wait8 = new WebDriverWait(driver, 60);                     
	  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[2]/h3/a"), "Regenerate all new constraints"));
	 
	  // Click the Finish option
	  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div[2]/button[1]")).click();
	  
		// Confirm the page is fully loaded
	      waitForPageLoad();
	
	   // Wait for page to fully load
	   WebDriverWait wait10 = new WebDriverWait(driver, 60);		
	   wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[5]"))); 
   
	   // Confirm the Template Editor Opens
	   //driver.manage().timeouts().pageLoadTimeout(30, TimeUnit.SECONDS);
	   WebDriverWait wait11 = new WebDriverWait(driver, 60);
	   wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[1]/a"), "Template/Profile"));
       assertTrue("Could not find \"Template/Profile\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Template/Profile[\\s\\S]*$"));
		
       // Wait for page to fully load
		  waitForPageLoad();
		  
	   // Confirm the the correct Template Name appears in the Editor
	   //driver.manage().timeouts().pageLoadTimeout(30, TimeUnit.SECONDS);
	   WebDriverWait wait12 = new WebDriverWait(driver, 60);
	   wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/h3/span[2]"), templateName));
	   assertTrue("Could not find \"Template Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateName) >= 0);
	  
	   // Wait for page to fully load
		  waitForPageLoad();
		  
	  // Save the Template	  
		 SaveTemplate();
		 
	   // Wait for page to fully load
		  waitForPageLoad();
		  
       // Return to the Trifolia Home Page
       ReturnHome("Welcome to Trifolia Workbench");  
}
  
 // TEST 5: Move Template to a new IG
  @Test
	  public void MoveTemplate(String templateCopy, String templateCopyOID, String templateIG, String origTemplateType, String movedTemplateType, String permissionUserName) throws Exception {
	  if (permissionUserName == "lcg.admin")
	  {
		// Wait for page to fully load
		  waitForPageLoad();
		  
		// Open the Template Browser
		   OpenTemplateBrowser();
		   
		  // Find the template.
		     FindTemplate(templateCopy, templateCopyOID, templateIG);	 
		   
		// Wait for page to fully load
		  waitForPageLoad();
		  
		  //  Open the Template Viewer
		      driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[6]/div/a[1]")).click();
		  
		   // Wait for page to fully load
			  waitForPageLoad();
			  
	      // Confirm the Template Viewer opens and the correct template appears
		 	 if (permissionUserName == "lcg.admin")
		 		{
		 		ConfirmTemplateViewer(templateCopy, templateCopyOID);
		 		}
		 	 
	 	// Confirm the page is fully loaded
	       waitForPageLoad();
	       
	     // Move the Template
	       WebDriverWait wait10 = new WebDriverWait(driver, 60);                     
	 	   wait10.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[7]/a")));
		  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[7]/a")).click();
		  
		// Confirm the page is fully loaded
	       waitForPageLoad();
	       Thread.sleep(2000);
	       
	       // Wait for Bindings to complete
		   // waitForBindings("MoveTemplate");
	       
		  // Confirm the Move Template form appears
		  WebDriverWait wait = new WebDriverWait(driver, 60);
		  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/h2"), "Move: Step 1"));
		
		  // Select the new IG     
		// Confirm list of IG's appears
		  if (origTemplateType == "Document")
		  {
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/select")).click();  
		   WebDriverWait wait2 = new WebDriverWait(driver, 60);                    
		   wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/select/option[2]"), templateIG));
		  }
		  if(origTemplateType == "Composition")
		  {
			   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/select/option[1]")).click();  
			   WebDriverWait wait2 = new WebDriverWait(driver, 60);                    
			   wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/select/option[1]"), templateIG));
		  }
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/select")).sendKeys(Keys.TAB);
	 
		  // Select Template Type field
	      WebDriverWait wait3 = new WebDriverWait(driver, 60);
	      WebElement element3 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/select")));
	      driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/select")).click();
	      
	      //Confirm the list of Template Types appears   
	      if (origTemplateType == "document")
	      {
	    	  WebDriverWait wait4 = new WebDriverWait(driver, 60);                    
			  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/select/option[1]"), origTemplateType));
		      Thread.sleep(1000);
	      }
	      if (origTemplateType == "Composition")
	      {
		      WebDriverWait wait20 = new WebDriverWait(driver, 60);                    
			  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/select/option[20]"), origTemplateType));
		      Thread.sleep(1000);
	      }
	      //Change the Template Type
		  WebDriverWait wait5 = new WebDriverWait(driver, 60);
	      WebElement element5 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/select")));
		  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/select")).sendKeys(movedTemplateType);
		  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/select")).sendKeys(Keys.TAB);
		  
		  // Click on the Next option   
		  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[4]/button[1]")).click();
		  WebDriverWait wait6 = new WebDriverWait(driver, 60);
		  WebElement element6 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[2]/div[2]/button[1]")));
		   
		  // Click the Finish option
		  WebDriverWait wait7 = new WebDriverWait(driver, 60);
	      WebElement element7 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[2]/div[2]/button[1]")));
		  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div[2]/button[1]")).click();
		
		  //Accept the confirmation.

		  //Confirm the Alert appears
		   WebDriverWait wait0 = new WebDriverWait(driver, 60);
		   wait0.until(ExpectedConditions.alertIsPresent());
		  
		  Alert alertDialog1 = driver.switchTo().alert();
		  // Get the alert text
		  Thread.sleep(1000);
		  String alertText1 = alertDialog1.getText();
		  // assertTrue(driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Successfully saved implementation guide![\\s\\S]*$"));
		  // Click the OK button on the alert.
		  Thread.sleep(1000);
		  alertDialog1.accept();
		  Thread.sleep(1000);
		  
		  // Confirm the user is returned to the Template Viewer 
		     WebDriverWait wait8 = new WebDriverWait(driver, 60);
		     WebElement element8 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("ViewTemplate")));
		 
		     // Confirm the correct Template appears in the Template Viewer
		     WebDriverWait wait9 = new WebDriverWait(driver, 60);
		  	 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/a"), templateCopy));
		     assertTrue("Could not find \"Template Copy Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateCopy) >= 0);		   
		     
		     // Wait for page to fully load
			 WebDriverWait wait30 = new WebDriverWait(driver, 60);		
			 wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[8]"))); 
	   
		  // Return to the Template Browser
		     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[1]/a")).click();
		
	     // Wait for Bindings to complete
	     waitForBindings("ViewTemplate");
		     
	     // Confirm the Template Browser appears
		  WebDriverWait wait11 = new WebDriverWait(driver, 60);
		  WebElement element11 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("BrowseTemplates")));
		  assertTrue("Could not find \"Browse Templates\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Browse Templates[\\s\\S]*$"));Thread.sleep(1000);
	
	     // Confirm the Template Browser Text appears
		  WebDriverWait wait12 = new WebDriverWait(driver, 60);                    
		  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[2]"), "Identifier"));
	    
		 // Confirm the new Template Type appears in the Browser
		  WebDriverWait wait13 = new WebDriverWait(driver, 60);                    
		  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[4]"), movedTemplateType));
		  
		  // Clear the existing Search Criteria
		   WebDriverWait wait14 = new WebDriverWait(driver, 60);
		   WebElement element15 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")));
	       driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")).click(); 
	       driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")).click();
	       
	       //Confirm the search criteria is cleared
		    WebDriverWait wait16 = new WebDriverWait(driver, 60);                    
		    wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[2]/td[5]")));       
		  
	       // Return to the Trifolia Home Page
	       ReturnHome("Welcome to Trifolia Workbench");  
	    }
	  
	 else 
	 	{
		 return;
	 	}	
}  
    
// TEST 6: Delete the Template Copy
  @Test
    public void DeleteTemplate(String templateCopy, String templateCopyOID, String templateIG, String permissionUserName) throws Exception {
	 
	  // Open the Template Browser
	      OpenTemplateBrowser();
	
	  // Find the Template to be Deleted
	    if (permissionUserName == "lcg.admin") 
		{
		FindTemplate(templateCopy, templateCopyOID, templateIG);	 
		}
		 
		// Confirm the page is fully loaded
	       waitForPageLoad();
	       
	    // Open the Template Viewer
		    if (permissionUserName == "lcg.admin") 
		    {
		    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[1]/td[6]/div/a[1]")).click();
		    }
		    else
		    {
		    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[1]/td[5]/div/a[1]")).click();
		    }
		
		 // Confirm the page is fully loaded
		       waitForPageLoad();
		       
		    // Confirm the Template Viewer opens and the correct template appears
		 	 if (permissionUserName == "lcg.admin")
		 		{
		 		ConfirmTemplateViewer(templateCopy, templateCopyOID);
		 		}
	        //Open the Template Delete Form and Confirm the correct Template Appears
		 	 
		 	 // Confirm the page is fully loaded
		        waitForPageLoad();
		 	 
		 	 if (permissionUserName == "lcg.admin") 
			    {
				 WebDriverWait wait = new WebDriverWait(driver, 60);
				 WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[9]/a")));
			     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[9]/a")).click();
			    }
		    else
			    {
		    	 WebDriverWait wait = new WebDriverWait(driver, 60);
				 WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[7]/a")));
			     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[7]/a")).click();
			    }
		// Wait for page to fully load
		  waitForPageLoad();
		  
		  // Wait for Bindings to complete
		     waitForBindings("DeleteTemplate");
	  
    // Confirm the Delete Templates form opens 
		WebDriverWait wait = new WebDriverWait(driver, 60);
		WebElement element = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("DeleteTemplate")));
	    assertTrue("Could not find \"Delete Templates\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Delete Template[\\s\\S]*$"));
	    
    // Confirm the correct template appears in the Delete Templates form
       WebDriverWait wait1 = new WebDriverWait(driver, 180);                   
	   wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div/h3/a"), templateCopy));
	   assertTrue("Could not find \"Template Copy Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateCopy) >= 0);
    
    //Execute the Delete command 
      driver.findElement(By.xpath("//*[@id=\"DeleteTemplate\"]/div/div[2]/div/button[1]")).click();
    
    // Accept the Delete Confirmation
      
      //Confirm the Alert appears
	   WebDriverWait wait0 = new WebDriverWait(driver, 60);
	   wait0.until(ExpectedConditions.alertIsPresent());
  
	    // Switch the driver context to the "Are you absolutely sure you want to delete this template" alert
	   Alert alertDialog3 = driver.switchTo().alert();
	    //Get the alert text
	    String alertText3 = alertDialog3.getText();
	    //Click the OK button on the alert.
	    alertDialog3.accept();
	    Thread.sleep(2000); 
		 //Switch the driver context to the "Successfully deleted value set?" alert
		 Alert alertDialog4 = driver.switchTo().alert();
		 //Get the alert text
		 String alertText4 = alertDialog4.getText();
		 // assertTrue(driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Successfully deleted value set[\\s\\S]*$"));
		 //Click the OK button on the alert.
		 alertDialog4.accept();
	
		 // Confirm the page is fully loaded
	        waitForPageLoad();
	        
		  // Wait for Bindings to complete
	         waitForBindings("BrowseTemplates");
	     
		  // Confirm the Template Browser appears
		  WebDriverWait wait7 = new WebDriverWait(driver, 60);
		  WebElement element7 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("BrowseTemplates")));
		  assertTrue("Could not find \"Browse Templates\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Browse Templates[\\s\\S]*$"));Thread.sleep(1000);
	
	     // Confirm the Template Browser Text appears
		  WebDriverWait wait6 = new WebDriverWait(driver, 60);                    
		  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[2]"), "Identifier"));
	    
		  // Clear the existing Search Criteria
		   WebDriverWait wait8 = new WebDriverWait(driver, 60);
		   WebElement element8 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")));
	       driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")).click(); 
	       driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")).click();
	       
	       //Confirm the search criteria is cleared
		    WebDriverWait wait9 = new WebDriverWait(driver, 60);                    
		    wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[2]/td[5]")));       
	    
	    // Return to the Trifolia Home Page
	    ReturnHome("Welcome to Trifolia Workbench");  

    }
}
