using kCura.IntegrationPoint.Tests.Core.Models.ImportFromLoadFile;
using kCura.IntegrationPoints.UITests.Components;
using kCura.Utility;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages.ImportFromLoadFile
{
	public class ImportFromLoadFileSecondPageLoadFileSettings : ImportFromLoadFileSecondPagePanel
	{
		[FindsBy(How = How.Id, Using = "import-importType")]
		protected IWebElement ImportTypeSelectWebElement { get; set; }

		[FindsBy(How = How.XPath, Using = "//*[@id='destination-location-select']/..")]
		protected IWebElement WorkspaceDestinationTreeWebElement { get; set; }

		[FindsBy(How = How.XPath, Using = "//*[@id='location-select']/..")]
		protected IWebElement ImportSourceTreeWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "import-columnname-numbers")]
		protected IWebElement StartLineElement { get; set; }

		public ImportFromLoadFileSecondPageLoadFileSettings(RemoteWebDriver driver) : base(driver)
		{
		}

		protected SelectElement ImportTypeSelectElement => new SelectElement(ImportTypeSelectWebElement);

		public string ImportType
		{
			get { return ImportTypeSelectElement.SelectedOption.Text; }
			set
			{
				if (value != null)
				{
					ImportTypeSelectElement.SelectByText(value);
				}
			}
		}

		protected TreeSelect WorkspaceDestinationTree => new TreeSelect(WorkspaceDestinationTreeWebElement);

		public string WorkspaceDestination
		{
			set { WorkspaceDestinationTree.ChooseChildElement(value); }
		}

		protected TreeSelect ImportSourceTree => new TreeSelect(ImportSourceTreeWebElement);

		public string ImportSource
		{
			set { ImportSourceTree.ChooseChildElement(value); }
		}

		public int StartLine
		{
			get { return int.Parse(StartLineElement.Text); }
			set { SetInputText(StartLineElement, value.ToString()); }
		}

		public override void SetupModel(ImportFromLoadFileModel model)
		{
			ImportLoadFileSettingsModel loadFileSettings = model.LoadFileSettings;
			ImportType = loadFileSettings.ImportType.GetDescription();
			WorkspaceDestination = loadFileSettings.WorkspaceDestinationFolder;
			ImportSource = loadFileSettings.ImportSource;
			StartLine = loadFileSettings.StartLine;
		}
	}
}
