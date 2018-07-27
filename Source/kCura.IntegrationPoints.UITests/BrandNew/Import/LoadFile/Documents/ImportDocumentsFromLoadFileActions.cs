using kCura.IntegrationPoint.Tests.Core.Models.Import.LoadFile.Documents;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Pages;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.LoadFile.Documents
{
	public class ImportDocumentsFromLoadFileActions : ImportActions
	{
		private readonly ImportDocumentsFromLoadFileModel _model;

		public ImportDocumentsFromLoadFileActions(RemoteWebDriver driver, TestContext context, ImportDocumentsFromLoadFileModel model) : base(driver, context)
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

			var secondPage = new SecondPage(Driver);
			new LoadFileSettingsPanelActions(Driver, Context).FillPanel(secondPage.LoadFileSettings, _model.LoadFileSettings);
			new FileEncodingPanelActions(Driver, Context).FillPanel(secondPage.FileEncoding, _model.FileEncoding);
			secondPage.Wizard.GoNext();

			var thirdPage = new ThirdPage(Driver);
			new FieldMappingPanelActions(Driver, Context).FillPanel(thirdPage.FieldMapping, _model.FieldsMapping);
			new SettingsPanelActions(Driver, Context).FillPanel(thirdPage.Settings, _model.Settings);
			thirdPage.Wizard.Save();
		}
	}
}