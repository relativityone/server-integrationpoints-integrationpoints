using kCura.IntegrationPoints.UITests.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.Components
{
	public class TreeSelect : Page
	{
		private readonly ISearchContext _parent;

		public TreeSelect(RemoteWebDriver driver, ISearchContext parent = null) : base(driver)
		{
			_parent = parent ?? driver;
		}

		public TreeSelect Expand()
		{
			IWebElement select = _parent.FindElement(By.XPath(@"//div[@id='location-select']"));
			select.Click();
			return this;
		}

		public TreeSelect ChooseRootElement()
		{
			Expand();

			IWebElement selectListPopup = _parent.FindElement(By.XPath(@"//div[@id='jstree-holder-div']"));
			IWebElement rootElement = selectListPopup.FindElements(By.XPath(@"//a"))[1];
			rootElement.Click();
			return this;
		}

	}
}