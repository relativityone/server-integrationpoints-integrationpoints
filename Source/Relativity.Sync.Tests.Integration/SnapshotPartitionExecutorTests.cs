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
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.RDOs;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public class SnapshotPartitionExecutorTests
	{
		private CompositeCancellationToken _token;

		private Mock<ISyncLog> _syncLog;
		private Mock<IObjectManager> _objectManager;
		private Mock<ISnapshotPartitionConfiguration> _snapshotPartitionConfiguration;
		private Mock<IRdoManager> _rdoManagerMock;

		private IExecutor<ISnapshotPartitionConfiguration> _instance;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_token = CompositeCancellationToken.None;
		}

		[SetUp]
		public void SetUp()
		{
			_syncLog = new Mock<ISyncLog>();
			_objectManager = new Mock<IObjectManager>();
			_snapshotPartitionConfiguration = new Mock<ISnapshotPartitionConfiguration>(MockBehavior.Loose);
			_rdoManagerMock = new Mock<IRdoManager>();

			ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.MockStepsExcept<ISnapshotPartitionConfiguration>(containerBuilder);

			containerBuilder.RegisterInstance(_syncLog.Object).As<ISyncLog>();
			containerBuilder.RegisterInstance(_rdoManagerMock.Object).As<IRdoManager>();

			var serviceFactoryMock = new Mock<ISourceServiceFactoryForAdmin>();
			serviceFactoryMock.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
			containerBuilder.RegisterInstance(serviceFactoryMock.Object).As<ISourceServiceFactoryForAdmin>();

			containerBuilder.RegisterType<SnapshotPartitionExecutor>().As<IExecutor<ISnapshotPartitionConfiguration>>();

			IContainer container = containerBuilder.Build();
			_instance = container.Resolve<IExecutor<ISnapshotPartitionConfiguration>>();
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

			_snapshotPartitionConfiguration.SetupGet(x => x.BatchSize).Returns(batchSize);
			_snapshotPartitionConfiguration.SetupGet(x => x.TotalRecordsCount).Returns(totalRecords);
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(testBatchResult).Verifiable();
			_objectManager.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<CreateRequest>())).ReturnsAsync(testCreateResult).Verifiable();

			// Act
			ExecutionResult actualResult = await _instance.ExecuteAsync(_snapshotPartitionConfiguration.Object, _token).ConfigureAwait(false);

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

			_snapshotPartitionConfiguration.SetupGet(x => x.BatchSize).Returns(batchSize);
			_snapshotPartitionConfiguration.SetupGet(x => x.TotalRecordsCount).Returns(numberOfItems);
			
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
			ExecutionResult actualResult = await _instance.ExecuteAsync(_snapshotPartitionConfiguration.Object, _token).ConfigureAwait(false);

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

			// Act
			ExecutionResult actualResult = await _instance.ExecuteAsync(_snapshotPartitionConfiguration.Object, _token).ConfigureAwait(false);

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

			_snapshotPartitionConfiguration.SetupGet(x => x.BatchSize).Returns(batchSize);
			_snapshotPartitionConfiguration.SetupGet(x => x.TotalRecordsCount).Returns(totalRecords);
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
			ExecutionResult actualResult = await _instance.ExecuteAsync(_snapshotPartitionConfiguration.Object, _token).ConfigureAwait(false);

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
	}
}