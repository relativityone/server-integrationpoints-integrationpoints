using kCura.IntegrationPoints.UITests.Components;
using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.BrandNew
{
	public class GeneralPanel : Component
	{
		public IWebElement Name => Parent.FindElement(By.Id("name"));

		public RadioField Type =>
			new RadioField(Parent.FindElement(By.CssSelector("#pointBody > div > div:nth-child(1) > div:nth-child(3)")));

		public SimpleSelectField Source => new SimpleSelectField(
			Parent.FindElement(
				By.CssSelector("#pointBody > div > div:nth-child(1) > div:nth-child(4) > div:nth-child(1) > div")));

		public SimpleSelectField Destination => new SimpleSelectField(Parent.FindElement(
			By.CssSelector("#pointBody > div > div:nth-child(1) > div:nth-child(4) > div:nth-child(2) > div:nth-child(1)")));

		public SimpleSelectField TransferredObject => new SimpleSelectField(Parent.FindElement(
			By.CssSelector("#pointBody > div > div:nth-child(1) > div:nth-child(4) > div:nth-child(2) > div:nth-child(2)")));

		public GeneralPanel(IWebElement parent) : base(parent)
		{
		}
	}
}