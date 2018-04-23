using kCura.IntegrationPoint.Tests.Core.Models.Import;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.Utility;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.LoadFile
{
	public class SettingsPanelActions : ImportActions
	{
		public SettingsPanelActions(RemoteWebDriver driver, TestContext context) : base(driver, context)
		{
		}

		public void FillPanel(SettingsPanel panel, SettingsModel model)
		{
			panel.Overwrite.SelectByText(model.Overwrite.GetDescription());
			if (model.Overwrite != OverwriteType.AppendOnly)
			{
				panel.MultiSelectFieldOverlayBehavior.SelectByText(model.MultiSelectFieldOverlayBehavior
					.GetDescription());
			}

			panel.CopyNativeFiles.Check(model.CopyNativeFiles.GetDescription());
			if (model.CopyNativeFiles != CopyNativeFiles.No)
			{
				panel.NativeFilePath.SelectByText(model.NativeFilePath);
			}

			panel.UseFolderPathInformation.Check(model.UseFolderPathInformation);
			if (model.UseFolderPathInformation)
			{
				panel.FolderPathInformation.SelectByText(model.FolderPathInformation);
				panel.MoveExistingDocuments.Check(model.MoveExistingDocuments);
			}

			panel.CellContainsFileLocation.Check(model.CellContainsFileLocation);
			if (model.CellContainsFileLocation)
			{
				panel.CellContainingFileLocation.SelectByText(model.CellContainingFileLocation);
				panel.EncodingForUndetectableFiles.SelectByText(model.EncodingForUndetectableFiles);
			}
		}
	}
}