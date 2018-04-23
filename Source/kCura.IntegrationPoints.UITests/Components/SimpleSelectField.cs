using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.Components
{
	public class SimpleSelectField : Component
	{
		public SelectElement Select => new SelectElement(Parent.FindElement(By.TagName("select")));

		public SimpleSelectField(IWebElement parent) : base(parent)
		{
		}

		public SimpleSelectField SelectByText(string text)
		{
			Select.SelectByText(text);
			return this;
		}
	}
}