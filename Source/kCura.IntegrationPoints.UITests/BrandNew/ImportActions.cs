using kCura.IntegrationPoints.UITests.Configuration;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew
{
	public abstract class ImportActions
	{
		protected RemoteWebDriver Driver { get; set; }

		protected TestContext Context { get; set; }

		protected ImportActions(RemoteWebDriver driver, TestContext context)
		{
			Driver = driver;
			Context = context;
		}
		
	}
}
