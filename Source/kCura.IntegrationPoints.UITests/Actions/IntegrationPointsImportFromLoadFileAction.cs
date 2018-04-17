using kCura.IntegrationPoint.Tests.Core.Models.ImportFromLoadFile;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.IntegrationPoints.UITests.Pages.ImportFromLoadFile;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Actions
{
	public class IntegrationPointsImportFromLoadFileAction : IntegrationPointsImportAction
	{
		public IntegrationPointsImportFromLoadFileAction(RemoteWebDriver driver, TestContext context) : base(driver, context)
		{
		}

		public IntegrationPointDetailsPage CreateNewImportFromLoadFileIntegrationPoint(ImportFromLoadFileModel model)
		{
			var generalPage = new GeneralPage(Driver);
			generalPage.ChooseWorkspace(Context.WorkspaceName);

			ImportFromLoadFileFirstPage firstPage =
				SetupImportFirstPage<ImportFromLoadFileFirstPage, ImportFromLoadFileSecondPage, ImportFromLoadFileModel>(
					generalPage,
					model.General,
					() => new ImportFromLoadFileFirstPage(Driver));

			ImportFromLoadFileSecondPage secondPage =
				SetupImportSecondPage(firstPage, model);

			if (model.LoadFileSettings.ImportType != ImportType.DocumentLoadFile)
			{
				return secondPage.SaveIntegrationPoint();
			}

			ImportThirdPage<ImportFromLoadFileModel> thirdPage =
				SetupImportThirdPage(secondPage, model, () => new ImportFromLoadFileThirdPage(Driver));

			return thirdPage.SaveIntegrationPoint();
		}
	}
}
