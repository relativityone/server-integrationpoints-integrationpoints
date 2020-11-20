using System;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.Components
{
	public class RadioField : Component
	{
		public RadioField(IWebElement parent, IWebDriver driver) : base(parent, driver)
		{
		}

		public RadioField Check(string label)
		{
			try
			{
				CheckRadioWithTextInLabelElement(label);
			}
			catch (WebDriverTimeoutException)
			{
				CheckRadioWithTextInInputElement(label);
			}

			return this;
		}

		private void CheckRadioWithTextInInputElement(string label)
		{
			IWebElement radioInput =
				Parent.FindElementEx(By.XPath($".//text()[contains(., '{label}')]/preceding-sibling::input[1]"));
			radioInput.ClickEx(Driver);
		}

		private void CheckRadioWithTextInLabelElement(string label)
		{
			IWebElement radioRow = Driver.FindElementEx(By.XPath($"//label[contains(text(), '{label}')]"));
			radioRow.FindElementEx(By.XPath("./..//input"), timeout: TimeSpan.FromSeconds(3))
				.ClickEx(Driver);
		}

		public RadioField Check(bool yesNo)
		{
			return Check(yesNo ? "Yes" : "No");
		}
	}
}