using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public abstract class ImportFirstPage<TSecondPage, TModel> : FirstPage where TSecondPage : ImportSecondBasePage<TModel>
	{
		[FindsBy(How = How.Id, Using = "isExportType")]
		protected IWebElement ImportExportRadio { get; set; }

		[FindsBy(How = How.Id, Using = "sourceProvider")]
		protected IWebElement SourceSelectWebElement { get; set; }

		protected SelectElement SourceSelect => new SelectElement(SourceSelectWebElement);

		[FindsBy(How = How.Id, Using = "destinationRdo")]
		protected IWebElement TransferredObjectWebElement { get; set; }

		protected SelectElement TransferredObjectSelect => new SelectElement(TransferredObjectWebElement);

		protected ImportFirstPage(RemoteWebDriver driver) : base(driver)
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

		public TSecondPage GoToNextPage() 
		{
			InitSecondPage();
			return Create(Driver);
		}

		protected abstract TSecondPage Create(RemoteWebDriver driver);

		private void InitSecondPage()
		{
			WaitForPage();
			NextButton.Click();
		}
	}
}
