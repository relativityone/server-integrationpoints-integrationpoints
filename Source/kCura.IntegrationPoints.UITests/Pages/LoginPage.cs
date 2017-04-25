using IntegrationPointsUITests.Common;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace IntegrationPointsUITests.Pages
{

    public class LoginPage
    {
        private readonly IWebDriver _driver;

        [FindsBy(How = How.Id, Using = "_email")]
        private IWebElement _username=null;

        [FindsBy(How = How.Id, Using = "continue")]
        private IWebElement _continueButton = null;

		[FindsBy(How = How.Id, Using = "_password__password_TextBox")]
        private IWebElement _password = null;

		[FindsBy(How = How.Id, Using = "_login")]
        private IWebElement _loginButton = null;

		public LoginPage(IWebDriver driver)
        {
            _driver = driver;
            PageFactory.InitElements(driver, this);
            ValidatePage();
        }

        public LoginPage ValidatePage()
        {
            if (!(_username.Displayed || _password.Displayed))
            {
                throw new PageException("Can't find required elements.");
            }
            return this;
        }

        public GeneralPage Login(string username, string password)
        {
            _username.SendKeys(username);
            _continueButton.Click();
            _password.SendKeys(password);
            _loginButton.Click();
            return new GeneralPage(_driver);
        }


    }
}
