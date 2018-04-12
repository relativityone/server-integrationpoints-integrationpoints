using System;
using System.Linq;
using kCura.IntegrationPoints.UITests.Logging;
using OpenQA.Selenium;
using Polly;
using Serilog;

namespace kCura.IntegrationPoints.UITests.Components
{
	public class JobStatusTable : Component
	{
	    const int jobTotalItems = 11;
	    const int jobItemsWithErrors = 12;
	    const int jobItemsTransfered = 10;

        protected static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(JobStatusTable));

		public JobStatusTable(IWebElement parent) : base(parent)
		{
		}

		public string GetLatestJobStatus()
		{
			string jobStatus = string.Empty;
			const int findUiElementTimeoutInMinutes = 5;
			const int numberOfRepeatsAfterStaleException = 10;
			
			Policy retry = Policy
				.Handle<StaleElementReferenceException>()
				.Or<IndexOutOfRangeException>()
				.WaitAndRetry(Enumerable.Repeat(TimeSpan.FromSeconds(1), numberOfRepeatsAfterStaleException));
			Policy tryFindStatusUpToTimeout = Policy.Timeout(TimeSpan.FromMinutes(findUiElementTimeoutInMinutes)).Wrap(retry);

			tryFindStatusUpToTimeout.Execute(() => jobStatus = ReadJobExecutionStatus());
			
			return jobStatus;
		}

		private string ReadJobExecutionStatus()
		{
			const int jobStatusColumnNumber = 7;
			By latestJobStatus = By.XPath("//table[@class='itemTable']//tbody/tr[@class='itemListRowAlt']/td");
			return  Parent.FindElements(latestJobStatus)[jobStatusColumnNumber].Text;
		}

	    public int GetTotalItems()
        {
            return GetNumberFromDetailsTable(jobTotalItems);
        }

        private int GetNumberFromDetailsTable(int columnNumber)
        {
            By latestJobStatus = By.XPath("//table[@class='itemTable']//tbody/tr[@class='itemListRowAlt']/td");
            int totalItems = -1;
            int.TryParse(Parent.FindElements(latestJobStatus)[columnNumber].Text, out totalItems);
            return totalItems;
        }

        public int GetItemsTransfered()
	    {
	        return GetNumberFromDetailsTable(jobItemsTransfered);

        }

        public int GetItemsWithErrors()
	    {
	        return GetNumberFromDetailsTable(jobItemsWithErrors);
        }

    }
}