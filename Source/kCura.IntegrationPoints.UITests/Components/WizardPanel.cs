using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.Components
{
	public class WizardPanel : Component
	{
		protected IWebElement BackButton => Parent.FindElement(By.Id("back"));

		protected IWebElement NextButton => Parent.FindElement(By.Id("next"));

		protected IWebElement SaveButton => Parent.FindElement(By.Id("save"));

		public WizardPanel(IWebElement parent) : base(parent)
		{
		}
		
		public void GoBack() => BackButton.ClickWhenClickable();

		public void GoNext() => NextButton.ClickWhenClickable();

		public void Save() => SaveButton.ClickWhenClickable();
	}
}