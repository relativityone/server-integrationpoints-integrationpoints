using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using kCura.IntegrationPoint.Tests.Core;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using Serilog;

namespace kCura.IntegrationPoints.UITests.Driver
{
	public static class DriverFactory
	{
		private static readonly int BrowserWidth = SharedVariables.UiBrowserWidth;

		private static readonly int BrowserHeight = SharedVariables.UiBrowserHeight;

		public static RemoteWebDriver CreateDriver()
		{
			ChromeDriverService driverService = ChromeDriverService.CreateDefaultService();
			// Otherwise console window appears for ChromeDriver process
			driverService.HideCommandPromptWindow = SharedVariables.UiDriverServiceHideCommandPromptWindow;
			driverService.LogPath = SharedVariables.UiDriverServiceLogPath;

			var options = new ChromeOptions
			{
				AcceptInsecureCertificates = SharedVariables.UiOptionsAcceptInsecureCertificates
			};

			// Disables "Save password" popup
			options.AddUserProfilePreference("credentials_enable_service",
				SharedVariables.UiOptionsProfilePreferenceCredentialsEnableService);
			options.AddUserProfilePreference("profile.password_manager_enabled",
				SharedVariables.UiOptionsProfilePreferenceProfilePasswordManagerEnabled);

			options.AddBrowserOptions();
			options.AddAdditionalCapabilities();

			RemoteWebDriver driver = new ChromeDriver(driverService, options);
			// Long implicit wait as Relativity uses IFrames and is usually quite slow
			driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(SharedVariables.UiImplicitWaitInSec);
			driver.Manage().Window.Size = new Size(BrowserWidth, BrowserHeight);

			Size browseSize = driver.Manage().Window.Size;
			Log.Information("Browser size: Width: {width}, Height: {height}", browseSize.Width, browseSize.Height);

			driver.Url = SharedVariables.RelativityFrontendUrlValue;
			return driver;
		}
	}
}

public static class ChromeOptionsExtensions
{
	public static ChromeOptions AddAdditionalCapabilities(this ChromeOptions value)
	{
		if (SharedVariables.UiOptionsAdditionalCapabilitiesAcceptSslCertificates)
		{
			value.AddAdditionalCapability(CapabilityType.AcceptSslCertificates, true, true);
		}
		if (SharedVariables.UiOptionsAdditionalCapabilitiesAcceptInsecureCertificates)
		{
			value.AddAdditionalCapability(CapabilityType.AcceptInsecureCertificates, true, true);
		}
		return value;
	}

	public static void AddBrowserOptions(this ChromeOptions value)
	{
		var optionsFromAppConfig = new Dictionary<string, bool>
		{
			// Disables "Chrome is being controlled by automated test software." bar
			["disable-infobars"] = SharedVariables.UiOptionsArgumentsDisableInfobars,
			["headless"] = SharedVariables.UiOptionsArgumentsHeadless,
			["ignore-certificate-errors"] = SharedVariables.UiOptionsArgumentsIgnoreCertificateErrors,
			["no-sandbox"] = SharedVariables.UiOptionsArgumentsNoSandbox
		};

		foreach (var option in optionsFromAppConfig.Where(x => x.Value))
		{
			value.AddArgument(option.Key);
		}
	}
}