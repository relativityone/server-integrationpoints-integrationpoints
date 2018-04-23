using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Pages;
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
					.SwitchTo().Frame("externalPage");
				return new WizardPanel(Driver.FindElementById("progressButtons"));
			}
		}

		public CreateIntegrationPointPage(RemoteWebDriver driver) : base(driver)
		{
		}
	}
}