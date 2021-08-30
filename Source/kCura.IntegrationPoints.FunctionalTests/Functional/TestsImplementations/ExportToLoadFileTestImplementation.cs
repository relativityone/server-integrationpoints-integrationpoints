using Atata;
using FluentAssertions;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models.ExportToLoadFileOutputSettings;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Web.Extensions;
using Relativity.Testing.Framework.Web.Navigation;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal class ExportToLoadFileTestImplementation
    {
        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;
        private readonly string _savedSearch = "All docs";
        private readonly int _startExportAtRecord = 1;
        private readonly int _workspaceArtifactID;


        public ExportToLoadFileTestImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
        {
            _testsImplementationTestFixture = testsImplementationTestFixture;
            _workspaceArtifactID = _testsImplementationTestFixture.Workspace.ArtifactID;
        }

        public void OnSetUpFixture()
        {
            RelativityFacade.Instance.ImportDocumentsFromCsv(_testsImplementationTestFixture.Workspace, LoadFilesGenerator.GetOrCreateNativesLoadFile());
        }

        public void ExportToLoadFilesNativesGoldFlow()
        {
            _testsImplementationTestFixture.LoginAsStandardUser();

            string integrationPointName = nameof(ExportToLoadFileTestImplementation);

            // Act 
            var integrationPointListPage = Being.On<IntegrationPointListPage>(_testsImplementationTestFixture.Workspace.ArtifactID);
            var integrationPointEditPage = integrationPointListPage.NewIntegrationPoint.ClickAndGo();

            var exportToLoadFileConnectToSourcePage = FillOutIntegrationPointEditPageForExportToLoadFileTest(integrationPointEditPage, integrationPointName);

            exportToLoadFileConnectToSourcePage.
                ApplyModel(new ExportToLoadFileConnectToSavedSearchSource()
                {
                    StartExportAtRecord = _startExportAtRecord,
                    SavedSearch = _savedSearch
                });
            var exportToLoadFileDestinationInformation = exportToLoadFileConnectToSourcePage.Next.ClickAndGo();

            var integrationPointViewPage = FillOutIntegrationPointEditPageForExportToLoadFileDestinationDetails(exportToLoadFileDestinationInformation, _workspaceArtifactID);

            integrationPointViewPage = integrationPointViewPage.RunIntegrationPoint(integrationPointName);

            // Assert
            int transferredItemsCount = integrationPointViewPage.GetTransferredItemsCount(integrationPointName);
            int workspaceDocumentCount = RelativityFacade.Instance.Resolve<IDocumentService>().GetAll(_workspaceArtifactID).Length;
            
            transferredItemsCount.Should().Be(workspaceDocumentCount);
        }

        
        private static ExportToLoadFileConnectToSourcePage FillOutIntegrationPointEditPageForExportToLoadFileTest(
            IntegrationPointEditPage integrationPointEditPage, string integrationPointName)
        {
            IntegrationPointEdit integrationPointEdit = new IntegrationPointEditExport
            {
                Name = integrationPointName,
                Destination = IntegrationPointDestinations.LoadFile
            };

            return integrationPointEditPage.ApplyModel(integrationPointEdit).ExportToLoadFileNext.ClickAndGo();
        }

        private static IntegrationPointViewPage FillOutIntegrationPointEditPageForExportToLoadFileDestinationDetails(ExportToLoadFileDestinationInformationPage webPage, int WorkspaceArtifactID)
        {
            webPage.LoadFile.Should.BeDisabled();
            webPage.LoadFile.Should.BeChecked();
            webPage.Natives.Check();
            webPage.Images.Check();

            webPage.SelectFolder.Click().SetItem(GetDataTransferLocationForWorkspace(WorkspaceArtifactID));
            webPage.ApplyModel(new ExportToLoadFileOutputSettingsModel()
            {
                ImageFileFormat = ImageFileFormats.Opticon,
                DataFileFormat = DataFileFormats.Relativity,
                NameOutputFilesAfter = NameOutputFilesAfterOptions.BeginProductionNumber,
                FileType = ImageFileTypes.PDF,
                ImagePrecedence = ImagePrecedences.OriginalImages
            });

            webPage.IncludeNativeFilesPath.Should.BeChecked();
            webPage.OverwriteFiles.Check();
            webPage.ExportMultipleChoiceFieldsAsNested.Check();
            webPage.AppendOriginalFileName.Check();

            return webPage.Save.ClickAndGo();
        }

        private static string GetDataTransferLocationForWorkspace(int workspaceArtifactID)
        {
            string dataTransferLocationSuffix = "DataTransfer\\Export";

            return $".\\EDDS{workspaceArtifactID}\\{dataTransferLocationSuffix}";
        }
    }
}
