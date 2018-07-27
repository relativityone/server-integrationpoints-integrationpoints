using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.PageObjects;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

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

		public GeneralPage PassWelcomeScreen()
		{
			Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(7);
			try
			{
				ReadOnlyCollection<IWebElement> buttons = Driver.FindElements(By.Id("_continue_button"));
				if (buttons.Any())
				{
					buttons[0].ClickWhenClickable();
				}
			}
			finally
			{
				Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(SharedVariables.UiImplicitWaitInSec);
			}
			return this;
		}

		public LoginPage LogOut()
		{
			UserDropdownMenu.Click();
			IWebElement logOutLink = Driver.FindElement(By.LinkText("Logout"));
			logOutLink.ClickWhenClickable();
			return new LoginPage(Driver);
		}

		public GeneralPage ChooseWorkspace(string name)
		{
			Driver.SwitchTo().DefaultContent();
			IWebElement workspaceLink = GetWorkspaceLink(name);
			workspaceLink.ClickWhenClickable();
			AcceptLeavingPage();
			return this;
		}

		private void AcceptLeavingPage()
		{
			IAlert alert = ExpectedConditions.AlertIsPresent().Invoke(Driver);
			alert?.Accept();
		}

		private IWebElement GetWorkspaceLink(string workspaceName)
		{
			WaitForPage();
			IWebElement quickSearchTextBox = Driver.FindElementById("qnTextBox");
			quickSearchTextBox.SendKeys(Keys.Control + "a");
			Thread.Sleep(500);
			quickSearchTextBox.SetText(workspaceName);
			IWebElement workspaceLink = Driver.FindElement(By.LinkText(workspaceName));
			WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
			wait.Until(ExpectedConditions.ElementToBeClickable(workspaceLink));
			return workspaceLink;
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
