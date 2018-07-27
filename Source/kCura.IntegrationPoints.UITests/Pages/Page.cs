using System;
using System.Diagnostics;
using System.Threading;
using kCura.IntegrationPoints.UITests.Logging;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using Serilog;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public abstract class Page
	{
		private static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(Page));

		protected readonly RemoteWebDriver Driver;

		private static int SleepInterval { get; } = 200;

		protected Page(RemoteWebDriver driver)
		{
			Driver = driver;
		}

		public void WaitForPage()
		{
			Driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
			try
			{
				Stopwatch timeWithoutProgressIndicator = Stopwatch.StartNew();
				Stopwatch totalTime = Stopwatch.StartNew();
				while (timeWithoutProgressIndicator.Elapsed.Seconds < SharedVariables.UiWaitForAjaxCallsInSec)
				{
					if (IsAnyElementVisible(By.Id("progressIndicatorContainer"), By.ClassName("ui-widget-overlay")))
					{
						timeWithoutProgressIndicator.Restart();
					}
					Thread.Sleep(SleepInterval);
					if (totalTime.Elapsed.Seconds > SharedVariables.UiWaitForPageInSec)
					{
						throw new WebDriverTimeoutException("Progress indicator is visible longer than 2 minutes. Some popup is displayed or your system is way too slow. Check screenshot.");
						// TODO popup is recognized as progress -> IsKreciolekVisible()
					}
				}
			}
			finally
			{
				Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(SharedVariables.UiImplicitWaitInSec);
			}
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

		public void Sleep(int milliseconds)
		{
			Sleep(TimeSpan.FromMilliseconds(milliseconds));
		}

		public void Sleep(TimeSpan timeSpan)
		{
			Thread.Sleep(timeSpan);
		}

		protected void SetInputText(IWebElement element, string text)
		{
			element.SendKeys(Keys.Control + "a");
			element.SetText(text);
		}
	}
}
