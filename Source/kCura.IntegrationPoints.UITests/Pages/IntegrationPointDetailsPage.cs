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

		[FindsBy(How = How.LinkText, Using = "Save as a Profile")]
		protected IWebElement SaveAsAProfileButton { get; set; }

        [FindsBy(How = How.LinkText, Using = "Edit")]
        protected IWebElement EditButton { get; set; }

		[FindsBy(How = How.Id, Using = "profile-name")]
		protected IWebElement ProfileNameInput { get; set; }

		public string ProfileName
		{
			get { return ProfileNameInput.Text; }
			set { ProfileNameInput.SetText(value); }
		}

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
            EditButton.ClickEx();
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
	        PropertiesTable propertiesTable = new PropertiesTable(Driver.FindElementById(tableName), title);
	        propertiesTable.Select();
	        return propertiesTable;
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

		private void SaveAsAProfileOnConfirmationDialog()
		{
			By saveAsProfileButtonLocator = By.Id("save-as-profile-confirm-button");
			Driver.FindElementEx(saveAsProfileButtonLocator).ClickEx();
		}
	}
}