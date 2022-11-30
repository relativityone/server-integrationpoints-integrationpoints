using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Core.Helpers.DataSet;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System.GoldFlows.Images
{
    internal sealed class ImageGoldFlowMultipleProductionsPrecedenceSetTests : SystemTest
    {
        private const string _HAS_IMAGES_YES_CHOICE = "5002224A-59F9-4C19-AA57-3765BDBFB676";

        private GoldFlowTestSuite _goldFlowTestSuite;

        private MultipleProductionsWithOriginalImagesDataSet _dataSet;

        private ProductionDto _production;

        protected override async Task ChildSuiteSetup()
        {
            _goldFlowTestSuite = await GoldFlowTestSuite.CreateAsync(Environment, User, ServiceFactory).ConfigureAwait(false);

            _dataSet = MultipleProductionsWithOriginalImagesDataSet.Create();

            _production = await ImportProductionTestDataAsync(_dataSet.DocumentsWithFirstProductionDataSet).ConfigureAwait(false);
            await ImportProductionTestDataAsync(_dataSet.DocumentsWithSecondProductionDataSet).ConfigureAwait(false);
            await ImportTestDataAsync(_dataSet.DocumentsWithOriginalImagesDataSet, DataTableFactory.CreateImageImportDataTable).ConfigureAwait(false);
            await ImportTestDataAsync(_dataSet.DocumentsWithoutImagesDataSet, DataTableFactory.CreateNativesImportDataTable).ConfigureAwait(false);
        }

        [IdentifiedTest("a768c7c7-d34c-4902-9fe3-c2536e72286c")]
        [TestType.MainFlow]
        public async Task SyncJob_ShouldSyncImagesWithLinks_WhenImagePrecedenceFromOneProductionIsSelected()
        {
            // Arrange
            IList<RelativityObject> expectedDocumentsWithImagesInDestinationWorkspace = await Rdos.QueryDocumentsAsync(ServiceFactory, _goldFlowTestSuite.SourceWorkspace.ArtifactID,
                $"'Production::Image Count' > 0 AND 'Production::ProductionSet' == '{_production.Name}'").ConfigureAwait(false);

            GoldFlowTestSuite.IGoldFlowTestRun goldFlowTestRun = await _goldFlowTestSuite.CreateTestRunAsync((sourceWorkspace, destinationWorkspace, config) =>
                ConfigureTestRunAsync(sourceWorkspace, destinationWorkspace, config, _production.ArtifactId, false)).ConfigureAwait(false);

            // Act
            SyncJobState result = await goldFlowTestRun.RunAsync().ConfigureAwait(false);

            // Assert
            IList<RelativityObject> documentsWithImagesInDestinationWorkspace = await Rdos.QueryDocumentsAsync(ServiceFactory, goldFlowTestRun.DestinationWorkspaceArtifactId,
                $"'Has Images' == CHOICE {_HAS_IMAGES_YES_CHOICE}").ConfigureAwait(false);

            await goldFlowTestRun.AssertAsync(result,
                expectedDocumentsWithImagesInDestinationWorkspace.Count,
                expectedDocumentsWithImagesInDestinationWorkspace.Count).ConfigureAwait(false);

            documentsWithImagesInDestinationWorkspace.Count.Should().Be(expectedDocumentsWithImagesInDestinationWorkspace.Count);

            goldFlowTestRun.AssertDocuments(
                expectedDocumentsWithImagesInDestinationWorkspace.Select(x => x.Name).ToArray(),
                documentsWithImagesInDestinationWorkspace.Select(x => x.Name).ToArray()
            );

            goldFlowTestRun.AssertImages(
                _goldFlowTestSuite.SourceWorkspace.ArtifactID, expectedDocumentsWithImagesInDestinationWorkspace.ToArray(),
                goldFlowTestRun.DestinationWorkspaceArtifactId, documentsWithImagesInDestinationWorkspace.ToArray()
            );
        }

        [IdentifiedTest("6176e481-2a75-4654-af46-096f24bb6bfe")]
        [TestType.MainFlow]
        public async Task SyncJob_ShouldSyncImagesWithLinks_WhenImagePrecedenceFromOneProductionIsSelectedAndOriginalImagesIfNotFoundInProduction()
        {
            // Arrange
            IList<RelativityObject> expectedDocumentsWithImagesInDestinationWorkspace = await Rdos.QueryDocumentsAsync(ServiceFactory, _goldFlowTestSuite.SourceWorkspace.ArtifactID,
                $"('Production::Image Count' > 0 AND 'Production::ProductionSet' == '{_production.Name}') OR 'Has Images' == CHOICE {_HAS_IMAGES_YES_CHOICE}").ConfigureAwait(false);

            GoldFlowTestSuite.IGoldFlowTestRun goldFlowTestRun = await _goldFlowTestSuite.CreateTestRunAsync((sourceWorkspace, destinationWorkspace, config) =>
                ConfigureTestRunAsync(sourceWorkspace, destinationWorkspace, config, _production.ArtifactId, true)).ConfigureAwait(false);

            // Act
            SyncJobState result = await goldFlowTestRun.RunAsync().ConfigureAwait(false);

            // Assert
            IList<RelativityObject> documentsWithImagesInDestinationWorkspace = await Rdos.QueryDocumentsAsync(ServiceFactory, goldFlowTestRun.DestinationWorkspaceArtifactId,
                $"'Has Images' == CHOICE {_HAS_IMAGES_YES_CHOICE}").ConfigureAwait(false);

            await goldFlowTestRun.AssertAsync(result,
                expectedDocumentsWithImagesInDestinationWorkspace.Count,
                expectedDocumentsWithImagesInDestinationWorkspace.Count).ConfigureAwait(false);

            documentsWithImagesInDestinationWorkspace.Count.Should().Be(expectedDocumentsWithImagesInDestinationWorkspace.Count);

            goldFlowTestRun.AssertDocuments(
                expectedDocumentsWithImagesInDestinationWorkspace.Select(x => x.Name).ToArray(),
                documentsWithImagesInDestinationWorkspace.Select(x => x.Name).ToArray()
            );

            goldFlowTestRun.AssertImages(
                _goldFlowTestSuite.SourceWorkspace.ArtifactID, expectedDocumentsWithImagesInDestinationWorkspace.ToArray(),
                goldFlowTestRun.DestinationWorkspaceArtifactId, documentsWithImagesInDestinationWorkspace.ToArray()
            );
        }

        private async Task ConfigureTestRunAsync(WorkspaceRef sourceWorkspace, WorkspaceRef destinationWorkspace, ConfigurationStub configuration,
            int productionId, bool includeOriginalIfProductionNotFound)
        {
            configuration.FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings;
            configuration.ImportOverwriteMode = ImportOverwriteMode.AppendOverlay;
            configuration.ImportImageFileCopyMode = ImportImageFileCopyMode.SetFileLinks;
            configuration.ImageImport = true;
            configuration.IncludeOriginalImageIfNotFoundInProductions = includeOriginalIfProductionNotFound;
            configuration.ProductionImagePrecedence = new[] { productionId };

            IList<FieldMap> identifierMapping = await GetDocumentIdentifierMappingAsync(sourceWorkspace.ArtifactID, destinationWorkspace.ArtifactID).ConfigureAwait(false);
            configuration.SetFieldMappings(identifierMapping);
        }

        private async Task<ProductionDto> ImportProductionTestDataAsync(Dataset productionDataSet)
        {
            ProductionDto production = await CreateAndImportProductionAsync(_goldFlowTestSuite.SourceWorkspace.ArtifactID, productionDataSet)
                .ConfigureAwait(false);

            TridentHelper.UpdateFilePathToLocalIfNeeded(_goldFlowTestSuite.SourceWorkspace.ArtifactID, productionDataSet);

            return production;
        }

        private async Task ImportTestDataAsync(Dataset dataSet, Func<Dataset, ImportDataTableWrapper> tableWrapperCreator)
        {
            ImportDataTableWrapper dataTableWrapper = tableWrapperCreator(dataSet);

            await _goldFlowTestSuite.ImportDocumentsAsync(dataTableWrapper).ConfigureAwait(false);

            TridentHelper.UpdateFilePathToLocalIfNeeded(_goldFlowTestSuite.SourceWorkspace.ArtifactID, dataSet);
        }
    }
}
