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
			panel.Host.SetTextEx(model.Host, Driver);
			panel.Protocol.SelectByTextEx(model.Protocol.GetDescription(), Driver);
			panel.Port.SetTextEx(model.Port.ToString(), Driver);
			panel.Username.SetTextEx(model.Username, Driver);
			panel.Password.SetTextEx(model.Password, Driver);
			panel.CsvFilepathInput.SetTextEx(model.CsvFilepath, Driver);
		}
	}
}