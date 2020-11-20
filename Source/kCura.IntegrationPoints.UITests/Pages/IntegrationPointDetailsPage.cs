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
		protected IWebElement RunButton => Driver.FindElementEx(By.LinkText("Run"));

		protected IWebElement SaveAsAProfileButton => Driver.FindElementEx(By.LinkText("Save as a Profile"));

        protected IWebElement EditButton => Driver.FindElementEx(By.LinkText("Edit"));

		protected IWebElement ProfileNameInput => Driver.FindElementEx(By.Id("profile-name"));

		public string ProfileName
		{
			get { return ProfileNameInput.Text; }
			set { ProfileNameInput.SetTextEx(value, Driver); }
		}

		public IntegrationPointDetailsPage(RemoteWebDriver driver) : base(driver)
		{
			WaitForPage();
		}

		public IntegrationPointDetailsPage RunIntegrationPoint()
		{
			RunButton.ClickEx(Driver);
			ClickOkOnConfirmationDialog();
			return this;
		}

		public IntegrationPointDetailsPage SaveAsAProfileIntegrationPoint()
		{
			return SaveAsAProfileIntegrationPoint(ProfileName);
		}

		public IntegrationPointDetailsPage SaveAsAProfileIntegrationPoint(string profileName)
		{
			SaveAsAProfileButton.ClickEx();
			ProfileName = profileName;
			SaveAsAProfileOnConfirmationDialog();
			return this;
		}

		public ExportFirstPage EditIntegrationPoint()
        {
            EditButton.ClickEx(Driver);
			return new ExportFirstPage(Driver);
        }

        public PropertiesTable SelectGeneralPropertiesTable()
        {
	        return SelectPropertiesTable("summaryPage", "General");
        }

        public PropertiesTable SelectSchedulingPropertiesTable()
        {
	        return SelectPropertiesTable("schedulerSummaryPage", "Scheduling");
        }

        private PropertiesTable SelectPropertiesTable(string tableName, string title)
        {
	        WaitForPage();
	        PropertiesTable propertiesTable = new PropertiesTable(Driver.FindElementEx(By.Id(tableName)), title, Driver);
	        propertiesTable.Select();
	        return propertiesTable;
		}

		public JobHistoryModel GetLatestJobHistoryFromJobStatusTable()
		{
			var jobStatusTable = new JobStatusTable(Driver.FindElementEx(By.Id("Table1")), Driver);
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
			Driver.FindElementEx(okButtonLocator).ClickEx(Driver);
		}

		private void SaveAsAProfileOnConfirmationDialog()
		{
			By saveAsProfileButtonLocator = By.Id("save-as-profile-confirm-button");
			Driver.FindElementEx(saveAsProfileButtonLocator).ClickEx(Driver);
		}
	}
}