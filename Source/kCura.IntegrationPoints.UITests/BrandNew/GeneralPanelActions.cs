using System.Threading;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Driver;
using kCura.Utility;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew
{
	public class GeneralPanelActions : ImportActions
	{
		public GeneralPanelActions(RemoteWebDriver driver, TestContext context) : base(driver, context)
		{
		}

		public void FillPanel(GeneralPanel panel, IntegrationPointGeneralModel model)
		{
			panel.Name.SetText(model.Name);
			panel.Type.Check(model.Type.GetDescription());
			Thread.Sleep(2000);
			panel.Source.SelectByText(model.SourceProvider);
			panel.TransferredObject.SelectByText(model.TransferredObject);
		}
	}
}
