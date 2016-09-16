package trifolia.pkg;

import java.util.regex.Pattern;
import java.util.concurrent.TimeUnit;

import org.junit.*;

import static org.junit.Assert.*;

import org.openqa.selenium.*;

import com.google.common.base.Predicate;

import org.openqa.selenium.support.ui.Select;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.WebDriverWait; 
import org.openqa.selenium.firefox.FirefoxDriver;
import org.openqa.selenium.firefox.FirefoxProfile;
import org.openqa.selenium.firefox.internal.ProfilesIni;


@Ignore
public class B_ImplementationGuideFunctions {
  private WebDriver driver;
  private String baseUrl;
  private boolean acceptNextAlert = true;
  private StringBuffer verificationErrors = new StringBuffer();
  private static Boolean createdImplementationGuide = false;
  public B_ImplementationGuideFunctions() {}
  
  public void setDriver(WebDriver driver){
	  this.driver = driver;
  }
  public void WebDriverWait(WebDriver driver, long timeOutInSeconds) {
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
  
  public void waitForPageLoad() 
  {
	WebDriverWait wait = new WebDriverWait(driver, 30);
	     wait.until(ExpectedConditions.jsReturnsValue("return document.readyState ==\"complete\";"));		
  }
  
  public void OpenIGBrowser()
  {
		// Confirm Welcome Message is present
		WebDriverWait wait = new WebDriverWait(driver, 60);
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/h2"),"Welcome to Trifolia Workbench!"));
		assertTrue("Unable to confirm Login",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Welcome to Trifolia Workbench![\\s\\S][\\s\\S]*$"));

	    //Open the IG Browser        
		driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[2]/a")).click();
	    driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[2]/ul/li[1]/a")).click();

	     // Wait for the page to fully load
	 	  waitForPageLoad();
	    
	    //Confirm the IG Browser appears
	    WebDriverWait wait1 = new WebDriverWait(driver, 60);
	    WebElement element1 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("BrowseImplementationGuides")));
	    assertTrue("Could not find \"Browse Implementation Guides\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Browse Implementation Guides[\\s\\S]*$"));
	       
	    // Clear existing Search Criteria
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/button")).click();
	    
