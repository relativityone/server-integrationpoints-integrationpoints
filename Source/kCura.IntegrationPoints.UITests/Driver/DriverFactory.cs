using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.UITests.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using Serilog;
using Serilog.Events;
using System;
using System.Drawing;
using System.Text;

namespace kCura.IntegrationPoints.UITests.Driver
{
	public static class DriverFactory
	{
		private const string _VERSION_CAPABILITY_NAME = "version";

		private const string _BROWSER_VERSION_CAPABILITY_NAME = "browserVersion";

		private static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(DriverFactory));
		
		public static RemoteWebDriver Create()
		{
			RemoteWebDriver driver = null;
			switch (SharedVariables.UiBrowser)
			{
				case "chrome":
				case "chromium":
				case "chromium-portable":
				case "firefox":
					break;
				default:
					throw new ArgumentException($"Unsupported browser '{SharedVariables.UiBrowser}'");
			}

			driver.Manage().Timeouts().ImplicitWait = DriverImplicitWait;
			driver.Manage().Window.Size = new Size(SharedVariables.UiBrowserWidth, SharedVariables.UiBrowserHeight);
			driver.Url = SharedVariables.RelativityFrontendUrlValue;
			
			LogDriverInformation(driver);
			return driver;
		}

		public static string GetBrowserVersion(IHasCapabilities driver)
		{
			ICapabilities caps = driver.Capabilities;
			if (caps.HasCapability(_VERSION_CAPABILITY_NAME))
			{
				return caps[_VERSION_CAPABILITY_NAME].ToString();
			}

			if (caps.HasCapability(_BROWSER_VERSION_CAPABILITY_NAME))
			{
				return caps[_BROWSER_VERSION_CAPABILITY_NAME].ToString();
			}

			Log.Warning($"Cannot retrieve {GetBrowserName(driver)} browser version.");
			return "Unknown";
		}

		private static TimeSpan DriverImplicitWait => TimeSpan.FromSeconds(SharedVariables.UiImplicitWaitInSec);

		private static string GetBrowserName(IHasCapabilities driver)
		{
			return driver.Capabilities["browserName"].ToString();
		}
		
		private static void LogDriverInformation(RemoteWebDriver driver)
		{
			if (Log.IsEnabled(LogEventLevel.Information))
			{
				Size browseSize = driver.Manage().Window.Size;
				StringBuilder builder = new StringBuilder()
					.AppendLine($"Browser name: {GetBrowserName(driver)}")
					.AppendLine($"Browser version: {GetBrowserVersion(driver)}")
					.AppendLine($"Browser width: {browseSize.Width}, height: {browseSize.Height}")
					.AppendLine($"Driver implicit wait: {DriverImplicitWait}")
					.AppendLine($"Driver URL: {driver.Url}");

				Log.Information("Driver info:\n{DriverInfo}", builder.ToString());
			}
		}

	}
}
