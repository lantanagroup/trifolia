package trifolia.pkg;

import java.util.regex.Pattern;
import java.util.concurrent.TimeUnit;

import org.junit.*;

import static org.junit.Assert.*;
import static org.hamcrest.CoreMatchers.*;

import org.openqa.selenium.*;
import org.openqa.selenium.firefox.FirefoxDriver;
import org.openqa.selenium.firefox.FirefoxProfile;
import org.openqa.selenium.firefox.internal.ProfilesIni;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.Select;
import org.openqa.selenium.support.ui.WebDriverWait;

@Ignore
public class G_TerminologyFunctions {
  private WebDriver driver;
  private String baseUrl;
  private boolean acceptNextAlert = true;
  private StringBuffer verificationErrors = new StringBuffer();
  public G_TerminologyFunctions() {}
  
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
//                      PART I - CREATE, EDIT and DELETE VALUE SETS
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

  public void waitForPageLoad() 
  {
	WebDriverWait wait = new WebDriverWait(driver, 30);
	     wait.until(ExpectedConditions.jsReturnsValue("return document.readyState ==\"complete\";"));		
  }
  public void OpenTerminologyBrowser() throws Exception {
	    
	  // waitForPageLoad();
	  
	  //  Open the Terminology Browser and find the Value Set
		driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[2]/a")).click();
		driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[2]/ul/li[3]/a")).click();
		
	   // Wait for the Terminology Browser to appear
      	WebDriverWait wait = new WebDriverWait(driver, 60);
		WebElement element = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/h3")));
		assertTrue("Could not find \"Browse Terminology\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Browse Terminology[\\s\\S][\\s\\S]*$"));
		
//       // Wait for page to completely load
//		  WebDriverWait wait1 = new WebDriverWait(driver, 120);		
//		  wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("//*[@id=\"ctl00_mainBody\"]/div[7]")));	
		 	
		  // Wait for page to completely load
		  WebDriverWait wait1 = new WebDriverWait(driver, 120);		
		  wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[8]")));	
	  	
		// Wait for page to completely load
		  WebDriverWait wait2 = new WebDriverWait(driver, 120);		                     
		  wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/div[5]")));
	  
