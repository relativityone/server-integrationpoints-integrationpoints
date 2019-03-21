using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.UITests.Common;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Driver
{
	public static class ChromiumBasedDriverFactory
	{
		public static RemoteWebDriver Create(string binaryLocation = "")
		{
			ChromeDriverService driverService = ChromeDriverService.CreateDefaultService();
			// Otherwise console window appears for ChromeDriver process
			driverService.HideCommandPromptWindow = SharedVariables.UiDriverServiceHideCommandPromptWindow;
			driverService.LogPath = SharedVariables.UiDriverServiceLogPath;

			var options = new ChromeOptions
			{
				AcceptInsecureCertificates = SharedVariables.UiOptionsAcceptInsecureCertificates,
			};

			if (!string.IsNullOrWhiteSpace(binaryLocation))
			{
				options.BinaryLocation = binaryLocation;
			}

			// Disables "Save password" popup
			options.AddUserProfilePreference("credentials_enable_service",
				SharedVariables.UiOptionsProfilePreferenceCredentialsEnableService);
			options.AddUserProfilePreference("profile.password_manager_enabled",
				SharedVariables.UiOptionsProfilePreferenceProfilePasswordManagerEnabled);
			options.SetLoggingPreference(LogType.Browser, LogLevel.Warning);

			options.AddBrowserOptions();
			options.AddAdditionalCapabilities();

			RemoteWebDriver driver = new ChromeDriver(driverService, options);

			CheckDriverAndBrowserCompatibility(DriverFactory.GetBrowserVersion(driver));

			return driver;
		}

		private static Version MaxChromeSupportedVersion => new Version(SharedVariables.UiMaxChromeSupportedVersion);

		private static void CheckDriverAndBrowserCompatibility(string browserVersion)
		{
			var version = new Version(browserVersion);
			if (MaxChromeSupportedVersion.Major < version.Major)
			{
				throw new UiTestException($"Please update Selenium.WebDriver.ChromeDriver package, as it is too old for Chrome version {version}.");
			}
		}

	}
}
