using kCura.IntegrationPoint.Tests.Core;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using Serilog;
using System;
using System.Drawing;

namespace kCura.IntegrationPoints.UITests.Driver
{
	public static class DriverFactory
	{
		private const int _BROWSER_WIDTH = 1920;

		private const int _BROWSER_HEIGHT = 1400;

		public static RemoteWebDriver CreateDriver()
		{
			ChromeDriverService driverService = ChromeDriverService.CreateDefaultService();
			// Otherwise console window appears for chromedriver process
			driverService.HideCommandPromptWindow = true;
			driverService.LogPath = "chromeLog.txt";
			var options = new ChromeOptions
			{
				AcceptInsecureCertificates = true
			};

			// Disables "Save password" popup
			options.AddUserProfilePreference("credentials_enable_service", false);
			options.AddUserProfilePreference("profile.password_manager_enabled", false);
			// Disables "Chrome is being controlled by automated test software." bar
			options.AddArguments("disable-infobars");
			options.AddArguments("headless");
			options.AddArguments("ignore-certificate-errors");
			options.AddAdditionalCapability(CapabilityType.AcceptSslCertificates, true, true);
			options.AddAdditionalCapability(CapabilityType.AcceptInsecureCertificates, true, true);

			RemoteWebDriver driver = new ChromeDriver(driverService, options);
			// Long implicit wait as Relativity uses IFrames and is usually quite slow
			driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(SharedVariables.UiImplicitWaitInSec);
			driver.Manage().Window.Size = new Size(_BROWSER_WIDTH, _BROWSER_HEIGHT);

			Size browseSize = driver.Manage().Window.Size;
			Log.Information("Browser size: Width: {width}, Height: {height}", browseSize.Width, browseSize.Height);

			driver.Url = SharedVariables.ProtocolVersion + "://" + SharedVariables.TargetHost + "/Relativity";
			return driver;
		}
	}
}
