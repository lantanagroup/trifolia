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
public class C_TemplateFunctions {
  private WebDriver driver;
  private String baseUrl;
  private boolean acceptNextAlert = true;
  private StringBuffer verificationErrors = new StringBuffer();
  public C_TemplateFunctions() {}

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
  
  public void OpenTemplateBrowser() throws Exception 
  {
     // Open the Template Browser
	driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[2]/a")).click();
	driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[2]/ul/li[2]/a")).click();

	// Confirm Template Browser opens
	 WebDriverWait wait = new WebDriverWait(driver, 60);
	 WebElement element = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("BrowseTemplates")));
	 assertTrue("Could not find \"Browse Templates\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Browse Templates[\\s\\S]*$"));
 }
  
  public void FindTemplate(String templateName, String templateOID, String templateIG) throws Exception 
	{
	  // Confirm the Template Listing appears
	  WebDriverWait wait = new WebDriverWait(driver, 60);                    
	  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[2]"), "Identifier"));
	 
	  // Clear the existing search criteria
	     driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")).click();
	  
	   //Search for the Template 
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")).click();
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/input")).sendKeys(templateName);
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/input")).sendKeys(Keys.TAB);
	  
	   // Confirm the correct Template Name is returned
	    WebDriverWait wait1 = new WebDriverWait(driver, 60);                     
	  	wait1.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[1]"), templateName));
	    assertTrue("Could not find \"Template Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateName) >= 0);
	    
	    // Confirm the correct Template IG is returned
	    WebDriverWait wait2 = new WebDriverWait(driver, 60);                    
	  	wait1.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath(" /html/body/div[2]/div/div/table/tbody/tr/td[3]"), templateIG));
	    assertTrue("Could not find \"Template IG\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateIG) >= 0);
}	
  
  public void ConfirmTemplateViewer(String templateName, String templateOID) throws Exception 
	{	
			 // Wait for page to fully load
			 WebDriverWait wait = new WebDriverWait(driver, 5);		
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
		
	   // Confirm the Template Editor opens
	       driver.manage().timeouts().pageLoadTimeout(30, TimeUnit.SECONDS);
		   WebDriverWait wait = new WebDriverWait(driver, 60);
		   WebElement element= wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("TemplateEditor")));
		   assertTrue("Could not find \"Browse Templates\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Implied Template/Profile[\\s\\S]*$"));
		   
		   // Wait for page to fully load
		   WebDriverWait wait1 = new WebDriverWait(driver, 5);		
		   wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[5]"))); 
	    
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

  public void SaveTemplate() throws Exception 
  {

  	  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[1]/div[1]/button")).click();
  	  WebDriverWait wait = new WebDriverWait(driver, 60);
	  WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[1]/div[1]/ul/li[1]/a")));
  	  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[1]/div[1]/ul/li[1]/a")).click();
  	      
  	   //Confirm template was updated successfully
  	   WebDriverWait wait1 = new WebDriverWait(driver, 60);
  	   WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("//*[@id=\"footer\"]/div/div[2]/span")));
  	   assertTrue("Could not find \"Done Saving\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Done saving.[\\s\\S]*$"));
  	   
  	 // Return to the Template Browser
  	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[1]/div[2]/button")).click();
  	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[1]/div[2]/ul/li[1]/a")).click();
  	  
  	   // Confirm Template Browser opens
  	   WebDriverWait wait2 = new WebDriverWait(driver, 60);
  	   WebElement element2 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("BrowseTemplates")));
  	   assertTrue("Could not find \"Browse Templates\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Browse Templates[\\s\\S]*$"));
  	   
  	  // Clear existing Search Criteria 
  	   driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")).click();
   }	
  
  public void ReturnHome(String welcomeMessage) throws Exception {
	  
	   // Return to the Trifolia Home Page
	    driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[1]/a")).click();
	    WebDriverWait wait = new WebDriverWait(driver, 60);
	    WebElement element = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("appnav")));
	    
	    //Confirm the Welcome Message appears
		WebDriverWait wait0 = new WebDriverWait(driver, 60);
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/h2"), welcomeMessage));
		assertTrue("Could not find \"Value Set OID\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(welcomeMessage) >= 0);   	
}


