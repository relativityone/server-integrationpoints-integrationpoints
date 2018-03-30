

using kCura.IntegrationPoint.Tests.Core.Models.FTP;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class ImportWithFTPFirstPage : ImportFirstPage<ImportWithFTPSecondPage, ImportFromFTPModel>
	{
		public ImportWithFTPFirstPage(RemoteWebDriver driver) : base(driver)
		{
		}


		protected override ImportWithFTPSecondPage Create(RemoteWebDriver Driver)
		{
			return new ImportWithFTPSecondPage(Driver);
		}
	}
}
