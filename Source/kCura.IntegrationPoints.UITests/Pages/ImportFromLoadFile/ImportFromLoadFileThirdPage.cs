using kCura.IntegrationPoint.Tests.Core.Models.ImportFromLoadFile;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Pages.ImportFromLoadFile
{
	public class ImportFromLoadFileThirdPage : ImportThirdPage<ImportFromLoadFileModel>
	{
		public ImportFromLoadFileThirdPage(RemoteWebDriver driver) : base(driver)
		{
		}

		public override void SetupModel(ImportFromLoadFileModel model)
		{
			SetUpDocumentSettingsModel(model.ImportDocumentSettings);
			SetUpSharedSettingsModel(model.SharedImportSettings);
		}
	}
}