	     // Wait for the page to fully re-load
	 	  waitForPageLoad();
  } 
  
  public void FindImplementationGuide(String implementationGuideName) throws Exception {
	
	// Wait for the page to fully load
	  waitForPageLoad();
	  
	// Confirm the Search options are available
		 WebDriverWait wait = new WebDriverWait(driver, 60);
	 	 WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[1]/div/div[2]/ul/li[2]/a")));
	 	    
    // Search for the Implementation Guide
    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/input")).sendKeys(implementationGuideName);
    
      // Wait for the page to fully load
 	     waitForPageLoad();
 	  
    //Confirm the search is complete
    WebDriverWait wait3 = new WebDriverWait(driver, 60);                    
    wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[2]/td[4]")));    
    
    //Confirm the correct IG is found
    WebDriverWait wait4 = new WebDriverWait(driver, 120);                    
    wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[1]"), implementationGuideName));
    assertTrue("Could not find \"Implementation Guide Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(implementationGuideName) >= 0);
 }
  
  public void ConfirmIGEditor(String implementationGuideName) throws Exception
  {
	// Wait for the page to fully load
      waitForPageLoad();
      
      // Confirm the Edit Implementation Guide Editor appears and click in some fields
	    WebDriverWait wait = new WebDriverWait(driver, 60);
	    WebElement element = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("EditImplementationGuide")));	    
	    assertTrue("Could not find \"Edit Implementation Guide\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Edit Implementation Guide[\\s\\S]*$"));
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div[2]/input")).click();         
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div[3]/input")).click();
	    
	    // Confirm the Template Types option is available.  
	    driver.manage().timeouts().pageLoadTimeout(10, TimeUnit.SECONDS);
	    WebDriverWait wait9 = new WebDriverWait(driver, 60);
	    WebElement element9 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[2]/a")));   

	  // Wait for the page to fully re-load
         waitForPageLoad();  
         Thread.sleep(1000);
     }
  public void SaveImplementationGuide(String implementationGuideName) throws Exception {
		
      // Save the Implementation Guide
      driver.findElement(By.xpath("/html/body/div[2]/div/div/div[3]/button[1]")).click();
 
	//Confirm the Alert appears
	WebDriverWait wait = new WebDriverWait(driver, 60);
	wait.until(ExpectedConditions.alertIsPresent());
	
	 // Switch the driver context to the "Successfully saved implementation Guide" alert
	 Alert alertDialog1 = driver.switchTo().alert();
	 // Get the alert text
	 String alertText1 = alertDialog1.getText();
	 // Click the OK button on the alert.
	 alertDialog1.accept();

	 // Wait for page to fully re-load
	 waitForPageLoad();
	 WebDriverWait wait1 = new WebDriverWait(driver, 5);		
	 wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[8]"))); 
	 
}
  public void ReturnHome(String welcomeMessage) throws Exception {
	  
	   // Return to the Trifolia Home Page
	    driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[1]/a")).click();
	    WebDriverWait wait = new WebDriverWait(driver, 60);
	    WebElement element = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("appnav")));
	    
	    //Confirm the Welcome Message appears
		WebDriverWait wait1 = new WebDriverWait(driver, 60);
		wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/h2"), welcomeMessage));
		assertTrue("Could not find \"Welcome To Trifolia\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(welcomeMessage) >= 0);   
}

  
  @Test
  //Browse an existing Implementation Guide
  public void BrowseImplementationGuide(String implementationGuideName, String Item, String Primitive, String Entries, String fileName, String validationText, String templateType, String cardinality, 
		  String customSchematron, String Permission, String Volume, String Category, String permissionUserName) throws Exception {
	
	  // Open the IG Browser
	     OpenIGBrowser();
	  
	  // Find the Implementation Guide
	  if (permissionUserName == "lcg.admin") 
	     {
		  FindImplementationGuide("Healthcare Associated Infection Reports Release 9");   
	     }
	  
	  if (permissionUserName == "lcg.user") 
	  	 {
		  FindImplementationGuide("PHARM HIT Demo");
	  	 }
	  
	  if (permissionUserName == "hl7.member") 
	  	 {
		  FindImplementationGuide("Public Health Case Report Release 1");
	  	 }
	  
	  if (permissionUserName == "hl7.user") 
	  	 {
		  FindImplementationGuide("Cath/PCI Registry Reporting Implementation Guide");
	  	 }
	  
	// Open the IG Viewer  
	    if (permissionUserName == "lcg.admin") 
		    {
		    	driver.findElement(By.xpath("//*[@id=\"BrowseImplementationGuides\"]/table/tbody/tr/td[5]/div/button[1]")).click();
		    }
	    if (permissionUserName == "lcg.user") 
		    {
		    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[4]/div/button")).click();
		    }
	    if (permissionUserName == "hl7.member" ) 
	    {
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[4]/div/button")).click();
	    }
	    if (permissionUserName == "hl7.user") 
	    {
	    	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[4]/div/button")).click();
	    }
    // Confirm the correct IG appears in the viewer 
	    
	 // Wait for the page to fully load
		  waitForPageLoad();
		  
        WebDriverWait wait = new WebDriverWait(driver, 60);                     
  	    wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[2]/div/h2"), implementationGuideName));
        assertTrue("Could not find \"Implementation Guide Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(implementationGuideName) >= 0);
    
    if (permissionUserName == "lcg.admin") 
	    {
	    // Notes Tab Validation
   	 
    	// Wait for the page to fully load
		  waitForPageLoad();
		  
    	WebDriverWait wait0 = new WebDriverWait(driver, 60);
 	    WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/ul/li[2]/a")));
    	driver.findElement(By.xpath("/html/body/div[2]/div/div/ul/li[2]/a")).click();
	    
    	WebDriverWait wait1 = new WebDriverWait(driver, 60);
	    wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[3]/div[2]/table/thead/tr/th[2]"), "Item"));
	    assertTrue("Could not find \"Item\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(Item) >= 0);
	    
	    // Primitives Tab Validation
	    WebDriverWait wait2 = new WebDriverWait(driver, 60);
 	    WebElement element2 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/ul/li[3]/a")));
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/ul/li[3]/a")).click();
	    WebDriverWait wait3 = new WebDriverWait(driver, 60);
	    wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[3]/div[3]/table/thead/tr/th[3]"), "Primitive"));
        assertTrue("Could not find \"Primitive\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(Primitive) >= 0);
	  
	    // Audit Trail Tab Validation
	    WebDriverWait wait4 = new WebDriverWait(driver, 60);
 	    WebElement element4 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/ul/li[4]/a")));
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/ul/li[4]/a")).click();
	    WebDriverWait wait5 = new WebDriverWait(driver, 60);
	    wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[3]/div[4]/table/thead/tr/th[1]"), "Who"));
	    assertTrue("Could not find \"Entries\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(Entries) >= 0);
	  
	    // Files Tab Validation 
	    WebDriverWait wait6 = new WebDriverWait(driver, 60);
 	    WebElement element6 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/ul/li[5]/a")));   	
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/ul/li[5]/a")).click();
	    WebDriverWait wait7 = new WebDriverWait(driver, 60);
	    wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[3]/div[5]/table/thead/tr/th[1]"), "Name"));	  
	    assertTrue("Could not find \"File Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(fileName) >= 0);
	    
	 //Confirm the Edit Menu options are available
	    // Click on the Edit top menu option and select Implementation Guide
	                               
	    driver.findElement(By.xpath("//*[@id=\"bs-example-navbar-collapse-1\"]/ul/li[2]/a")).click();         
	    driver.findElement(By.xpath("//*[@id=\"bs-example-navbar-collapse-1\"]/ul/li[2]/ul/li[1]/a")).click();
	    
	    // Confirm the Edit Implementation Guide Viewer appears and click in some fields
	    WebDriverWait wait8 = new WebDriverWait(driver, 60);
	    WebElement element8 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("EditImplementationGuide")));	    
	    assertTrue("Could not find \"Edit Implementation Guide\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Edit Implementation Guide[\\s\\S]*$"));
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div[2]/input")).click();         
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div[3]/input")).click();
	    
	    // Confirm the Template Types option is available.  
	    driver.manage().timeouts().pageLoadTimeout(10, TimeUnit.SECONDS);
	    WebDriverWait wait9 = new WebDriverWait(driver, 60);
	    WebElement element9 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[2]/a")));
	    
		 // Wait for the page to fully load
		    waitForPageLoad();
	    
	    // Open the Template Types Page and validate information in the Templates Types Tab
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[2]/a")).click(); 
	    WebDriverWait wait10 = new WebDriverWait(driver, 60);                   
	    wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/div[2]/div[1]/div[1]"), "document"));
		assertTrue("Could not find \"Template Type\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateType) >= 0);
		  
	    // Confirm the Cardinality Tab option is available
	    WebDriverWait wait11 = new WebDriverWait(driver, 60);
	    WebElement element11 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[3]/a")));
	    
	    // Click on Cardinality Tab and validate information in the Cardinality Tab
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[3]/a")).click();  
	    WebDriverWait wait12 = new WebDriverWait(driver, 60);
	    WebElement element12 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[3]/div[1]/div")));
	    assertTrue("Could not find \"Cardinality\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(cardinality) >= 0);
		
	    // Confirm the Custom Schematron Tab option is available
	    WebDriverWait wait13 = new WebDriverWait(driver, 60);
	    WebElement element13 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[4]/a")));
	    
	    // Click on Custom Schematron Tab and validate information in the Custom Schematron Tab
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[4]/a")).click();  
	    WebDriverWait wait14 = new WebDriverWait(driver, 60);
	    WebElement element14 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[2]/input")));
	    assertTrue("Could not find \"Cardinality\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(customSchematron) >= 0);

	    // Confirm the Permissions Tab option is available
	    WebDriverWait wait15 = new WebDriverWait(driver, 60);
	    WebElement element15 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[5]/a")));
	    
	    // Click on Permissions Tab and validate information in the Permissions Tab
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[5]/a")).click();  
	    WebDriverWait wait16 = new WebDriverWait(driver, 60);
	    WebElement element16 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[2]/input")));
	    assertTrue("Could not find \"Permissions\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(Permission) >= 0);

	    // Confirm the Volumes Tab option is available
	    WebDriverWait wait17 = new WebDriverWait(driver, 60);
	    WebElement element17 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[6]/a")));
	    
	    // Click on Volumes Tab and validate information in the Volumes Tab
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[6]/a")).click();  
	    WebDriverWait wait18 = new WebDriverWait(driver, 60);
	    WebElement element18 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[2]/input")));
	    assertTrue("Could not find \"Volume\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(Volume) >= 0);

	    // Confirm the Categories Tab option is available
	    WebDriverWait wait19 = new WebDriverWait(driver, 60);
	    WebElement element19 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[7]/a")));
	    
	    // Click on Categories Tab and validate information in the Categories Tab
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[7]/a")).click();  
	    WebDriverWait wait20 = new WebDriverWait(driver, 60);  
	    WebElement element20 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[7]/div[2]/div")));
		assertTrue("Could not find \"Category\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(Category) >= 0);

	    // Click on Cancel to return to the IG Viewer
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[3]/button[2]")).click();  
	    WebDriverWait wait21 = new WebDriverWait(driver, 60);                     
	  	wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[2]/div/h2"), implementationGuideName));
	    assertTrue("Could not find \"Template Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(implementationGuideName) >= 0);
	     
	    // Click on the Edit top menu option and select Bookmarks 
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[2]/a")).click();
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[2]/ul/li[2]/a")).click();
	    
	    // Confirm the Bookmarks page opens
	    WebDriverWait wait22 = new WebDriverWait(driver, 60);
	    WebElement element22 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("EditBookmarks")));    
	    assertTrue("Could not find \"Edit Bookmarks\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Edit Bookmarks[\\s\\S]*$"));

	    WebDriverWait wait23 = new WebDriverWait(driver, 60);
	    WebElement element23 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[2]/div[97]/div[1]/input")));
	  	 
	    // Click on one entry in the list
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/div[97]/div[1]/input")).click();  
	    
	    // Click on Cancel to return to the IG Viewer
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[3]/div/div/button[2]")).click();
	    WebDriverWait wait24 = new WebDriverWait(driver, 60);                     
	  	wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[2]/div/h2"), implementationGuideName));
	    assertTrue("Could not find \"Template Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(implementationGuideName) >= 0);
	    
	    // Click on the Edit top menu option and select Files 
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[2]/a")).click();
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[2]/ul/li[3]/a")).click();
	   
	    // Confirm the Files Page Opens
	    WebDriverWait wait25 = new WebDriverWait(driver, 60);
	    WebElement element25 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/table/thead/tr/th[1]")));
	    assertTrue("Could not find \"Manage Implementation Guide Files\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Manage Implementation Guide Files[\\s\\S]*$"));
	    
	    // Confirm the correct IG appears in the Files Tab
	    WebDriverWait wait26 = new WebDriverWait(driver, 60);
	    wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/p"), implementationGuideName));
	    assertTrue("Could not find \"Implementation Guide Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(implementationGuideName) >= 0);
	    
	    // Click on Cancel to return to the IG Viewer
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/button[2]")).click();
	    WebDriverWait wait27 = new WebDriverWait(driver, 60);                     
	  	wait25.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[2]/div/h2"), implementationGuideName));
	    assertTrue("Could not find \"Implemenation Guide Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(implementationGuideName) >= 0);
	    
	    // Confirm Export Menu options are available
	    driver.findElement(By.xpath("//*[@id=\"bs-example-navbar-collapse-1\"]/ul/li[2]/a")).click();
	    
	    // Return to the Trifolia Home Page    
	    driver.findElement(By.xpath("//*[@id=\"appnav\"]/div/div[2]/ul/li[1]/a")).click();
	    
	    WebDriverWait wait28 = new WebDriverWait(driver, 60);
	    WebElement element28 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("appnav")));
	    }
    else if (permissionUserName == "hl7.member") 
	    {
    	
	    driver.findElement(By.xpath("//*[@id=\"ViewImplementationGuide\"]/ul/li[2]/a")).click();
	    Thread.sleep(500);
	    driver.findElement(By.xpath("//*[@id=\"ViewImplementationGuide\"]/ul/li[3]/a")).click();
	    Thread.sleep(500);
	    driver.findElement(By.xpath("//*[@id=\"ViewImplementationGuide\"]/ul/li[4]/a")).click();
	    Thread.sleep(500);
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/ul/li[1]/a")).click();
	    
	    //Confirm the user is returned to the Templates Tab
	    WebDriverWait wait26 = new WebDriverWait(driver, 60);
	    wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[3]/div[1]/div[2]/div/div/div[1]"), "document"));
	   
	    // Return to the Trifolia Home Page
	     ReturnHome("Welcome to Trifolia Workbench");   
	     
	    }
    else
	    {
	    
	     // Return to the Trifolia Home Page
	     ReturnHome("Welcome to Trifolia Workbench");    
	     
	    }  
  }  
  
//TEST 2:  Add Permissions to an existing Implementation Guide
  @Test
  public void PermissionImplementationGuide(String implementationGuideName, 
		  String validationText, String permissionUserName) throws Exception {
	  
	  // Open the IG Browser
	     OpenIGBrowser();
	     
	  // Find the Implementation Guide
	  if (permissionUserName == "lcg.admin") 
	     {
		  FindImplementationGuide("Test IHE PCC");   
	     }
 	      
	  // Launch the Implementation Guide Editor
		if (permissionUserName == "lcg.admin") 
		  {                              
		  	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[5]/div/a[2]")).click();  	
		  }
		else
		  {
		  	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[4]/div/a[2]")).click();
		  }
 
		 // Wait for the page to fully re-load
            waitForPageLoad();
        
             ConfirmIGEditor("Test IHE PCC");
          
		    // Confirm the Permissions tab is visible  
	        WebDriverWait wait = new WebDriverWait(driver, 60);
		  	wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[5]/a"), "Permissions"));
		    
	      // Navigate to the permissions page
	         WebDriverWait wait1 = new WebDriverWait(driver, 60);
	         WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[2]/a")));       
	         driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[2]/a")).click();
	         driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[3]/a")).click();
	         driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[4]/a")).click();
	         driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[5]/a")).click();
	         
	         // Wait for the page to fully load
			    waitForPageLoad();
	         
	         // Confirm the Permissions page appears
	         WebDriverWait wait2 = new WebDriverWait(driver, 60);
			 wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[5]/div[1]/div[1]/strong"), "View Permission"));
			  assertTrue("Could not find \"Add Permission\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*View Permission[\\s\\S]*$"));
	   
	         // Click the Add option for View permissions and confirm the Add Permission form appears
	         driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[5]/div[1]/div[2]/div/button")).click();
	         WebDriverWait wait3 = new WebDriverWait(driver, 60);
	   	     WebElement element3 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/div[4]/div/div/div[1]/h4")));
	   	     assertTrue("Could not find \"Add Permission\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Add Permission[\\s\\S]*$"));
	      
	   	     // Select the Organization
	   	        driver.findElement(By.xpath("/html/body/div[2]/div/div/div[4]/div/div/div[2]/div/div[1]/div[1]/div/input")).click();

   	     // Click OK
   	        driver.findElement(By.xpath("/html/body/div[2]/div/div/div[4]/div/div/div[3]/button[1]")).click();

   	     // Wait for the page to fully re-load
		    waitForPageLoad();
            
   	     // Confirm the View permissions were added.
   	        WebDriverWait wait4 = new WebDriverWait(driver, 60);
		  	wait4.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[5]/div[2]/div[2]/div[1]/span[1]"), "Entire Organization (LCG)"));
		         
		 // Wait for the page to fully re-load
		    waitForPageLoad();
		    
		    // Confirm the Edit Permission text appears
	        WebDriverWait wait6 = new WebDriverWait(driver, 60);
		  	wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[5]/div[3]/div[1]/strong"), "Edit Permission"));
	
   	     // Confirm the Edit Permission "Add" option is clickable
   	        WebDriverWait wait7 = new WebDriverWait(driver, 60);
   	        WebElement element7 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[5]/div[3]/div[2]/div/button")));
   	           
   	     // Wait for the page to fully re-load
		    waitForPageLoad();
		    Thread.sleep(1000);
			   
   	     // Click the Add Option for Edit permissions
   	        driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[5]/div[3]/div[2]/div/button")).click();
	         
	      // Confirm the Add Permission form appears
   	        WebDriverWait wait8 = new WebDriverWait(driver, 60);
		  	wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[4]/div/div/div[1]/h4"), "Add Permission"));
		    assertTrue("Could not find \"Add Permission\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Add Permission[\\s\\S]*$"));
	        
   	     // Select the Organization
   	        driver.findElement(By.xpath("/html/body/div[2]/div/div/div[4]/div/div/div[2]/div/div[1]/div[1]/div/input")).click();

   	     // Click OK
   	        driver.findElement(By.xpath("/html/body/div[2]/div/div/div[4]/div/div/div[3]/button[1]")).click();

   	     // Confirm the Edit permissions were added.
	        WebDriverWait wait9 = new WebDriverWait(driver, 60);
		  	wait4.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[5]/div[5]/div[1]/span[1]"), "Entire Organization (LCG)"));
		   
   	     // Wait for the page to fully re-load
		    waitForPageLoad();
		    Thread.sleep(1000);
   	        
   	        // Confirm the user is returned to the Permissions page
   	        WebDriverWait wait10 = new WebDriverWait(driver, 60);
   	        WebElement element10 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[5]/div[1]/div[1]/strong")));
   	        assertTrue("Could not find \"View Permission\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*View Permission[\\s\\S]*$"));
   	  
   	        // Confirm the Notify New Users option is available.
	        WebDriverWait wait11 = new WebDriverWait(driver, 60);
		  	wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[2]"), "Notify new users and groups that they have been granted permissions"));
		    
   	        // Click the option to Notify New users they have been granted permissions 	                                        
   	           driver.findElement(By.xpath("/html/body/div[2]/div/div/div[2]/input")).click();
		        
	     // Return to the Trifolia Home Page
	     ReturnHome("Welcome to Trifolia Workbench");    
 }
  
//TEST 3:  Create an Implementation Guide
    @Test
    public void CreateImplementationGuide(String implementationGuideName,String ImplementationGuideDisplayName, String iGWebDisplayName, 
    		String iGWebDescription, String implementationGuideType, String permissionUserName) throws Exception {
    	if (createdImplementationGuide) 
    	{
			return;
		}

    	 // Open the IG Browser
	     OpenIGBrowser();
	     
	    // Search for the Implementation Guide
	    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/input")).sendKeys(implementationGuideName);
	        
//	    {
//	    assertFalse("Found\"Implementation Guide Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(implementationGuideName) >= 0);
//	    createdImplementationGuide = true;
//	    }
//      // Open the IG Editor if the IG does not already exist
//	  	if (createdImplementationGuide = false)
//	  	{
	    
	    // Open the Implementation Guide Editor
		driver.findElement(By.xpath("//*[@id=\"BrowseImplementationGuides\"]/div/div/button/i")).click();
		
	    if (permissionUserName == "lcg.admin" || permissionUserName == "test.user") 
	    	{                            
	    	driver.findElement(By.xpath("//*[@id=\"BrowseImplementationGuides\"]/table/thead/tr/th[5]/div/button")).click();
	    	}
	    else 
	    	{
	    	driver.findElement(By.xpath("//*[@id=\"BrowseImplementationGuides\"]/table/thead/tr/th[4]/div/button")).click();
		    }

		    //Confirm Correct Form Opens and Enter IG Meta Data
		     WebDriverWait wait = new WebDriverWait(driver, 60);
			 WebElement element = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("EditImplementationGuide")));
			 assertTrue("Could not find \"Edit Implementation Guides\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Edit Implementation Guide[\\s\\S]*$"));
			 
			 // Add IG Name
		    driver.findElement(By.xpath("//*[@id=\"general\"]/div[1]/input")).sendKeys(implementationGuideName);
		    driver.findElement(By.xpath("//*[@id=\"general\"]/div[1]/input")).sendKeys(Keys.TAB);
		    
		    // Add IG Display Name
		    driver.findElement(By.xpath("//*[@id=\"general\"]/div[2]/input")).sendKeys(ImplementationGuideDisplayName);
		    driver.findElement(By.xpath("//*[@id=\"general\"]/div[2]/input")).sendKeys(Keys.TAB);
		    
		    // Add Web Display Name
		    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div[3]/input")).sendKeys(iGWebDisplayName);
		    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div[3]/input")).sendKeys(Keys.TAB);
		    
		    // Add Web IG Description
		    driver.findElement(By.xpath("/html/body")).sendKeys(iGWebDescription);
		    driver.findElement(By.xpath("/html/body")).sendKeys(Keys.TAB);
		    
            // Add IG Type
		    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div[6]/select")).sendKeys(implementationGuideType);
		    WebDriverWait wait5 = new WebDriverWait(driver, 60);
		  	wait5.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div[6]/select"), implementationGuideType));
		    
		    // Add Consolidated format Option
		    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div[7]/select")).sendKeys("Yes");
		    driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[1]/div[7]/select")).sendKeys(Keys.TAB);
		    
		    // Wait until the Save option is available.
		    WebDriverWait wait6 = new WebDriverWait(driver, 60);
		    WebElement element6 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[3]/button[1]")));
		    
		    // Find the Implementation Guide
			  if (permissionUserName == "lcg.admin") 
			     {
				  SaveImplementationGuide("1Automation Test IG");   
			     }
			  if (permissionUserName == "hl7.member") 
			  	 {
				  SaveImplementationGuide("1HL7 Member Test IG");
			  	 }   
		      createdImplementationGuide = true;
		      			  
		     // Return to the Trifolia Home Page
		     ReturnHome("Welcome to Trifolia Workbench");    
} 