		//Confirm the Value Set Page appears.
			WebDriverWait wait3 = new WebDriverWait(driver, 60);                   
			wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/table/thead/tr/th[1]"), "Name"));
}
  
  public void FindValueSet(String valueSetName, String valueSetOID, String fieldLabel1) throws Exception {
	  
	   // Confirm the page is loaded
	      // waitForPageLoad();
	  
	  //Confirm the Value Set Page appears.
		WebDriverWait wait = new WebDriverWait(driver, 60);                   
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/table/thead/tr/th[2]"), "Identifier"));
	  		  
		  // Wait for page to completely load
		  WebDriverWait wait1 = new WebDriverWait(driver, 120);		
		  wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[8]")));	
	  	
		// Wait for page to completely load
		  WebDriverWait wait2 = new WebDriverWait(driver, 120);		                     
		  wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/div[5]")));
	      
	      //Clear existing search criteria
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/form/div/span/button[1]")).click();	
		 
	    // Wait for page to completely load
		  WebDriverWait wait3 = new WebDriverWait(driver, 120);		                     
		  wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/div[5]")));
	      Thread.sleep(2000);
	      
	      WebDriverWait wait4 = new WebDriverWait(driver, 60);                   
		  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/table/thead/tr/th[2]"), "Identifier"));
		  
		//Add new search criteria 		                       
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/form/div/input")).click();
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/form/div/input")).sendKeys(valueSetName);
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/form/div/input")).sendKeys(Keys.TAB);
		
		//Click Search               
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/form/div/span/button[2]")).click();	
		
		 // Wait for page to completely load
		  WebDriverWait wait5 = new WebDriverWait(driver, 120);		
		  wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("//*[@id=\"ctl00_mainBody\"]/div[8]")));	

		//Confirm the search is complete   
		WebDriverWait wait6 = new WebDriverWait(driver, 60);                   
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/table/thead/tr/th[1]"), "Name"));
		
		//Confirm the correct Value Set appears
		WebDriverWait wait7 = new WebDriverWait(driver, 60);                   
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/table/tbody/tr/td[2]"), valueSetOID));
		assertTrue("Could not find \"Value Set OID\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(valueSetOID) >= 0); 		
}
  
  public void SaveValueSet() throws Exception {
	  
	  // Confirm the page is loaded
      // waitForPageLoad();
  
	    //Save the Value Set
	    WebDriverWait wait = new WebDriverWait(driver, 60);
		WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("//*[@id=\"editValueSetDialog\"]/div/div/div[3]/button[1]")));
	    driver.findElement(By.xpath("//*[@id=\"editValueSetDialog\"]/div/div/div[3]/button[1]")).click();

	    //Confirm the Alert appears
		   WebDriverWait wait0 = new WebDriverWait(driver, 60);
		   wait0.until(ExpectedConditions.alertIsPresent());

	      // 3.1 Switch the driver context to the "Successfully saved value set" alert
	      Alert alertDialog1 = driver.switchTo().alert();
	      // 3.2 Get the alert text
	      String alertText1 = alertDialog1.getText();
	      // 3.3 Click the OK button on the alert.
	      alertDialog1.accept();
	      // assertTrue(driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Successfully saved value set[\\s\\S]*$"));
	      Thread.sleep(10000);
	      
	      // Wait for page to completely load
		  WebDriverWait wait1 = new WebDriverWait(driver, 120);		
		  wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/div[4]")));	
	  
	      // Wait for page to completely load
		  WebDriverWait wait2 = new WebDriverWait(driver, 120);		           
		  wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/div[5]")));
	  
		  // Confirm Terminology Browser  page refreshes
		  WebDriverWait wait3 = new WebDriverWait(driver, 60);
		  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/table/thead/tr/th[2]"), "Identifier"));
		  
		   //Clear existing search criteria
			driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/form/div/span/button[1]")).click();	
			 
		  // Wait for page to completely load
			  WebDriverWait wait4 = new WebDriverWait(driver, 120);		
			  wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("//*[@id=\"ctl00_mainBody\"]/div[8]")));	
}
  public void FindCodeSystem(String codeSystemName, String codeSystemOID, String fieldLabel2) throws Exception {
		  
	  // Confirm the page is loaded
         // waitForPageLoad();
  
		// Confirm the Code Systems page appears
		WebDriverWait wait = new WebDriverWait(driver, 60);
		WebElement element = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/table/thead/tr/th[3]")));
		assertTrue("Could not find \"Code System Members\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*#/Members[\\s\\S][\\s\\S]*$"));
		
	    // Wait for page to fully load
	    WebDriverWait wait1 = new WebDriverWait(driver, 120);		
	    wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[8]"))); 

	    // Wait for page to refresh
	    WebDriverWait wait2 = new WebDriverWait(driver, 120);		
	    wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/div[5]")));    
	    
	   //Clear existing search criteria 
		 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/form/div/span/button[1]")).click();

		 // Wait for page to refresh
	    WebDriverWait wait3 = new WebDriverWait(driver, 120);		
	    wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/div[5]"))); 

	    // Confirm the search box is clickable
	    WebDriverWait wait4 = new WebDriverWait(driver, 60);
		WebElement element4 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/form/div/input")));
	    
		// Search for the Code System
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/form/div/input")).sendKeys(codeSystemName);
		Thread.sleep(1000);
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/form/div/span/button[2]")).click();

	    //Confirm the correct Code System is returned
		WebDriverWait wait5 = new WebDriverWait(driver, 60);
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/table/tbody/tr/td[2]"), codeSystemOID));
	 	assertTrue("Could not find \"Code System OID\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(codeSystemOID) >= 0);
}

  public void SaveCodeSystem() throws Exception {
	  
	  // Confirm the page is loaded
         // waitForPageLoad();
  
	   //Save the Code System
	     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[3]/div/div/div[3]/button[1]")).click();

	     //Confirm the Alert appears
		   WebDriverWait wait = new WebDriverWait(driver, 60);
		   wait.until(ExpectedConditions.alertIsPresent());
		   
	     // Switch the driver context to the "Successfully saved code system" alert
		    Alert alertDialog5 = driver.switchTo().alert();
		    //Get the alert text
		    String alertText5 = alertDialog5.getText();
		    //Click the OK button on the alert.
		    alertDialog5.accept();
		    // assertTrue(driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Successfully saved code system[\\s\\S]*$"));
		    Thread.sleep(2000);
		    
		  // Confirm the user is returned to the Code Systems Page
		    WebDriverWait wait2 = new WebDriverWait(driver, 60);
			WebElement element = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/table/thead/tr/th[3]")));
			assertTrue("Could not find \"Code System Members\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*#/Members[\\s\\S][\\s\\S]*$"));
			
			// Confirm Code System Browser text appears
			WebDriverWait wait3 = new WebDriverWait(driver, 60);
			wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/table/thead/tr/th[2]"), "Identifier"));
		
		  //Clear existing search criteria 
			 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/form/div/span/button[2]")).click();
			                               		
		    // Wait for page to fully load
		    WebDriverWait wait4 = new WebDriverWait(driver, 120);		
		    wait.until(ExpectedConditions.invisibilityOfElementLocated(By.cssSelector("div.blockUI.blockOverlay"))); 

  }
 
  public void ReturnHome(String welcomeMessage) throws Exception {
	  
	   // Return to the Trifolia Home Page
	    driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[1]/a")).click();
	    WebDriverWait wait4 = new WebDriverWait(driver, 60);
	    WebElement element4 = wait4.until(ExpectedConditions.visibilityOfElementLocated(By.id("appnav")));
	    
	    //Confirm the Welcome Message appears
		WebDriverWait wait = new WebDriverWait(driver, 60);
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/h2"), welcomeMessage));
		assertTrue("Could not find \"Value Set OID\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(welcomeMessage) >= 0);   
	
}

  
// TEST 1:  Browse a Value Set
  @Test
    public void BrowseValueSet(String valueSetName, String valueSetOID, String conceptID, String permissionUserName) throws Exception {
    
	// Open Terminology Browser
	  
	   OpenTerminologyBrowser();
	
	   // Confirm the page is loaded
	      // waitForPageLoad();
	  
	// Find the Value Set
	if (permissionUserName == "lcg.admin")
	{
		FindValueSet("LanguageCode", "urn:oid:2.16.840.1.113883.2.20.3.190", "Identifier");
	}
	
	if (permissionUserName == "lcg.user")
	{
		FindValueSet("Family Member Value Set", "urn:oid:2.16.840.1.113883.1.11.19579", "Identifier");
	}
	
	if (permissionUserName == "hl7.member")
	{
		FindValueSet("Observation Interpretation (HL7)", "urn:oid:2.16.840.1.113883.1.11.78", "Identifier");
	}
	
	if (permissionUserName == "hl7.user")
	{
		FindValueSet("HL7 BasicConfidentialityKind", "2.16.840.1.113883.1.11.16926", "Identifier");
	}
	
	//Expand the Value Set and check members
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/table/tbody/tr/td[1]/div/a")).click();
	driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/table/tbody/tr/td[1]/div/ul/li[1]/a")).click();
	
	// Confirm the View Value Sets form appears
	WebDriverWait wait = new WebDriverWait(driver, 60);
	WebElement element = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/h1/span")));
    assertTrue("Could not find \"Value Set Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(valueSetName) >= 0);

    //Confirm the Value Set Member appear
	WebDriverWait wait3 = new WebDriverWait(driver, 60);
	WebElement element3 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/div[2]/div[1]/table/tbody/tr[2]/td[1]")));
    assertTrue("Could not find \"Value Set Concept\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(conceptID) >= 0);
    
    // Return to the Value Set Browser
       OpenTerminologyBrowser();
    
       // Confirm the page is loaded
	      // waitForPageLoad();
		  WebDriverWait wait1 = new WebDriverWait(driver, 120);		           
		  wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/div[5]")));
	    
	//Clear existing search criteria
	  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/form/div/span/button[1]")).click();
	 	
	  // Confirm the page is re-loaded
        // waitForPageLoad();
	   WebDriverWait wait2 = new WebDriverWait(driver, 120);		
	   wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/div[5]"))); 

    // Return to the Trifolia Home Page
    ReturnHome("Welcome to Trifolia Workbench");
    
 }
    
// TEST 2:  Create the Value Set
  @Test
    public void CreateValueSet(String valueSetName, String valueSetOid, String valueSetCode, String valueSetDescription, String valueSetSourceUrl
    		) throws Exception { 
    

	     OpenTerminologyBrowser();
	     
	     // Confirm the page is loaded
	      // waitForPageLoad();
	      
		// Confirm the Add Value Set option is available.
		WebDriverWait wait = new WebDriverWait(driver, 60);
		WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/div[1]/div[2]/div/button")));
		assertTrue("Could not find \"Browse Terminology\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Add Value Set[\\s\\S][\\s\\S]*$"));
	
		// Launch the Value Set Editor
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/div[1]/div[2]/div/button")).click();

		 // Confirm the page is loaded
	      // waitForPageLoad();
	      
		 //Confirm the Value Set Editor appears
		WebDriverWait wait1 = new WebDriverWait(driver, 60);
	    WebElement element1 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("//*[@id=\"editValueSetDialog\"]/div/div/div[1]/h4")));
	    assertTrue("Could not find \"Edit Value Set\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Edit Value Set[\\s\\S][\\s\\S]*$"));
	    
	    // Open the Add new Value Set form
	    // driver.findElement(By.xpath("//*[@id=\"valuesets\"]/div[1]/span/button[2]")).click();

		// Add Value Set Meta Data
		driver.findElement(By.xpath("//*[@id=\"valueSetName\"]")).sendKeys(valueSetName);
		driver.findElement(By.xpath("//*[@id=\"valueSetOid\"]")).sendKeys(valueSetOid);
	    driver.findElement(By.xpath("//*[@id=\"valueSetCode\"]")).sendKeys(valueSetCode);
	    driver.findElement(By.xpath("//*[@id=\"valueSetDescription\"]")).sendKeys(valueSetDescription);
	    driver.findElement(By.xpath("//*[@id=\"valueSetIncomplete\"]")).click();
	    driver.findElement(By.xpath("//*[@id=\"valueSetSourceUrl\"]")).sendKeys(valueSetSourceUrl);
    
	    SaveValueSet();
	    
	    // Confirm the page is loaded
	      // waitForPageLoad();
	      
	    // Return to the Trifolia Home Page
	    ReturnHome("Welcome to Trifolia Workbench");     
}
      
