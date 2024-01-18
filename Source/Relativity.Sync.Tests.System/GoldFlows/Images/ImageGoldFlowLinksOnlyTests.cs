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
    internal class ImageGoldFlowLinksOnlyTests : SystemTest
    {
        private const string HAS_IMAGES_YES_CHOICE = "5002224A-59F9-4C19-AA57-3765BDBFB676";

        private GoldFlowTestSuite _goldFlowTestSuite;
        private readonly Dataset _dataset;

        public ImageGoldFlowLinksOnlyTests()
        {
            _dataset = Dataset.MultipleImagesPerDocument;
        }

        protected override async Task ChildSuiteSetup()
        {
            _goldFlowTestSuite = await GoldFlowTestSuite.CreateAsync(Environment, User, ServiceFactory).ConfigureAwait(false);
        }

        [IdentifiedTest("E61E099A-79D0-4271-83D9-54E29937EB46")]
        [TestType.MainFlow]
        public async Task SyncJob_ShouldPushOriginalImages_UsingLinks()
        {
            // Arrange
            await _goldFlowTestSuite.ImportDocumentsAsync(DataTableFactory.CreateImageImportDataTable(_dataset)).ConfigureAwait(false);
            TridentHelper.UpdateFilePathToLocalIfNeeded(_goldFlowTestSuite.SourceWorkspace.ArtifactID, _dataset);
            GoldFlowTestSuite.IGoldFlowTestRun goldFlowTestRun = await _goldFlowTestSuite.CreateTestRunAsync(ConfigureTestRunForImagesWithLinksOnlyAsync).ConfigureAwait(false);

            // Act
            SyncJobState result = await goldFlowTestRun.RunAsync().ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(SyncJobStatus.Completed);

            string condition = $"'Has Images' == CHOICE {HAS_IMAGES_YES_CHOICE}";
            IList<RelativityObject> documentsWithImagesInSourceWorkspace = await Rdos.QueryDocumentsAsync(ServiceFactory, _goldFlowTestSuite.SourceWorkspace.ArtifactID, condition).ConfigureAwait(false);
            IList<RelativityObject> documentsWithImagesInDestinationWorkspace = await Rdos.QueryDocumentsAsync(ServiceFactory, goldFlowTestRun.DestinationWorkspaceArtifactId, condition).ConfigureAwait(false);

            await goldFlowTestRun.AssertAsync(result, _dataset.TotalItemCount, _dataset.TotalItemCount).ConfigureAwait(false);

            documentsWithImagesInDestinationWorkspace.Count.Should().Be(_dataset.TotalDocumentCount);

            goldFlowTestRun.AssertDocuments(
                documentsWithImagesInSourceWorkspace.Select(x => x.Name).ToArray(),
                documentsWithImagesInDestinationWorkspace.Select(x => x.Name).ToArray());

            goldFlowTestRun.AssertImages(
                _goldFlowTestSuite.SourceWorkspace.ArtifactID, documentsWithImagesInSourceWorkspace.ToArray(),
                goldFlowTestRun.DestinationWorkspaceArtifactId, documentsWithImagesInDestinationWorkspace.ToArray());
        }

        [IdentifiedTest("C8B0E268-CC1C-4E0F-97BC-EE1CDE657AA5")]
        [TestType.MainFlow]
        public async Task SyncJob_ShouldOverlayWithOriginalImages_UsingLinks()
        {
            // Arrange
            await _goldFlowTestSuite.ImportDocumentsAsync(DataTableFactory.CreateImageImportDataTable(_dataset)).ConfigureAwait(false);
            TridentHelper.UpdateFilePathToLocalIfNeeded(_goldFlowTestSuite.SourceWorkspace.ArtifactID, _dataset);
            GoldFlowTestSuite.IGoldFlowTestRun pushMetadataOnlyJob = await _goldFlowTestSuite.CreateTestRunAsync(ConfigureTestRunForMetadataOnlyAsync).ConfigureAwait(false);
            GoldFlowTestSuite.IGoldFlowTestRun pushImagesWithLinksJob = await _goldFlowTestSuite.CreateTestRunAsync(ConfigureTestRunForImagesWithLinksOnlyAsync, pushMetadataOnlyJob.DestinationWorkspaceArtifactId).ConfigureAwait(false);

            // Act & Assert
            SyncJobState pushMetadataOnlyResult = await pushMetadataOnlyJob.RunAsync().ConfigureAwait(false);
            pushMetadataOnlyResult.Status.Should().Be(SyncJobStatus.Completed);

            SyncJobState pushImagesWithLinksResult = await pushImagesWithLinksJob.RunAsync().ConfigureAwait(false);
            pushImagesWithLinksResult.Status.Should().Be(SyncJobStatus.Completed);

            string condition = $"'Has Images' == CHOICE {HAS_IMAGES_YES_CHOICE}";
            IList<RelativityObject> documentsWithImagesInSourceWorkspace = await Rdos.QueryDocumentsAsync(ServiceFactory, _goldFlowTestSuite.SourceWorkspace.ArtifactID, condition).ConfigureAwait(false);
            IList<RelativityObject> documentsWithImagesInDestinationWorkspace = await Rdos.QueryDocumentsAsync(ServiceFactory, pushMetadataOnlyJob.DestinationWorkspaceArtifactId, condition).ConfigureAwait(false);

            await pushImagesWithLinksJob.AssertAsync(pushMetadataOnlyResult, _dataset.TotalItemCount, _dataset.TotalItemCount).ConfigureAwait(false);

            documentsWithImagesInDestinationWorkspace.Count.Should().Be(_dataset.TotalDocumentCount);

            pushImagesWithLinksJob.AssertDocuments(
                documentsWithImagesInSourceWorkspace.Select(x => x.Name).ToArray(),
                documentsWithImagesInDestinationWorkspace.Select(x => x.Name).ToArray());

            pushImagesWithLinksJob.AssertImages(
                _goldFlowTestSuite.SourceWorkspace.ArtifactID, documentsWithImagesInSourceWorkspace.ToArray(),
                pushMetadataOnlyJob.DestinationWorkspaceArtifactId, documentsWithImagesInDestinationWorkspace.ToArray());
        }

        private async Task ConfigureTestRunForImagesWithLinksOnlyAsync(WorkspaceRef sourceWorkspace, WorkspaceRef destinationWorkspace, ConfigurationStub configuration)
        {
            await ConfigureTestRunAsync(sourceWorkspace, destinationWorkspace, configuration).ConfigureAwait(false);

            configuration.ImportOverwriteMode = ImportOverwriteMode.AppendOverlay;
            configuration.ImportImageFileCopyMode = ImportImageFileCopyMode.SetFileLinks;
            configuration.ImageImport = true;
        }

        private async Task ConfigureTestRunForMetadataOnlyAsync(WorkspaceRef sourceWorkspace, WorkspaceRef destinationWorkspace, ConfigurationStub configuration)
        {
            await ConfigureTestRunAsync(sourceWorkspace, destinationWorkspace, configuration).ConfigureAwait(false);

            configuration.ImportOverwriteMode = ImportOverwriteMode.AppendOnly;
        }

        private async Task ConfigureTestRunAsync(WorkspaceRef sourceWorkspace, WorkspaceRef destinationWorkspace, ConfigurationStub configuration)
        {
            configuration.FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings;
            IList<FieldMap> identifierMapping = await GetDocumentIdentifierMappingAsync(sourceWorkspace.ArtifactID, destinationWorkspace.ArtifactID).ConfigureAwait(false);
            configuration.SetFieldMappings(identifierMapping);
        }
    }
}
