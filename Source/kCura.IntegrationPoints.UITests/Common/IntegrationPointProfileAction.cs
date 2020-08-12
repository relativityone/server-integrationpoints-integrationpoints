using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Pages;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Common
{
	public class IntegrationPointProfileAction : IntegrationPointsAction
	{
		public IntegrationPointProfileAction(RemoteWebDriver driver, string workspaceName)
			: base(driver, workspaceName)
		{
		}

		public IntegrationPointDetailsPage CreateNewRelativityProviderIntegrationPointProfile(
			RelativityProviderModel model)
		{
			GeneralPage generalPage = GoToWorkspacePage();

			ExportFirstPage firstPage = SetupFirstIntegrationPointProfilePage(generalPage, model);

			PushToRelativitySecondPage secondPage = SetupPushToRelativitySecondPage(firstPage, model);

			PushToRelativityThirdPage thirdPage = SetupPushToRelativityThirdPage(secondPage, model);

			return thirdPage.SaveIntegrationPoint();
		}

		private ExportFirstPage GoToFirstPageIntegrationPointProfile(GeneralPage generalPage)
		{
			IntegrationPointProfilePage integrationPointProfilePage = generalPage.GoToIntegrationPointProfilePage();
			return integrationPointProfilePage.CreateNewIntegrationPointProfile();
		}

		private ExportFirstPage SetupFirstIntegrationPointProfilePage(GeneralPage generalPage,
			IntegrationPointGeneralModel model)
		{
			ExportFirstPage firstPage = GoToFirstPageIntegrationPointProfile(generalPage);
			ExportFirstPage firstPageWithModelApplied = ApplyModelToFirstPage(firstPage, model);

			return firstPageWithModelApplied;
		}
	}
}