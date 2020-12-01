using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.Components
{
	public class Select : Component
	{
		protected IWebElement SelectLink => Parent.FindElementEx(By.CssSelector("a.select2-choice"));
		protected IWebElement Dropdown => Parent.FindElementEx(By.XPath("/*")).FindElementEx(By.Id("select2-drop"));
		protected IWebElement DropdownSearch => Dropdown.FindElementEx(By.TagName("input"));

		public string Value => Parent.FindElementEx(By.ClassName("select2-chosen")).Text;

		public Select(IWebElement parent, IWebDriver driver) : base(parent, driver)
		{
		}

		public Select Choose(string element)
		{
			EnsureOpen();
			DropdownSearch.SendKeys(element + Keys.Enter);
			return this;
		}

		protected Select Toggle()
		{
			SelectLink.ClickEx(Driver);
			return this;
		}

		protected bool IsOpen()
		{
			return Parent.GetCssValue("class").Contains("select2-dropdown-open");
		}

		protected Select EnsureOpen()
		{
			if (!IsOpen())
			{
				Toggle();
			}
			return this;
		}

	}
}
