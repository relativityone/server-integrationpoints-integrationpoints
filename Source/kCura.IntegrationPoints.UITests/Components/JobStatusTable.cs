using System;
using System.Diagnostics;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Logging;
using OpenQA.Selenium;
using Polly;
using Serilog;

namespace kCura.IntegrationPoints.UITests.Components
{
	public class JobStatusTable : Component
	{
		protected static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(JobStatusTable));

		public JobStatusTable(IWebElement parent) : base(parent)
		{
		}

		public string GetLatestJobStatus()
		{
			string jobStatus = string.Empty;
			const int findUiElementTimeoutInMinutes = 5;
			
			Policy retry = Policy.Handle<StaleElementReferenceException>().Retry();
			Policy tryFindStatusUpToTimeout = Policy.Timeout(TimeSpan.FromMinutes(findUiElementTimeoutInMinutes)).Wrap(retry);

			tryFindStatusUpToTimeout.Execute(() =>
			{
				jobStatus = ReadJobExecutionStatus();
			});
			
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
	        const int jobStatusColumnNumber = 11;
	        By latestJobStatus = By.XPath("//table[@class='itemTable']//tbody/tr[@class='itemListRowAlt']/td");
	        return int.Parse(Parent.FindElements(latestJobStatus)[jobStatusColumnNumber].Text);
	    }

        public int GetItemsTransfered()
	    {
	        const int jobStatusColumnNumber = 10;
	        By latestJobStatus = By.XPath("//table[@class='itemTable']//tbody/tr[@class='itemListRowAlt']/td");
	        return int.Parse(Parent.FindElements(latestJobStatus)[jobStatusColumnNumber].Text);
	    }

	    public int GetItemsWithErrors()
	    {
	        const int jobStatusColumnNumber = 12;
	        By latestJobStatus = By.XPath("//table[@class='itemTable']//tbody/tr[@class='itemListRowAlt']/td");
	        return int.Parse(Parent.FindElements(latestJobStatus)[jobStatusColumnNumber].Text);
	    }

    }
}