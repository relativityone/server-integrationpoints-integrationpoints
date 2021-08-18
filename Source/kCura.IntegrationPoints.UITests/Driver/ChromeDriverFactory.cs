using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Driver
{
	public static class ChromeDriverFactory
	{
		public static RemoteWebDriver Create()
		{
			return ChromiumBasedDriverFactory.Create();
		}
	}
}