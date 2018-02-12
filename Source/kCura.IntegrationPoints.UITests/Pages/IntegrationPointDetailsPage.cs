using System;
using System.Threading;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Components;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

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
			RunButton.Click();

			const int timeoutForWarningBoxSeconds = 5;
			By okButtonLocator = By.XPath("//span[text()='OK']");
			var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutForWarningBoxSeconds));
			IWebElement okButton = wait.Until(ExpectedConditions.ElementIsVisible(okButtonLocator));
			okButton.Click();

			return this;
		}

		public PropertiesTable SelectGeneralPropertiesTable()
		{
			Thread.Sleep(300);
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
			return new JobHistoryModel
			{
				//JobId = int.Parse(Parent.FindElement(By.ClassName("itemTable")).FindElements(By.XPath("tbody/tr/td"))[1].Text),
				//StartTime = DateTime.Parse(Parent.FindElement(By.ClassName("itemTable")).FindElements(By.XPath("tbody/tr/td"))[2].Text, CultureInfo.GetCultureInfo(1033)),
				//ArtifactId = int.Parse(Parent.FindElement(By.ClassName("itemTable")).FindElements(By.XPath("tbody/tr/td"))[3].Text),
				//Name = Parent.FindElement(By.ClassName("itemTable")).FindElements(By.XPath("tbody/tr/td"))[4].Text,
				//IntegrationPoint = Parent.FindElement(By.ClassName("itemTable")).FindElements(By.XPath("tbody/tr/td"))[5].Text,
				//JobType = Parent.FindElement(By.ClassName("itemTable")).FindElements(By.XPath("tbody/tr/td"))[6].Text,
				JobStatus = jobStatusTable.GetLatestJobStatus(),
				//DestinationWorkspace = Parent.FindElement(By.ClassName("itemTable")).FindElements(By.XPath("tbody/tr/td"))[8].Text,
				//DestinationInstance = Parent.FindElement(By.ClassName("itemTable")).FindElements(By.XPath("tbody/tr/td"))[9].Text,
				//ItemsTransferred = int.Parse(Parent.FindElement(By.ClassName("itemTable")).FindElements(By.XPath("tbody/tr/td"))[10].Text),
				//TotalItems = int.Parse(Parent.FindElement(By.ClassName("itemTable")).FindElements(By.XPath("tbody/tr/td"))[11].Text),
				//ItemsWithErrors = int.Parse(Parent.FindElement(By.ClassName("itemTable")).FindElements(By.XPath("tbody/tr/td"))[12].Text)
			};
		}
	}
}
