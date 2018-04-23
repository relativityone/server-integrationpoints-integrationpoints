using kCura.IntegrationPoints.UITests.Components;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.FTP
{
	public class ConnectionAndFileInfoPanel : Component
	{
		public IWebElement Host => Parent.FindElement(By.Id("host"));

		public SelectElement Protocol => new SelectElement(Parent.FindElement(By.Id("protocol")));

		public IWebElement Port => Parent.FindElement(By.Id("port"));

		public IWebElement Username => Parent.FindElement(By.Id("username"));

		public IWebElement Password => Parent.FindElement(By.Id("password"));

		public IWebElement CsvFilepathInput => Parent.FindElement(By.Id("filename_prefix"));

		public ConnectionAndFileInfoPanel(IWebElement parent) : base(parent)
		{
		}
	}
}