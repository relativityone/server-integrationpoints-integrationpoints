using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Driver;
using kCura.IntegrationPoints.UITests.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew
{
	public class CreateIntegrationPointPage : GeneralPage
	{
		public WizardPanel Wizard
		{
			get
			{
				Driver.SwitchTo().DefaultContent()
					.SwitchToFrameEx(_mainFrameNameOldUi);
				return new WizardPanel(Driver.FindElementEx(By.Id("progressButtons")), Driver);
			}
		}

		public CreateIntegrationPointPage(RemoteWebDriver driver) : base(driver)
		{
		}
	}
}