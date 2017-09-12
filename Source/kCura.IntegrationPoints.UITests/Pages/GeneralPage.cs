using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace IntegrationPointsUITests.Pages
{
    public class GeneralPage : Page
    {
        
        // TODO Move to sam "SthBar", "Navigator" or something similar
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

        public GeneralPage(IWebDriver driver) : base(driver)
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
	        IWebElement viewMenu = Driver.SwitchTo().Frame("externalPage").FindElement(By.Id("viewMenu"));
			var viewMenuSelect = new SelectElement(viewMenu);
			viewMenuSelect.SelectByText("All Case Templates");

	        IWebElement element = Driver.FindElement(By.LinkText(name));
            element.Click();
            return this;
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
