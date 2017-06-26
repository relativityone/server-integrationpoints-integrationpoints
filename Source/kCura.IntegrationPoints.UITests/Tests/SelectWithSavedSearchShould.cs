using System;
using System.Threading;
using IntegrationPointsUITests.Components;
using IntegrationPointsUITests.Pages;
using NUnit.Framework;

namespace IntegrationPointsUITests.Tests
{
    [TestFixture]
    public class SelectWithSavedSearchShould : UiTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Console.WriteLine("Setup");
            EnsureGeneralPageIsOpened();
        }
        
        [Test, Order(10)]
        public void ChangeValueWhenSavedSearchIsChosenInDialog()
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
            SavedSearchDialog ssd = second.OpenSavedSearchSelectionDialog();
            ssd.ChooseSavedSearch("All Documents");
            
            // THEN
            Thread.Sleep(1000);
            Assert.AreEqual("All Documents", second.SavedSearch);
        }

    }
}
