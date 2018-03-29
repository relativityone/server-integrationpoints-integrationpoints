

using kCura.IntegrationPoint.Tests.Core.Models.Ldap;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Pages.Ldap
{
	public class ImportWithLdapFirstPage : ImportFirstPage<ImportWithLdapSecondPage, ImportFromLdapModel>
	{
		public ImportWithLdapFirstPage(RemoteWebDriver driver) : base(driver)
		{
		}

		protected override ImportWithLdapSecondPage Create(RemoteWebDriver driver)
		{
			return new ImportWithLdapSecondPage(driver);
		}
	}
}
