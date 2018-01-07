using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.Components
{
	public class Select
	{
		private readonly IWebDriver _driver;
        
		private readonly string _id;

		protected IWebElement SelectLink => _driver.FindElement(By.CssSelector($"#{_id} a"));
		protected IWebElement Dropdown => _driver.FindElement(By.Id("select2-drop"));
		protected IWebElement DropdownSearch => Dropdown.FindElement(By.TagName("input"));

		public Select(IWebDriver driver, string id)
		{
			_driver = driver;
			_id = id;
		}

		protected Select Toggle()
		{
			SelectLink.Click();
			return this;
		}

		protected bool IsOpen()
		{
			return _driver.FindElement(By.Id(_id)).GetCssValue("class").Contains("select2-dropdown-open");
		}

		protected Select EnsureOpen()
		{
			if (!IsOpen())
			{
				Toggle();
			}
			return this;
		}

		// read current value
		// read all values

		public Select Choose(string element)
		{
			EnsureOpen();
			DropdownSearch.SendKeys(element + Keys.Enter);
			return this;
		}
	}
}