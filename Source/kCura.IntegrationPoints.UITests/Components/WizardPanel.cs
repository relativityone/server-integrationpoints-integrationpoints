using System.Threading;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.Components
{
	public class WizardPanel : Component
	{
		protected IWebElement BackButton => Parent.FindElementEx(By.Id("back"));

		protected IWebElement NextButton => Parent.FindElementEx(By.Id("next"));

		protected IWebElement SaveButton => Parent.FindElementEx(By.Id("save"));

		public WizardPanel(IWebElement parent, IWebDriver driver) : base(parent, driver)
		{
		}
		
		public void GoBack() => BackButton.ClickEx(Driver);

		public void GoNext()
		{
			NextButton.ClickEx(Driver);
			Thread.Sleep(500);
		}

		public void Save() => SaveButton.ClickEx(Driver);
	}
}