// TEST 3:  Edit the Value Set
  @Test
    public void EditValueSet(String valueSetName, String valueSetOID, String concept1Code, String concept1Name, String concept1CodeSystem, String concept1Status,String concept2Code, String concept2Name,
    		 String concept2CodeSystem, String concept2Status, String concept3Code, String concept3Name, String concept3CodeSystem, String concept3Status, String permissionUserName) throws Exception {  

	 // Open Terminology Browser
	    OpenTerminologyBrowser();
	    
	    // Confirm the page is loaded
	      // waitForPageLoad();
	      
	    // Find the Value Set
	    if (permissionUserName == "lcg.admin")
			{
				FindValueSet("Automation Test Gender Value Set", "urn:oid:2.2.2.2.2.2.2.2", "Identifier");
			}
		if (permissionUserName == "hl7.member")
			{
				FindValueSet("HL7 Member Test Gender Value Set", "urn:oid:1.2.3.4.5.6.7.8", "Identifier");
			}
		
		//Open the Value Set Editor and confirm the correct Value Set appears
		WebDriverWait wait = new WebDriverWait(driver, 60);
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/table/tbody/tr/td[2]"), valueSetOID));
		assertTrue("Could not find \"Value Set OID\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(valueSetOID) >= 0);
	    
		 //Launch the Member Editor 
		 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/table/tbody/tr/td[1]/div/a/span[2]")).click();
		 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/table/tbody/tr/td[1]/div/ul/li[3]/a")).click();
		 
		 // Confirm the page is loaded
	      // waitForPageLoad();
	      
		 // Confirm the Edit Concept Form appears
		 WebDriverWait wait1 = new WebDriverWait(driver, 60);
		 WebElement element1 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("//*[@id=\"EditValueSetConcepts\"]/h2")));
		 assertTrue("Could not find \"Value Set Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(valueSetName) >= 0);
    
      // Add Concept Member Meta Data
		 
		 // Concept Code
	     driver.findElement(By.xpath("//*[@id=\"EditValueSetConcepts\"]/table/tfoot/tr/td[1]/input")).sendKeys(concept1Code);
	     // Concept Name
	 	 driver.findElement(By.xpath("//*[@id=\"EditValueSetConcepts\"]/table/tfoot/tr/td[2]/input")).sendKeys(concept1Name);
	 	 // Concept Code System
	     driver.findElement(By.xpath("//*[@id=\"EditValueSetConcepts\"]/table/tfoot/tr/td[3]/select")).sendKeys(concept1CodeSystem);
	     driver.findElement(By.xpath("//*[@id=\"EditValueSetConcepts\"]/table/tfoot/tr/td[3]/select")).sendKeys(Keys.TAB);
	     //Concept Status
	     driver.findElement(By.xpath("//*[@id=\"EditValueSetConcepts\"]/table/tfoot/tr/td[4]/select")).sendKeys(concept1Status);
	     driver.findElement(By.xpath("//*[@id=\"EditValueSetConcepts\"]/table/tfoot/tr/td[4]/select")).sendKeys(Keys.TAB);
	     // Select 
    	 driver.findElement(By.xpath("//*[@id=\"EditValueSetConcepts\"]/table/tfoot/tr/td[5]/div/div[2]/button/i")).click();
    	 driver.findElement(By.xpath("//*[@id=\"ctl00_mainBody\"]/div[6]/div[1]/table/tfoot/tr[1]/th")).click();
    	 Thread.sleep(200);
	     driver.findElement(By.xpath("//*[@id=\"EditValueSetConcepts\"]/table/tfoot/tr/td[6]/div/button")).click();

	     
     // Add Concept 2 details
	     //Concept Code
	     driver.findElement(By.xpath("//*[@id=\"EditValueSetConcepts\"]/table/tfoot/tr/td[1]/input")).sendKeys(concept2Code);
	     //Concept Name
	 	 driver.findElement(By.xpath("//*[@id=\"EditValueSetConcepts\"]/table/tfoot/tr/td[2]/input")).sendKeys(concept2Name);
	 	 //Concept Code System
	     driver.findElement(By.xpath("//*[@id=\"EditValueSetConcepts\"]/table/tfoot/tr/td[3]/select")).sendKeys(concept2CodeSystem);
	     driver.findElement(By.xpath("//*[@id=\"EditValueSetConcepts\"]/table/tfoot/tr/td[3]/select")).sendKeys(Keys.RETURN);
	     //Concept Status
	     driver.findElement(By.xpath("//*[@id=\"EditValueSetConcepts\"]/table/tfoot/tr/td[4]/select")).sendKeys(concept2Status);
	     driver.findElement(By.xpath("//*[@id=\"EditValueSetConcepts\"]/table/tfoot/tr/td[4]/select")).sendKeys(Keys.RETURN);
	     //Select
		 driver.findElement(By.xpath("//*[@id=\"EditValueSetConcepts\"]/table/tfoot/tr/td[5]/div/div[2]/button/i")).click();
		 driver.findElement(By.xpath("//*[@id=\"ctl00_mainBody\"]/div[6]/div[1]/table/tfoot/tr[1]/th")).click();
		 Thread.sleep(500);
	     driver.findElement(By.xpath("//*[@id=\"EditValueSetConcepts\"]/table/tfoot/tr/td[6]/div/button")).click();
     
    //Add Concept 3    
	     //Concept Code
	    driver.findElement(By.xpath("//*[@id=\"EditValueSetConcepts\"]/table/tfoot/tr/td[1]/input")).sendKeys(concept3Code);
	    //Concept Name
		driver.findElement(By.xpath("//*[@id=\"EditValueSetConcepts\"]/table/tfoot/tr/td[2]/input")).sendKeys(concept3Name);
		 //Concept Code system
	    driver.findElement(By.xpath("//*[@id=\"EditValueSetConcepts\"]/table/tfoot/tr/td[3]/select")).sendKeys(concept3CodeSystem);
	    driver.findElement(By.xpath("//*[@id=\"EditValueSetConcepts\"]/table/tfoot/tr/td[3]/select")).sendKeys(Keys.TAB);
	    //Concept Status
	    driver.findElement(By.xpath("//*[@id=\"EditValueSetConcepts\"]/table/tfoot/tr/td[4]/select")).sendKeys(concept3Status);
	    driver.findElement(By.xpath("//*[@id=\"EditValueSetConcepts\"]/table/tfoot/tr/td[4]/select")).sendKeys(Keys.TAB);
	    // Select Date
		driver.findElement(By.xpath("//*[@id=\"EditValueSetConcepts\"]/table/tfoot/tr/td[5]/div/div[2]/button/i")).click();
		driver.findElement(By.xpath("//*[@id=\"ctl00_mainBody\"]/div[6]/div[1]/table/tfoot/tr[1]/th")).click();
		// Add Concept
	    driver.findElement(By.xpath("//*[@id=\"EditValueSetConcepts\"]/table/tfoot/tr/td[6]/div/button")).click();

    //Save the updated Value Set Members 
    driver.findElement(By.xpath("//*[@id=\"EditValueSetConcepts\"]/div[2]/button")).click();

    //Confirm the Alert appears
	   WebDriverWait wait0 = new WebDriverWait(driver, 60);
	   wait0.until(ExpectedConditions.alertIsPresent());
	   
      // Switch the driver context to the "Successfully saved value set" alert
      Alert alertDialog2 = driver.switchTo().alert();
      // Get the alert text
      String alertText2 = alertDialog2.getText();
      // Click the OK button on the alert.
      alertDialog2.accept();
      // assertTrue(driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Successfully saved value set[\\s\\S]*$"));
      
      // Wait for page to completely load
        // waitForPageLoad();
		 WebDriverWait wait2 = new WebDriverWait(driver, 120);		
		 wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("//*[@id=\"ctl00_mainBody\"]/div[7]")));
 
	  // Click Cancel to return to the Value Set Browser    
		 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/a")).click();
	     Thread.sleep(1000);
		 
		  // Wait for page to completely load
	      // waitForPageLoad();
		  WebDriverWait wait3 = new WebDriverWait(driver, 120);		
		  wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[8]")));
	
		//Clear existing search criteria 
		   WebDriverWait wait4 = new WebDriverWait(driver, 60);                                                    
		   WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/form/div/span/button[1]")));	
		   driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/form/div/span/button[1]")).click();
		
    	 // Wait for page to re-load
		    // waitForPageLoad();
			 WebDriverWait wait5 = new WebDriverWait(driver, 120);		
			 wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/div[5]"))); 
	     
       // Return to the Trifolia Home Page
       ReturnHome("Welcome to Trifolia Workbench");
    }
   
