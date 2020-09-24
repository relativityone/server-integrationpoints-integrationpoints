using kCura.IntegrationPoint.Tests.Core.Models;
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
			ExportFirstPage firstPage = SetupFirstIntegrationPointProfilePage(model);

			PushToRelativitySecondPage secondPage = SetupPushToRelativitySecondPage(firstPage, model);

			PushToRelativityThirdPage thirdPage = SetupPushToRelativityThirdPage(secondPage, model);

			return thirdPage.SaveIntegrationPoint();
		}

		public ExportFirstPage GoToFirstPageIntegrationPointProfile()
		{
			IntegrationPointProfilePage integrationPointProfilePage = GoToWorkspacePage().GoToIntegrationPointProfilePage();
			return integrationPointProfilePage.CreateNewIntegrationPointProfile();
		}

		private ExportFirstPage SetupFirstIntegrationPointProfilePage(IntegrationPointGeneralModel model)
		{
			ExportFirstPage firstPage = GoToFirstPageIntegrationPointProfile();
			ExportFirstPage firstPageWithModelApplied = ApplyModelToFirstPage(firstPage, model);

			return firstPageWithModelApplied;
		}
	}
}