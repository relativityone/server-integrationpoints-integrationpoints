using kCura.IntegrationPoint.Tests.Core.Models.Import.LoadFile.ImagesAndProductions.Images;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.Utility;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.LoadFile.Images
{
	public class ImportSettingsPanelActions : ImportActions
	{
		public ImportSettingsPanelActions(RemoteWebDriver driver, TestContext context) : base(driver, context)
		{
		}

		public void FillPanel(ImportSettingsPanel panel, ImportSettingsModel model)
		{
			panel.Numbering.Check(model.Numbering.GetDescription());
			panel.ImportMode.SelectByText(model.ImportMode.GetDescription());
			panel.CopyFilesToDocumentRepository.Check(model.CopyFilesToDocumentRepository);
			panel.LoadExtractedText.Check(model.LoadExtractedText);

			if (model.LoadExtractedText)
			{
				panel.EncodingForUndetectableFiles.SelectByText(model.EncodingForUndetectableFiles);
			}
		}
	}
}