using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using kCura.IntegrationPoint.Tests.Core;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class GeneralPage : Page
	{

		// TODO Move to some "SthBar", "Navigator" or something similar
		[FindsBy(How = How.Id, Using = "GetNavigateHomeScript")]
		protected IWebElement NavigateHome;

		[FindsBy(How = How.ClassName, Using = "headerUpperRow")]
		protected IWebElement Header;

		[FindsBy(How = How.Id, Using = "horizontal-tabstrip")]
		protected IWebElement MainMenu;

		[FindsBy(How = How.ClassName, Using = "quickNavIcon")]
		protected IWebElement QuickNavigation;

		[FindsBy(How = How.CssSelector, Using = ".quickNavTextbox")]
		protected IWebElement QuickNavigationInput;

		[FindsBy(How = How.XPath, Using =
				"//div[@class='quickNavOuterContainer']/ul/li[@class='quickNavResultItem ui-menu-item']/a[@title='Integration Points']")]
		protected IWebElement QuickNavigationResult;

		[FindsBy(How = How.CssSelector, Using = "span[title = 'User Dropdown Menu']")]
		protected IWebElement UserDropdownMenu;

		public GeneralPage(RemoteWebDriver driver) : base(driver)
		{
			PageFactory.InitElements(driver, this);
		}

		public LoginPage LogOut()
		{
			UserDropdownMenu.Click();
			IWebElement logOutLink = Driver.FindElement(By.LinkText("Logout"));
			logOutLink.Click();
			return new LoginPage(Driver);
		}

		public GeneralPage ChooseWorkspace(string name)
		{
			NavigateHome.Click();
			MainMenu.FindElement(By.LinkText("Workspaces")).Click();
			IWebElement workspaceLink = GetWorkspaceLink(name);

			//IWebElement viewMenu = Driver.SwitchTo().Frame("externalPage").FindElement(By.Id("viewMenu"));
			//var viewMenuSelect = new SelectElement(viewMenu);
			//viewMenuSelect.SelectByText("All Case Templates");

			workspaceLink.Click();
			return this;
		}

		private IWebElement GetWorkspaceLink(string workspaceName)
		{
			IWebDriver externalPage = Driver.SwitchTo().Frame("externalPage");
			IWebElement nameSearchFilter = FindFilterTextboxInColumn("Name", externalPage);
			nameSearchFilter.SendKeys(Keys.Control + "a");
			Thread.Sleep(500);
			nameSearchFilter.SendKeys(workspaceName);
			nameSearchFilter.SendKeys(Keys.Return);
			WaitForPage();
			IWebElement workspaceLink = Driver.FindElement(By.LinkText(workspaceName));
			return workspaceLink;
		}

		private IWebElement FindFilterTextboxInColumn(string title, IWebDriver driver)
		{
			// We assume that column exists in current view
			IWebElement table = driver.FindElement(By.ClassName("ui-jqgrid-htable"));
			ReadOnlyCollection<IWebElement> rows = table.FindElements(By.TagName("tr"));

			int columnIndex = FindColumnIndex(title, rows[0]);
			if (columnIndex >= 0)
			{
				ReadOnlyCollection<IWebElement> thElements = rows[1].FindElements(By.TagName("th"));
				IWebElement input = thElements[columnIndex].FindElement(By.TagName("input"));
				return input;
			}
			else
			{
				throw new TestException($"Could not find column '{title}' in Workspaces tab.");
			}
		}

		private int FindColumnIndex(string columnTitle, IWebElement webElement)
		{
			return webElement
				.FindElements(By.TagName("th"))
				.ToList()
				.FindIndex(x => x.GetAttribute("title").Equals(columnTitle));
		}

		public IntegrationPointsPage GoToIntegrationPointsPage()
		{
			WaitForPage();
			QuickNavigation.Click();
			QuickNavigationInput.SendKeys("Integration Points" + Keys.Enter);
			Sleep(300);
			QuickNavigationInput.SendKeys(Keys.Enter);
			//QuickNavigationResult.Click();
			return new IntegrationPointsPage(Driver);
		}

	}
}
