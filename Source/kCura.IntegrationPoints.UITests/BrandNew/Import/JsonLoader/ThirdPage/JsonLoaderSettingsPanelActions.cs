using kCura.IntegrationPoint.Tests.Core.Models.Import.JsonLoader;
using kCura.IntegrationPoint.Tests.Core.Models.Shared;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.Utility;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.JsonLoader.ThirdPage
{
    public class JsonLoaderSettingsPanelActions : ImportActions
    {
        public  JsonLoaderSettingsPanelActions(RemoteWebDriver driver, TestContext context) : base(driver, context)
        {
        }
        public void FillPanel(JsonLoaderSettingsPanel panel, ImportDocumentsFromJsonLoaderModel model)
        {
            panel.Overwrite.SelectByText(model.Settings.Overwrite.GetDescription());
            if (model.Settings.Overwrite != OverwriteType.AppendOnly)
            {
                panel.MultiSelectFieldOverlayBehavior.SelectByText(model.Settings.MultiSelectFieldOverlayBehavior.GetDescription());
            }

            panel.UniqueIdentifier.SelectByText(model.JsonLoaderSettings.UniqueIdentifier);
        }
	}
}
