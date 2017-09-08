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
public class B_SecurityFunctions {
	private WebDriver driver;
	private String baseUrl;
	private boolean acceptNextAlert = true;
	private StringBuffer verificationErrors = new StringBuffer();
	public B_SecurityFunctions() {}
	
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

	
	
	
}