// TEST 4:  Delete the Value Set
  @Test
    public void DeleteValueSet(String valueSetName, String valueSetOID, String permissionUserName) throws Exception {  

	// Open Terminology Browser
	  
	   OpenTerminologyBrowser();
	   
	   // Find the Value Set
	    if (permissionUserName == "lcg.admin")
			{
				FindValueSet("Automation Test Gender Value Set", "urn:oid:2.2.2.2.2.2.2.2", "Identifier");
			}
		if (permissionUserName == "hl7.member")
			{
				FindValueSet("HL7 Member Test Gender Value Set", "urn:oid:1.2.3.4.5.6.7.8", "Identifier");
			}
	    
    //Delete the Value Set 
		
		 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/table/tbody/tr/td[1]/div/a/span[2]")).click();
		 driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/table/tbody/tr/td[1]/div/ul/li[4]/a")).click();
    
		 //Confirm the Alert appears
		   WebDriverWait wait = new WebDriverWait(driver, 60);
		   wait.until(ExpectedConditions.alertIsPresent());
	    
	    //Switch the driver context to the "Are you sure you want to delete this value set?" alert
	    Alert alertDialog3 = driver.switchTo().alert();
	    //Get the alert text
	    String alertText3 = alertDialog3.getText();
	    //Click the OK button on the alert.
	    alertDialog3.accept();
	    Thread.sleep(1000); 
		 //Switch the driver context to the "Successfully deleted value set?" alert
		 Alert alertDialog4 = driver.switchTo().alert();
		 //Get the alert text
		 String alertText4 = alertDialog4.getText();
		 // assertTrue(driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Successfully deleted value set[\\s\\S]*$"));
		 //Click the OK button on the alert.
		 alertDialog4.accept();
		 
		 // Wait for page to completely load
		 WebDriverWait wait1 = new WebDriverWait(driver, 120);		
		 wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("//*[@id=\"ctl00_mainBody\"]/div[7]"))); 

	   // Confirm the Terminology Browser appears
		 WebDriverWait wait2 = new WebDriverWait(driver, 60);                   
	     wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/table/thead/tr/th[2]"), "Identifier"));
   
