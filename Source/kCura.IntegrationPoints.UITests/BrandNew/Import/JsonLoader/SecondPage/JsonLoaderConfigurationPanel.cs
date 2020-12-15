using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.JsonLoader.SecondPage
{
	public class JsonLoaderConfigurationPanel : Component
	{
		public IWebElement FieldLocationInput =>
			Parent.FindElementEx(By.XPath("//body/div[@id='jsonConfiguration']/div[1]/div[2]/input[1]"));

		public IWebElement DataLocationInput =>
			Parent.FindElementEx(By.XPath("//body/div[@id='jsonConfiguration']/div[2]/div[2]/input[1]"));
		
		public JsonLoaderConfigurationPanel(IWebElement parent, IWebDriver driver) : base(parent, driver)
		{
		}
	}
}