using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class IntegrationPointDetailsPage : GeneralPage
	{
		[FindsBy(How = How.LinkText, Using = "Run")]
		protected IWebElement RunButton;

		public IntegrationPointDetailsPage(RemoteWebDriver driver) : base(driver)
		{
			WaitForPage();
			PageFactory.InitElements(driver, this);
		}

		public IntegrationPointDetailsPage RunIntegrationPoint()
		{
			RunButton.ClickEx();
			ClickOkOnConfirmationDialog();
			return this;
		}

		public PropertiesTable SelectGeneralPropertiesTable()
		{
			WaitForPage();
			var t = new PropertiesTable(Driver.FindElementById("summaryPage"), "General");
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
			var jobHistoryModel = new JobHistoryModel
			{
				JobStatus = jobStatusTable.GetLatestJobStatus(),
				ItemsWithErrors = jobStatusTable.GetItemsWithErrors(),
				ItemsTransferred = jobStatusTable.GetItemsTransfered(),
				TotalItems = jobStatusTable.GetTotalItems()
			};

			return jobHistoryModel;
		}

		private void ClickOkOnConfirmationDialog()
		{
			By okButtonLocator = By.XPath("//*[text()='OK']");
			Driver.FindElementEx(okButtonLocator).ClickEx();
		}
	}
}
