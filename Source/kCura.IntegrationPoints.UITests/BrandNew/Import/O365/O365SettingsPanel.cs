using kCura.IntegrationPoints.UITests.Components;
using kCura.IntegrationPoints.UITests.Driver;
using OpenQA.Selenium;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.O365
{
	public class O365SettingsPanel : Component
	{
		protected IWebElement FileNameTreeParent =>
			Parent.FindElementEx(By.XPath("//*[@id='location-select']/.."));

		protected TreeSelect FileNameTreeSelect => new TreeSelect(FileNameTreeParent,
			"location-select", "jstree-holder-div", Driver);

		public O365SettingsPanel(IWebElement parent, IWebDriver driver) : base(parent, driver)
		{
		}

		public string FileName
		{
			set
			{
				FileNameTreeSelect.ChooseChildElement(value);
			}
		}
	}
}