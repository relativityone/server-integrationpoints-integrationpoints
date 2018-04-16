using System;
using OpenQA.Selenium;
using System.Linq;
using System.Reflection;
using System.Threading;
using Polly;
using Polly.Retry;


namespace kCura.IntegrationPoints.UITests.Driver
{
	public static class DriverExtensions
	{
		private const int _DEFAULT_CLICKABLE_TIMEOUT_IN_SECONDS = 30;

		private const int _DEFAULT_SEND_KEYS_TIMEOUT_IN_SECONDS = 30;

		public static IWebElement ClickWhenClickable(this IWebElement element, TimeSpan? timeout = null)
		{
			const int sleepInMs = 250;

			TimeSpan ts = timeout ?? TimeSpan.FromSeconds(_DEFAULT_CLICKABLE_TIMEOUT_IN_SECONDS);
			Policy retry = Policy.Handle<TargetInvocationException>()
				.WaitAndRetry(Enumerable.Repeat(TimeSpan.FromMilliseconds(sleepInMs), int.MaxValue));
			Policy clickUpToTimeout = Policy.Timeout(ts).Wrap(retry);

			clickUpToTimeout.Execute(element.Click);

			return element;
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
	}
}
