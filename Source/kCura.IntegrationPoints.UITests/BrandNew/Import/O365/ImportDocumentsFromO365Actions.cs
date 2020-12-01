using kCura.IntegrationPoint.Tests.Core.Models.Import.O365;
using kCura.IntegrationPoints.UITests.BrandNew.Import.LoadFile;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Pages;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.O365
{
	public class ImportDocumentsFromO365Actions : ImportActions
	{
		private readonly ImportDocumentsFromO365Model _model;

		public ImportDocumentsFromO365Actions(RemoteWebDriver driver, TestContext context, ImportDocumentsFromO365Model model) : base(driver, context)
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

			var secondPage = new O365SecondPage(Driver);
			new O365SettingsPanelActions(Driver, Context).FillPanel(secondPage.O365SettingsPanel, _model.O365Settings);
			secondPage.Wizard.GoNext();

			var thirdPage = new ThirdPage(Driver);
			new FieldMappingPanelActions(Driver, Context).FillPanel(thirdPage.FieldMapping, _model.FieldsMapping);
			new SettingsPanelActions(Driver, Context).FillPanel(thirdPage.Settings, _model.Settings);
			thirdPage.Wizard.Save();
		}
	}
}