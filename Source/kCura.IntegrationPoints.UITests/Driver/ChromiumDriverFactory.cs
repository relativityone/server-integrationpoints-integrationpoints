using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Driver
{
	public static class ChromiumDriverFactory
	{
		private const string CHROMIUM_EXE_LOCATION = "C:\\Program Files (x86)\\Chromium\\Application\\chrome.exe";

		public static RemoteWebDriver Create()
		{
			return ChromiumBasedDriverFactory.Create(CHROMIUM_EXE_LOCATION);
		}
	}
}
