using System;
using System.Diagnostics;
using OpenQA.Selenium;
using System.Linq;
using System.Reflection;
using System.Threading;
using kCura.IntegrationPoints.UITests.Logging;
using OpenQA.Selenium.Internal;
using Polly;
using Polly.Retry;
using Serilog;

namespace kCura.IntegrationPoints.UITests.Driver
{
	public static class DriverExtensions
	{
		private static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(DriverExtensions));

		private const int _DEFAULT_CLICKABLE_TIMEOUT_IN_SECONDS = 30;

		private const int _DEFAULT_SEND_KEYS_TIMEOUT_IN_SECONDS = 30;

		public static IWebElement ClickWhenClickable(this IWebElement element, TimeSpan? timeout = null)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				const int sleepInMs = 250;

				TimeSpan ts = timeout ?? TimeSpan.FromSeconds(_DEFAULT_CLICKABLE_TIMEOUT_IN_SECONDS);
				Policy retry = Policy.Handle<TargetInvocationException>().Or<InvalidOperationException>()
					.WaitAndRetry(Enumerable.Repeat(TimeSpan.FromMilliseconds(sleepInMs), int.MaxValue));
				Policy clickUpToTimeout = Policy.Timeout(ts).Wrap(retry);

				clickUpToTimeout.Execute(element.Click);

				return element;
			}
			finally
			{
				Log.Verbose("ClickWhenClickable() took {Seconds} sec.", stopwatch.Elapsed.Seconds);
			}
		}

		public static IWebElement SetText(this IWebElement element, string text)
		{
			const int sleepInMs = 250;
			RetryPolicy<bool> retryUntilTrue = Policy.HandleResult(false)
				.WaitAndRetryForever(retryAttempt => TimeSpan.FromMilliseconds(sleepInMs));

			element.Clear();
			Thread.Sleep(500);
			Policy.Timeout(TimeSpan.FromSeconds(_DEFAULT_SEND_KEYS_TIMEOUT_IN_SECONDS))
				.Wrap(retryUntilTrue)
				.Execute(() => element.GetAttribute("value") == "");

			element.SendKeys(text);
			Thread.Sleep(500);
			Policy.Timeout(TimeSpan.FromSeconds(_DEFAULT_SEND_KEYS_TIMEOUT_IN_SECONDS))
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

	}
}
