using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class ImportFirstPage : FirstPage
	{
		[FindsBy(How = How.Id, Using = "isExportType")]
		protected IWebElement ImportExportRadio { get; set; }

		[FindsBy(How = How.Id, Using = "sourceProvider")]
		protected IWebElement SourceSelectWebElement { get; set; }

		protected SelectElement SourceSelect => new SelectElement(SourceSelectWebElement);

		[FindsBy(How = How.Id, Using = "destinationRdo")]
		protected IWebElement TransferredObjectWebElement { get; set; }

		protected SelectElement TransferredObjectSelect => new SelectElement(TransferredObjectWebElement);

		public ImportFirstPage(RemoteWebDriver driver) : base(driver)
		{
			PageFactory.InitElements(driver, this);
			Driver.SwitchTo().Frame("externalPage");
			WaitForPage();
		}

		public void SelectImport()
		{
			ImportExportRadio.FindElements(By.TagName("label")).First(e => e.Text == "Import").Click();
		}

		public string Source
		{
			get { return SourceSelect.SelectedOption.Text; }
			set { SourceSelect.SelectByText(value); }
		}

		public string TransferredObject
		{
			get { return TransferredObjectSelect.SelectedOption.Text; }
			set { TransferredObjectSelect.SelectByText(value); }
		}

		public ExportToFileSecondPage GoToNextPage()
		{
			NextButton.Click();
			//			return new ExportToFileSecondPage(Driver); // TODO
			return null;
		}
	}
}
