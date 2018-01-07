using System;
using System.Diagnostics;
using System.Threading;
using kCura.IntegrationPoints.UITests.Logging;
using kCura.IntegrationPoint.Tests.Core;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
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
			foreach (By by in bys)
			{
				try
				{
					if (ExpectedConditions.ElementIsVisible(by)(Driver) != null)
					{
						return true;
					}
				}
				catch (NoSuchElementException ex)
				{
					Log.Verbose(ex, "Exception occured while checking elements' visibility.");
				}
			}
			return false;
		}

		public void Sleep(int milliseconds)
		{
			Thread.Sleep(milliseconds);
		}

	}
}
