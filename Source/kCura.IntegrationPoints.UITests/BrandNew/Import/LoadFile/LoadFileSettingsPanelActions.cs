using kCura.IntegrationPoint.Tests.Core.Models.Import.LoadFile;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.Utility;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.LoadFile
{
	public class LoadFileSettingsPanelActions : ImportActions
	{
		public LoadFileSettingsPanelActions(RemoteWebDriver driver, TestContext context) : base(driver, context)
		{
		}

		public void FillPanel(LoadFileSettingsPanel panel, LoadFileSettingsModel model)
		{
			panel.ImportType = model.ImportType.GetDescription();
			panel.WorkspaceDestination = model.WorkspaceDestinationFolder;
			panel.ImportSource = model.ImportSource;
			panel.StartLine = model.StartLine;
		}
	}
}