using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.BrandNew
{
	public class GeneralPanel : Component
	{
		public IWebElement Name => Parent.FindElementEx(By.Id("name"));

		public RadioField Type =>
			new RadioField(Parent.FindElementEx(By.CssSelector("#pointBody > div > div:nth-child(1) > div:nth-child(3)")), Driver);

		public SimpleSelectField Source => new SimpleSelectField(
			Parent.FindElementEx(
				By.CssSelector("#pointBody > div > div:nth-child(1) > div:nth-child(4) > div:nth-child(1) > div")), Driver);

		public SimpleSelectField Destination => new SimpleSelectField(Parent.FindElementEx(
			By.CssSelector("#pointBody > div > div:nth-child(1) > div:nth-child(4) > div:nth-child(2) > div:nth-child(1)")), Driver);

		public SimpleSelectField TransferredObject => new SimpleSelectField(Parent.FindElementEx(
			By.CssSelector("#pointBody > div > div:nth-child(1) > div:nth-child(4) > div:nth-child(2) > div:nth-child(2)")), Driver);

		public GeneralPanel(IWebElement parent, IWebDriver driver) : base(parent, driver)
		{
		}
	}
}