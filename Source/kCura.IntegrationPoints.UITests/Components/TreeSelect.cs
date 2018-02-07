using System;
using System.Threading;
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
			IWebElement select = Parent.FindElement(By.XPath(@".//div[@id='location-select']"));
			Thread.Sleep(TimeSpan.FromMilliseconds(200));
			select.Click();
			return this;
		}

		public TreeSelect ChooseRootElement()
		{
			Expand();

			IWebElement selectListPopup = Parent.FindElement(By.XPath(@".//div[@id='jstree-holder-div']"));
			Thread.Sleep(TimeSpan.FromMilliseconds(1000));
			IWebElement rootElement = selectListPopup.FindElements(By.XPath(@".//a"))[0];
			Thread.Sleep(TimeSpan.FromMilliseconds(1000));
			rootElement.Click();
			return this;
		}

	}
}