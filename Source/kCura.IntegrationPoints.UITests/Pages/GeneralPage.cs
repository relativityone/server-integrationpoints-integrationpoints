using IntegrationPointsUITests.Common;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace IntegrationPointsUITests.Pages
{
    public class GeneralPage
    {

        private readonly IWebDriver _driver;

        [FindsBy(How = How.ClassName, Using = "headerUpperRow")]
        private IWebElement _header;

        public GeneralPage(IWebDriver driver)
        {
            _driver = driver;
            PageFactory.InitElements(driver, this);
            ValidatePage();
        }

        public GeneralPage ValidatePage()
        {
            if (!_header.Displayed) {
                throw new PageException("Can't find required elements.");
            }
            return this;
        }
    }
}
