using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoint.Tests.Core
{
	using System;
	using System.Collections.ObjectModel;

	public static class Selenium
	{
		public static void GoToUrl(this IWebDriver _driver, string url)
		{
			_driver.Navigate().GoToUrl(url);
		}

		public static void LogIntoRelativity(this IWebDriver _driver, string username, string password)
		{
			string relativityUrl = $"http://{SharedVariables.TargetHost}/relativity";
			_driver.GoToUrl(relativityUrl);

			_driver.FindElement(By.Id("_email")).SendKeys(username);
			_driver.FindElement(By.Id("continue")).Click();

			_driver.FindElement(By.Id("_password__password_TextBox")).SendKeys(password);
			_driver.FindElement(By.Id("_login")).Click();
		}

		public static void GoToWorkspace(this IWebDriver _driver, int artifactId)
		{
			string workspaceXpath = $"//a[@href='/Relativity/RedirectHandler.aspx?defaultCasePage=1&AppID={artifactId}&RootFolderID=1003697']";

			_driver.SwitchTo().DefaultContent();
			_driver.SwitchTo().Frame("ListTemplateFrame");

			_driver.FindElement(By.XPath(workspaceXpath)).Click();
		}

		public static void GoToTab(this IWebDriver _driver, string tabName)
		{
			ReadOnlyCollection<IWebElement> webElementCollection = _driver.FindElements(By.Id("horizontal-tabstrip"));
			IWebElement navigationList = webElementCollection[0].FindElement(By.XPath("//ul[@class='nav navbar-nav']"));
			ReadOnlyCollection<IWebElement> listElements = navigationList.FindElements(By.TagName("li"));
			foreach (IWebElement listElement in listElements)
			{
				ReadOnlyCollection<IWebElement> anchorCollectoin = listElement.FindElements(By.TagName("a"));

				foreach (IWebElement anchor in anchorCollectoin)
				{
					if (anchor.Text.Equals(tabName))
					{
						anchor.Click();
						return;
					}
				}
			}
		}

		public static void GoToObjectInstance(this IWebDriver _driver, int workspaceArtifactId, int integrationPointArtifactId, int artifactTypeId)
		{
			string integrationPointUrl = $"http://{SharedVariables.TargetHost}/Relativity/Case/Mask/View.aspx?AppID={workspaceArtifactId}&ArtifactID={integrationPointArtifactId}&ArtifactTypeID={artifactTypeId}";
			_driver.Navigate().GoToUrl(integrationPointUrl);
		}

		public static bool PageShouldContain(this IWebDriver _driver, string message)
		{
			return _driver.PageSource.Contains(message);
		}

		public static void WaitUntilElementExists(this IWebDriver _driver, ElementType elementType, string value, int timeSeconds)
		{
			WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeSeconds));
			switch (elementType)
			{
				case ElementType.Id:
					{
						wait.Until(ExpectedConditions.ElementExists(By.Id(value)));
						break;
					}
				case ElementType.Xpath:
					{
						wait.Until(ExpectedConditions.ElementExists(By.XPath(value)));
						break;
					}
				default:
					{
						break;
					}
			}
		}

		public static void WaitUntilElementIsClickable(this IWebDriver _driver, ElementType elementType, string value, int timeSeconds)
		{
			WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeSeconds));
			switch (elementType)
			{
				case ElementType.Id:
					{
						wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(value)));
						break;
					}
				case ElementType.Xpath:
					{
						wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(value)));
						break;
					}
				default:
					{
						break;
					}
			}
		}

		public static void WaitUntilElementIsVisible(this IWebDriver _driver, ElementType elementType, string value, int timeSeconds)
		{
			WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeSeconds));
			switch (elementType)
			{
				case ElementType.Id:
					{
						wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.Id(value)));
						break;
					}
				case ElementType.Xpath:
					{
						wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath(value)));
						break;
					}
				default:
					{
						break;
					}
			}
		}

		public static void WaitUntilIdExists(this IWebDriver _driver, string id, int timeSeconds)
		{
			WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeSeconds));
			wait.Until(ExpectedConditions.ElementExists(By.Id(id)));
		}

		public static void CloseSeleniumBrowser(this IWebDriver _driver)
		{
			try
			{
				_driver.Quit();
			}
			catch (Exception)
			{
			}
		}

		public static void SelectFromDropdownList(this IWebDriver _driver, string dropdownId, string value)
		{
			IWebElement dropDown = _driver.FindElement(By.Id(dropdownId));
			SelectElement selectValue = new SelectElement(dropDown);
			selectValue.SelectByText(value);
		}
	}

	public enum ElementType
	{
		Id,
		Xpath,
		CssSelector,
		Name,
		TagName,
		LinkText
	}
}