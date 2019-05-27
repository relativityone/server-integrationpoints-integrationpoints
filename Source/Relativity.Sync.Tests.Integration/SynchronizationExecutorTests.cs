using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Tests.Common;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed class SynchronizationExecutorTests
	{
		private ConfigurationStub _config;
		private IExecutor<ISynchronizationConfiguration> _executor;
		private Mock<IObjectManager> _objectManagerMock;
		private Mock<ISyncImportBulkArtifactJob> _importBulkArtifactJob;

		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 123;

		private static readonly Guid BatchObjectTypeGuid = new Guid("18C766EB-EB71-49E4-983E-FFDE29B1A44E");
		private static readonly Guid FailedItemsCountGuid = new Guid("DC3228E4-2765-4C3B-B3B1-A0F054E280F6");

		private static readonly Guid LockedByGuid = new Guid("BEFC75D3-5825-4479-B499-58C6EF719DDB");
		private static readonly Guid ProgressGuid = new Guid("8C6DAF67-9428-4F5F-98D7-3C71A1FF3AE8");
		private static readonly Guid StartingIndexGuid = new Guid("B56F4F70-CEB3-49B8-BC2B-662D481DDC8A");
		private static readonly Guid StatusGuid = new Guid("D16FAF24-BC87-486C-A0AB-6354F36AF38E");

		private static readonly Guid TotalItemsCountGuid = new Guid("F84589FE-A583-4EB3-BA8A-4A2EEE085C81");
		private static readonly Guid TransferredItemsCountGuid = new Guid("B2D112CA-E81E-42C7-A6B2-C0E89F32F567");

		[SetUp]
		public void SetUp()
		{
			ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.MockStepsExcept<ISynchronizationConfiguration>(containerBuilder);
			containerBuilder.RegisterType<ImportJobFactoryStub>().As<IImportJobFactory>();

			_importBulkArtifactJob = new Mock<ISyncImportBulkArtifactJob>();
			containerBuilder.RegisterInstance(_importBulkArtifactJob.Object).As<ISyncImportBulkArtifactJob>();

			Mock<ISyncMetrics> syncMetrics = new Mock<ISyncMetrics>();
			containerBuilder.RegisterInstance(syncMetrics.Object).As<ISyncMetrics>();

			Mock<ISemaphoreSlim> semaphoreSlim = new Mock<ISemaphoreSlim>();
			containerBuilder.RegisterInstance(semaphoreSlim.Object).As<ISemaphoreSlim>();

			_config = new ConfigurationStub()
			{
				SourceWorkspaceArtifactId = _SOURCE_WORKSPACE_ARTIFACT_ID,
				FieldMappings = new List<FieldMap>()
				{
					new FieldMap()
					{
						DestinationField = new FieldEntry
						{
							IsIdentifier = true,
						}
					}
				},
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure
			};
			containerBuilder.RegisterInstance(_config).AsImplementedInterfaces();

			_objectManagerMock = new Mock<IObjectManager>();
			var destinationServiceFactoryForUser = new Mock<IDestinationServiceFactoryForUser>();
			var sourceServiceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();
			var sourceServiceFactoryForAdmin = new Mock<ISourceServiceFactoryForAdmin>();

			destinationServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).Returns(Task.FromResult(_objectManagerMock.Object));
			sourceServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).Returns(Task.FromResult(_objectManagerMock.Object));
			sourceServiceFactoryForAdmin.Setup(x => x.CreateProxyAsync<IObjectManager>()).Returns(Task.FromResult(_objectManagerMock.Object));

			containerBuilder.RegisterInstance(destinationServiceFactoryForUser.Object).As<IDestinationServiceFactoryForUser>();
			containerBuilder.RegisterInstance(sourceServiceFactoryForUser.Object).As<ISourceServiceFactoryForUser>();
			containerBuilder.RegisterInstance(sourceServiceFactoryForAdmin.Object).As<ISourceServiceFactoryForAdmin>();
			containerBuilder.RegisterType<SynchronizationExecutor>().As<IExecutor<ISynchronizationConfiguration>>();

			CorrelationId correlationId = new CorrelationId(Guid.NewGuid().ToString());

			containerBuilder.RegisterInstance(new EmptyLogger()).As<ISyncLog>();
			containerBuilder.RegisterInstance(correlationId).As<CorrelationId>();

			IContainer container = containerBuilder.Build();
			_executor = container.Resolve<IExecutor<ISynchronizationConfiguration>>();
		}

		[Test]
		public async Task ItShouldSuccessfullyRunImportAndTagDocuments()
		{
			const int newBatchArtifactId = 1234;
			const int numberOfNewBatches = 1;
			QueryResult queryResultForNewBatches = new QueryResult()
			{
				Objects = new List<RelativityObject>()
				{
					new RelativityObject
					{
						ArtifactID = newBatchArtifactId
					}
				},
				TotalCount = numberOfNewBatches
			};

			_objectManagerMock.Setup(x => x.QueryAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, It.Is<QueryRequest>(q => q.ObjectType.Guid == BatchObjectTypeGuid), It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(queryResultForNewBatches)
				.Verifiable();

			const int totalItemsCount = 10;
			ReadResult readResultForBatch = new ReadResult()
			{
				Object = new RelativityObject()
				{
					FieldValues = new List<FieldValuePair>()
					{
						CreateFieldValuePair(TotalItemsCountGuid, totalItemsCount),
						CreateFieldValuePair(StartingIndexGuid, 0),
						CreateFieldValuePair(StatusGuid, BatchStatus.New.ToString()),
						CreateFieldValuePair(FailedItemsCountGuid, 0),
						CreateFieldValuePair(TransferredItemsCountGuid, 0),
						CreateFieldValuePair(ProgressGuid, (decimal)0),
						CreateFieldValuePair(LockedByGuid, "locked by")
					}
				}
			};

			_objectManagerMock.Setup(x => x.ReadAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, It.Is<ReadRequest>(r => r.Object.ArtifactID == newBatchArtifactId)))
				.ReturnsAsync(readResultForBatch)
				.Verifiable();
			
			_objectManagerMock.Setup(x => x.RetrieveResultsBlockFromExportAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(GetDocumentsToTag(totalItemsCount))
				.Verifiable();

			MassUpdateResult massUpdateResult = new MassUpdateResult()
			{
				Success = true
			};
			_objectManagerMock.Setup(x => x.UpdateAsync(_SOURCE_WORKSPACE_ARTIFACT_ID, 
					It.Is<MassUpdateByObjectIdentifiersRequest>(request => request.Objects.Count == totalItemsCount),
					It.Is<MassUpdateOptions>(options => options.UpdateBehavior == FieldUpdateBehavior.Merge), It.IsAny<CancellationToken>()))
					.ReturnsAsync(massUpdateResult)
					.Verifiable();

			// act
			ExecutionResult result = await _executor.ExecuteAsync(_config, CancellationToken.None).ConfigureAwait(false);

			// assert
			result.Status.Should().Be(ExecutionStatus.Completed);
			_objectManagerMock.Verify();
			_importBulkArtifactJob.Verify(x => x.Execute(), Times.Once);
		}

		private RelativityObjectSlim[] GetDocumentsToTag(int totalItemsCount)
		{
			RelativityObjectSlim[] documentsToTag = new RelativityObjectSlim[totalItemsCount];
			for (int i = 0; i < documentsToTag.Length; i++)
			{
				const int someNumber = 100;
				documentsToTag[i] = new RelativityObjectSlim()
				{
					ArtifactID = someNumber + i
				};
			}

			return documentsToTag;
		}

		private FieldValuePair CreateFieldValuePair(Guid guid, object value)
		{
			return new FieldValuePair()
			{
				Field = new Field()
				{
					Guids = new List<Guid>()
					{
						guid
					}
				},
				Value = value
			};
		}
	}
}