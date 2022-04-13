using Relativity.API;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Moq;
using NUnit.Framework;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.RDOs;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal abstract class SnapshotPartitionExecutorTestsBase<T> where T : ISnapshotPartitionConfiguration
	{
		private CompositeCancellationToken _token;

		private Mock<IAPILog> _syncLog;
		private Mock<IObjectManager> _objectManager;
		private Mock<IRdoManager> _rdoManagerMock;
        private IExecutor<T> _instance;

        protected ContainerBuilder ContainerBuilder;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_token = CompositeCancellationToken.None;
		}

		[SetUp]
		public virtual void SetUp()
		{
			_syncLog = new Mock<IAPILog>();
			_objectManager = new Mock<IObjectManager>();
			_rdoManagerMock = new Mock<IRdoManager>();

            if (ContainerBuilder == null)
            {
				ContainerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			}
			IntegrationTestsContainerBuilder.MockStepsExcept<T>(ContainerBuilder);

            ContainerBuilder.RegisterInstance(_syncLog.Object).As<IAPILog>();
            ContainerBuilder.RegisterInstance(_rdoManagerMock.Object).As<IRdoManager>();

			var serviceFactoryForAdminMock = new Mock<ISourceServiceFactoryForAdmin>();
			serviceFactoryForAdminMock.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
            ContainerBuilder.RegisterInstance(serviceFactoryForAdminMock.Object).As<ISourceServiceFactoryForAdmin>();

			IContainer container = ContainerBuilder.Build();
			_instance = container.Resolve<IExecutor<T>>();
		}

		[Test]
		public async Task ExecuteAsyncGoldFlowCreatesTwoBatchesTest()
		{
			// Arrange
			const int batchSize = 100;
			const int totalRecords = 165;
			int expectedNumberOfBatches = (totalRecords + batchSize - 1) / batchSize;

			var testBatchResult = new QueryResult
			{
				TotalCount = 0
			};
			var testCreateResult = new CreateResult
			{
				Object = new RelativityObject
				{
					ArtifactID = 1
				}
			};

            T snapshotPartitionConfiguration = GetSnapshotPartitionConfigurationMockAndSetup(batchSize, totalRecords);
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(testBatchResult).Verifiable();
			_objectManager.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<CreateRequest>())).ReturnsAsync(testCreateResult).Verifiable();

			// Act
			ExecutionResult actualResult = await _instance.ExecuteAsync(snapshotPartitionConfiguration, _token).ConfigureAwait(false);

			// Assert
			Assert.AreEqual(ExecutionStatus.Completed, actualResult.Status);
			
			_rdoManagerMock.Verify(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<SyncBatchRdo>(), It.IsAny<int>()), Times.Exactly(expectedNumberOfBatches));
			_syncLog.Verify(x => x.LogError(It.IsAny<NotAuthorizedException>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
		}

		[Test]
		public async Task ExecuteAsyncCreatesNoNewBatchesTest()
		{
			// Arrange
			const int numberOfItems = 100;
			const int batchSize = 100;
			const int batchArtifactId = 1;

            T snapshotPartitionConfiguration =
                GetSnapshotPartitionConfigurationMockAndSetup(batchSize,
                    numberOfItems);


			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), 1, 1))
				.ReturnsAsync(new QueryResult
				{
					Objects = new List<RelativityObject>
					{
						new RelativityObject
						{
							ArtifactID = batchArtifactId
						}
					},
					TotalCount = 1
				}).Verifiable();
			
			_rdoManagerMock.Setup(x => x.GetAsync<SyncBatchRdo>(It.IsAny<int>(), batchArtifactId))
				.ReturnsAsync(new SyncBatchRdo
				{
					ArtifactId = batchArtifactId,
					TotalDocumentsCount = numberOfItems
				})
				.Verifiable();

			// Act
			ExecutionResult actualResult = await _instance.ExecuteAsync(snapshotPartitionConfiguration, _token).ConfigureAwait(false);

			// Assert
			Assert.AreEqual(ExecutionStatus.Completed, actualResult.Status);

			Mock.Verify(_objectManager);
			_rdoManagerMock.Verify();
			_rdoManagerMock.Verify(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<SyncBatchRdo>(), It.IsAny<int>()), Times.Never);
		}

		[Test]
		public async Task ExecuteAsyncReadBatchQueryThrowsErrorTest()
		{
			// Arrange
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), 1, 1)).Throws<NotAuthorizedException>().Verifiable();
            T snapshotPartitionConfiguration = GetSnapshotPartitionConfigurationMock();

			// Act
			ExecutionResult actualResult = await _instance.ExecuteAsync(snapshotPartitionConfiguration, _token).ConfigureAwait(false);

			// Assert
			Assert.AreEqual(ExecutionStatus.Failed, actualResult.Status);
			Assert.AreEqual("Cannot read last batch.", actualResult.Message);

			Mock.Verify(_objectManager);
			_syncLog.Verify(x => x.LogError(
				It.IsAny<NotAuthorizedException>(),
				It.Is<string>(y => y.StartsWith("Unable to retrieve last batch", StringComparison.InvariantCulture)),
				It.IsAny<object[]>()), Times.Once);
			_rdoManagerMock.Verify();
		}

		[Test]
		public async Task ExecuteAsyncCreateBatchQueryThrowsErrorTest()
		{
			// Arrange
			const int numberOfItems = 100;
			const int batchSize = 100;
			const int totalRecords = 200;
			const int batchArtifactId = 1;

            T snapshotPartitionConfiguration = GetSnapshotPartitionConfigurationMockAndSetup(batchSize, totalRecords);

			_rdoManagerMock.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<SyncBatchRdo>(), It.IsAny<int>()))
				.Throws<NotAuthorizedException>()
				.Verifiable();
			
			_rdoManagerMock.Setup(x => x.GetAsync<SyncBatchRdo>(It.IsAny<int>(), batchArtifactId))
				.ReturnsAsync(new SyncBatchRdo
				{
					ArtifactId = batchArtifactId,
					TotalDocumentsCount = numberOfItems
				})
				.Verifiable();
			
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), 1, 1))
				.ReturnsAsync(new QueryResult
				{
					Objects = new List<RelativityObject>
					{
						new RelativityObject
						{
							ArtifactID = batchArtifactId
						}
					},
					TotalCount = 1
				});

			// Act
			ExecutionResult actualResult = await _instance.ExecuteAsync(snapshotPartitionConfiguration, _token).ConfigureAwait(false);

			// Assert
			Assert.AreEqual(ExecutionStatus.Failed, actualResult.Status);
			Assert.AreEqual("Unable to create batches.", actualResult.Message);

			Mock.Verify(_objectManager);
			Mock.Verify(_rdoManagerMock);
			_syncLog.Verify(x => x.LogError(
				It.IsAny<NotAuthorizedException>(),
				It.Is<string>(y => y.StartsWith("Unable to create batch", StringComparison.InvariantCulture)),
				It.IsAny<object[]>()), Times.Once);
		}

        protected virtual T GetSnapshotPartitionConfigurationMock()
        {
            Mock<ISnapshotPartitionConfiguration> snapshotPartitionConfiguration = new Mock<ISnapshotPartitionConfiguration>(MockBehavior.Loose);

            return (T)snapshotPartitionConfiguration.Object;
        }

		protected virtual T GetSnapshotPartitionConfigurationMockAndSetup(int batchSize, int totalRecords)
        {
            Mock<ISnapshotPartitionConfiguration> snapshotPartitionConfiguration = new Mock<ISnapshotPartitionConfiguration>(MockBehavior.Loose);

            snapshotPartitionConfiguration.Setup(x => x.GetSyncBatchSizeAsync()).ReturnsAsync(batchSize);
			snapshotPartitionConfiguration.SetupGet(x => x.TotalRecordsCount).Returns(totalRecords);

            return (T)snapshotPartitionConfiguration.Object;
        }
    }
}
