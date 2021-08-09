using System;
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

		private IExecutionConstrains<ISnapshotPartitionConfiguration> _instance;

		private readonly Guid _totalDocumentsCountGuid = new Guid("C30CE15E-45D6-49E6-8F62-7C5AA45A4694");
		private readonly Guid _transferredDocumentsCountGuid = new Guid("A5618A97-48C5-441C-86DF-2867481D30AB");
		private readonly Guid _failedDocumentsCountGuid = new Guid("4FA1CF50-B261-4157-BD2D-50619F0347D6");
		private readonly Guid _transferredItemsCountGuid = new Guid("B2D112CA-E81E-42C7-A6B2-C0E89F32F567");
		private readonly Guid _failedItemsCountGuid = new Guid("DC3228E4-2765-4C3B-B3B1-A0F054E280F6");
		private readonly Guid _startingIndexGuid = new Guid("B56F4F70-CEB3-49B8-BC2B-662D481DDC8A");
		private readonly Guid _statusGuid = new Guid("D16FAF24-BC87-486C-A0AB-6354F36AF38E");
		private readonly Guid _taggedDocumentsCountGuid = new Guid("AF3C2398-AF49-4537-9BC3-D79AE1734A8C");
		private readonly Guid _metadataBytesTransferredGuid = new Guid("2BE1C011-0A0C-4A10-B77A-74BB9405A68A");
		private readonly Guid _filesBytesTransferredGuid = new Guid("4A21D596-6961-4389-8210-8D2D8C56DBAD");
		private readonly Guid _totalBytesTransferredGuid = new Guid("511C4B49-2E05-4DFB-BB3E-771EC0BDEFED");

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

			QueryResult testBatchResult = GetBatchQueryResult(0, numberOfItems);

			_snapshotPartitionConfiguration.SetupGet(x => x.BatchSize).Returns(batchSize);
			_snapshotPartitionConfiguration.SetupGet(x => x.TotalRecordsCount).Returns(testTotalNumberOfItems);
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(testBatchResult).Verifiable();

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
								Field = new Field{ Guids = new List<Guid>{_totalDocumentsCountGuid}},
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
								Field = new Field{ Guids = new List<Guid>{_failedDocumentsCountGuid}}
							},
							new FieldValuePair
							{
								Field = new Field{ Guids = new List<Guid>{_transferredDocumentsCountGuid}}
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
								Field = new Field{ Guids = new List<Guid>{_taggedDocumentsCountGuid}}
							},
							new FieldValuePair
							{
								Field = new Field{ Guids = new List<Guid>{_metadataBytesTransferredGuid}}
							},
							new FieldValuePair
							{
								Field = new Field{ Guids = new List<Guid>{_filesBytesTransferredGuid}}
							},
							new FieldValuePair
							{
								Field = new Field{ Guids = new List<Guid>{_totalBytesTransferredGuid}}
							}
						}
					}
				}
			};
			return testBatchResult;
		}
	}
}