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
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System.GoldFlows.Images
{
    internal sealed class ImageGoldFlowProductionPrecedenceWithIncludeOriginalSetTests : SystemTest
    {
        private const string HAS_IMAGES_YES_CHOICE = "5002224A-59F9-4C19-AA57-3765BDBFB676";

        private GoldFlowTestSuite _goldFlowTestSuite;

        private readonly Dataset _productionDataset;
        private readonly Dataset _imageDataset;

        public ImageGoldFlowProductionPrecedenceWithIncludeOriginalSetTests()
        {
            _productionDataset = Dataset.TwoDocumentProduction;
            _imageDataset = Dataset.Images;
        }

        protected override async Task ChildSuiteSetup()
        {
            _goldFlowTestSuite = await GoldFlowTestSuite.CreateAsync(Environment, User, ServiceFactory).ConfigureAwait(false);
        }

        [IdentifiedTest("03621650-804B-4B79-9825-CA8822981D52")]
        [TestType.MainFlow]
        public async Task SyncJob_ShouldSyncImages_WhenImagePrecedenceAndIncludeOriginalIfNotFoundInProductionIsSelected()
        {
            // Arrange
            int productionId = await ImportProductionTestDataAsync(_productionDataset).ConfigureAwait(false);

            await ImportImageTestDataAsync(_imageDataset).ConfigureAwait(false);

            GoldFlowTestSuite.IGoldFlowTestRun goldFlowTestRun = await _goldFlowTestSuite.CreateTestRunAsync((sourceWorkspace, destinationWorkspace, config) =>
                ConfigureTestRunAsync(sourceWorkspace, destinationWorkspace, config, productionId)).ConfigureAwait(false);

            // Act
            SyncJobState result = await goldFlowTestRun.RunAsync().ConfigureAwait(false);

            // Assert
            IList<RelativityObject> documentsWithImagesInSourceWorkspace = await Rdos.QueryDocumentsAsync(ServiceFactory, _goldFlowTestSuite.SourceWorkspace.ArtifactID,
                $"'Production::Image Count' > 0 OR 'Has Images' == CHOICE {HAS_IMAGES_YES_CHOICE}").ConfigureAwait(false);
            IList<RelativityObject> documentsWithImagesInDestinationWorkspace = await Rdos.QueryDocumentsAsync(ServiceFactory, goldFlowTestRun.DestinationWorkspaceArtifactId,
                $"'Has Images' == CHOICE {HAS_IMAGES_YES_CHOICE}").ConfigureAwait(false);

            await goldFlowTestRun.AssertAsync(result,
                documentsWithImagesInSourceWorkspace.Count,
                documentsWithImagesInSourceWorkspace.Count).ConfigureAwait(false);

            documentsWithImagesInDestinationWorkspace.Count.Should().Be(documentsWithImagesInSourceWorkspace.Count);

            goldFlowTestRun.AssertDocuments(
                documentsWithImagesInSourceWorkspace.Select(x => x.Name).ToArray(),
                documentsWithImagesInDestinationWorkspace.Select(x => x.Name).ToArray()
            );

            goldFlowTestRun.AssertImages(
                _goldFlowTestSuite.SourceWorkspace.ArtifactID, documentsWithImagesInSourceWorkspace.ToArray(),
                goldFlowTestRun.DestinationWorkspaceArtifactId, documentsWithImagesInDestinationWorkspace.ToArray()
            );
        }

        private async Task ConfigureTestRunAsync(WorkspaceRef sourceWorkspace, WorkspaceRef destinationWorkspace, ConfigurationStub configuration, int productionId)
        {
            configuration.FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings;
            configuration.ImportOverwriteMode = ImportOverwriteMode.AppendOverlay;
            configuration.ImportImageFileCopyMode = ImportImageFileCopyMode.CopyFiles;
            configuration.ImageImport = true;
            configuration.IncludeOriginalImageIfNotFoundInProductions = true;
            configuration.ProductionImagePrecedence = new[] { productionId };

            IList<FieldMap> identifierMapping = await GetDocumentIdentifierMappingAsync(sourceWorkspace.ArtifactID, destinationWorkspace.ArtifactID).ConfigureAwait(false);
            configuration.SetFieldMappings(identifierMapping);
        }

        private async Task<int> ImportProductionTestDataAsync(Dataset productionDataset)
        {
            ProductionDto production = await CreateAndImportProductionAsync(_goldFlowTestSuite.SourceWorkspace.ArtifactID, productionDataset)
                .ConfigureAwait(false);

            TridentHelper.UpdateFilePathToLocalIfNeeded(_goldFlowTestSuite.SourceWorkspace.ArtifactID, productionDataset);

            return production.ArtifactId;
        }

        private async Task ImportImageTestDataAsync(Dataset dataset)
        {
            ImportDataTableWrapper dataTableWrapper = DataTableFactory.CreateImageImportDataTable(dataset);

            await _goldFlowTestSuite.ImportDocumentsAsync(dataTableWrapper).ConfigureAwait(false);

            TridentHelper.UpdateFilePathToLocalIfNeeded(_goldFlowTestSuite.SourceWorkspace.ArtifactID, dataset);
        }
    }
}
