using kCura.IntegrationPoint.Tests.Core.Models.Import.LoadFile.Documents;
using kCura.IntegrationPoints.UITests.Configuration;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.LoadFile.Documents
{
	public class FileEncodingPanelActions : ImportActions
	{
		public FileEncodingPanelActions(RemoteWebDriver driver, TestContext context) : base(driver, context)
		{
		}

		public void FillPanel(FileEncodingPanel panel, FileEncodingModel model)
		{
			panel.FileEncoding = model.FileEncoding;
			panel.Column = model.Column;
			panel.Quote = model.Quote;
			panel.Newline = model.Newline;
			panel.MultiValue = model.MultiValue;
			panel.NestedValue = model.NestedValue;
		}
	}
}