//	   //Clear existing search criteria
//		  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/form/div/span/button[1]")).click();	
			 
	   // Wait for page to completely load
		  WebDriverWait wait3 = new WebDriverWait(driver, 120);		
		  wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("//*[@id=\"ctl00_mainBody\"]/div[8]")));	
	
	    // Return to the Trifolia Home Page
	    ReturnHome("Welcome to Trifolia Workbench");
		   
}

// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
//                               PART II - CREATE, EDIT and DELETE CODE SYSTEMS
// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    
// TEST 1: Browse a Code System
  @Test
    public void BrowseCodeSystem(String codeSystemName, String codeSystemOID, String permissionUserName) throws Exception {

		// Open Terminology Browser
	  
	       OpenTerminologyBrowser();
	   
	    // Confirm the Value Set Page appears
			WebDriverWait wait = new WebDriverWait(driver, 60);                   
			wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[1]/table/thead/tr/th[2]"), "Identifier"));
      
		// Click the Code Systems tab
		driver.findElement(By.xpath("/html/body/div[2]/div/div/ul/li[2]/a")).click();
		
		// Find the Code System
		if (permissionUserName == "lcg.admin")
		{
			FindCodeSystem("FIPS 5-2 (State)", "urn:oid:2.16.840.1.113883.6.92", "#/Members");
		}
		if (permissionUserName == "hl7.member")
		{
			FindCodeSystem("HL7 V2.5 Route of Administration codes", "2.16.840.1.113883.12.162", "#/Members");
		}

		//Open the Code System Editor for Admin Users
		if (permissionUserName == "lcg.admin") 
	       {
				driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/table/tbody/tr/td[1]/div/a/span[2]")).click();
				driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/table/tbody/tr/td[1]/div/ul/li[1]/a")).click();
				
				//Confirm the Code System Editor is opened
				 WebDriverWait wait1 = new WebDriverWait(driver, 60);
				 WebElement element = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("//*[@id=\"editCodeSystemDialog\"]/div/div/div[1]/h4")));
			     assertTrue("Could not find \"Edit Code System\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Edit Code System[\\s\\S][\\s\\S]*$"));
		
			     //Cancel and return to the Code System Browser
			     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[3]/div/div/div[3]/button[2]")).click();
			     WebDriverWait wait2 = new WebDriverWait(driver, 60);
				 WebElement element2 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/form/div/span/button[1]")));	
				 Thread.sleep(1000);
				 
			    // Wait for page to re-load
				  WebDriverWait wait3 = new WebDriverWait(driver, 120);		
				  wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/div[3]/div/div/div[1]/h4"))); 
		
			   //Clear existing search criteria 
			     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/form/div/span/button[1]")).click();
			    
				 // Wait for page to re-load
				 WebDriverWait wait4 = new WebDriverWait(driver, 120);		
				 wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/div[5]"))); 
			
			  // Confirm screen is refreshed
				 WebDriverWait wait5 = new WebDriverWait(driver, 60);
				 WebElement element5 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/table/tbody/tr[3]/td[1]/div/a")));	
					
			    // Return to the Trifolia Home Page
			    ReturnHome("Welcome to Trifolia Workbench");	    
		   }
			if (permissionUserName == "lcg.user")
			{
				FindCodeSystem("HL7 Ethnicity", "urn:oid:2.16.840.1.113883.5.50", "#/Members");
			}
			if (permissionUserName == "hl7.user")
			{
				FindCodeSystem("HL7 V2.5 Route of Administration codes", "2.16.840.1.113883.12.162", "#/Members");
			}
			
	    // Return to the Trifolia Home Page
	    ReturnHome("Welcome to Trifolia Workbench");
	    
 }
    
