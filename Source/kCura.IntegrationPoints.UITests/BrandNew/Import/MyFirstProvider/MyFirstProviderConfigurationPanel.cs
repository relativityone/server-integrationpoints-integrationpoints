using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.MyFirstProvider
{
	public class MyFirstProviderConfigurationPanel : Component
	{
		public IWebElement FileLocationInput =>
			Parent.FindElementEx(By.Id("fileLocation"));
		
		public MyFirstProviderConfigurationPanel(IWebElement parent, IWebDriver driver) : base(parent, driver)
		{
		}
	}
}