using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.ExecutorTests.TestsSetup;
using Relativity.Sync.Tests.System.Helpers;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System.ExecutorTests
{
    [TestFixture]
    [Feature.DataTransfer.IntegrationPoints.Sync]
    internal class ImageSynchronizationExecutorTests : SystemTest
    {
        private readonly string _sourceWorkspaceName = $"Source.{Guid.NewGuid()}";
        private readonly string _destinationWorkspaceName = $"Destination.{Guid.NewGuid()}";

        [IdentifiedTestCase("57FE2538-3079-47BF-B40A-7ADF15C9A304")]
        public async Task SynchronizationExecutor_ShouldSyncImagesInBatches()
        {
            // Arrange
            Dataset dataSet = Dataset.ThreeImages;

            const int _BATCH_SIZE = 2;

            List<FieldMap> IdentifierFieldMap(int sourceWorkspaceId, int destinationWorkspaceId)
                => GetDocumentIdentifierMappingAsync(sourceWorkspaceId, destinationWorkspaceId).GetAwaiter().GetResult();

            ExecutorTestSetup setup = new ExecutorTestSetup(Environment, ServiceFactory)
                .ForWorkspaces(_sourceWorkspaceName, _destinationWorkspaceName)
                .ImportImageData(dataSet)
                .SetupImageConfiguration(IdentifierFieldMap, batchSize: _BATCH_SIZE)
                .SetupContainer()
                .SetDocumentTracking()
                .SetupDestinationWorkspaceTag()
                .PrepareBatches();

            IExecutor<IImageSynchronizationConfiguration> sut = setup.Container.Resolve<IExecutor<IImageSynchronizationConfiguration>>();

            // Act
            ExecutionResult syncResult = await sut.ExecuteAsync(setup.Configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            var synchronizationValidator = new SynchronizationExecutorValidator(setup.Configuration, setup.ServiceFactory);

            synchronizationValidator.AssertResult(syncResult, ExecutionStatus.Completed);

            synchronizationValidator.AssertTransferredItemsInBatches(TrackingDocumentTagRepository.TaggedDocumentsInSourceWorkspaceWithDestinationInfoCounts[setup.Configuration.SourceWorkspaceArtifactId]);
            synchronizationValidator.AssertTransferredItemsInBatches(TrackingDocumentTagRepository.TaggedDocumentsInDestinationWorkspaceWithSourceInfoCounts[setup.Configuration.DestinationWorkspaceArtifactId]);
        }
    }
}