@Test
//TEST 1: View an existing Template
    public void ViewTemplate(String templateName, String templateOID, String containedTemplate, String linkedTemplateOID, 
    		String verificationText, String permissionUserName) throws Exception {
    
	// Open the Template Browser
	   OpenTemplateBrowser();
	
	// Find the Template
	if (permissionUserName == "lcg.admin") 
	{
	FindTemplate("Acuity score data section", "urn:oid:2.16.840.1.113883.10.20.17.2.3", "Neonatal Care Report Release 1");	 
	}
	
	if (permissionUserName == "lcg.user") 
	{
	FindTemplate("MAP Topic", "http://www.lantanagroup.com/camara/fhir/map-topic", "PHARM HIT Demo");	 
	}
	
	if (permissionUserName == "hl7.member") 
	{
	FindTemplate("LTPAC Home Health Summary", "urn:oid:2.16.840.1.113883.10.20.22.1.11.2", "Long-Term Post-Acute Care (LTPAC) Summary Release 1");	 
	}
	
	if (permissionUserName == "hl7.user") 
	{
	FindTemplate("Activated partial thromboplastin time", "urn:oid:2.16.840.1.113883.10.20.17.3.44", "Neonatal Care Report Release 1");	 
	}
	
	// Clear main Search box
    driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")).click();
    
    if (permissionUserName == "lcg.admin") 
    	{
	      // Filter by Organization
		    driver.findElement(By.xpath("//*[@id=\"BrowseTemplates\"]/table/thead/tr[2]/td[5]/select")).sendKeys("LCG");
		    driver.findElement(By.xpath("//*[@id=\"BrowseTemplates\"]/table/thead/tr[2]/td[5]/select")).sendKeys(Keys.RETURN);
            
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
	  	    
		    //Send the new filter criteria
	  	    driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[2]/td[1]/input")).click();
	  	    driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[2]/td[1]/input")).sendKeys(templateName); 
	  	    
	  	    // Confirm the correct Template OID is returned
	  	    WebDriverWait wait9 = new WebDriverWait(driver, 60);                     
		   	wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[2]"), templateOID));
	  	    assertTrue("Could not find \"Template OID\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateOID) >= 0);
  	    
  	    // Filter by Template OID
	  	    // Clear Filter
	  	    driver.findElement(By.xpath("//*[@id=\"SearchQuery\"]")).clear();
	  	    driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[2]/td[6]/button")).click();
	  	    
	  	    // Enter new search criteria
	  	    driver.findElement(By.xpath("//*[@id=\"BrowseTemplates\"]/table/thead/tr[2]/td[2]/input")).click();
	  	    driver.findElement(By.xpath("//*[@id=\"BrowseTemplates\"]/table/thead/tr[2]/td[2]/input")).sendKeys(templateOID);

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
		    
		      // Filter by Template OID
		    driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[2]/td[2]/input")).click();
		    driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[2]/td[2]/input")).sendKeys(templateOID);
	
		    // Confirm the correct Template OID is returned
		    driver.manage().timeouts().pageLoadTimeout(30, TimeUnit.SECONDS);
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
		   
		    else if (permissionUserName == "hl7.member")
		    {   
		    	WebDriverWait wait = new WebDriverWait(driver, 60);
		    	WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[5]/div/a[1]")));	    	  
		    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[5]/div/a[1]")).click();
		    }
		    
		    else if (permissionUserName == "lcg.user" || permissionUserName == "hl7.user")
		    {
		    	WebDriverWait wait = new WebDriverWait(driver, 60);
		    	WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[5]/div/a")));	    	  
		    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[5]/div/a")).click();
		    }
    // Confirm the Template Viewer opens
		
	    if (permissionUserName == "lcg.admin") 
	    {
	    	ConfirmTemplateViewer("Acuity score data section", "urn:oid:2.16.840.1.113883.10.20.17.2.3");
	    }
	    if (permissionUserName == "lcg.user") 
	    {
	    	ConfirmTemplateViewer("MAP Topic", "http://www.lantanagroup.com/camara/fhir/map-topic");
	    }
	    if (permissionUserName == "hl7.member") 
	    {
	    	ConfirmTemplateViewer("LTPAC Home Health Summary", "urn:oid:2.16.840.1.113883.10.20.22.1.11.2");
	    }
	    if (permissionUserName == "hl7.user") 
	    {
	    	ConfirmTemplateViewer("Activated partial thromboplastin time", "urn:oid:2.16.840.1.113883.10.20.17.3.44");
	    }
	    // Confirm the correct text appears for the selected template
	    assertTrue("Could not find Template Verification Text on page\"Constraint Text\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(verificationText) >= 0);
	   
    // Check Template Links
    driver.findElement(By.xpath("//*[@id=\"ViewTemplate\"]/ul/li[2]/a")).click();
    // Thread.sleep(2000);
    WebDriverWait wait = new WebDriverWait(driver, 60);
    WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("//*[@id=\"ViewTemplate\"]/ul/li[2]/a")));
    assertTrue("Could not find \"Contained Template\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(containedTemplate) >= 0);
    
    if (permissionUserName == "lcg.admin") 
    {
	    // Click on a linked Template
	    driver.findElement(By.xpath("//*[@id=\"references\"]/div[4]/ul/li/a/span[1]")).click();
	    
	    // Confirm the correct template appears
	    WebDriverWait wait13 = new WebDriverWait(driver, 60);
	    WebElement element13 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("//*[@id=\"ViewTemplate\"]/div[1]/div/div[1]/a")));
	    assertTrue("Could not find \"Linked Template OID\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(linkedTemplateOID) >= 0);
	    
	    // Return to the Template Listing page
	    driver.findElement(By.xpath("//*[@id=\"bs-example-navbar-collapse-1\"]/ul/li[1]/a")).click();
	    WebDriverWait wait14 = new WebDriverWait(driver, 60);
	    WebElement element14 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("BrowseTemplates")));
	    
	    // Confirm the Template Listing appears
		  WebDriverWait wait15 = new WebDriverWait(driver, 60);                    
		  wait15.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[2]"), "Identifier"));
		 
	    // Clear existing Filters
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[2]/td[6]/button")).click();
		
	    // Clear existing Search criteria
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")).click();
	    
    }
    else
    {
	    //Return to Template Listing and confirm the correct form opens
	    driver.findElement(By.xpath("//*[@id=\"bs-example-navbar-collapse-1\"]/ul/li[1]/a")).click();
	    WebDriverWait wait1 = new WebDriverWait(driver, 60);
	    WebElement element1 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("BrowseTemplates")));
	     
	    // Confirm the Template Listing appears
		  WebDriverWait wait2 = new WebDriverWait(driver, 60);                    
		  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[2]"), "Identifier"));
		 
		  // Confirm existing filters still exist
		  WebDriverWait wait3 = new WebDriverWait(driver, 60);                    
		  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]"), "Page 1 of 1, 1 templates/profiles"));
		  
	    // Clear existing filters    
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[2]/td[5]/button")).click();
	    
	    // Clear existing Search criteria
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")).click();
    }
    
    // Return to the Trifolia Home Page
    ReturnHome("Welcome to Trifolia Workbench");   
}
    