//Find and Edit an Implementation Guide
    
@Test
    public void EditImplementationGuide(String implementationGuideName, String permissionUserName, String customSchematron) throws Exception {
	if (createdImplementationGuide)
	{

		 // Open the IG Browser
	     OpenIGBrowser();
	     
	  // Find the Implementation Guide
	  if (permissionUserName == "lcg.admin") 
	     {
		  FindImplementationGuide("1Automation Test IG");   
	     }
	  if (permissionUserName == "hl7.member") 
	  	 {
		  FindImplementationGuide("1HL7 Member Test IG");
	  	 }
		  
	  // Open the IG Editor
      if (permissionUserName == "lcg.admin") 
  	  	{
    	  driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr[1]/td[5]/div/a[2]")).click();
  		   
  	  	}
      else 
  		{                              
    	  driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[4]/div/a")).click();
  		}
        
      // Confirm the IG Editor opens with the correct IG
         ConfirmIGEditor("1Automation Test IG");
          
	  // Add Custom Schematron
	      // Wait for page to load
		  WebDriverWait wait2 = new WebDriverWait(driver, 60);
		  WebElement element2 = wait2.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[4]/a")));
          
		  // Click the "Custom Schematron" tab
		  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[1]/a")).click();  
		  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[2]/a")).click();  
		  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[3]/a")).click();  
		  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/ul/li[4]/a")).click();  

          // Confirm the Custom Schematron form appears
		  WebDriverWait wait = new WebDriverWait(driver, 60);
		  WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[4]/div[1]/div[4]/div/button")));
		  assertTrue("Could not find \"Pattern ID\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Pattern ID[\\s\\S]*$"));
		  
	      // Add the Custom Schematron 
		  // Open Edit Custom Schematron form
		  driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[4]/div[1]/div[4]/div/button")).click();
		  WebDriverWait wait4 = new WebDriverWait(driver, 60);
		  WebElement element4 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/div/div[5]/div/div/div[2]/div[1]/select")));
		  assertTrue("Could not find \"Edit Custom Schematron\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Edit Custom Schematron[\\s\\S]*$"));
		 
		  
	      driver.findElement(By.xpath("//*[@id=\"customSchematronDialog\"]/div/div/div[2]/div[1]/select")).sendKeys("Error");
	      driver.findElement(By.xpath("//*[@id=\"customSchematronDialog\"]/div/div/div[2]/div[2]/input")).sendKeys("document-errors");
	      driver.findElement(By.xpath("//*[@id=\"customSchematronDialog\"]/div/div/div[2]/div[3]/textarea")).sendKeys(customSchematron);
	      driver.findElement(By.xpath("//*[@id=\"customSchematronDialog\"]/div/div/div[3]/button[1]")).click();
	      Thread.sleep(500);
	      		  
	      //Save the Implementation Guide
	       // driver.findElement(By.xpath("//*[@id=\"EditImplementationGuide\"]/div[3]/button[1]")).click();

	      // Find the Implementation Guide
		  if (permissionUserName == "lcg.admin") 
		     {
			  SaveImplementationGuide("1Automation Test IG");   
		     }
		  if (permissionUserName == "hl7.member") 
		  	 {
			  SaveImplementationGuide("1HL7 Member Test IG");
		  	 }	
		  
		     // Return to the Trifolia Home Page
		     ReturnHome("Welcome to Trifolia Workbench");    
		     
	}
	else
		{
				return;
		}
} 

