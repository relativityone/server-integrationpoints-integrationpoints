using System;
using System.Collections.ObjectModel;
using OpenQA.Selenium;
using System.Linq;
using System.Reflection;
using System.Threading;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Logging;
using OpenQA.Selenium.Internal;
using OpenQA.Selenium.Support.UI;
using Polly;
using Polly.Retry;
using Serilog;

namespace kCura.IntegrationPoints.UITests.Driver
{
	public static class DriverExtensions
	{
		private const int _DEFAULT_SEND_KEYS_TIMEOUT_IN_SECONDS = 20;

		private static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(DriverExtensions));

		public static readonly TimeSpan DefaultRetryInterval = TimeSpan.FromMilliseconds(250);

		public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

		public static IWebElement ClickEx(this IWebElement element, TimeSpan? timeout = null)
		{
			TimeSpan timeoutTs = MultiplyTimeout(timeout ?? DefaultTimeout);
			ExecuteWithTimeout(element.Click, timeoutTs, DefaultRetryInterval);
			return element;
		}

		public static IWebElement FindElementEx(this ISearchContext searchContext, By by, TimeSpan? timeout = null)
		{
			TimeSpan timeoutTs = MultiplyTimeout(timeout ?? DefaultTimeout);
			return ExecuteWithTimeout<IWebElement>(() => searchContext.FindElement(by), timeoutTs, DefaultRetryInterval);
		}

		public static ReadOnlyCollection<IWebElement> FindElementsEx(this IWebElement element, By by, TimeSpan? timeout = null)
		{
			IWebDriver driver = ((IWrapsDriver)element).WrappedDriver;
			driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(0);
			TimeSpan timeoutTs = MultiplyTimeout(timeout ?? TimeSpan.FromSeconds(10));
			try
			{
				try
				{
					var wait = new WebDriverWait(driver, timeoutTs);
					wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(by));
					return element.FindElements(by);
				}
				catch (WebDriverTimeoutException)
				{
					return new ReadOnlyCollection<IWebElement>(Enumerable.Empty<IWebElement>().ToList());
				}
			}
			finally
			{
				driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(SharedVariables.UiImplicitWaitInSec);
			}
		}

		private static TimeSpan MultiplyTimeout(TimeSpan timeout)
		{
			return TimeSpan.FromMilliseconds(timeout.TotalMilliseconds * SharedVariables.UiTimeoutMultiplier);
		}

		public static IWebElement SetText(this IWebElement element, string text)
		{
			TimeSpan timeout = MultiplyTimeout(TimeSpan.FromSeconds(_DEFAULT_SEND_KEYS_TIMEOUT_IN_SECONDS));
			
			const int sleepInMs = 250;
			RetryPolicy<bool> retryUntilTrue = Policy.HandleResult(false)
				.WaitAndRetryForever(retryAttempt => TimeSpan.FromMilliseconds(sleepInMs));

			element.Clear();
			Thread.Sleep(500);
			Policy.Timeout(timeout)
				.Wrap(retryUntilTrue)
				.Execute(() => element.GetAttribute("value") == "");

			element.SendKeys(text);
			Thread.Sleep(500);
			Policy.Timeout(timeout)
				.Wrap(retryUntilTrue)
				.Execute(() => element.GetAttribute("value") == text);

			return element;
		}

		public static IWebElement ScrollIntoView(this IWebElement element, IWebDriver driver = null)
		{
			driver = driver ?? ((IWrapsDriver) element).WrappedDriver;

			var jse = (IJavaScriptExecutor) driver;
			jse.ExecuteScript("arguments[0].scrollIntoView(true)", element);

			return element;
		}

		public static IWebElement ScrollIntoView(this IWrapsElement element)
		{
			return ScrollIntoView(element.WrappedElement);
		}
		
		public static void ExecuteWithTimeout(Action action, TimeSpan timeout, TimeSpan retryInterval)
		{
			try
			{
				Policy clickUpToTimeout = CreatePolicy(timeout, retryInterval);
				clickUpToTimeout.Execute(action);
			}
			catch (Exception ex)
			{
				throw new UiTestException("Action timed out.", ex);
			}
		}

		public static T ExecuteWithTimeout<T>(Func<T> action, TimeSpan timeout, TimeSpan retryInterval)
		{
			try
			{
				Policy clickUpToTimeout = CreatePolicy(timeout, retryInterval);
				PolicyResult<T> result = clickUpToTimeout.ExecuteAndCapture<T>(action);

				switch (result.Outcome)
				{
					case OutcomeType.Successful:
						return result.Result;
					case OutcomeType.Failure:
						throw result.FinalException;
					default:
						throw new NotImplementedException();
				}
			}
			catch (Exception ex)
			{
				throw new UiTestException("Action timed out.", ex);
			}
		}

		private static Policy CreatePolicy(TimeSpan timeout, TimeSpan retryInterval)
		{
			Policy retry = Policy
				.Handle<TargetInvocationException>()
				.Or<InvalidOperationException>()
				.Or<WebDriverException>()
				.WaitAndRetry(Enumerable.Repeat(retryInterval, int.MaxValue));
			Policy policy = Policy.Timeout(timeout).Wrap(retry);
			return policy;
		}

	}
}
