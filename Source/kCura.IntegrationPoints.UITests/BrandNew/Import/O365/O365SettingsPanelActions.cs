using kCura.IntegrationPoint.Tests.Core.Models.Import.O365;
using kCura.IntegrationPoints.UITests.Configuration;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.O365
{
	public class O365SettingsPanelActions : ImportActions
	{
		public O365SettingsPanelActions(RemoteWebDriver driver, TestContext context) : base(driver, context)
		{
		}

		public void FillPanel(O365SettingsPanel panel, O365SettingsModel model)
		{
			panel.FileName = model.FileName;
		}

	}
}