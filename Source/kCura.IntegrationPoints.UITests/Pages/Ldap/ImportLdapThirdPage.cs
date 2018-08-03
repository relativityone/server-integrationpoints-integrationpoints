using kCura.IntegrationPoint.Tests.Core.Models.Import.Ldap;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Pages.Ldap
{
	public class ImportLdapThirdPage : ImportThirdPage<ImportFromLdapModel>
	{
		public ImportLdapThirdPage(RemoteWebDriver driver) : base(driver)
		{
		}

		public override void SetupModel(ImportFromLdapModel model)
		{
			SetUpEntitySettingsModel(model.ImportEntitySettingsModel);
			SetUpSharedSettingsModel(model.SharedImportSettings);
		}
	}
}
