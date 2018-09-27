using System;
using System.Threading;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.PageObjects;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class IntegrationPointDetailsPage : GeneralPage
	{
		[FindsBy(How = How.LinkText, Using = "Edit")]
		protected IWebElement EditButton;

		[FindsBy(How = How.LinkText, Using = "Delete")]
		protected IWebElement DeleteButton;

		[FindsBy(How = How.LinkText, Using = "Back")]
		protected IWebElement BackButton;

		[FindsBy(How = How.LinkText, Using = "Edit Permissions")]
		protected IWebElement EditPermissionsButton;

		[FindsBy(How = How.LinkText, Using = "View Audit")]
		protected IWebElement ViewAuditButton;

		[FindsBy(How = How.LinkText, Using = "Run")]
		protected IWebElement RunButton;

		[FindsBy(How = How.LinkText, Using = "Save as a Profile")]
		protected IWebElement SaveProfileButton;

		[FindsBy(How = How.LinkText, Using = "General")]
		protected IWebElement GeneralTabLink;

		public IntegrationPointDetailsPage(RemoteWebDriver driver) : base(driver)
		{
			WaitForPage();
			PageFactory.InitElements(driver, this);
			//Driver.SwitchTo().Frame("externalPage");
		}

		public IntegrationPointDetailsPage RunIntegrationPoint()
		{
			RunButton.ClickEx();

			const int timeoutForWarningBoxSeconds = 5;
			By okButtonLocator = By.XPath("//button[text()='OK']");
			var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutForWarningBoxSeconds));
			IWebElement okButton = wait.Until(ExpectedConditions.ElementIsVisible(okButtonLocator));
			okButton.ClickEx();

			return this;
		}

		public PropertiesTable SelectGeneralPropertiesTable()
		{
			WaitForPage();
			var t = new PropertiesTable(Driver.FindElementById("summaryPage"), "General");
			t.Select();
			return t;
		}

		public PropertiesTable SelectSchedulingPropertiesTable()
		{
			var t = new PropertiesTable(Driver.FindElementById("schedulerSummaryPage"), "Scheduling");
			t.Select();
			return t;
		}

		public JobHistoryModel GetLatestJobHistoryFromJobStatusTable()
		{
			var jobStatusTable = new JobStatusTable(Driver.FindElementById("Table1"));
			return BuildLatestJobHistory(jobStatusTable);
		}

		private static JobHistoryModel BuildLatestJobHistory(JobStatusTable jobStatusTable)
		{
		    //TODO implement remaining methods and assing JobHistoryModel remaining properties
		    var jobHistoryModel = new JobHistoryModel
		    {
                //JobId = int.Parse(Parent.FindElement(By.ClassName("itemTable")).FindElements(By.XPath("tbody/tr/td"))[1].Text),
                //StartTime = DateTime.Parse(Parent.FindElement(By.ClassName("itemTable")).FindElements(By.XPath("tbody/tr/td"))[2].Text, CultureInfo.GetCultureInfo(1033)),
                //ArtifactId = int.Parse(Parent.FindElement(By.ClassName("itemTable")).FindElements(By.XPath("tbody/tr/td"))[3].Text),
                //Name = Parent.FindElement(By.ClassName("itemTable")).FindElements(By.XPath("tbody/tr/td"))[4].Text,
                //IntegrationPoint = Parent.FindElement(By.ClassName("itemTable")).FindElements(By.XPath("tbody/tr/td"))[5].Text,
                //JobType = Parent.FindElement(By.ClassName("itemTable")).FindElements(By.XPath("tbody/tr/td"))[6].Text,
		        //DestinationWorkspace = Parent.FindElement(By.ClassName("itemTable")).FindElements(By.XPath("tbody/tr/td"))[8].Text,
		        //DestinationInstance = Parent.FindElement(By.ClassName("itemTable")).FindElements(By.XPath("tbody/tr/td"))[9].Text,
		        JobStatus = jobStatusTable.GetLatestJobStatus(),
		        ItemsWithErrors = jobStatusTable.GetItemsWithErrors(),
		        ItemsTransferred = jobStatusTable.GetItemsTransfered(),
		        TotalItems = jobStatusTable.GetTotalItems()
		    };


		    return jobHistoryModel;
		}
	}
}
