using kCura.IntegrationPoint.Tests.Core.Models.Import.MyFirstProvider;
using kCura.IntegrationPoints.UITests.BrandNew.Import.LoadFile;
using kCura.IntegrationPoints.UITests.Configuration;
using kCura.IntegrationPoints.UITests.Pages;
using OpenQA.Selenium.Remote;

namespace kCura.IntegrationPoints.UITests.BrandNew.Import.MyFirstProvider
{
    public class ImportDocumentsFromMyFirstProviderActions : ImportActions
    {
        private readonly ImportDocumentsFromMyFirstProviderModel _model;
        
        public ImportDocumentsFromMyFirstProviderActions(RemoteWebDriver driver, TestContext context, ImportDocumentsFromMyFirstProviderModel model) : base(driver, context)
        {
            _model = model;
        }

        public void Setup()
        {
            new GeneralPage(Driver)
                .PassWelcomeScreen()
                .ChooseWorkspace(Context.WorkspaceName)
                .GoToIntegrationPointsPage();
            new IntegrationPointsPage(Driver).CreateNewIntegrationPoint();

            var firstPage = new FirstPage(Driver);
            new GeneralPanelActions(Driver, Context).FillPanel(firstPage.General, _model.General);
            firstPage.Wizard.GoNext();

            var secondPage = new MyFirstProviderSecondPage(Driver);
            new MyFirstProviderConfigurationPanelActions(Driver, Context).FillPanel(secondPage.MyFirstProviderConfigurationPanel, _model.MyFirstProviderSettings);
            secondPage.Wizard.GoNext();

            var thirdPage = new ThirdPage(Driver);
            new FieldMappingPanelActions(Driver, Context).FillPanel(thirdPage.FieldMapping, _model.FieldsMapping);
            new SettingsPanelActions(Driver, Context).FillPanel(thirdPage.Settings, _model.Settings);
            thirdPage.Wizard.Save();
        }
	}
}
