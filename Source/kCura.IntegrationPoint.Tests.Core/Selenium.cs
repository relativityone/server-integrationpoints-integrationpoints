using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.ObjectModel;
using kCura.IntegrationPoint.Tests.Core.Models;
using OpenQA.Selenium.Chrome;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Selenium
	{
		private static bool _fluidEnabled;

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
			string workspaceUrl = $"http://{SharedVariables.TargetHost}/relativity/List.aspx?AppID={artifactId}&ArtifactID=1003663&ArtifactTypeID=10";
			driver.GoToUrl(workspaceUrl);			
		}

		public static void GoToTab(this IWebDriver driver, string tabName)
		{

			try
			{
				//If the workspace has too many tabs, some tabs get wrapped in the further navigation bar and thus their tab text is not visible.
				//It first tries grabbing the text of the tab. If it's found then the driver will click it.
				driver.WaitUntilElementExists(ElementType.Id, "horizontal-tabstrip", 10);
				ReadOnlyCollection<IWebElement> webElementCollection = driver.FindElements(By.Id("horizontal-tabstrip"));
				IWebElement navigationList = webElementCollection[0].FindElement(By.XPath("//ul[@class='nav navbar-nav']"));
				ReadOnlyCollection<IWebElement> listElements = navigationList.FindElements(By.TagName("li"));
				foreach (IWebElement listElement in listElements)
				{
					ReadOnlyCollection<IWebElement> anchorCollection = listElement.FindElements(By.TagName("a"));

					foreach (IWebElement anchor in anchorCollection)
					{
						if (anchor.Text.Equals(tabName))
						{
							anchor.Click();
							return;
						}
					}
				}

				//If the text is not found, it then tries going through the vertical bar and grabs the URL of the tab.
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
				throw new Exception($"Unable to find tab {tabName}", exception);
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

		public static void SetFluidStatus(this IWebDriver driver, int userArtifactId)
		{
			UserModel user = User.ReadUser(userArtifactId);
			_fluidEnabled = user.BetaUser;
		}

		//TODO Make ReadUser(email) returns correct infor by userJObject.ToObject<UserModel>();
		//Currently userJObject.ToObject<UserModel>() doesn't get anything even though userJObject = JObject.Parse(response) has correct info.
		public static void SetFluidStatus(this IWebDriver driver, string email)
		{
			UserModel user = User.ReadUser(email);
			_fluidEnabled = user.BetaUser;
		}

		public static void ClickNewIntegrationPoint(this IWebDriver driver)
		{
			string templateFrame = "ListTemplateFrame";
			string externalPage = "_externalPage";
			string newIntegraionPoint;
			driver.SwitchTo().DefaultContent();

			if (_fluidEnabled)
			{
				driver.WaitUntilElementExists(ElementType.Id, externalPage, 15);
				driver.SwitchTo().Frame(externalPage);
				newIntegraionPoint = "//button[@title='New Integration Point']";
			}
			else
			{
				driver.WaitUntilElementExists(ElementType.Id, templateFrame, 15);
				driver.SwitchTo().Frame(templateFrame);
				newIntegraionPoint = "//a[@title='New Integration Point']";
			}
			
			driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(15);
			driver.FindElement(By.XPath(newIntegraionPoint)).Click();
			driver.WaitUntilElementExists(ElementType.Id, externalPage, 5);
			driver.SwitchTo().Frame(externalPage);
		}

		public static IWebDriver GetWebDriver(TestBrowser browser)
		{
			switch (browser)
			{
				case TestBrowser.Chrome:
					return new ChromeDriver();
				case TestBrowser.InternetExplorer:
					return null;
				case TestBrowser.Firefox:
					return null;
			}
			return null;
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

	public enum TestBrowser
	{
		Chrome,
		InternetExplorer,
		Firefox,
		Safari
	}
}