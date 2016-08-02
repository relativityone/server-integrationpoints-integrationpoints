using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoint.Tests.Core
{
	using System;
	using System.Collections.ObjectModel;

	public static class Selenium
	{
		static bool IsBetaUser = true;
		public static void GoToUrl(this IWebDriver driver, string url)
		{
			driver.Navigate().GoToUrl(url);
		}

		public static void LogIntoRelativity(this IWebDriver driver, string username, string password)
		{
			string relativityUrl = $"http://{SharedVariables.TargetHost}/relativity";
			driver.GoToUrl(relativityUrl);

			driver.FindElement(By.Id("_email")).SendKeys(username);
			driver.FindElement(By.Id("continue")).Click();

			driver.FindElement(By.Id("_password__password_TextBox")).SendKeys(password);
			driver.FindElement(By.Id("_login")).Click();
		}

		public static void GoToWorkspace(this IWebDriver driver, int artifactId)
		{
			string workspaceXpath;
			if (!IsBetaUser)
			{
				workspaceXpath = $"//a[@href='/Relativity/RedirectHandler.aspx?defaultCasePage=1&AppID={artifactId}&RootFolderID=1003697']";

				driver.SwitchTo().DefaultContent();
				driver.SwitchTo().Frame("ListTemplateFrame");
				driver.WaitUntilElementExists(ElementType.Xpath, workspaceXpath, 15);
				driver.FindElement(By.XPath(workspaceXpath)).Click();
			}

			else if (IsBetaUser)
			{
				workspaceXpath = $"//a[@href='/Relativity/RedirectHandler.aspx?defaultCasePage=1&AppID={artifactId}']";

				driver.SwitchTo().DefaultContent();
				driver.SwitchTo().Frame("_externalPage");
				driver.WaitUntilElementExists(ElementType.Xpath, workspaceXpath, 15);
				driver.FindElement(By.XPath(workspaceXpath)).Click();
			}
		}

		public static void GoToTab(this IWebDriver driver, string tabName)
		{

			try
			{
				driver.WaitUntilElementExists(ElementType.Id, "horizontal-tabstrip", 10);
				ReadOnlyCollection<IWebElement> webElementCollection = driver.FindElements(By.Id("horizontal-tabstrip"));
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

				ReadOnlyCollection<IWebElement> anchors = driver.FindElements(By.XPath("//div[@id='vertical-tabstrip']/accordion/div/ul/li/div/div[1]/h4/a/a[1]"));
				foreach (IWebElement anchor in anchors)
				{
					IWebElement anchorSpan = anchor.FindElement(By.XPath(".//span"));
					string spanText = anchorSpan.GetAttribute("innerText");
					if(spanText == tabName)
					{
						string anchorHref = anchor.GetAttribute("href");
						driver.GoToUrl(anchorHref);
						return;
					}
				}
			}
			catch (Exception exception)
			{
				throw exception;
			}
		}

		public static void GoToObjectInstance(this IWebDriver driver, int workspaceArtifactId, int integrationPointArtifactId, int artifactTypeId)
		{
			string integrationPointUrl = $"http://{SharedVariables.TargetHost}/Relativity/Case/Mask/View.aspx?AppID={workspaceArtifactId}&ArtifactID={integrationPointArtifactId}&ArtifactTypeID={artifactTypeId}";
			driver.Navigate().GoToUrl(integrationPointUrl);
		}

		public static bool PageShouldContain(this IWebDriver driver, string message)
		{
			return driver.PageSource.Contains(message);
		}

		public static void WaitUntilElementExists(this IWebDriver driver, ElementType elementType, string value, int timeSeconds)
		{
			WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeSeconds));
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

		public static void WaitUntilElementIsClickable(this IWebDriver driver, ElementType elementType, string value, int timeSeconds)
		{
			WaitUntilElementExists(driver, elementType, value, timeSeconds);
			WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeSeconds));
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

		public static void WaitUntilElementIsVisible(this IWebDriver driver, ElementType elementType, string value, int timeSeconds)
		{
			WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeSeconds));
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

		public static void WaitUntilIdExists(this IWebDriver driver, string id, int timeSeconds)
		{
			WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeSeconds));
			wait.Until(ExpectedConditions.ElementExists(By.Id(id)));
		}

		public static void CloseSeleniumBrowser(this IWebDriver driver)
		{
			try
			{
				driver.Quit();
			}
			catch (Exception)
			{
			}
		}

		public static void SelectFromDropdownList(this IWebDriver driver, string dropdownId, string value)
		{
			IWebElement dropDown = driver.FindElement(By.Id(dropdownId));
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