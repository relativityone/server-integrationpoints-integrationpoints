﻿using System;
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
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Tests.Common;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public class SnapshotPartitionExecutorTests
	{
		private CorrelationId _correlationId;
		private CancellationToken _token;

		private Mock<ISyncLog> _syncLog;
		private Mock<IObjectManager> _objectManager;
		private Mock<ISnapshotPartitionConfiguration> _snapshotPartitionConfiguration;

		private IExecutor<ISnapshotPartitionConfiguration> _instance;

		private readonly Guid _failedItemsCountGuid = new Guid("DC3228E4-2765-4C3B-B3B1-A0F054E280F6");
		private readonly Guid _lockedByGuid = new Guid("BEFC75D3-5825-4479-B499-58C6EF719DDB");
		private readonly Guid _progressGuid = new Guid("8C6DAF67-9428-4F5F-98D7-3C71A1FF3AE8");
		private readonly Guid _startingIndexGuid = new Guid("B56F4F70-CEB3-49B8-BC2B-662D481DDC8A");
		private readonly Guid _statusGuid = new Guid("D16FAF24-BC87-486C-A0AB-6354F36AF38E");
		private readonly Guid _totalItemsCountGuid = new Guid("F84589FE-A583-4EB3-BA8A-4A2EEE085C81");
		private readonly Guid _transferredItemsCountGuid = new Guid("B2D112CA-E81E-42C7-A6B2-C0E89F32F567");

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			string correlationGuid = Guid.NewGuid().ToString();
			_correlationId = new CorrelationId(correlationGuid);

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
			containerBuilder.RegisterInstance(_correlationId).As<CorrelationId>();

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

			Mock.Verify(_objectManager);
			_objectManager.Verify(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<CreateRequest>()), Times.Exactly(expectedNumberOfBatches));
			_syncLog.Verify(x => x.LogError(It.IsAny<NotAuthorizedException>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
		}

		[Test]
		public async Task ExecuteAsyncCreatesNoNewBatchesTest()
		{
			// Arrange
			const int numberOfItems = 100;
			const int batchSize = 100;

			QueryResult testBatchResult = GetBatchQueryResult(0, numberOfItems);

			_snapshotPartitionConfiguration.SetupGet(x => x.BatchSize).Returns(batchSize);
			_snapshotPartitionConfiguration.SetupGet(x => x.TotalRecordsCount).Returns(numberOfItems);
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(testBatchResult).Verifiable();

			// Act
			ExecutionResult actualResult = await _instance.ExecuteAsync(_snapshotPartitionConfiguration.Object, _token).ConfigureAwait(false);

			// Assert
			Assert.AreEqual(ExecutionStatus.Completed, actualResult.Status);

			Mock.Verify(_objectManager);
			_objectManager.Verify(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<CreateRequest>()), Times.Never());
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
		}

		[Test]
		public async Task ExecuteAsyncCreateBatchQueryThrowsErrorTest()
		{
			// Arrange
			const int numberOfItems = 100;
			const int batchSize = 100;
			const int totalRecords = 200;

			QueryResult testBatchResult = GetBatchQueryResult(0, numberOfItems);

			_snapshotPartitionConfiguration.SetupGet(x => x.BatchSize).Returns(batchSize);
			_snapshotPartitionConfiguration.SetupGet(x => x.TotalRecordsCount).Returns(totalRecords);
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(testBatchResult).Verifiable();
			_objectManager.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<CreateRequest>())).Throws<NotAuthorizedException>().Verifiable();

			// Act
			ExecutionResult actualResult = await _instance.ExecuteAsync(_snapshotPartitionConfiguration.Object, _token).ConfigureAwait(false);

			// Assert
			Assert.AreEqual(ExecutionStatus.Failed, actualResult.Status);
			Assert.AreEqual("Unable to create batches.", actualResult.Message);

			Mock.Verify(_objectManager);
			_syncLog.Verify(x => x.LogError(
				It.IsAny<NotAuthorizedException>(),
				It.Is<string>(y => y.StartsWith("Unable to create batch", StringComparison.InvariantCulture)),
				It.IsAny<object[]>()), Times.Once);
		}

		private QueryResult GetBatchQueryResult(int startingIndex, int numberOfItems)
		{
			var testBatchResult = new QueryResult
			{
				TotalCount = 1,
				Objects = new List<RelativityObject>
				{
					new RelativityObject
					{
						ArtifactID = 1,
						FieldValues = new List<FieldValuePair>
						{
							new FieldValuePair
							{
								Field = new Field{ Guids = new List<Guid>{_totalItemsCountGuid}},
								Value = numberOfItems
							},
							new FieldValuePair
							{
								Field = new Field{ Guids = new List<Guid>{_startingIndexGuid}},
								Value = startingIndex
							},
							new FieldValuePair
							{
								Field = new Field{ Guids = new List<Guid>{_statusGuid}},
								Value = "New"
							},
							new FieldValuePair
							{
								Field = new Field{ Guids = new List<Guid>{_failedItemsCountGuid}}
							},
							new FieldValuePair
							{
								Field = new Field{ Guids = new List<Guid>{_transferredItemsCountGuid}}
							},
							new FieldValuePair
							{
								Field = new Field{ Guids = new List<Guid>{_progressGuid}}
							},
							new FieldValuePair
							{
								Field = new Field{ Guids = new List<Guid>{_lockedByGuid}}
							}
						}
					}
				}
			};
			return testBatchResult;
		}
	}
}