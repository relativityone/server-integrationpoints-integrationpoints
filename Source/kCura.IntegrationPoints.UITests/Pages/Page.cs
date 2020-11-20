using System;
using System.Diagnostics;
using System.Threading;
using kCura.IntegrationPoints.UITests.Logging;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using Serilog;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public abstract class Page
	{
		private static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(Page));

		protected readonly RemoteWebDriver Driver;

		private static int PollingIntervalInMilliseconds { get; } = 500;

		protected Page(RemoteWebDriver driver)
		{
			Driver = driver;
		}

		public void WaitForPage()
		{
			Thread.Sleep(500);
			using (new ImplicitTimeoutSetter(Driver, TimeSpan.FromSeconds(0)))
			{
				var wait = new WebDriverWait(Driver
					, TimeSpan.FromSeconds(SharedVariables.UiWaitForPageInSec)
				);
				wait.PollingInterval = TimeSpan.FromMilliseconds(PollingIntervalInMilliseconds);
				wait.Until(d =>
				{
					return elementIsNotDisplayed(d, By.Id("progressIndicatorContainer"))
						   && elementIsNotDisplayed(d, By.ClassName("ui-widget-overlay"))
						   && d.ExecuteJavaScript<string>("return document.readyState").ToString() == "complete";
				});
			}
		}

		private static bool elementIsNotDisplayed(IWebDriver driver, By by)
		{
			try
			{
				return !driver.FindElement(by).Displayed;
			}
			catch (NoSuchElementException)
			{
				return true;
			}
		}


		public void Refresh()
		{
			Driver.Navigate().Refresh();
		}

		public bool IsAnyElementVisible(params By[] bys)
		{
			return IsAnyElementVisible(Driver, bys);
		}

		public bool IsAnyElementVisible(ISearchContext parent, params By[] bys)
		{
			foreach (By by in bys)
			{
				try
				{
					if (parent.FindElement(by).Displayed)
					{
						return true;
					}
				}
				catch (Exception ex) when (ex is NoSuchElementException || ex is WebDriverException)
				{
					Log.Verbose(ex, "Exception occured while checking elements' visibility.");
				}
			}
			return false;
		}

		protected void SetInputText(IWebElement element, string text)
		{
			element.SetTextEx(text, Driver);
		}
	}
}
