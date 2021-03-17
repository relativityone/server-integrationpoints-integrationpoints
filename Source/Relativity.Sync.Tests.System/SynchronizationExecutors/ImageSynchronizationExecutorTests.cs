using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Helpers;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System.SynchronizationExecutors
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.Sync]
	internal class ImageSynchronizationExecutorTests : SystemTest
	{
		private string SourceWorkspaceName => $"Source.{Guid.NewGuid()}";
		private string DestinationWorkspaceName => $"Destination.{Guid.NewGuid()}";

		[IdentifiedTestCase("57FE2538-3079-47BF-B40A-7ADF15C9A304")]
		public async Task SynchronizationExecutor_ShouldSyncImagesInBatches()
		{
			// Arrange
			Dataset dataSet = Dataset.ThreeImages;

			const int _BATCH_SIZE = 2;

			List<FieldMap> identifierFieldMap(int sourceWorkspaceId, int destinationWorkspaceId)
				=> GetIdentifierMappingAsync(sourceWorkspaceId, destinationWorkspaceId).GetAwaiter().GetResult();

			SynchronizationExecutorSetup setup = new SynchronizationExecutorSetup(Environment, ServiceFactory)
				.ForWorkspaces(SourceWorkspaceName, DestinationWorkspaceName)
				.ImportImageData(dataSet)
				.SetupImageConfiguration(identifierFieldMap, batchSize: _BATCH_SIZE)
				.SetupContainer()
				.SetDocumentTracking()
				.ExecutePreSynchronizationExecutors();

			// Act
			ExecutionResult syncResult = await ExecuteSynchronizationExecutorAsync(setup.Container, setup.Configuration).ConfigureAwait(false);

			// Assert
			var synchronizationValidator = new SynchronizationExecutorValidator(setup.Configuration, setup.ServiceFactory);

			synchronizationValidator.AssertResult(syncResult, ExecutionStatus.Completed);

			synchronizationValidator.AssertTransferredItemsInBatches(TrackingDocumentTagRepository.TaggedDocumentsInSourceWorkspaceWithDestinationInfoCounts[setup.Configuration.SourceWorkspaceArtifactId]);
			synchronizationValidator.AssertTransferredItemsInBatches(TrackingDocumentTagRepository.TaggedDocumentsInDestinationWorkspaceWithSourceInfoCounts[setup.Configuration.DestinationWorkspaceArtifactId]);
		}	

		private static Task<ExecutionResult> ExecuteSynchronizationExecutorAsync(IContainer container, ConfigurationStub configuration)
		{
			IExecutor<IImageSynchronizationConfiguration> syncExecutor = container.Resolve<IExecutor<IImageSynchronizationConfiguration>>();
			return syncExecutor.ExecuteAsync(configuration, CompositeCancellationToken.None);
		}
	}
}
