using kCura.IntegrationPoint.Tests.Core.Models.Import.FTP;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Driver;
using kCura.Utility;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.FTP
{
	public class ConnectionAndFileInfoPanelActions : ImportActions
	{
		public ConnectionAndFileInfoPanelActions(RemoteWebDriver driver, TestContext context) : base(driver, context)
		{
		}

		public void FillPanel(ConnectionAndFileInfoPanel panel, ConnectionAndFileInfoModel model)
		{
			panel.Host.SetText(model.Host);
			panel.Protocol.SelectByText(model.Protocol.GetDescription());
			panel.Port.SetText(model.Port.ToString());
			panel.Username.SetText(model.Username);
			panel.Password.SetText(model.Password);
			panel.CsvFilepathInput.SetText(model.CsvFilepath);
		}
	}
}