using kCura.IntegrationPoint.Tests.Core.Models.Import.JsonLoader;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.JsonLoader.SecondPage
{
	public class JsonLoaderConfigurationPanelActions : ImportActions
	{
		public JsonLoaderConfigurationPanelActions(RemoteWebDriver driver, TestContext context) : base(driver, context)
		{
		}

		public void FillPanel(JsonLoaderConfigurationPanel panel, JsonLoaderSettingsModel model)
		{
			panel.DataLocationInput.SetTextEx(model.DataLocation, Driver);
			panel.FieldLocationInput.SetTextEx(model.FieldLocation, Driver);
		}

	}
}