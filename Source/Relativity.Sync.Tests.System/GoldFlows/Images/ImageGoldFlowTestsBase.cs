using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;

namespace Relativity.Sync.Tests.System.GoldFlows.Images
{
    [TestFixture]
    internal abstract class ImageGoldFlowTestsBase : SystemTest
    {
        public int ExpectedItemsForRetry { get; }

        protected readonly Dataset _dataset;
        protected const string _HAS_IMAGES_YES_CHOICE = "5002224A-59F9-4C19-AA57-3765BDBFB676";

        protected GoldFlowTestSuite _goldFlowTestSuite;

        internal ImageGoldFlowTestsBase(Dataset dataset, int expectedItemsForRetry)
        {
            ExpectedItemsForRetry = expectedItemsForRetry;
            _dataset = dataset;
        }

        protected override async Task ChildSuiteSetup()
        {
            _goldFlowTestSuite = await GoldFlowTestSuite.CreateAsync(Environment, User, ServiceFactory).ConfigureAwait(false);
            await _goldFlowTestSuite.ImportDocumentsAsync(DataTableFactory.CreateImageImportDataTable(_dataset)).ConfigureAwait(false);
            TridentHelper.UpdateFilePathToLocalIfNeeded(_goldFlowTestSuite.SourceWorkspace.ArtifactID, _dataset);
        }

        public virtual async Task SyncJob_Should_SyncImages()
        {
            // Arrange
            GoldFlowTestSuite.IGoldFlowTestRun goldFlowTestRun = await _goldFlowTestSuite.CreateTestRunAsync(ConfigureTestRunAsync).ConfigureAwait(false);

            // Act
            SyncJobState result = await goldFlowTestRun.RunAsync().ConfigureAwait(false);

            // Assert
            string condition = $"'Has Images' == CHOICE {_HAS_IMAGES_YES_CHOICE}";
            IList<RelativityObject> documentsWithImagesInSourceWorkspace = await Rdos.QueryDocumentsAsync(ServiceFactory, _goldFlowTestSuite.SourceWorkspace.ArtifactID, condition).ConfigureAwait(false);
            IList<RelativityObject> documentsWithImagesInDestinationWorkspace = await Rdos.QueryDocumentsAsync(ServiceFactory, goldFlowTestRun.DestinationWorkspaceArtifactId, condition).ConfigureAwait(false);

            await goldFlowTestRun.AssertAsync(result, _dataset.TotalItemCount, _dataset.TotalItemCount).ConfigureAwait(false);

            documentsWithImagesInDestinationWorkspace.Count.Should().Be(_dataset.TotalDocumentCount);

            goldFlowTestRun.AssertDocuments(
                documentsWithImagesInSourceWorkspace.Select(x => x.Name).ToArray(),
                documentsWithImagesInDestinationWorkspace.Select(x => x.Name).ToArray()
                );

            goldFlowTestRun.AssertImages(
                _goldFlowTestSuite.SourceWorkspace.ArtifactID, documentsWithImagesInSourceWorkspace.ToArray(),
                goldFlowTestRun.DestinationWorkspaceArtifactId, documentsWithImagesInDestinationWorkspace.ToArray()
            );
        }

        public virtual async Task SyncJob_Should_RetryImages()
        {
            // Arrange
            int jobHistoryToRetryId = -1;
            GoldFlowTestSuite.IGoldFlowTestRun goldFlowTestRun = await _goldFlowTestSuite.CreateTestRunAsync(async (sourceWorkspace, destinationWorkspace, configuration) =>
            {
                await ConfigureTestRunAsync(sourceWorkspace, destinationWorkspace, configuration).ConfigureAwait(false);

                jobHistoryToRetryId = await Rdos.CreateJobHistoryInstanceAsync(_goldFlowTestSuite.ServiceFactory, _goldFlowTestSuite.SourceWorkspace.ArtifactID)
                    .ConfigureAwait(false);
                configuration.JobHistoryToRetryId = jobHistoryToRetryId;
            }).ConfigureAwait(false);

            const int numberOfTaggedDocuments = 1;
            await Rdos.TagDocumentsAsync(ServiceFactory, _goldFlowTestSuite.SourceWorkspace.ArtifactID, jobHistoryToRetryId, numberOfTaggedDocuments).ConfigureAwait(false);

            // Act
            SyncJobState result = await goldFlowTestRun.RunAsync().ConfigureAwait(false);

            // Assert
            IList<RelativityObject> documentsWithImagesInSourceWorkspace = await Rdos.QueryDocumentsAsync(ServiceFactory, _goldFlowTestSuite.SourceWorkspace.ArtifactID, $"('Has Images' == CHOICE {_HAS_IMAGES_YES_CHOICE}) AND (NOT 'Job History' SUBQUERY ('Job History' INTERSECTS MULTIOBJECT [{jobHistoryToRetryId}]))").ConfigureAwait(false);
            IList<RelativityObject> documentsWithImagesInDestinationWorkspace = await Rdos.QueryDocumentsAsync(ServiceFactory, goldFlowTestRun.DestinationWorkspaceArtifactId, $"'Has Images' == CHOICE {_HAS_IMAGES_YES_CHOICE}").ConfigureAwait(false);

            await goldFlowTestRun.AssertAsync(result, ExpectedItemsForRetry, ExpectedItemsForRetry).ConfigureAwait(false);

            goldFlowTestRun.AssertDocuments(
                documentsWithImagesInSourceWorkspace.Select(x => x.Name).ToArray(),
                documentsWithImagesInDestinationWorkspace.Select(x => x.Name).ToArray()
            );
        }

        private async Task ConfigureTestRunAsync(WorkspaceRef sourceWorkspace, WorkspaceRef destinationWorkspace, ConfigurationStub configuration)
        {
            configuration.FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings;
            configuration.ImportOverwriteMode = ImportOverwriteMode.AppendOverlay;
            configuration.ImportImageFileCopyMode = ImportImageFileCopyMode.CopyFiles;
            configuration.ImageImport = true;

            IList<FieldMap> identifierMapping = await GetDocumentIdentifierMappingAsync(sourceWorkspace.ArtifactID, destinationWorkspace.ArtifactID).ConfigureAwait(false);
            configuration.SetFieldMappings(identifierMapping);
        }
    }
}