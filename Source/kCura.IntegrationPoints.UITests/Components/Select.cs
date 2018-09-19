using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.Components
{
	public class Select : Component
	{
		protected IWebElement SelectLink => Parent.FindElement(By.CssSelector("a.select2-choice"));
		protected IWebElement Dropdown => Parent.FindElement(By.XPath("/*")).FindElement(By.Id("select2-drop"));
		protected IWebElement DropdownSearch => Dropdown.FindElement(By.TagName("input"));

		public string Value => Parent.FindElement(By.ClassName("select2-chosen")).Text;

		public Select(IWebElement parent) : base(parent)
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
			SelectLink.ClickEx();
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
