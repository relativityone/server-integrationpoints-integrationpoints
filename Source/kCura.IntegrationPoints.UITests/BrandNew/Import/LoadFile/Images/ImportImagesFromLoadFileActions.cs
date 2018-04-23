using kCura.IntegrationPoint.Tests.Core.Models.Import.LoadFile.ImagesAndProductions.Images;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Pages;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.LoadFile.Images
{
	public class ImportImagesFromLoadFileActions : ImportActions
	{
		private readonly ImportImagesFromLoadFileModel _model;

		public ImportImagesFromLoadFileActions(RemoteWebDriver driver, TestContext context, ImportImagesFromLoadFileModel model) : base(driver, context)
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
			new LoadFileSettingsPanelActions(Driver, Context).FillPanel(secondPage.LoadFileSettings, _model.LoadFileSettings);
			new ImportSettingsPanelActions(Driver, Context).FillPanel(secondPage.ImportSettings, _model.ImportSettings);
			secondPage.Wizard.Save();
		}
	}
}