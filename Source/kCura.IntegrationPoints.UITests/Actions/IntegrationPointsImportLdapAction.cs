﻿
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Models.Ldap;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Pages;
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

			//ImportFirstPage firstPage = SetupImportFromFTPFirstPage(generalPage, model);

			//ExportToFileSecondPage secondPage = SetupExportToFileSecondPage(firstPage, model);

			//ExportToFileThirdPage thirdPage = SetupExportToFileThirdPage(secondPage, model);

			return null;//thirdPage.SaveIntegrationPoint();
		}

	}
}
