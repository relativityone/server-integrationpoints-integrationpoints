using kCura.IntegrationPoints.UITests.Common;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SeleniumExtras.PageObjects;

namespace kCura.IntegrationPoints.UITests.Pages
{

    public class LoginPage : Page
    {
        protected IWebElement Username => Driver.FindElementEx(By.Id("_email"));

        protected IWebElement ContinueButton => Driver.FindElementEx(By.Id("continue"));

        protected IWebElement Password => Driver.FindElementEx(By.Id("_password__password_TextBox"));

        protected IWebElement LoginButton => Driver.FindElementEx(By.Id("_login"));

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
            Username.SetTextEx(username, Driver);
            ContinueButton.ClickEx(Driver);
            Password.SetTextEx(password, Driver);
            LoginButton.ClickEx(Driver);
            return new GeneralPage(Driver);
        }

    }
}