// TEST 2: Create a new Code System
  @Test
    public void CreateCodeSystem(String codeSystemName, String codeSystemOID, String codeSystemDescription) throws Exception {

		// Open Terminology Browser
	  
          OpenTerminologyBrowser();

		// Click the Code Systems tab
		driver.findElement(By.xpath("/html/body/div[2]/div/div/ul/li[2]/a")).click();
		
		// Confirm the Code Systems page appears
		WebDriverWait wait = new WebDriverWait(driver, 60);
		WebElement element = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/table/thead/tr/th[3]")));
		assertTrue("Could not find \"Browse Terminology\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*#/Members[\\s\\S][\\s\\S]*$"));
	
	    // Open the Add Code System form
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/div[1]/div[2]/div/button")).click();

		//Confirm the Code System Editor appears
	    WebDriverWait wait2 = new WebDriverWait(driver, 60);
		WebElement element2 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("//*[@id=\"editCodeSystemDialog\"]/div/div/div[1]/h4")));
		assertTrue("Could not find \"Edit Code System\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Edit Code System[\\s\\S][\\s\\S]*$"));
    
	    //Add the Code System data elements
	 	 driver.findElement(By.xpath("//*[@id=\"codeSystemName\"]")).sendKeys(codeSystemName);
	 	 driver.findElement(By.xpath("//*[@id=\"codeSystemOid\"]")).sendKeys(codeSystemOID);
	     driver.findElement(By.xpath("//*[@id=\"editCodeSystemDialog\"]/div/div/div[2]/div[3]/textarea")).sendKeys(codeSystemDescription);
     
	     // Save the Code System
	     
	     SaveCodeSystem();
     
	     // Return to the Trifolia Home Page
	     ReturnHome("Welcome to Trifolia Workbench"); 
    }
     
