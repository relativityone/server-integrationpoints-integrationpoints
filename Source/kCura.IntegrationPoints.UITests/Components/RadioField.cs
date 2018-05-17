using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.Components
{
	public class RadioField : Component
	{
		public RadioField(IWebElement parent) : base(parent)
		{
		}

		public RadioField Check(string label)
		{
			try
			{
				CheckRadioWithTextInLabelElement(label);
			}
			catch (NoSuchElementException)
			{
				CheckRadioWithTextInInputElement(label);
			}

			return this;
		}

		private void CheckRadioWithTextInInputElement(string label)
		{
			IWebElement radioInput =
				Parent.FindElement(By.XPath($".//text()[contains(., '{label}')]/preceding-sibling::input[1]"));
			radioInput.ClickWhenClickable();
		}

		private void CheckRadioWithTextInLabelElement(string label)
		{
			IWebElement radioRow = Parent.FindElement(By.XPath($".//label[contains(text(), '{label}')]"));
			radioRow.FindElement(By.XPath("./..//input"))
				.ClickWhenClickable();
		}

		public RadioField Check(bool yesNo)
		{
			return Check(yesNo ? "Yes" : "No");
		}
	}
}