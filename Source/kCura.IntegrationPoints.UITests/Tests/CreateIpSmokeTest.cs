using System;
using IntegrationPointsUITests.Common;
using IntegrationPointsUITests.Components;
using IntegrationPointsUITests.Pages;
using NUnit.Framework;

namespace IntegrationPointsUITests.Tests
{
    [TestFixture]
    [Category(TestCategory.SMOKE)]
    public class CreateIpSmokeTest : UiTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Console.WriteLine("Setup");
            EnsureGeneralPageIsOpened();
        }
        
        [Test, Order(10)]
        public void CreateIp()
        {
            Console.WriteLine("testooo");
            // GIVEN
            var generalPage = new GeneralPage(Driver);
            generalPage.ChooseWorkspace("Smoke Workspace");

            // WHEN
            IntegrationPointsPage ipPage = generalPage.GoToIntegrationPointsPage();
            ExportFirstPage first = ipPage.CreateNewIntegrationPoint();
            first.Name = "Test IP";
            first.Destination = "Load File";

            ExportToFileSecondPage second = first.GoToNextPage();
            //second.SavedSearch = "All Documents";
            second.SelectAllDocuments();

            ExportToFileThirdPage third = second.GoToNextPage();
            IntegrationPointDetailsPage detailsPage = third.SaveIntegrationPoint();
            PropertiesTable generalProperties = detailsPage.SelectGeneralPropertiesTable();

            // THEN
            Assert.AreEqual("Relativity (.dat); Unicode", generalProperties.Properties["Load file format:"]);
        }

    }
}