// TEST 3: Edit the "Automation Test Vaccine Code System"
  @Test
    public void EditCodeSystem(String codeSystemName, String codeSystemOID, String codeSystemDescription, String permissionUserName) throws Exception {
	  
	// Open Terminology Browser
	  
      OpenTerminologyBrowser();
      
		// Click the Code Systems tab
		driver.findElement(By.xpath("/html/body/div[2]/div/div/ul/li[2]/a")).click();
		
		// Confirm the Code Systems page appears
		WebDriverWait wait = new WebDriverWait(driver, 60);
		WebElement element = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/table/thead/tr/th[3]")));
		assertTrue("Could not find \"Browse Terminology\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*#/Members[\\s\\S][\\s\\S]*$"));
	
		// Find the Code System
			if (permissionUserName == "lcg.admin")
			{
				FindCodeSystem("Automation Test Vaccine Code System", "urn:oid:2.4.4.4.4.4.4.4", "#/Members");
			}
			if (permissionUserName == "hl7.member")
			{
				FindCodeSystem("HL7 Member Test Vaccine Code System", "urn:oid:1.5.5.5.5.5.5.5", "#/Members");
			}

		// Open the Code System Editor and Change the Description
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/table/tbody/tr/td[1]/div/a")).click();
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/table/tbody/tr/td[1]/div/ul/li[1]/a")).click();

		//Confirm the Code System Editor appears
	    WebDriverWait wait3 = new WebDriverWait(driver, 60);
		WebElement element3 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("//*[@id=\"editCodeSystemDialog\"]/div/div/div[1]/h4")));
		assertTrue("Could not find \"Edit Code System\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Edit Code System[\\s\\S][\\s\\S]*$"));
    
	    //Change the Description
	    driver.findElement(By.xpath("//*[@id=\"editCodeSystemDialog\"]/div/div/div[2]/div[3]/textarea")).sendKeys(codeSystemDescription);

	    // Save the Code System
	     SaveCodeSystem();
     
	     // Return to the Trifolia Home Page
	     ReturnHome("Welcome to Trifolia Workbench");
     
    }
   
