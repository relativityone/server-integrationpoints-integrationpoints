using kCura.IntegrationPoint.Tests.Core.Models.ImportFromLoadFile;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Pages.ImportFromLoadFile
{
	public class ImportFromLoadFileFirstPage : ImportFirstPage<ImportFromLoadFileSecondPage, ImportFromLoadFileModel>
	{
		public ImportFromLoadFileFirstPage(RemoteWebDriver driver) : base(driver)
		{
		}

		protected override ImportFromLoadFileSecondPage Create(RemoteWebDriver driver)
		{
			return new ImportFromLoadFileSecondPage(driver);
		}
	}
}
