
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Models.FTP;
using kCura.IntegrationPoint.Tests.Core.Models.Ldap;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Pages.Ldap;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Actions
{
	public class IntegrationPointsImportLdapAction : IntegrationPointsAction
	{
		public IntegrationPointsImportLdapAction(RemoteWebDriver driver, TestContext context) : base(driver, context)
		{
		}

		public IntegrationPointDetailsPage CreateNewImportLdapIntegrationPoint(ImportFromLdapModel model)
		{
			var generalPage = new GeneralPage(_driver);
			generalPage.ChooseWorkspace(_context.WorkspaceName);

			ImportWithLdapFirstPage firstPage = SetupImportFirstPage<ImportWithLdapFirstPage, ImportWithLdapSecondPage, ImportFromLdapModel>(generalPage, model.General,
				() => new ImportWithLdapFirstPage(_driver));


			ImportWithLdapSecondPage secondPage = SetupImportSecondPage(firstPage, model);

			//PushToRelativityThirdPage thirdPage = SetupPushToRelativityThirdPage(secondPage);
			//ImportFirstPage firstPage = SetupImportFromFTPFirstPage(generalPage, model);

			//ExportToFileSecondPage secondPage = SetupExportToFileSecondPage(firstPage, model);

			//ExportToFileThirdPage thirdPage = SetupExportToFileThirdPage(secondPage, model);

			return null;//thirdPage.SaveIntegrationPoint();
		}

		

	}
}