//TEST 4: Delete the "Automation Test Vaccine Code System"
  @Test
    public void DeleteCodeSystem(String codeSystemName, String codeSystemOID, String permissionUserName) throws Exception {
	  
// Open Terminology Browser
	  
      OpenTerminologyBrowser();
      
		// Click the Code Systems tab
		driver.findElement(By.xpath("/html/body/div[2]/div/div/ul/li[2]/a")).click();
		
		// Confirm the Code Systems page appears
		WebDriverWait wait = new WebDriverWait(driver, 60);
		WebElement element = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/table/thead/tr/th[3]")));
		assertTrue("Could not find \"Browse Terminology\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*#/Members[\\s\\S][\\s\\S]*$"));
	
		// Find the Code System
			if (permissionUserName == "lcg.admin")
			{
				FindCodeSystem("Automation Test Vaccine Code System", "urn:oid:2.4.4.4.4.4.4.4", "#/Members");
			}
			if (permissionUserName == "hl7.member")
			{
				FindCodeSystem("HL7 Member Test Vaccine Code System", "urn:oid:1.5.5.5.5.5.5.5", "#/Members");
			}
	
     // Delete the Code System
	 	
		WebDriverWait wait1 = new WebDriverWait(driver, 60);
		WebElement element1 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/table/tbody/tr/td[1]/div/a/span[2]")));
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/table/tbody/tr/td[1]/div/a/span[2]")).click();
		WebDriverWait wait2 = new WebDriverWait(driver, 60);
		WebElement element2 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/table/tbody/tr/td[1]/div/ul/li[2]/a")));
		driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/table/tbody/tr/td[1]/div/ul/li[2]/a")).click();

    //Confirm the Alert appears
	   WebDriverWait wait0 = new WebDriverWait(driver, 120);
	   wait0.until(ExpectedConditions.alertIsPresent());
	   
	 //Switch the driver context to the "Are you sure you want to delete this code system?" alert
	 Alert alertDialog7 = driver.switchTo().alert();
	 // Get the alert text
	 String alertText7 = alertDialog7.getText();
	 // Click the OK button on the alert.
	 alertDialog7.accept();
	 //Switch the driver context to the "Successfully deleted code system?" alert
	 Alert alertDialog8 = driver.switchTo().alert();
	 //Get the alert text
	 String alertText8 = alertDialog8.getText();
	 // 3.6 Click the OK button on the alert.
	 alertDialog8.accept();
	// 3.4 Clear the search criteria
	 Thread.sleep(1000);
	 
	   //Clear existing search criteria 
     driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/form/div/span/button[1]")).click();
    
	 // Wait for page to re-load
	 WebDriverWait wait3 = new WebDriverWait(driver, 120);		
	 wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/div[5]"))); 

  // Confirm screen is refreshed
	 WebDriverWait wait4 = new WebDriverWait(driver, 60);
	 WebElement element4 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div[2]/table/tbody/tr[3]/td[1]/div/a")));	
		
    // Return to the Trifolia Home Page
    ReturnHome("Welcome to Trifolia Workbench");	    
     
  }
}
