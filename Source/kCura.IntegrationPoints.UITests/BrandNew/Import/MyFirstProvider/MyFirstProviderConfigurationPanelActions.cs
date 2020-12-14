using kCura.IntegrationPoint.Tests.Core.Models.Import.MyFirstProvider;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.MyFirstProvider
{
	public class MyFirstProviderConfigurationPanelActions : ImportActions
	{
		public MyFirstProviderConfigurationPanelActions(RemoteWebDriver driver, TestContext context) : base(driver, context)
		{
		}

		public void FillPanel(MyFirstProviderConfigurationPanel panel, MyFirstProviderSettingsModel model)
		{
			panel.FileLocationInput.SetTextEx(model.FileLocation, Driver);
		}

	}
}