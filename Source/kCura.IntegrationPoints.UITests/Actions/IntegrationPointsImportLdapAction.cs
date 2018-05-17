using kCura.IntegrationPoint.Tests.Core.Models.Import.Ldap;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Pages.Ldap;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Actions
{
	public class IntegrationPointsImportLdapAction : IntegrationPointsImportAction
	{
		public IntegrationPointsImportLdapAction(RemoteWebDriver driver, TestContext context) : base(driver, context)
		{
		}

		public IntegrationPointDetailsPage CreateNewImportLdapIntegrationPoint(ImportFromLdapModel model)
		{
			var generalPage = new GeneralPage(Driver);
			generalPage.ChooseWorkspace(Context.WorkspaceName);

			ImportWithLdapFirstPage firstPage = 
				SetupImportFirstPage<ImportWithLdapFirstPage, ImportWithLdapSecondPage, ImportFromLdapModel>(generalPage, model.General,
				() => new ImportWithLdapFirstPage(Driver));

			ImportWithLdapSecondPage secondPage = 
				SetupImportSecondPage(firstPage, model);

			ImportThirdPage<ImportFromLdapModel> thirdPage = 
				SetupImportThirdPage(secondPage, model, () => new ImportLdapThirdPage(Driver));

			return thirdPage.SaveIntegrationPoint();
		}
	}
}
