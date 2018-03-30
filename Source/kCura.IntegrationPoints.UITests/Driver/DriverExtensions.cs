using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.Driver
{
	public static class DriverExtensions
	{
		private const int _DEFAULT_CLICKABLE_TIMEOUT_IN_SECONDS = 30;

		public static void ClickWhenClickable(this IWebElement element, IWebDriver driver, TimeSpan? timeout = null)
		{
			TimeSpan ts = timeout ?? TimeSpan.FromSeconds(_DEFAULT_CLICKABLE_TIMEOUT_IN_SECONDS);
			var wait = new WebDriverWait(driver, ts);
			wait.Until(ExpectedConditions.ElementToBeClickable(element));
			element.Click();
		}
	}
}