//TEST 5: Web View of an Existing Implementation Guide
public void WebViewImplementationGuide(String baseURL, String implementationGuideName, String iGDisplayName, String overviewText, String templateText, String valueSetText, String codeSystemText, String permissionUserName) throws Exception {

	 // Open the IG Browser
        OpenIGBrowser();
        
	  // Find the Implementation Guide
	  if (permissionUserName == "lcg.admin") 
	     {
		  FindImplementationGuide("Healthcare Associated Infection Reports Release 2 DSTU 1 V8");   
	     }
	  if (permissionUserName == "hl7.member") 
	  	 {
		  FindImplementationGuide("Public Health Case Report Release 1");
	  	 }
   String parentHandle = driver.getWindowHandle();
   //Open the IG Editor
   
   //Load the WebViewer page
   if (permissionUserName == "lcg.admin") 
   {
   	    driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[5]/div/a[1]")).click();
   }
   if (permissionUserName == "hl7.member") 
   {
   	   driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[5]/div/a[1]")).click();
   }  
	       
	   // Wait for the page to fully load
	   	  waitForPageLoad();
   
	      for (String winHandle : driver.getWindowHandles()) {
		 // switch focus of WebDriver to WebView page
		   driver.switchTo().window(winHandle); 
		   
		    if (baseURL == "http://dev.trifolia.lantanagroup.com/")
		    {
		    	driver.get("http://dev.trifolia.lantanagroup.com/IG/View/3247#/overview");
		    }
		    if (baseURL == "http://staging.lantanagroup.com:1234/") 
		    {
		    	driver.get("http://staging.lantanagroup.com:1234/IG/View/2231#/overview");
		    }
		    
		    // Wait for the page to fully re-load
		    waitForPageLoad();
		    WebDriverWait wait = new WebDriverWait(driver, 120);                    
		    wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div/div[2]/div")));       
	
		   // Wait for the Web Viewer to be loaded
		    WebDriverWait wait1 = new WebDriverWait(driver, 60);                    
		    wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div/div[1]/div/h3"), "Table of Contents"));
		    assertTrue("WebView Home Page did not appear", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Welcome to the Web-based Implementation Guide[\\s\\S]*$"));	    		    
		   	   
	    // Confirm correct IG is loaded  
		    assertTrue("Could not find \"Implementation Guide Display Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(iGDisplayName) >= 0);
	    
	// Overview page validation
	     
	    // Open the Overview Page 
	    WebDriverWait wait4 = new WebDriverWait(driver, 60);
		WebElement element4 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div/div[1]/div/h4[1]/a")));
		driver.findElement(By.xpath("/html/body/div/div[1]/div/h4[1]/a")).click();
	    
		// Wait for the page to fully load
	   	  waitForPageLoad();
	   	  
		// Confirm the overview page is loaded 
		WebDriverWait wait5 = new WebDriverWait(driver, 60);
	    WebElement element5 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div/div[1]/div/h1")));
	  
	    //Validate the text within the overview page
	    WebDriverWait wait6 = new WebDriverWait(driver, 60);                    
	    wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div/div[1]/div/div/div[1]/div[1]/h1/span[2]"), overviewText));
	    assertTrue("Could not find \"Overview Text\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(overviewText) >= 0);
	    
	    //Return to the Web IG Home page and confirm the Home page appears
	    driver.findElement(By.xpath("/html/body/div/div[1]/span[1]/span/a")).click();
	    WebDriverWait wait7 = new WebDriverWait(driver, 60);
	    WebElement element6 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div/div[1]/div/h2")));
	
	 //Template page validation
	    
	    // Open the Template Page 
		driver.findElement(By.xpath("/html/body/div/div[1]/div/h4[2]/a")).click();
	
		// Wait for the page to fully load
	   	  waitForPageLoad();
	   	  
		//Confirm the Template page is loaded
	    WebDriverWait wait8 = new WebDriverWait(driver, 60);
	    WebElement element8 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div/div[1]/div/h1")));
	  
	    //Validate the text within the Template page
	    WebDriverWait wait9 = new WebDriverWait(driver, 60);                    
	    wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div/div[1]/div/accordion/div/div[1]/div[2]/div/div[2]/ul/li[1]/a"), templateText));
	    assertTrue("Could not find \"Template Text\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(templateText) >= 0);
		   
	    //Return to the Web IG Home page and confirm the Home page appears
	    driver.findElement(By.xpath("/html/body/div/div[1]/span[1]/span/a")).click();
	    WebDriverWait wait10 = new WebDriverWait(driver, 60);
	    WebElement element10 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div/div[1]/div/h2")));
	
     //Value Sets page validation
	    
	    // Wait for the page to fully load
	   	  waitForPageLoad();
	   	  
	    // Open the Value Sets Page 
		driver.findElement(By.xpath("/html/body/div/div[1]/div/h4[3]/a")).click();
	
		//Confirm the Value Sets page is loaded
	    WebDriverWait wait11 = new WebDriverWait(driver, 60);
	    WebElement element11 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div/div[1]/div/h1")));
	  
	    //Validate the text within the Value Sets page
	    WebDriverWait wait12 = new WebDriverWait(driver, 60);                    
	    wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div/div[1]/div/div[3]/div/div/a/strong"), valueSetText));
	    assertTrue("Could not find \"Value Set Text\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(valueSetText) >= 0);
		   
	    //Return to the Web IG Home page 
	    driver.findElement(By.xpath("/html/body/div/div[1]/span[1]/span/a")).click();
	    
	    // Wait for the page to fully load
	   	  waitForPageLoad();
	   	  
	   	// Confirm the Home page appears
	    WebDriverWait wait13 = new WebDriverWait(driver, 60);
	    WebElement element13 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div/div[1]/div/h2")));
	
        //Code Systems page validation
	    
	    // Open the Code Systems Page 
		driver.findElement(By.xpath("/html/body/div/div[1]/div/h4[4]/a")).click();
	
		// Wait for the page to fully load
	   	  waitForPageLoad();
	   	  
		//Confirm the Code Systems page is loaded
	    WebDriverWait wait14 = new WebDriverWait(driver, 60);
	    WebElement element14 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("/html/body/div/div[1]/div/h1")));
	  
	    //Validate the text within the Code Systems page
	    WebDriverWait wait15 = new WebDriverWait(driver, 60);                    
	    wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div/div[1]/div/div/table/tbody/tr[5]/td[1]"), codeSystemText));
	    assertTrue("Could not find \"Code System Text\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(codeSystemText) >= 0);
		   
	    //Return to the Web IG Home page 
	    driver.findElement(By.xpath("/html/body/div/div[1]/span[1]/span/a")).click();
	    
		// Wait for the page to fully load
	   	  waitForPageLoad();
	   	
	   	 // Switch focus to Trifolia Implementation Guide listing Page
		   
		    // driver.switchTo().window(parentHandle);    http://dev.trifolia.lantanagroup.com/IGManagement/List
		      
		      if (baseURL == "http://dev.trifolia.lantanagroup.com/")
			    {
		    	  driver.navigate().to("http://dev.trifolia.lantanagroup.com/IGManagement/List");
			    }
			    if (baseURL == "http://staging.lantanagroup.com:1234/") 
			    {
			    	driver.get("http://staging.lantanagroup.com:1234//IGManagement/List");
			    }
			    
	    // Return to the Trifolia Home Page
	     ReturnHome("Welcome to Trifolia Workbench"); 	     
	 }
	         
}

//TEST 6: Delete an Implementation Guide and it's Version
    @Test
    public void DeleteImplementationGuide(String implementationGuideVersioned, String implementationGuideName, String permissionUserName) throws Exception {
    	
    	
    	 // Open the IG Browser
	        OpenIGBrowser();
	     
	  	  //Search for the IG to be Deleted
	      driver.findElement(By.xpath("//*[@id=\"BrowseImplementationGuides\"]/div/div/button")).click();
	      driver.findElement(By.xpath("//*[@id=\"SearchQuery\"]")).sendKeys(implementationGuideVersioned);
	      
	      //Confirm the correct IG is found
	        WebDriverWait wait = new WebDriverWait(driver, 60);                    
		    wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[1]"), implementationGuideName));
		    assertTrue("Could not find \"Implementation Guide Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(implementationGuideName) >= 0);
	      
	      //Open the IG Viewer
	      if (permissionUserName == "lcg.admin") 
	      {
	      	driver.findElement(By.xpath("//*[@id=\"BrowseImplementationGuides\"]/table/tbody/tr/td[5]/div/button[1]")).click();
	      }
	      else
	      {
	      	driver.findElement(By.xpath("//*[@id=\"BrowseImplementationGuides\"]/table/tbody/tr/td[4]/div/button")).click();
	      }
	      
	      //Confirm the IG Viewer opens 
	      WebDriverWait wait1 = new WebDriverWait(driver, 60);
	      WebElement element1 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("//*[@id=\"ViewImplementationGuide\"]/ul/li[1]/a")));
	      assertTrue("Could not find \"Implementation Guide Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(implementationGuideVersioned) >= 0);
	      
	   //Delete the Versioned IG
	      driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[6]/a")).click();
	      // 2.1 Confirm the Delete IG form opens
	      WebDriverWait wait2 = new WebDriverWait(driver, 180);
	      WebElement element2 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("DeleteImplementationGuide")));
	      assertTrue("Could not find \"Delete Implementation Guides\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Delete Implementation Guide[\\s\\S]*$"));
          
	      //Confirm the correct IG appears 
	      WebDriverWait wait3 = new WebDriverWait(driver, 60);                    
		  wait3.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("//*[@id=\"DeleteImplementationGuide\"]/div/h3"), implementationGuideVersioned));
	      assertTrue("Could not find \"Versioned Implementation Guide\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(implementationGuideVersioned) >= 0);
	      
	      //Delete the Versioned IG and accept Confirmation
	      driver.findElement(By.xpath("/html/body/div[2]/div/div/div/div[2]/button")).click();

	      //Confirm the Alert appears
		   WebDriverWait wait10 = new WebDriverWait(driver, 60);
		   wait10.until(ExpectedConditions.alertIsPresent());
	  
	      // Switch the driver context to the "Successfully Deleted implementation Guide" alert
	      Alert alertDialog11 = driver.switchTo().alert();
	      // Get the alert text
	      String alertText11 = alertDialog11.getText();
	      // assertTrue(driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Are you absolutely sure you want to delete this implementation guide?[\\s\\S]*$"));
	      Thread.sleep(500);
	      // Click the OK button on the alert.
	      alertDialog11.accept();
	      
	      
	   //Delete the primary Implementation Guide
	      //Confirm the user is returned to the IG Browser
	      WebDriverWait wait4 = new WebDriverWait(driver, 60);
	      WebElement element4 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("BrowseImplementationGuides")));
	      assertTrue("Could not find \"Browse Implementation Guides\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Browse Implementation Guides[\\s\\S]*$"));
	  	  
	      //Find the IG to be Deleted
	      driver.findElement(By.xpath("//*[@id=\"BrowseImplementationGuides\"]/div/div/button")).click();
	      driver.findElement(By.xpath("//*[@id=\"SearchQuery\"]")).sendKeys(implementationGuideName);

	      //Confirm the correct IG is found
	        WebDriverWait wait0 = new WebDriverWait(driver, 60);                    
		    wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[1]"), implementationGuideName));
		    assertTrue("Could not find \"Implementation Guide Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(implementationGuideName) >= 0);
	      
	      // 3.3 Open the IG Viewer
	      if (permissionUserName == "lcg.admin") 
	      {
	      	driver.findElement(By.xpath("/html/body/div[2]/div/div/table/tbody/tr/td[5]/div/button")).click();
	      }
	      else
	      {
	      	driver.findElement(By.xpath("//*[@id=\"BrowseImplementationGuides\"]/table/tbody/tr/td[4]/div/button")).click();
	      }
	      
	      //Confirm the correct IG appears in the viewer
	      WebDriverWait wait5 = new WebDriverWait(driver, 60);
	      WebElement element5 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.xpath("//*[@id=\"ViewImplementationGuide\"]/ul/li[1]/a")));
	      assertTrue("Could not find \"Implementation Guide Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(implementationGuideName) >= 0);
	      
	    //Launch the Delete IG form with the selected IG
	      driver.findElement(By.xpath("/html/body/div[2]/div/div/div[1]/div/div[2]/ul/li[6]/a")).click();
	      
	      //Confirm the Delete IG form opens
	      WebDriverWait wait6 = new WebDriverWait(driver, 60);
	      WebElement element6 = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("DeleteImplementationGuide")));
	      assertTrue("Could not find \"Delete Implementation Guides\" on page.",driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Delete Implementation Guide[\\s\\S]*$"));
          
	      //Confirm the correct IG appears 
	      WebDriverWait wait7 = new WebDriverWait(driver, 60);                    
		  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("//*[@id=\"DeleteImplementationGuide\"]/div/h3"), implementationGuideName));
	      assertTrue("Could not find \"Implementation Guide Name\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(implementationGuideName) >= 0);
	      
	      //Delete IG and accept Confirmation, and return to the Trifolia Home Page
	      driver.findElement(By.xpath("/html/body/div[2]/div/div/div/div[2]/button")).click();

	    //Confirm the Alert appears
		   WebDriverWait wait22 = new WebDriverWait(driver, 60);
		   wait.until(ExpectedConditions.alertIsPresent());
	  
	      // 5.1 Switch the driver context to the "Successfully Deleted implementation Guide" alert
	      Alert alertDialog12 = driver.switchTo().alert();
	      // 5.2 Get the alert text
	      String alertText12 = alertDialog12.getText();
	      // assertTrue(driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Are you absolutely sure you want to delete this implementation guide?[\\s\\S]*$"));
	      Thread.sleep(500);
	      // 5.3 Click the OK button on the alert.
	      alertDialog12.accept();

	      // Re-set the IG Created Flag
	      createdImplementationGuide = false;
	      
		     // Return to the Trifolia Home Page
		     ReturnHome("Welcome to Trifolia Workbench");    
		     
    }
}