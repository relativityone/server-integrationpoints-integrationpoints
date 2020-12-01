using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.Components
{
	public class SimpleSelectField : Component
	{
		public SelectElement Select => new SelectElement(Parent.FindElementEx(By.TagName("select")));

		public SimpleSelectField(IWebElement parent, IWebDriver driver) : base(parent, driver)
		{
		}

		public SimpleSelectField SelectByText(string text)
		{
			Select.SelectByTextEx(text, Driver);
			return this;
		}
	}
}