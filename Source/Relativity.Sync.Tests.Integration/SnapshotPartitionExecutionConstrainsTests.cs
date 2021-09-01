using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Moq;
using NUnit.Framework;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.RDOs;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public class SnapshotPartitionExecutionConstrainsTests
	{
		private CancellationToken _token;

		private Mock<ISyncLog> _syncLog;
		private Mock<IObjectManager> _objectManager;
		private Mock<ISnapshotPartitionConfiguration> _snapshotPartitionConfiguration;
		private Mock<IRdoManager> _rdoManagerMock;

		private IExecutionConstrains<ISnapshotPartitionConfiguration> _instance;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_token = CancellationToken.None;
		}

		[SetUp]
		public void SetUp()
		{
			_syncLog = new Mock<ISyncLog>();
			_objectManager = new Mock<IObjectManager>();
			_snapshotPartitionConfiguration = new Mock<ISnapshotPartitionConfiguration>(MockBehavior.Loose);

			ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.MockStepsExcept<ISnapshotPartitionConfiguration>(containerBuilder);

			containerBuilder.RegisterInstance(_syncLog.Object).As<ISyncLog>();

			var serviceFactoryMock = new Mock<ISourceServiceFactoryForAdmin>();
			serviceFactoryMock.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
			containerBuilder.RegisterInstance(serviceFactoryMock.Object).As<ISourceServiceFactoryForAdmin>();

			containerBuilder.RegisterType<SnapshotPartitionExecutionConstrains>().As<IExecutionConstrains<ISnapshotPartitionConfiguration>>();

			_rdoManagerMock = new Mock<IRdoManager>();
			containerBuilder.RegisterInstance(_rdoManagerMock.Object).As<IRdoManager>();
			
			IContainer container = containerBuilder.Build();
			_instance = container.Resolve<IExecutionConstrains<ISnapshotPartitionConfiguration>>();
		}

		[Test]
		[TestCase(100, ExpectedResult = false)]
		[TestCase(200, ExpectedResult = true)]
		public async Task<bool> CanExecuteAsyncTests(int testTotalNumberOfItems)
		{
			// Arrange
			const int numberOfItems = 100;
			const int batchSize = 100;

			_snapshotPartitionConfiguration.SetupGet(x => x.BatchSize).Returns(batchSize);
			_snapshotPartitionConfiguration.SetupGet(x => x.TotalRecordsCount).Returns(testTotalNumberOfItems);
			const int batchArtifactId = 1;
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

			_rdoManagerMock.Setup(x => x.GetAsync<SyncBatchRdo>(It.IsAny<int>(), batchArtifactId))
				.ReturnsAsync(() => new SyncBatchRdo
				{
					ArtifactId = 1,
					TotalDocumentsCount = numberOfItems
				});

			// Act
			bool actualResult = await _instance.CanExecuteAsync(_snapshotPartitionConfiguration.Object, _token).ConfigureAwait(false);

			// Assert
			Mock.Verify(_objectManager);

			return actualResult;
		}

		[Test]
		public void CanExecuteAsyncReadBatchQueryThrowsErrorTest()
		{
			// Arrange
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), 1, 1)).Throws<NotAuthorizedException>().Verifiable();

			// Act & Assert
			Assert.ThrowsAsync<NotAuthorizedException>(async () => await _instance.CanExecuteAsync(_snapshotPartitionConfiguration.Object, _token).ConfigureAwait(false));

			Mock.Verify(_objectManager);
			_syncLog.Verify(x => x.LogError(It.IsAny<NotAuthorizedException>(), It.Is<string>(y => y == "Exception occurred when looking for created batches.")));
		}
	}
}