// TEST 2: Create a New Template
@Test
    public void CreateTemplate(String templateName, String templateOID, String templateIG, String templateType, String templateText,
			String permissionUserName) throws Exception {
	
	// Open the Template Browser
	   OpenTemplateBrowser(); 
	   
    // Clear existingFilters 
	   WebDriverWait wait = new WebDriverWait(driver, 60);
	   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")));   
       driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")).click();
    
    //Open the Template Editor
    if (permissionUserName == "lcg.admin") 
    {
        WebDriverWait wait1 = new WebDriverWait(driver, 60);
 	    WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[6]/div/a")));   
    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[6]/div/a")).click();       	
    }
    
    else
    {
    	WebDriverWait wait1 = new WebDriverWait(driver, 60);
  	    WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[5]/div/a")));   
    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[5]/div/a")).click();
    }
    
       // Confirm the Template Editor Opens
    WebDriverWait wait1 = new WebDriverWait(driver, 60);
    WebElement element1 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("TemplateEditor")));
    assertTrue("Could not find \"Owning Implementation Guide is required\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Owning Implementation Guide is required[\\s\\S]*$"));
    
    // Add Template Meta Data
    // Add Owning IG
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select")).sendKeys(templateIG);
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div/select")).sendKeys(Keys.TAB);

    // Add Template Type
    WebDriverWait wait2 = new WebDriverWait(driver, 60);
    WebElement element2 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")));
    assertTrue("Could not find \"Type\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Type:[\\s\\S]*$"));
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(templateType);
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")).sendKeys(Keys.TAB);

    // Wait for new field to load and add Template Type
    WebDriverWait wait3 = new WebDriverWait(driver, 60);
    WebElement element3 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[1]/div[2]/select")));
    assertTrue("Could not find \"Type\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Name:[\\s\\S]*$"));
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[1]/input")).sendKeys(templateName);
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[1]/input")).sendKeys(Keys.TAB);

    // Add Template OID
    WebDriverWait wait4 = new WebDriverWait(driver, 60);
    WebElement element4 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[1]/input")));
    assertTrue("Could not find \"Type\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Type:[\\s\\S]*$"));
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[2]/input[1]")).sendKeys(templateOID);
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[2]/input[1]")).sendKeys(Keys.TAB);
   
    // Add Bookmark
       //driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[3]/input")).sendKeys(templateName);
       // driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[3]/input")).sendKeys(Keys.TAB);
    
    // Add Template Description
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[7]/textarea")).sendKeys(templateText);
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[7]/textarea")).sendKeys(Keys.TAB);
    
    // Save the Template
    SaveTemplate();
    
    // Return to the Trifolia Home Page
    ReturnHome("Welcome to Trifolia Workbench");  
    
}
    
