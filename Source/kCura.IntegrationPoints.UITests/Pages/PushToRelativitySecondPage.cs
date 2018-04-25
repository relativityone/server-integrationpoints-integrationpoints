using System;
using System.Threading;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;
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

		[FindsBy(How = How.Id, Using = "s2id_sourceProductionSetsSelector")]
		protected IWebElement SourceProductionSelectWebElement { get; set; }

		[FindsBy(How = How.Id, Using = "s2id_productionSetsSelector")]
		protected IWebElement ProductionLocationSelectWebElement { get; set; }

		protected Select SourceProductionSelect => new Select(SourceProductionSelectWebElement);

		protected Select ProductionLocationSelect => new Select(ProductionLocationSelectWebElement);

		protected Select DestinationWorkspaceSelect { get; set; }

		protected Select RelativityInstanceSelect { get; set; }

		protected SelectElement SourceSelectElement => new SelectElement(SourceSelectWebElement);
		protected SavedSearchSelector SavedSearchSelector { get; }

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
			FolderLocationSelect = new TreeSelect(driver.FindElement(By.XPath(@"//div[@id='location-select']/..")));
			SavedSearchSelector = new SavedSearchSelector(Driver);
		}

		public PushToRelativitySecondPage SelectSavedSearch()
		{
			SavedSearchSelector.SelectSavedSearch("All Documents");
			return this;
		}

		public PushToRelativitySecondPage SelectSourceProduction(string productionName)
		{
			SourceProductionSelect.Choose(productionName);
			WaitForPage();
			return this;
		}

		public PushToRelativitySecondPage SelectFolderLocation()
		{
			WaitForPage();
			FolderLocation.ClickWhenClickable();
			return this;
		}

		public PushToRelativitySecondPage SelectProductionLocation(string productionName)
		{
			ProductionLocation.ClickWhenClickable();
			WaitForPage();
			ProductionLocationSelect.Choose(productionName);
			return this;
		}

		public PushToRelativityThirdPage GoToNextPage()
		{
			WaitForPage();

			Driver.SwitchTo().ParentFrame();
			Driver.SwitchTo().ParentFrame();
			Driver.SwitchTo().Frame("externalPage");

			Thread.Sleep(TimeSpan.FromMilliseconds(200));
			NextButton.ClickWhenClickable();
			return new PushToRelativityThirdPage(Driver);
		}
	}
}
