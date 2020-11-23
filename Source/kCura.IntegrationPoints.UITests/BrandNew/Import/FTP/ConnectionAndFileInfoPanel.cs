using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.FTP
{
	public class ConnectionAndFileInfoPanel : Component
	{
		public IWebElement Host => Parent.FindElementEx(By.Id("host"));

		public SelectElement Protocol => new SelectElement(Parent.FindElementEx(By.Id("protocol")));

		public IWebElement Port => Parent.FindElementEx(By.Id("port"));

		public IWebElement Username => Parent.FindElementEx(By.Id("username"));

		public IWebElement Password => Parent.FindElementEx(By.Id("password"));

		public IWebElement CsvFilepathInput => Parent.FindElementEx(By.Id("filename_prefix"));

		public ConnectionAndFileInfoPanel(IWebElement parent, IWebDriver driver) : base(parent, driver)
		{
		}
	}
}