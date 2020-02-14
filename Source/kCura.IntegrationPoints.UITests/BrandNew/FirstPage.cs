using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew
{
	public class FirstPage : CreateIntegrationPointPage
	{
		public GeneralPanel General { get; }

		public FirstPage(RemoteWebDriver driver) : base(driver)
		{
			Driver.SwitchTo().DefaultContent()
				.SwitchTo().Frame(_mainFrameNameOldUi);
			WaitForPage();

			General = new GeneralPanel(Driver.FindElementByCssSelector("#pointBody > div > div:nth-child(1)"));
		}
	}
}