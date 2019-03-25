using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Driver
{
	public static class ChromeDriverFactory
	{
		private const string _CHROME_CAPABILITIES_NAME = "chrome";
		private const string _CHROME_DRIVER_VERSION_CAPABILITY_NAME = "chromedriverVersion";

		public static RemoteWebDriver Create()
		{
			return ChromiumBasedDriverFactory.Create();
		}
	}
}