// TEST 3: Find and Edit a Template
@Test
    public void EditTemplate(String templateName, String templateOID, String templateNotes, String permissionUserName) throws Exception {
	
	// Open the Template Browser
	   OpenTemplateBrowser();
	   
	// Find the template
	if (permissionUserName == "lcg.admin") 
	{
	FindTemplate("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "Automation Test IG");	 
	}
	
	
	if (permissionUserName == "hl7.member") 
	{
	FindTemplate("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2", "HL7 Member Test IG");	 
	}
	
	   
    if (permissionUserName == "lcg.admin")
    	{
    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[6]/div/a[2]")).click();
    	}
    else
    	{
    	driver.findElement(By.xpath("//*[@id=\"BrowseTemplates\"]/table/tbody/tr/td[5]/div/a[2]")).click();
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
   
  	    
    // Add Template Notes
	    if (permissionUserName == "lcg.admin")
			{
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[8]/textarea")).sendKeys(templateNotes);
			}
	    else
			{
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[7]/textarea")).sendKeys(templateNotes);
			}
    
    // Change the Template Status
	    if (permissionUserName == "lcg.admin")
			{
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[6]/select")).sendKeys("Draft");
	        driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[6]/select")).sendKeys(Keys.TAB);
			}
	    else
			{
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[5]/select")).sendKeys("Draft");
	        driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div/div[1]/div[2]/div[5]/select")).sendKeys(Keys.TAB);
			}
	    
    //  //Add Implied Template "Entry Reference (NEW)"
    //    driver.findElement(By.xpath("//*[@id=\"template\"]/div/div[1]/div[8]/span[2]/button[2]")).click();
    //    driver.findElement(By.xpath("//*[@id=\"ctl00_mainBody\"]/div[6]/div/div/div[2]/input")).sendKeys("2.16.840.1.113883.10.20.22.4.122");
    //    driver.findElement(By.xpath("//*[@id=\"ctl00_mainBody\"]/div[6]/div/div/div[2]/input")).sendKeys(Keys.RETURN);
    //    driver.findElement(By.xpath("//*[@id=\"ctl00_mainBody\"]/div[6]/div/div/div[3]/button[1]")).click();
    
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
	   
	    //Find the Template
	    if (permissionUserName == "lcg.admin") 
		{
		FindTemplate("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10", "Automation Test IG");	 
		}
		
		
		if (permissionUserName == "hl7.member") 
		{
		FindTemplate("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2", "HL7 Member Test IG");	 
		}
		  		    
	  // Open the Template Viewer
	  if (permissionUserName == "lcg.admin") 
	    {
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[6]/div/a[1]")).click();
	    }
	    else
	    {
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[5]/div/a[1]")).click();
	    }
	  
	  // Confirm the Template Viewer opens and the correct template appears
	  
	 	 if (permissionUserName == "lcg.admin")
	 		{
	 		ConfirmTemplateViewer("Automation Test Template", "urn:oid:1.2.3.4.5.6.7.8.9.10");
	 		}
	 	 if (permissionUserName == "hl7.member")
		 	{
	 		ConfirmTemplateViewer("HL7 Member Test Template", "urn:oid:2.2.2.2.2.2.2.2");
		 	}	  
	  //Open the Copy Template form
	  if (permissionUserName == "lcg.admin") 
	    {
		  // Confirm the Copy option is clickable
		  WebDriverWait wait = new WebDriverWait(driver, 60);                     
		  wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[5]/a")));
		  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[5]/a")).click();
	    }
	  else
	    {
		  // Confirm the Copy option is clickable
		  WebDriverWait wait = new WebDriverWait(driver, 60);                     
		  wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[4]/a")));
	      driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[4]/a")).click();
	    }
	  
	  // Confirm the Copy Template form open 
	  WebDriverWait wait = new WebDriverWait(driver, 60);
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
	  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/input")).clear();
	  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/input")).sendKeys(templateCopy);
	  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/input")).sendKeys(Keys.TAB);
	  
	  // IG Assignment
	  driver.findElement(By.xpath("//*[@id=\"ImplementationGuideId\"]")).sendKeys(templateIG);
	  driver.findElement(By.xpath("//*[@id=\"ImplementationGuideId\"]")).sendKeys(Keys.TAB);
	 
	  // Click Next
	  driver.findElement(By.xpath("//*[@id=\"Step1\"]/div[6]/button[1]")).click();
  
	  //Copy Step 2 - Constraint Generation
	  WebDriverWait wait7 = new WebDriverWait(driver, 60);
	  WebElement element7 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("Step2")));
	  assertTrue("Could not find \"Copy Step 2\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Copy Step 2:[\\s\\S]*$"));

	  // Click the Finish option
	  driver.findElement(By.xpath("//*[@id=\"FinishButton\"]")).click();
	  
	  // Confirm the Template Editor opens
       driver.manage().timeouts().pageLoadTimeout(30, TimeUnit.SECONDS);
	   WebDriverWait wait12 = new WebDriverWait(driver, 60);
	   WebElement element12 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("TemplateEditor")));
	   assertTrue("Could not find \"Browse Templates\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Implied Template/Profile[\\s\\S]*$"));
	   
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[4]/a")).click();
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[3]/a")).click();
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[2]/a")).click();
	   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[1]/a")).click();
	   
	   // Confirm the the correct Template Name appears in the Editor
	   driver.manage().timeouts().pageLoadTimeout(30, TimeUnit.SECONDS);
	   WebDriverWait wait13 = new WebDriverWait(driver, 60);
	   wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/h3/span[2]"), templateCopy));
	   assertTrue("Could not find \"Template Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateCopy) >= 0);
	   
	  // Return to the Template Browser
	  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[1]/div[2]/button")).click();
	  WebDriverWait wait14 = new WebDriverWait(driver, 60);
	  WebElement element14 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[1]/div[2]/ul/li[1]/a")));
	  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div/div[1]/div[2]/ul/li[1]/a")).click();
	  
	  // Confirm the Clear Filter option is available and clear existing Search Criteria
	   WebDriverWait wait15 = new WebDriverWait(driver, 60);
	   WebElement element15 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")));
       driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")).click();

       // Return to the Trifolia Home Page
       ReturnHome("Welcome to Trifolia Workbench");  
}
  
 // TEST 5: Move Template to a new IG
  @Test
	  public void MoveTemplate(String templateCopy, String templateCopyOID, String templateIG, String templateType, String permissionUserName) throws Exception {
	  if (permissionUserName == "lcg.admin")
	  {
		  
		// Open the Template Browser
		   OpenTemplateBrowser();
		   
		  // Find the template.
		  FindTemplate("Automation Test Template Copy", "urn:oid:2.2.2.2.2.2.2.2", "Automation Test IG");	 
		   
		  //  Open the Template Viewer
		      driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[6]/div/a[1]")).click();
		  
	      // Confirm the Template Viewer opens and the correct template appears
		 	 if (permissionUserName == "lcg.admin")
		 		{
		 		ConfirmTemplateViewer("Automation Test Template Copy", "urn:oid:2.2.2.2.2.2.2.2");
		 		}
		 	 
		 	 if (permissionUserName == "hl7.member")
			 	{
		 		ConfirmTemplateViewer("HL7 Member Test Template Copy", "urn:oid:2.2.2.2.2.2.2.2");
			 	}
		 	 
	   // Move the Template
		  driver.findElement(By.xpath("//*[@id=\"bs-example-navbar-collapse-1\"]/ul/li[7]/a")).click();
		  
		  // Confirm the Move Template form appears
		  WebDriverWait wait = new WebDriverWait(driver, 60);
		  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/h2"), "Move: Step 1"));
		
		  // Select the new IG     
		  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/select")).sendKeys(templateIG); 
		  WebDriverWait wait1 = new WebDriverWait(driver, 60);
		  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/select"), templateIG));
		  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/select")).sendKeys(Keys.TAB);

		  // Select the new Template Type
		  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/select")).sendKeys(templateType);
		  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/select")).sendKeys(Keys.TAB);
		  
		  // Click on the Next option   
		  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[4]/button[1]")).click();
		  WebDriverWait wait2 = new WebDriverWait(driver, 60);
		  WebElement element2 = wait1.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[2]/div[2]/button[1]")));
		   
		  // Click the Finish option
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
		     WebDriverWait wait5 = new WebDriverWait(driver, 60);
		     WebElement element5 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("ViewTemplate")));
		 
		     // Confirm the correct Template appears in the Template Viewer
		     WebDriverWait wait9= new WebDriverWait(driver, 60);
		  	 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/a"), templateCopy));
		     assertTrue("Could not find \"Template Copy Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateCopy) >= 0);
		   
		  // Return to the Template Browser
		     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[1]/a")).click();
		
	     // Confirm the Template Browser appears
		  WebDriverWait wait6 = new WebDriverWait(driver, 60);
		  WebElement element6 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("BrowseTemplates")));
		  assertTrue("Could not find \"Browse Templates\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Browse Templates[\\s\\S]*$"));Thread.sleep(1000);
	
		  // Confirm the Clear Filter option is available and clear existing Search Criteria
		   WebDriverWait wait7 = new WebDriverWait(driver, 60);
		   WebElement element7 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")));
	       driver.findElement(By.xpath("/html/body/div[2]/div/div/form/div/span/button[1]")).click(); 
		  
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
    public void DeleteTemplate(String templateCopy, String templateCopyOID, String permissionUserName) throws Exception {
	 
	  // Open the Template Browser
	   OpenTemplateBrowser();
	
	  // Find the Template to be Deleted
	    if (permissionUserName == "lcg.admin") 
		{
		FindTemplate("Automation Test Template Copy", "urn:hl7ii:1.2.3.4.5.6.7.8.9.10", "Consolidation V2");	 
		}
		
		
		if (permissionUserName == "hl7.member") 
		{
		FindTemplate("HL7 Member Test Template Copy", "urn:oid:2.2.2.2.2.2.2.2", "HL7 Member Test IG");	 
		}
		 
	    // Open the Template Viewer
		    if (permissionUserName == "lcg.admin") 
		    {
		    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[1]/td[6]/div/a[1]")).click();
		    }
		    else
		    {
		    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[1]/td[5]/div/a[1]")).click();
		    }
		
		    // Confirm the Template Viewer opens and the correct template appears
		 	 if (permissionUserName == "lcg.admin")
		 		{
		 		ConfirmTemplateViewer("Automation Test Template Copy", "urn:oid:1.2.3.4.5.6.7.8.9.10");
		 		}
		 	 if (permissionUserName == "hl7.member")
			 	{
		 		ConfirmTemplateViewer("HL7 Member Test Template Copy", "urn:oid:2.2.2.2.2.2.2.2");
			 	}
		 	 
	        //Open the Template Delete Form and Confirm the correct Template Appears
			if (permissionUserName == "lcg.admin") 
			    {
					driver.findElement(By.xpath("//*[@id=\"bs-example-navbar-collapse-1\"]/ul/li[9]/a")).click();
			    }
		    else
			    {
			    	driver.findElement(By.xpath("//*[@id=\"bs-example-navbar-collapse-1\"]/ul/li[7]/a")).click();
			    }
		
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
	    Thread.sleep(500); 
		 //Switch the driver context to the "Successfully deleted value set?" alert
		 Alert alertDialog4 = driver.switchTo().alert();
		 //Get the alert text
		 String alertText4 = alertDialog4.getText();
		 // assertTrue(driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Successfully deleted value set[\\s\\S]*$"));
		 //Click the OK button on the alert.
		 alertDialog4.accept();
	
	    // Confirm the Template Listing appears
		  WebDriverWait wait15 = new WebDriverWait(driver, 60);                    
		  wait15.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/thead/tr[1]/th[2]"), "Identifier"));
	    
	    // Return to the Trifolia Home Page
	    ReturnHome("Welcome to Trifolia Workbench");  

    }
}
