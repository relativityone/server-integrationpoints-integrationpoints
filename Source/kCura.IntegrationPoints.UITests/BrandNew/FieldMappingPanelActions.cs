using kCura.IntegrationPoint.Tests.Core.Models.Import;
using kCura.IntegrationPoints.UITests.Configuration;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew
{
	public class FieldMappingPanelActions : ImportActions
	{
		public FieldMappingPanelActions(RemoteWebDriver driver, TestContext context) : base(driver, context)
		{
		}

		public void FillPanel(FieldMappingPanel panel, FieldsMappingModel model)
		{
			panel.MapFields(model.FieldsMapping);
		}
	}
}