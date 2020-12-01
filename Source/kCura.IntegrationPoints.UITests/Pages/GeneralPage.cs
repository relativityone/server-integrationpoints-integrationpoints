using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using kCura.EDDS.WebAPI.DocumentManagerBase;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using Polly;
using SeleniumExtras.PageObjects;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace kCura.IntegrationPoints.UITests.Pages
{
	public class GeneralPage : Page
	{
		internal string _mainFrameNameNewUi = "ListPage";
		internal string _mainFrameNameOldUi = "externalPage";

		// TODO Move to some "SthBar", "Navigator" or something similar
		protected IWebElement NavigateHome => Driver.FindElementEx(By.Id("GetNavigateHomeScript"));

		protected IWebElement Header => Driver.FindElementEx(By.ClassName("headerUpperRow"));

		protected IWebElement MainMenu => Driver.FindElementEx(By.Id("horizontal-tabstrip"));

		protected IWebElement QuickNavigationInput => Driver.FindElementEx(By.Id("qnTextBox"));

		protected IWebElement UserDropdownMenu => Driver.FindElementEx(By.CssSelector("span[title = 'User Dropdown Menu']"));

		public GeneralPage(RemoteWebDriver driver) : base(driver)
		{
			PageFactory.InitElements(driver, this);
		}

		public GeneralPage PassWelcomeScreen()
		{
			Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(7);
			try
			{
				ReadOnlyCollection<IWebElement> buttons = Driver.FindElementsEx(By.Id("_continue_button"));
				if (buttons.Any())
				{
					buttons[0].ClickEx(Driver);
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
			UserDropdownMenu.ClickEx(Driver);
			IWebElement logOutLink = Driver.FindElementEx(By.LinkText("Logout"));
			logOutLink.ClickEx(Driver);
			return new LoginPage(Driver);
		}

		public GeneralPage ChooseWorkspace(string name)
		{
			if (Driver.Title.Contains(name))
			{
				return this;
			}
			Driver.SwitchTo().DefaultContent();
			GoToPage(name);
			AcceptLeavingPage();
			return this;
		}

		private void AcceptLeavingPage()
		{
			IAlert alert = ExpectedConditions.AlertIsPresent().Invoke(Driver);
			alert?.Accept();
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

		public void GoToPage(string pageName)
		{
			Driver.GoToPage(pageName);
		}
	}
}