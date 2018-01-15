using System;
using System.Threading;
using kCura.IntegrationPoints.UITests.Components;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class PushToRelativitySecondPage : GeneralPage
	{
		[FindsBy(How = How.Id, Using = "sourceSelector")]
		protected IWebElement SourceSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "workspaceSelector")]
		protected IWebElement DestinationSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "relativitySelector")]
		protected IWebElement InstanceSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "next")]
		protected IWebElement NextButton { get; set; }

		[FindsBy(How = How.Id, Using = "location-0")]
		protected IWebElement FolderLocation;

		[FindsBy(How = How.Id, Using = "location-1")]
		protected IWebElement ProductionLocation;

		[FindsBy(How = How.Id, Using = "saved-search-selection-button")]
		protected IWebElement SavedSearchSelectionButton { get; set; }

		[FindsBy(How = How.CssSelector, Using = "#s2id_savedSearchSelector a")]
		protected IWebElement SavedSearchSelectWebElement { get; set; }

		protected Select SavedSearchSelect { get; set; }

		protected Select DestinationWorkspaceSelect { get; set; }

		protected Select RelativityInstanceSelect { get; set; }

		protected SelectElement SourceSelectElement => new SelectElement(SourceSelectWebElement);

		public string SourceSelect
		{
			get { return SourceSelectElement.SelectedOption.Text; }
			set { SourceSelectElement.SelectByText(value); }
		}

		protected SelectElement DestinationWorkspaceElement => new SelectElement(DestinationSelectWebElement);

		public string DestinationWorkspace
		{
			get { return DestinationWorkspaceElement.SelectedOption.Text; }
			set { DestinationWorkspaceElement.SelectByText(value); }
		}

		protected SelectElement RelativityInstanceElement => new SelectElement(InstanceSelectWebElement);

		public TreeSelect FolderLocationSelect;

		public string RelativityInstance
		{
			get { return RelativityInstanceElement.SelectedOption.Text; }
			set { RelativityInstanceElement.SelectByText(value); }
		}

		public PushToRelativitySecondPage(RemoteWebDriver driver) : base(driver)
		{
			driver.SwitchTo().Frame("configurationFrame");
			WaitForPage();
			PageFactory.InitElements(driver, this);
			SavedSearchSelect = new Select(Driver.FindElementById("s2id_savedSearchSelector"));
			FolderLocationSelect = new TreeSelect(driver.FindElement(By.XPath(@"//div[@id='location-select']/..")));
		}

		public PushToRelativitySecondPage SelectAllDocuments()
		{
			SavedSearchSelect.Choose("All Documents");
			Sleep(200);
			return this;
		}

		public PushToRelativitySecondPage SelectFolderLocation()
		{
			FolderLocation.Click();
			return this;
		}

		public PushToRelativitySecondPage SelectProductionLocation()
		{
			ProductionLocation.Click();
			return this;
		}

		public PushToRelativityThirdPage GoToNextPage()
		{
			WaitForPage();

			Driver.SwitchTo().ParentFrame();
			Driver.SwitchTo().ParentFrame();
			Driver.SwitchTo().Frame("externalPage");

			Thread.Sleep(TimeSpan.FromMilliseconds(200));
			NextButton.Click();
			return new PushToRelativityThirdPage(Driver);
		}
	}
}
