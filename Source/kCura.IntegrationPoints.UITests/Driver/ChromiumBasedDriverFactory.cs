using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.UITests.Common;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.UITests.Driver
{
	public static class ChromiumBasedDriverFactory
	{
		private const string _CHROME_CAPABILITIES_NAME = "chrome";
		private const string _CHROME_DRIVER_VERSION_CAPABILITY_NAME = "chromedriverVersion";

		/// <summary>
		/// This method creates Selenium Web Driver for Chromium based browser.
		/// If no 'binary location' parameter specified it uses default Chrome installation path.
		/// </summary>
		/// <param name="binaryLocation">Specify custom path for chromium based browser executable file.</param>
		public static RemoteWebDriver Create(string binaryLocation = "")
		{
			ChromeDriverService driverService = ChromeDriverService.CreateDefaultService();
			// Otherwise console window appears for ChromeDriver process
			driverService.HideCommandPromptWindow = SharedVariables.UiDriverServiceHideCommandPromptWindow;
			driverService.LogPath = SharedVariables.UiDriverServiceLogPath;

			var options = new ChromeOptions
			{
				AcceptInsecureCertificates = SharedVariables.UiOptionsAcceptInsecureCertificates
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

			return CreateDriver(driverService, options);
		}

		private static RemoteWebDriver CreateDriver(ChromeDriverService driverService, ChromeOptions options)
		{
			RemoteWebDriver driver = new ChromeDriver(driverService, options);
			HandleIncompatibleDriverAndBrowserVersions(driver);

			return driver;
		}

		private static void HandleIncompatibleDriverAndBrowserVersions(RemoteWebDriver driver)
		{
			string browserVersion = DriverFactory.GetBrowserVersion(driver);
			string driverVersion = GetChromeDriverVersion(driver);
			if (!IsDriverAndBrowserCompatible(browserVersion, driverVersion))
			{
				driver.Dispose();
				throw new UiTestException($"Please update Selenium.WebDriver.ChromeDriver package {driverVersion}, as it is not compatible with Chrome {browserVersion}.");
			}
		}

		private static bool IsDriverAndBrowserCompatible(string browserVersion, string driverVersion)
		{
			string browserMajorVersion = GetMajorVersion(browserVersion);
			string driverMajorVersion = GetMajorVersion(driverVersion);

			if (string.IsNullOrEmpty(browserVersion) || string.IsNullOrEmpty(driverMajorVersion))
			{
				return false;
			}

			return browserMajorVersion == driverMajorVersion;
		}

		private static string GetChromeDriverVersion(RemoteWebDriver driver)
		{
			ICapabilities capabilities = driver.Capabilities;
			if (!capabilities.HasCapability(_CHROME_CAPABILITIES_NAME))
			{
				return string.Empty;
			}

			var chromeCapabilities = capabilities[_CHROME_CAPABILITIES_NAME] as IDictionary<string, object>;
			if (!chromeCapabilities.ContainsKey(_CHROME_DRIVER_VERSION_CAPABILITY_NAME))
			{
				return string.Empty;
			}

			var chromeDriverVersion = chromeCapabilities[_CHROME_DRIVER_VERSION_CAPABILITY_NAME] as string;
			return chromeDriverVersion;
		}

		private static string GetMajorVersion(string version)
		{
			return version?.Split('.').FirstOrDefault();
		}
	}
}