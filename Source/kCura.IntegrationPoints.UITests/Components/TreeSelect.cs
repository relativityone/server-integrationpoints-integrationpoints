using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.Components
{
	public class TreeSelect : Component
	{
		public TreeSelect(IWebElement parent) : base(parent)
		{
		}

		public TreeSelect Expand()
		{
			IWebElement select = Parent.FindElement(By.XPath(@"//div[@id='location-select']"));
			select.Click();
			return this;
		}

		public TreeSelect ChooseRootElement()
		{
			Expand();

			IWebElement selectListPopup = Parent.FindElement(By.XPath(@"//div[@id='jstree-holder-div']"));
			IWebElement rootElement = selectListPopup.FindElements(By.XPath(@"//a"))[1];
			rootElement.Click();
			return this;
		}

	}
}