using kCura.IntegrationPoint.Tests.Core.Models.FTP;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Pages.FTP
{
	public class ImportWithFTPFirstPage : ImportFirstPage<ImportWithFTPSecondPage, ImportFromFTPModel>
	{
		public ImportWithFTPFirstPage(RemoteWebDriver driver) : base(driver)
		{
		}

		protected override ImportWithFTPSecondPage Create(RemoteWebDriver driver)
		{
			return new ImportWithFTPSecondPage(driver);
		}
	}
}
