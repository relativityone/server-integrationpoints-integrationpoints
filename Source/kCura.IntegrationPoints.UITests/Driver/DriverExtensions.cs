using System;
using System.Collections.ObjectModel;
using OpenQA.Selenium;
using System.Linq;
using System.Reflection;
using kCura.IntegrationPoint.Tests.Core;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.PageObjects;

namespace kCura.IntegrationPoints.UITests.Driver
{
	public static class DriverExtensions
	{
		private const int _DEFAULT_SEND_KEYS_TIMEOUT_IN_SECONDS = 20;

		public static IWebElement ClickEx(this IWebElement element, IWebDriver driver, bool pageShouldChange = false, TimeSpan? timeout = null)
		{
			return element.PerformAction(driver, el =>
			{
				string currentUrl = driver.Url;
				el.Click();
				if (pageShouldChange && currentUrl == driver.Url)
				{
					return null;
				}

				return el;
			}, timeout);
		}

		/// <summary>
		/// This method allows to chain By instances. It makes the IWebDriver search for the next By inside of the parent, and so on.
		/// </summary>
		/// <param name="parent">parent</param>
		/// <param name="children">selectors for next levels of search</param>
		/// <returns></returns>
		public static By Chain(this By parent, params By[] children)
		{
			return new ByChained(new[] { parent }.Concat(children).ToArray());
		}

		public static IWebElement SendKeysEx(this IWebElement element, IWebDriver driver, string keys, TimeSpan? timeout = null)
		{
			element.PerformAction(driver, el =>
			{
				el.SendKeys(keys);
				return el;
			}, timeout);
			return element;
		}

		public static IWebElement FindElementEx(this IWebElement element, By by, TimeSpan? timeout = null)
		{
			IWebDriver driver = ((IWrapsDriver)element).WrappedDriver;

			using (new ImplicitTimeoutSetter(driver, TimeSpan.Zero))
			{
				return driver.GetConfiguredWait(timeout).Until(d => element.FindElement(by));
			}
		}

		public static ReadOnlyCollection<IWebElement> FindElementsEx(this IWebElement element, By by, TimeSpan? timeout = null)
		{
			IWebDriver driver = ((IWrapsDriver)element).WrappedDriver;

			using (new ImplicitTimeoutSetter(driver, TimeSpan.Zero))
			{
				return driver.GetConfiguredWait(timeout).Until(d =>
				{
					ReadOnlyCollection<IWebElement> elements = element.FindElements(by);
					return elements.Any() ? elements : null;
				});
			}
		}

		public static IWebElement FindElementEx(this IWebDriver driver, By by, TimeSpan? timeout = null)
		{
			using (new ImplicitTimeoutSetter(driver, TimeSpan.Zero))
			{
				return driver.GetConfiguredWait(timeout).Until(d => d.FindElement(by));
			}
		}

		public static IWebElement FindElementEx(this By by, IWebDriver driver, TimeSpan? timeout = null)
		{
			using (new ImplicitTimeoutSetter(driver, TimeSpan.Zero))
			{
				return driver.GetConfiguredWait(timeout).Until(d => d.FindElement(by));
			}
		}

		public static ReadOnlyCollection<IWebElement> FindElementsEx(this IWebDriver driver, By by, TimeSpan? timeout = null)
		{
			using (new ImplicitTimeoutSetter(driver, TimeSpan.Zero))
			{
				return driver.GetConfiguredWait(timeout).Until(d => d.FindElements(by));
			}
		}

		public static IWebElement SetTextEx(this IWebElement element, string text, IWebDriver driver)
		{
			element.PerformAction(driver, el =>
			{
				el.Click();
				el.Clear();
				el.SendKeys(text);

				if (el.GetAttribute("value") != text)
				{
					return null;
				}
				return el;
			}, TimeSpan.FromSeconds(_DEFAULT_SEND_KEYS_TIMEOUT_IN_SECONDS));

			return element;
		}

		public static void GoToPage(this IWebDriver driver, string pageName)
		{
			IWebElement quickNavigationInput = driver.FindElementEx(By.Id("qnTextBox"));


			PerformAction(quickNavigationInput, driver, _ =>
			{
				quickNavigationInput.Click();
				quickNavigationInput.Clear();
				quickNavigationInput.SendKeys(pageName);

				IWebElement resultLinkLinkName = driver.FindElement(By.LinkText(pageName));
				resultLinkLinkName.Click();

				return resultLinkLinkName;
			});
		}

		public static IWebDriver SwitchToFrameEx(this IWebDriver driver, string frameName)
		{
			WebDriverWait wait = driver.GetConfiguredWait();
			return wait.Until(d =>
			{
				try
				{
					return driver.SwitchTo().Frame(frameName);
				}
				catch (NoSuchFrameException)
				{
					return (IWebDriver)null;
				}
			});
		}

		internal static WebDriverWait GetConfiguredWait(this IWebDriver driver, TimeSpan? timeout = null)
		{
			var wait = new WebDriverWait(driver,
				timeout ?? TimeSpan.FromSeconds(SharedVariables.UiWaitForAjaxCallsInSec));

			wait.IgnoreExceptionTypes(
				typeof(ElementNotInteractableException),
				typeof(ElementClickInterceptedException),
				typeof(InvalidOperationException),
				typeof(WebDriverException),
				typeof(NoSuchElementException),
				typeof(TargetInvocationException));
			return wait;
		}

		internal static TResult PerformAction<TResult>(this IWebElement element, IWebDriver driver,
			Func<IWebElement, TResult> action, TimeSpan? timeout = null)
		{
			WebDriverWait wait = driver.GetConfiguredWait(timeout);

			return wait.Until(_ =>
			{
				return action(element);
			});
		}

		public static IWebElement ScrollIntoView(this IWebElement element, IWebDriver driver)
		{
			var jse = (IJavaScriptExecutor)driver;
			jse.ExecuteScript("arguments[0].scrollIntoView(true)", element);

			return element;
		}

		public static IWebElement ScrollIntoView(this IWrapsElement element, IWebDriver driver)
		{
			return ScrollIntoView(element.WrappedElement, driver);
		}

		public static void SelectByTextEx(this SelectElement el, string text, IWebDriver driver)
		{
			WebDriverWait wait = driver.GetConfiguredWait();

			try
			{
				wait.Until(d =>
				{
					el.SelectByText(text);
					return true;
				});
			}
			catch (WebDriverTimeoutException ex)
			{
				string availableOptions = el.Options.Aggregate("", (s, element) => s + "," + element.Text);
				throw new WebDriverException($"Could not locate element: '{text}'. Available options: {Environment.NewLine}{availableOptions}", ex);
			}
		}
	}
}
