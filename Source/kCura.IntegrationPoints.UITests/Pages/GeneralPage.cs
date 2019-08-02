﻿using System;
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
					buttons[0].ClickEx();
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
			UserDropdownMenu.ClickEx();
			IWebElement logOutLink = Driver.FindElement(By.LinkText("Logout"));
			logOutLink.ClickEx();
			return new LoginPage(Driver);
		}

		public GeneralPage ChooseWorkspace(string name)
		{
			Driver.SwitchTo().DefaultContent();
			IWebElement workspaceLink = GetWorkspaceLink(name);
			workspaceLink.ClickEx();
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
			GoToPage("Integration Points");
			return new IntegrationPointsPage(Driver);
		}

		public IntegrationPointProfilePage GoToIntegrationPointProfilePage()
		{
			GoToPage("Integration Point Profile");
			return new IntegrationPointProfilePage(Driver);
		}

		private void GoToPage(string pageName)
		{
			WaitForPage();
			QuickNavigation.ClickEx();
			QuickNavigationInput.SendKeys(pageName);
			Sleep(300);
			string resultLinkXPath =
				$"//div[@class='quickNavOuterContainer']/ul/li[@class='quickNavResultItem ui-menu-item']/a[@title='{pageName}']";
			IWebElement resultLink = Driver.FindElementByXPath(resultLinkXPath);
			resultLink.ClickEx();
		}
	}
}