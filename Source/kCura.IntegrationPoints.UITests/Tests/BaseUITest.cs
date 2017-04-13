using IntegrationPointsUITests.Config;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace IntegrationPointsUITests.Tests
{
    public abstract class BaseUiTest
    {
        protected IWebDriver Driver { get; set; }

        [OneTimeSetUp]
        protected void CreateDriver()
        {
            ChromeDriverService driverService = ChromeDriverService.CreateDefaultService();
            // Otherwise console window appears for chromedriver process
            driverService.HideCommandPromptWindow = true;
            var options = new ChromeOptions();

            // Disables "Save password" popup
            options.AddUserProfilePreference("credentials_enable_service", false);
            options.AddUserProfilePreference("profile.password_manager_enabled", false);
            // Disables "Chrome is being controlled by automated test software." bar
            options.AddArguments("disable-infobars");

            Driver = new ChromeDriver(driverService, options);
            Driver.Manage().Window.Maximize();
            Driver.Url = TestConfig.ServerAddress;
        }

        [OneTimeTearDown]
        protected void CloseAndQuitDriver()
        {
            Driver.Close();
            Driver.Quit();
        }
        
    }
}
