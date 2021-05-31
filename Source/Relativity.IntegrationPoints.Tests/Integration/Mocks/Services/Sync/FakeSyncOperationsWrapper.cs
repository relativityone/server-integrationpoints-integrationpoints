using Autofac;
using kCura.IntegrationPoints.RelativitySync;
using Moq;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Sync;
using Relativity.Sync.SyncConfiguration;
using Relativity.Sync.SyncConfiguration.FieldsMapping;
using Relativity.Sync.SyncConfiguration.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.Sync
{
	internal class FakeSyncOperationsWrapper : ISyncOperationsWrapper, IExtendedFakeSyncOperations
	{
		RelativityInstanceTest _relativity;

		private readonly Mock<ISyncJob> _syncJob;

		public FakeSyncOperationsWrapper(RelativityInstanceTest relativity)
		{
			_relativity = relativity;
			_syncJob = new Mock<ISyncJob>();
		}

		public void SetupSyncJob(Func<CompositeCancellationToken, Task> action)
		{
			_syncJob.Setup(x => x.ExecuteAsync(It.IsAny<CompositeCancellationToken>()))
				.Returns(async (CompositeCancellationToken token) => await action(token).ConfigureAwait(false));
			_syncJob.Setup(x => x.ExecuteAsync(It.IsAny<IProgress<SyncJobState>>(), It.IsAny<CompositeCancellationToken>()))
				.Returns(async (IProgress<SyncJobState> progress, CompositeCancellationToken token) => await action(token).ConfigureAwait(false));
		}

		public IRelativityServices CreateRelativityServices()
		{
			return new Mock<IRelativityServices>().Object;
		}

		public ISyncJobFactory CreateSyncJobFactory()
		{
			Mock<ISyncJobFactory> syncJobFactory = new Mock<ISyncJobFactory>();
			syncJobFactory.Setup(x => x.Create(It.IsAny<IContainer>(), It.IsAny<SyncJobParameters>(),
					It.IsAny<IRelativityServices>(), It.IsAny<ISyncLog>()))
				.Returns(_syncJob.Object);

			return syncJobFactory.Object;
		}

		public ISyncLog CreateSyncLog()
		{
			return new Mock<ISyncLog>().Object;
		}

		public ISyncConfigurationBuilder GetSyncConfigurationBuilder(ISyncContext context)
		{
			Mock<ISyncJobConfigurationBuilder> syncConfigurationBuilderMock = new Mock<ISyncJobConfigurationBuilder>();
			Mock<IDocumentSyncConfigurationBuilder> documentSyncConfigurationBuilderMock = new Mock<IDocumentSyncConfigurationBuilder>();
			Mock<IImageSyncConfigurationBuilder>  imageSyncConfigurationBuilderMock = new Mock<IImageSyncConfigurationBuilder>();

			documentSyncConfigurationBuilderMock.Setup(x => x.CreateSavedSearch(It.IsAny<CreateSavedSearchOptions>()))
				.Returns(documentSyncConfigurationBuilderMock.Object);
			documentSyncConfigurationBuilderMock.Setup(x => x.DestinationFolderStructure(It.IsAny<DestinationFolderStructureOptions>()))
					.Returns(documentSyncConfigurationBuilderMock.Object);
			documentSyncConfigurationBuilderMock.Setup(x => x.EmailNotifications(It.IsAny<EmailNotificationsOptions>()))
					.Returns(documentSyncConfigurationBuilderMock.Object);
			documentSyncConfigurationBuilderMock.Setup(x => x.IsRetry(It.IsAny<RetryOptions>()))
					.Returns(documentSyncConfigurationBuilderMock.Object);
			documentSyncConfigurationBuilderMock.Setup(x => x.OverwriteMode(It.IsAny<OverwriteOptions>()))
					.Returns(documentSyncConfigurationBuilderMock.Object);
			documentSyncConfigurationBuilderMock.Setup(x => x.WithFieldsMapping(It.IsAny<Action<IFieldsMappingBuilder>>()))
					.Returns(documentSyncConfigurationBuilderMock.Object);
			documentSyncConfigurationBuilderMock.Setup(x => x.SaveAsync())
					.Returns(() => SaveSyncConfiguration(context));

			imageSyncConfigurationBuilderMock.Setup(x => x.CreateSavedSearch(It.IsAny<CreateSavedSearchOptions>()))
					.Returns(imageSyncConfigurationBuilderMock.Object);
			imageSyncConfigurationBuilderMock.Setup(x => x.EmailNotifications(It.IsAny<EmailNotificationsOptions>()))
					.Returns(imageSyncConfigurationBuilderMock.Object);
			imageSyncConfigurationBuilderMock.Setup(x => x.IsRetry(It.IsAny<RetryOptions>()))
					.Returns(imageSyncConfigurationBuilderMock.Object);
			imageSyncConfigurationBuilderMock.Setup(x => x.OverwriteMode(It.IsAny<OverwriteOptions>()))
					.Returns(imageSyncConfigurationBuilderMock.Object);
			imageSyncConfigurationBuilderMock.Setup(x => x.ProductionImagePrecedence(It.IsAny<ProductionImagePrecedenceOptions>()))
					.Returns(imageSyncConfigurationBuilderMock.Object);
			imageSyncConfigurationBuilderMock.Setup(x => x.SaveAsync())
					.Returns(() => SaveSyncConfiguration(context));

			syncConfigurationBuilderMock.Setup(x => x.ConfigureDocumentSync(It.IsAny<DocumentSyncOptions>()))
					.Returns(documentSyncConfigurationBuilderMock.Object);
			syncConfigurationBuilderMock.Setup(x => x.ConfigureImageSync(It.IsAny<ImageSyncOptions>()))
					.Returns(imageSyncConfigurationBuilderMock.Object);

			Mock<ISyncConfigurationBuilder> configurationBuilderMock = new Mock<ISyncConfigurationBuilder>();
			configurationBuilderMock.Setup(x => x.ConfigureRdos(It.IsAny<RdoOptions>()))
				.Returns(syncConfigurationBuilderMock.Object);

			return configurationBuilderMock.Object;
		}

		public Task PrepareSyncConfigurationForResumeAsync(int workspaceId, int syncConfigurationId)
		{
			_relativity.Workspaces.Single(x => x.ArtifactId == workspaceId)
				.SyncConfigurations.Single(x => x.ArtifactId == syncConfigurationId).Resuming = true;

			return Task.CompletedTask;
		}

		private Task<int> SaveSyncConfiguration(ISyncContext context)
		{
			SyncConfigurationTest syncConfiguration = new SyncConfigurationTest
			{
				JobHistoryId = context.JobHistoryId
			};

			_relativity.Workspaces.Single(x => x.ArtifactId == context.SourceWorkspaceId)
				.SyncConfigurations.Add(syncConfiguration);

			return Task.FromResult(syncConfiguration.ArtifactId);
		}
	}
}
