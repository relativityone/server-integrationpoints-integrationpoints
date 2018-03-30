
using kCura.IntegrationPoint.Tests.Core.Models.Ldap;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class ImportWithLdapSecondPage : ImportSecondBasePage<ImportFromLdapModel>
	{
		public ImportWithLdapSecondPage(RemoteWebDriver driver) : base(driver)
		{
		}

		public override void SetupModel(ImportFromLdapModel model)
		{
			throw new System.NotImplementedException();
		}
	}
}
