using kCura.IntegrationPoint.Tests.Core.Models.Import.JsonLoader;
using kCura.IntegrationPoints.UITests.BrandNew.Import.JsonLoader.SecondPage;
using kCura.IntegrationPoints.UITests.BrandNew.Import.JsonLoader.ThirdPage;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Pages;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.JsonLoader
{
	public class ImportDocumentsFromJsonLoaderActions : ImportActions
	{
		private readonly ImportDocumentsFromJsonLoaderModel _model;

		public ImportDocumentsFromJsonLoaderActions(RemoteWebDriver driver, TestContext context, ImportDocumentsFromJsonLoaderModel model) : base(driver, context)
		{
			_model = model;
		}

		public void Setup()
		{
			new GeneralPage(Driver)
				.PassWelcomeScreen()
				.ChooseWorkspace(Context.WorkspaceName)
				.GoToIntegrationPointsPage();
			new IntegrationPointsPage(Driver).CreateNewIntegrationPoint();

			var firstPage = new FirstPage(Driver);
			new GeneralPanelActions(Driver, Context).FillPanel(firstPage.General, _model.General);
			firstPage.Wizard.GoNext();

			var secondPage = new JsonLoaderSecondPage(Driver);
			new JsonLoaderConfigurationPanelActions(Driver, Context).FillPanel(secondPage.JsonLoaderConfigurationPanel, _model.JsonLoaderSettings);
			secondPage.Wizard.GoNext();

			var thirdPage = new JsonLoaderThirdPage(Driver);
			new FieldMappingPanelActions(Driver, Context).FillPanel(thirdPage.FieldMapping, _model.FieldsMapping);
			new JsonLoaderSettingsPanelActions(Driver, Context).FillPanel(thirdPage.Settings, _model);
			thirdPage.Wizard.Save();
		}
	}
}