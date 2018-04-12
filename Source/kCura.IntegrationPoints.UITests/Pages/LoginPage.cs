using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages
{

    public class LoginPage : Page
    {
        
        [FindsBy(How = How.Id, Using = "_email")]
        protected IWebElement Username;

        [FindsBy(How = How.Id, Using = "continue")]
        protected IWebElement ContinueButton;

        [FindsBy(How = How.Id, Using = "_password__password_TextBox")]
        protected IWebElement Password;

        [FindsBy(How = How.Id, Using = "_login")]
        protected IWebElement LoginButton;

        public LoginPage(RemoteWebDriver driver) : base(driver)
        {
            PageFactory.InitElements(driver, this);
        }

        public bool IsOnLoginPage()
        {
            return Username.Displayed || Password.Displayed;
        }

        public LoginPage ValidatePage()
        {
            if (!IsOnLoginPage())
            {
                throw new PageException("Can't find required elements.");
            }
            return this;
        }

        public GeneralPage Login(string username, string password)
        {
            Username.SetText(username);
            ContinueButton.ClickWhenClickable();
            Password.SetText(password);
            LoginButton.ClickWhenClickable();
            return new GeneralPage(Driver);
        }

    }
}
