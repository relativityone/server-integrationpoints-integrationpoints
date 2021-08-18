using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.UITests.Common;
using OpenQA.Selenium.Remote;
using System.IO;
using System.Reflection;

namespace kCura.IntegrationPoints.UITests.Driver
{
	public static class ChromiumDriverFactory
	{
		private static readonly string _chromium_exe_location = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), SharedVariables.UiBrowserPath);

		public static RemoteWebDriver Create()
		{
			if (!File.Exists(_chromium_exe_location))
			{
				throw new UiTestException($"Specified chromium exe file {_chromium_exe_location} doesn't exist. Ensure that relative chromium path in app.config is correct.");
			}

			return ChromiumBasedDriverFactory.Create(_chromium_exe_location);
		}
	}
}