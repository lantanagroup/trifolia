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

public class A_TrifoliaLogin {
  private WebDriver driver;
  private String baseUrl;
  private boolean acceptNextAlert = true;
  private StringBuffer verificationErrors = new StringBuffer();
  
  public A_TrifoliaLogin() {}
  
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
  public void LCGAdminLogin(String permissionUserName, String baseURL) throws Exception {
	  //Login to Trifolia Workbench Dev environment as Lantana Administrator 
	 
		 //Confirm Login Page Appears
		 assertTrue("Login Page did not appear", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Welcome to Trifolia Workbench![\\s\\S]*$"));
		 
		 if (baseURL == "http://dev.trifolia.lantanagroup.com/")
		 {
			 //Switch URL to Debug Mode to bypass Captcha
			 driver.navigate().to("dev.trifolia.lantanagroup.com/Account/Login?debug");
			
			//Enter Username and Password
			 driver.findElement(By.xpath("/html/body/div[2]/div/form/div[2]/input")).clear();
			 driver.findElement(By.xpath("/html/body/div[2]/div/form/div[2]/input")).sendKeys("<ADMIN_USERNAME>");
			 driver.findElement(By.xpath("/html/body/div[2]/div/form/div[3]/input")).clear();
			 driver.findElement(By.xpath("/html/body/div[2]/div/form/div[3]/input")).sendKeys("<ADMIN_PASSWORD>");
		 }
		 if (baseURL == "http://staging.lantanagroup.com:1234/")
		 {
			 //Switch URL to Debug Mode to bypass Captcha
			 driver.navigate().to("http://staging.lantanagroup.com:1234/Account/Login?debug");
			 
			//Enter Username and Password
			 driver.findElement(By.xpath("//*[@id=\"ctl00_mainBody\"]/div[2]/div/form/div[2]/input")).clear();
			 driver.findElement(By.xpath("//*[@id=\"ctl00_mainBody\"]/div[2]/div/form/div[2]/input")).sendKeys("<STAGING_ADMIN_USERNAME>");
			 driver.findElement(By.xpath("//*[@id=\"ctl00_mainBody\"]/div[2]/div/form/div[3]/input")).clear();
			 driver.findElement(By.xpath("//*[@id=\"ctl00_mainBody\"]/div[2]/div/form/div[3]/input")).sendKeys("<STAGING_ADMIN_PASSWORD>"); 
		 }
		 
		 driver.findElement(By.xpath("//*[@id=\"ctl00_mainBody\"]/div[2]/div/form/button ")).click();
		 
		// Confirm Login is Successful
		 WebDriverWait wait = new WebDriverWait(driver, 120);
		 WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.id("appnav")));
		 assertTrue("Unable to confirm Login", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Welcome to Trifolia Workbench![\\s\\S][\\s\\S]*$"));
		
		 // Confirm Welcome Page Text appears
		 WebDriverWait wait1 = new WebDriverWait(driver, 60);                     
	  	 wait1.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/p/strong"), "Did you know?"));	 
	}
  @Test
  public void LCGUserLogin(String permissionUserName, String baseURL) throws Exception {

//Login to Trifolia Workbench Dev environment as Lantana User
	 
		 //Confirm Trifolia Home Page Appears
		 assertTrue("Trifolia Home Page did not appear", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Welcome to Trifolia Workbench![\\s\\S]*$"));

		  if (baseURL == "http://dev.trifolia.lantanagroup.com/")
		   {
			 //Switch URL to Debug Mode to bypass Captcha
			 driver.navigate().to("dev.trifolia.lantanagroup.com/Account/Login?debug");
			 
			//Enter Username and Password
			 driver.findElement(By.xpath("/html/body/div[2]/div/form/div[2]/input")).clear();
			 driver.findElement(By.xpath("/html/body/div[2]/div/form/div[2]/input")).sendKeys("<BASIC_USER_USERNAME>");
			 driver.findElement(By.xpath("/html/body/div[2]/div/form/div[3]/input")).clear();
			 driver.findElement(By.xpath("/html/body/div[2]/div/form/div[3]/input")).sendKeys("<BASIC_USER_PASSWORD>");
		     driver.findElement(By.xpath("//*[@id=\"ctl00_mainBody\"]/div[2]/div/form/button ")).click();

		   }
		   else if (baseURL == "http://staging.lantanagroup.com:1234/")  
		   {
			 //Switch URL to Debug Mode to bypass Captcha
			   driver.navigate().to("http://staging.lantanagroup.com:1234/Account/Login?debug");
				
			 //Enter Username and Password
			 driver.findElement(By.xpath("/html/body/div[2]/div/form/div[2]/input")).clear();
			 driver.findElement(By.xpath("/html/body/div[2]/div/form/div[2]/input")).sendKeys("<STAGING_USER_USERNAME>");
			 driver.findElement(By.xpath("/html/body/div[2]/div/form/div[3]/input")).clear();
			 driver.findElement(By.xpath("/html/body/div[2]/div/form/div[3]/input")).sendKeys("<STAGING_USER_PASSWORD>");
			 driver.findElement(By.xpath("//*[@id=\"ctl00_mainBody\"]/div[2]/div/form/button")).click(); 
		   }
		  
		 	//Complete User Profile Page if it appears
		  if (driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*New Profile[\\s\\S]*$"))
	        	{
		    		driver.findElement(By.xpath("//*[@id=\"mainBody\"]/div/div[1]/input")).sendKeys("Lantana");
		    		driver.findElement(By.xpath("//*[@id=\"mainBody\"]/div/div[2]/input")).sendKeys("User");
		    		driver.findElement(By.xpath("//*[@id=\"mainBody\"]/div/div[3]/input")).sendKeys("111-222-3333");
		    		driver.findElement(By.xpath("//*[@id=\"mainBody\"]/div/div[4]/input")).sendKeys("lantana.user@lcg.org");
		    		driver.findElement(By.xpath("//*[@id=\"mainBody\"]/div/div[5]/input")).sendKeys("Lantana Consulting Group");
		    		driver.findElement(By.xpath("//*[@id=\"mainBody\"]/div/div[7]/button")).click(); 
		        }
	        else 
	        	{
		        	//Confirm Login is Successful
		   		 	assertTrue("Login was not Successful", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Welcome to Trifolia Workbench![\\s\\S][\\s\\S]*$"));
		   		 	
		   		 // Confirm Welcome Page Text appears
		   		 WebDriverWait wait1 = new WebDriverWait(driver, 60);                     
		   	  	 wait1.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/p/strong"), "Did you know?"));
	  	      	}  
	}
  @Test
  public void HL7MemberLogin(String hl7Welcome) throws Exception {
   // Login to Trifolia Workbench Dev environment as HL7 Member via HL7 QCommerce site 

  	   // Step 1 - Confirm the Trifolia Home Page Appears
  	   assertTrue("Trifolia Home Page did not appear", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Welcome to Trifolia Workbench![\\s\\S]*$"));
  	   Thread.sleep(500);
  	   // get the current window handle
  	   String parentHandle = driver.getWindowHandle();
  	   
  	   //Launch the HL7 Login Page
  	     driver.findElement(By.xpath("/html/body/div[2]/div/h3/a")).click();

 	     for (String winHandle : driver.getWindowHandles()) 
 	     {
	  		 // switch focus of WebDriver to HL7 QCommerce login page
	  		   driver.switchTo().window(winHandle); 
	  		   
	  		  WebDriverWait wait = new WebDriverWait(driver, 60);                     
		  	  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/form/div[3]/div[1]/div/div[2]/div[1]"), hl7Welcome));
		      assertTrue("Could not find \"Health Level Seven\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(hl7Welcome) >= 0);
	  		   
	  	     // Enter HL7 Member Username and Password
	  		   driver.findElement(By.xpath("//*[@id=\"ctl00_MainContent_txtUserNameHL7\"]")).sendKeys("<HL7_MEMBER_USERNAME>");
	  		   driver.findElement(By.xpath("//*[@id=\"ctl00_MainContent_txtPasswordHL7\"]")).sendKeys("<HL7_MEMBER_PASSWORD>"); 
	  		   driver.findElement(By.xpath("//*[@id=\"ctl00_MainContent_SubmitHL7\"]")).click();
	  	   }
 	     
  	    // Switch focus to Trifolia Home Page
  	     driver.switchTo().window(parentHandle);
  	     WebDriverWait wait = new WebDriverWait(driver, 60);
  	     WebElement element = wait.until(ExpectedConditions.visibilityOfElementLocated(By.id("appnav")));
  	     assertTrue("Could not find \"Welcome to Trifolia\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Welcome to Trifolia Workbench![\\s\\S]*$"));

  }
  
  @Test
  public void HL7UserLogin(String hl7Welcome) throws Exception {
  // Login to the Trifolia Workbench as HL7 User via HL7 QCommerce site 
  	   
	  //Confirm the Trifolia Home Page Appears
 	   assertTrue("Trifolia Home Page did not appear", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Welcome to Trifolia Workbench![\\s\\S]*$"));
 	   // get the current window handle
 	   String parentHandle = driver.getWindowHandle();
 	   
 	   //Launch the HL7 Login Page
 	     driver.findElement(By.xpath("/html/body/div[2]/div/h3/a")).click();
 	     // assertTrue("QCommerce Login Page did not appear", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Health Level Seven International[\\s\\S]*$"));
 	     for (String winHandle : driver.getWindowHandles()) {
 		
 	    // switch focus of WebDriver to HL7 QCommerce login page
 		   driver.switchTo().window(winHandle); 
 		   
 		  WebDriverWait wait = new WebDriverWait(driver, 60);                     
	  	  wait.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/form/div[3]/div[1]/div/div[2]/div[1]"), hl7Welcome));
	      assertTrue("Could not find \"Health Level Seven\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(hl7Welcome) >= 0);
  				   
  	    //Enter HL7 Username and Password
  		   driver.findElement(By.xpath("//*[@id=\"ctl00_MainContent_txtUserNameHL7\"]")).sendKeys("<HL7_USER_USERNAME>");
  		   Thread.sleep(500);
  		   driver.findElement(By.xpath("//*[@id=\"ctl00_MainContent_txtPasswordHL7\"]")).sendKeys("<HL7_USER_PASSWORD>"); 
  		   driver.findElement(By.xpath("//*[@id=\"ctl00_MainContent_SubmitHL7\"]")).click();
      
  	  	    // Switch focus to Trifolia Home Page
  	  	    Thread.sleep(20000);
  	  	    driver.switchTo().window(parentHandle); 
  	  	    
  		  if (driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*New Profile[\\s\\S]*$"))
        	{
  	    		driver.findElement(By.xpath("//*[@id=\"mainBody\"]/div/div[1]/input")).sendKeys("HL7");
  	    		driver.findElement(By.xpath("//*[@id=\"mainBody\"]/div/div[2]/input")).sendKeys("User");
  	    		driver.findElement(By.xpath("//*[@id=\"mainBody\"]/div/div[3]/input")).sendKeys("111-222-3333");
  	    		driver.findElement(By.xpath("//*[@id=\"mainBody\"]/div/div[4]/input")).sendKeys("hl7.user@hl7.org");
  	    		driver.findElement(By.xpath("//*[@id=\"mainBody\"]/div/div[5]/input")).sendKeys("HL7 International");
  	    		driver.findElement(By.xpath("//*[@id=\"mainBody\"]/div/div[7]/button")).click(); 
  	        }
  		   
  		   else 
      		{
	        	//Confirm Login is Successful
	   		 	assertTrue("Login was not Successful", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Welcome to Trifolia Workbench![\\s\\S][\\s\\S]*$"));
	      	}  
  	   }
  	    // Switch focus to Trifolia Home Page     
  	     driver.switchTo().window(parentHandle); 
  	     WebDriverWait wait1 = new WebDriverWait(driver, 60);                     
	  	 wait1.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/h2"), "Welcome to Trifolia Workbench!"));
	      assertTrue("Could not find \"Welcome to Trifolia\" on page.", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Welcome to Trifolia Workbench![\\s\\S]*$"));
 	
  	    
  }
  
   //Lantana Administrator and Lantana User Logout of Trifolia Workbench 
    @Test
    public void LCGLogout(String permissionUserName) throws Exception {
	 
		//Confirm the User is still logged in 
    	assertTrue("Welcome to Trifolia Workbench!", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Welcome to Trifolia Workbench![\\s\\S]*$"));
		 
    	//Logout the Lantana user
    	if (permissionUserName == "lcg.admin") 
        	{
    		driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[8]/a/span")).click();
    		driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[8]/ul/li[3]/a")).click();
    		
   		    //Confirm Logout is Successful
    		 WebDriverWait wait = new WebDriverWait(driver,60);
    		 WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("//*[@id=\"ctl00_mainBody\"]/div[2]/div/h2")));
   	        assertTrue("Logout was not Successful", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Welcome to Trifolia Workbench![\\s\\S]*$"));
   	        
   	        // Confirm Welcome Page text appears.
  	         WebDriverWait wait1 = new WebDriverWait(driver, 60);                    
  	         wait1.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]"), "Did you know?"));
  	    
        	}
        else if (permissionUserName == "lcg.user")
        	{
        	driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[5]/a/span")).click();
    		driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[5]/ul/li[3]/a")).click();

   		    //Confirm Logout is Successful
    		 WebDriverWait wait = new WebDriverWait(driver, 60);
    		 WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("//*[@id=\"appnav\"]/div/div[2]/ul/li[3]/a/span")));
   	         assertTrue("Logout was not Successful", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Welcome to Trifolia Workbench![\\s\\S]*$"));
   	         
   	         // Confirm Welcome Page text appears.
   	         WebDriverWait wait1 = new WebDriverWait(driver, 60);                    
   	         wait1.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[2]/div/div/div[1]"), "Did you know?"));
   	         
  	      	}  
       }
    
    //HL7 Member and HL7 User Logout of Trifolia Workbench 
    @Test
    public void HL7Logout(String hl7Welcome, String baseURL, String permissionUserName) throws Exception { 	
        	
    		 // Log the HL7 Member or User out of Trifolia
    	
    	     if (permissionUserName == "hl7.member")
	    	     {
	    	     driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[5]/a/span")).click();
	        	 WebDriverWait wait = new WebDriverWait(driver, 60);
	    		 WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[1]/div/div[2]/ul/li[5]/ul/li[3]/a"))); 	       
	    		 driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[5]/ul/li[3]/a")).click();
	    	     }

    	     if (permissionUserName == "hl7.user")
	    	     {
	    	     driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[5]/a/span")).click();
	        	 WebDriverWait wait = new WebDriverWait(driver, 60);
	    		 WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[1]/div/div[2]/ul/li[5]/ul/li[3]/a"))); 	       
	    		 driver.findElement(By.xpath("/html/body/div[1]/div/div[2]/ul/li[5]/ul/li[3]/a")).click();
	    	     }
    	     
    		// Confirm Trifolia Logout is Successful
    		 WebDriverWait wait = new WebDriverWait(driver, 60);
    		 WebElement element = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/h3/a"))); 	       
   	         assertTrue("Logout was not Successful", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Welcome to Trifolia Workbench![\\s\\S]*$"));
   	      
   	         String parentHandle = driver.getWindowHandle();
   	         
   	         
   		    // Navigate to the HL7 Test Server page
   	         if (baseURL == "http://dev.trifolia.lantanagroup.com/")
   	         	{
   	        	 	driver.navigate().to("http://hl7.amg-hq.net/");
   	         	}
   	         if (baseURL == "http://staging.lantanagroup.com:1234/")
	         	{
	        	 	driver.navigate().to("http://www.hl7.org/index.cfm");
	         	}
 	         
  	         for (String winHandle : driver.getWindowHandles()) 
  	         
  	         {
	           // switch focus of WebDriver to HL7 Test Server page
	             driver.switchTo().window(winHandle); 

	           // Confirm the HL7 Welcome Page appears
	   		     WebDriverWait wait2 = new WebDriverWait(driver, 60);                     
	 	  	     wait2.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/div[1]/div[1]/div[1]/div[1]"), hl7Welcome));
	 	         assertTrue("Could not find \"Health Level Seven\" on page.", driver.findElement(By.cssSelector("BODY")).getText().indexOf(hl7Welcome) >= 0);
	   		   
	 	        //Logout the HL7 Member or User from the HL7 Test Server
	             driver.findElement(By.xpath("/html/body/div[1]/div[1]/div[3]/a")).click();	
	             
	             if (baseURL == "http://dev.trifolia.lantanagroup.com/")
	   	         	{
	            	  WebDriverWait wait3 = new WebDriverWait(driver, 60);                     
	 		  	     wait3.until(ExpectedConditions.textToBePresentInElementLocated(By.xpath("/html/body/form/div[4]/div[3]/div[3]/div[3]/div[2]/div[2]/div/div[1]/div[1]/h3"), "Existing Users, Please Log In"));
	 		         // driver.findElement(By.xpath("/html/body/div[1]/div[1]/div[3]/a")).click();
	   	         	}
	   	       
  		         // Wait for page to fully load
  				 WebDriverWait wait4 = new WebDriverWait(driver, 5);		
  				 wait.until(ExpectedConditions.invisibilityOfElementLocated(By.xpath("/html/body/div[1]/div[1]/div[3]/a"))); 
  	         
	            //Switch focus back to Trifolia Home Page
                driver.navigate().to("http://dev.trifolia.lantanagroup.com/");
                WebDriverWait wait5 = new WebDriverWait(driver, 60);
       		    WebElement element5 = wait.until(ExpectedConditions.elementToBeClickable(By.xpath("/html/body/div[2]/div/h3/a"))); 	       
      	         assertTrue("Logout was not Successful", driver.findElement(By.cssSelector("BODY")).getText().matches("^[\\s\\S]*Welcome to Trifolia Workbench![\\s\\S]*$"));  
        }
  	}
  	       
  	@After
       public void tearDown() throws Exception {
         // driver.quit();
         String verificationErrorString = verificationErrors.toString();
         if (!"".equals(verificationErrorString)) {
           fail(verificationErrorString);
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
