using kCura.IntegrationPoint.Tests.Core.Models.Import.FTP;
using kCura.IntegrationPoints.UITests.BrandNew.Import.LoadFile;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Pages;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.FTP
{
	public class ImportDocumentsFromFtpActions : ImportActions
	{
		private readonly ImportFromFtpModel _model;

		public ImportDocumentsFromFtpActions(RemoteWebDriver driver, TestContext context, ImportFromFtpModel model) : base(driver, context)
		{
			_model = model;
		}

		public void Setup()
		{
			new GeneralPage(Driver)
				.ChooseWorkspace(Context.WorkspaceName)
				.GoToIntegrationPointsPage();
			new IntegrationPointsPage(Driver).CreateNewIntegrationPoint();

			var firstPage = new FirstPage(Driver);
			new GeneralPanelActions(Driver, Context).FillPanel(firstPage.General, _model.General);
			firstPage.Wizard.GoNext();

			var secondPage = new SecondPage(Driver);
			new ConnectionAndFileInfoPanelActions(Driver, Context).FillPanel(secondPage.InfoPanel, _model.ConnectionAndFileInfo);
			secondPage.Wizard.GoNext();

			var thirdPage = new ThirdPage(Driver);
			new FieldMappingPanelActions(Driver, Context).FillPanel(thirdPage.FieldMapping, _model.FieldsMapping);
			new SettingsPanelActions(Driver, Context).FillPanel(thirdPage.Settings, _model.Settings);
			thirdPage.Wizard.Save();
		}
	}
}