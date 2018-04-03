

using kCura.IntegrationPoint.Tests.Core.Models.FTP;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class ImportWithFtpFirstPage : ImportFirstPage<ImportWithFTPSecondPage, ImportFromFTPModel>
	{
		public ImportWithFtpFirstPage(RemoteWebDriver driver) : base(driver)
		{
		}


		protected override ImportWithFTPSecondPage Create(RemoteWebDriver Driver)
		{
			return new ImportWithFTPSecondPage(Driver);
		}
	}
}
