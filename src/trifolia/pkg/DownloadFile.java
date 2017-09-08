package trifolia.pkg;

import org.junit.*;

import static org.junit.Assert.*;
import org.openqa.selenium.*;
import org.openqa.selenium.firefox.FirefoxDriver;
import org.openqa.selenium.firefox.FirefoxProfile;
import org.openqa.selenium.firefox.internal.ProfilesIni;
import org.openqa.selenium.WebDriver;
public class DownloadFile {

	public static void main(String[] args) {
		  
		// Create a profile
		FirefoxProfile profile=new FirefoxProfile();
		 
		// Set preferences for file type 
		profile.setPreference("browser.helperApps.neverAsk.openFile", "application/octet-stream");
	    profile.setPreference("browser,helperapps.neverAsk.SaveToDisk", "application/octet-stream");
	    profile.setPreference("browser.download.dir", "E:\\Selenium\\TrifoliaDownloads\\");
	    
		// Open browser with profile                   
		WebDriver driver=new FirefoxDriver(profile);
		  
//		// Set implicit wait
//		driver.manage().timeouts().implicitlyWait(30, TimeUnit.SECONDS);
		  
		// Maximize window
		driver.manage().window().maximize();
		  
		// Open APP to download application
		driver.get("https://www.mozilla.org/en-US/firefox/new/");
		  
		// Click on download 
		driver.findElement(By.xpath("/html/body/div[2]/div/main/div[1]/div/div/div/div/section/div[1]/div/ul/li[2]/a")).click();
		    
		 }
}
