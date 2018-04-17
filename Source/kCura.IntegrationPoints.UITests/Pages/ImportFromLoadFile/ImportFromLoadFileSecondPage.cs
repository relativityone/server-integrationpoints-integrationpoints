using kCura.IntegrationPoint.Tests.Core.Models.ImportFromLoadFile;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages.ImportFromLoadFile
{
	public class ImportFromLoadFileSecondPage : ImportSecondBasePage<ImportFromLoadFileModel>
	{
		[FindsBy(How = How.Id, Using = "save")]
		protected IWebElement SaveButton { get; set; }

		public ImportFromLoadFileSecondPage(RemoteWebDriver driver) : base(driver)
		{
			LoadFileSettings = new ImportFromLoadFileSecondPageLoadFileSettings(driver);
			FileEncoding = new ImportFromLoadFileSecondPageEncodingSettings(driver);
			ImportSettings = new ImportFromLoadFileSecondPageImportSettings(driver);

			Sleep(200);
			WaitForPage();
			PageFactory.InitElements(driver, this);
		}

		public ImportFromLoadFileSecondPageLoadFileSettings LoadFileSettings { get; set; }
		
		public ImportFromLoadFileSecondPageEncodingSettings FileEncoding { get; set; }

		public ImportFromLoadFileSecondPageImportSettings ImportSettings { get; set; }

		public override void SetupModel(ImportFromLoadFileModel model)
		{
			LoadFileSettings.SetupModel(model);
			ImportType importType = model.LoadFileSettings.ImportType;
			if (importType == ImportType.DocumentLoadFile)
			{
				FileEncoding.SetupModel(model);
			}
			else if (importType == ImportType.ProductionLoadFile || importType == ImportType.ImageLoadFile)
			{
				ImportSettings.SetupModel(model);
			}
		}

		public IntegrationPointDetailsPage SaveIntegrationPoint()
		{
			Driver.SwitchTo().DefaultContent();
			SaveButton.ClickWhenClickable();
			return new IntegrationPointDetailsPage(Driver);
		}
	}
}
