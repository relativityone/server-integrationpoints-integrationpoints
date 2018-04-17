using kCura.IntegrationPoint.Tests.Core.Models.ImportFromLoadFile;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.Utility;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages.ImportFromLoadFile
{
	public class ImportFromLoadFileSecondPageImportSettings : ImportFromLoadFileSecondPagePanel
	{
		[FindsBy(How = How.XPath, Using = "(//input[name='imageProductionNumbering')[1]")]
		protected IWebElement UseLoadFilePageIdsElement { get; set; }

		[FindsBy(How = How.XPath, Using = "(//input[name='imageProductionNumbering')[2]")]
		protected IWebElement AutoNumberPagesElement { get; set; }

		[FindsBy(How = How.Id, Using = "overwrite")]
		protected IWebElement ImportModeSelectWebElement { get; set; }

		[FindsBy(How = How.XPath, Using = "(//input[name='copyFilesToDocumentRepo')[1]")]
		protected IWebElement CopyFilesToDocumentRepositoryElement { get; set; }

		[FindsBy(How = How.XPath, Using = "(//input[name='copyFilesToDocumentRepo')[2]")]
		protected IWebElement DoNotCopyFilesToDocumentRepositoryElement { get; set; }

		[FindsBy(How = How.XPath, Using = "(//input[name='import_extractedTextLocation')[1]")]
		protected IWebElement LoadExtractedTextElement { get; set; }

		[FindsBy(How = How.XPath, Using = "(//input[name='import_extractedTextLocation')[2]")]
		protected IWebElement DoNotLoadExtractedTextElement { get; set; }

		[FindsBy(How = How.Id, Using = "production-sets")]
		protected IWebElement ProductionSelectWebElement { get; set; }

		public ImportFromLoadFileSecondPageImportSettings(RemoteWebDriver driver) : base(driver)
		{
		}

		public Numbering Numbering
		{
			set
			{
				if (value == Numbering.UseLoadFilePageIds)
				{
					UseLoadFilePageIdsElement.Click();
				}
				else if (value == Numbering.AutoNumberPages)
				{
					AutoNumberPagesElement.Click();
				}
			}
		}

		protected SelectElement ImportModeSelectElement => new SelectElement(ImportModeSelectWebElement);

		public OverwriteType ImportMode
		{
			set
			{
				ImportModeSelectElement.SelectByText(value.GetDescription());
			}
		}

		public bool CopyFilesToDocumentRepository
		{
			set
			{
				IWebElement element = value ? CopyFilesToDocumentRepositoryElement : DoNotCopyFilesToDocumentRepositoryElement;
				element.Click();
			}
		}

		public bool LoadExtractedText
		{
			set
			{
				IWebElement element = value ? LoadExtractedTextElement : DoNotLoadExtractedTextElement;
				element.Click();
			}
		}

		protected SelectElement ProductionSelectElement => new SelectElement(ProductionSelectWebElement);

		public string Production
		{
			set
			{
				ProductionSelectElement.SelectByText(value);
			}
		}

		public override void SetupModel(ImportFromLoadFileModel model)
		{
			ImportLoadFileImageProductionSettingsModel settings = model.ImageProductionSettings;
			Numbering = settings.Numbering;
			ImportMode = settings.ImportMode;
			CopyFilesToDocumentRepository = settings.CopyFilesToDocumentRepository;
			if (model.LoadFileSettings.ImportType == ImportType.ImageLoadFile)
			{
				LoadExtractedText = settings.LoadExtractedText;
			}
			else if (model.LoadFileSettings.ImportType == ImportType.ProductionLoadFile)
			{
				Production = settings.Production;
			}
		}
	}
}
