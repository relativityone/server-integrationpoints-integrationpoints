using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using Moq.Language;
using NUnit.Framework;
using Relativity.Services.Interfaces.File;
using Relativity.Services.Interfaces.File.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	internal sealed partial class SourceWorkspaceDataReaderTests : IDisposable
	{
		private SourceDataReaderConfiguration _configuration;
		private SourceWorkspaceDataReader _instance;
		private Mock<IObjectManager> _objectManager;
		private Mock<IFileManager> _fileManager;


		private static readonly Guid BatchObjectTypeGuid = new Guid("18C766EB-EB71-49E4-983E-FFDE29B1A44E");

		private static readonly Guid NameGuid = new Guid("3AB49704-F843-4E09-AFF2-5380B1BF7A35");
		private static readonly Guid TotalItemsCountGuid = new Guid("F84589FE-A583-4EB3-BA8A-4A2EEE085C81");
		private static readonly Guid StartingIndexGuid = new Guid("B56F4F70-CEB3-49B8-BC2B-662D481DDC8A");
		private static readonly Guid StatusGuid = new Guid("D16FAF24-BC87-486C-A0AB-6354F36AF38E");
		private static readonly Guid FailedItemsCountGuid = new Guid("DC3228E4-2765-4C3B-B3B1-A0F054E280F6");
		private static readonly Guid TransferredItemsCountGuid = new Guid("B2D112CA-E81E-42C7-A6B2-C0E89F32F567");
		private static readonly Guid ProgressGuid = new Guid("8C6DAF67-9428-4F5F-98D7-3C71A1FF3AE8");

		private static readonly Guid LockedByGuid = new Guid("BEFC75D3-5825-4479-B499-58C6EF719DDB");
		private static readonly Guid SyncConfigurationRelationGuid = new Guid("F673E67F-E606-4155-8E15-CA1C83931E16");

		[SetUp]
		public void SetUp()
		{
			_configuration = new SourceDataReaderConfiguration
			{
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.None,
				MetadataMapping = new MetadataMapping(DestinationFolderStructureBehavior.None, 0, new List<FieldMap>()),
				RunId = Guid.NewGuid(),
				SourceJobId = 0,
				SourceWorkspaceId = 0,
				SyncConfigurationId = 0
			};

			Mock<ISourceServiceFactoryForUser> userServiceFactory = new Mock<ISourceServiceFactoryForUser>();
			Mock<ISourceServiceFactoryForAdmin> adminServiceFactory = new Mock<ISourceServiceFactoryForAdmin>();

			_objectManager = new Mock<IObjectManager>();
			SetupServiceMock(_objectManager, userServiceFactory, adminServiceFactory);

			_fileManager = new Mock<IFileManager>();
			SetupServiceMock(_fileManager, userServiceFactory, adminServiceFactory);

			ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.MockReporting(containerBuilder);
			containerBuilder.RegisterInstance(userServiceFactory.Object).As<ISourceServiceFactoryForUser>();
			containerBuilder.RegisterInstance(adminServiceFactory.Object).As<ISourceServiceFactoryForAdmin>();
			IContainer container = containerBuilder.Build();

			_instance = new SourceWorkspaceDataReader(_configuration,
				container.Resolve<IBatchDataTableBuilderFactory>(),
				container.Resolve<IRelativityExportBatcher>(),
				Mock.Of<ISyncLog>());
		}

		private void SetupServiceMock<T>(Mock<T> serviceMock,
			Mock<ISourceServiceFactoryForUser> userServiceFactory,
			Mock<ISourceServiceFactoryForAdmin> adminServiceFactory) where T : class, IDisposable
		{
			userServiceFactory.Setup(x => x.CreateProxyAsync<T>())
				.ReturnsAsync(serviceMock.Object);
			adminServiceFactory.Setup(x => x.CreateProxyAsync<T>())
				.ReturnsAsync(serviceMock.Object);
		}

		public void Dispose()
		{
			_instance?.Dispose();
		}

		[Test]
		public void ItShouldReadAcrossMultipleBatches()
		{
			// Arrange
			List<Dictionary<string, object>> testData = GenerateMultipleBatchesTestCase(0, 1);
			const int totalItemCount = 15;
			const int batchSize = 5;
			const int numFields = 5;

			SetupBatches(totalItemCount, batchSize);
			SetupExportResultsBlocks(numFields, totalItemCount, batchSize);

			_fileManager.Setup(x => x.GetNativesForSearchAsync(0, null))
				.ReturnsAsync(Array.Empty<FileResponse>());

			// Act
			bool result = _instance.Read();


			// Assert
			result.Should().Be(true);
		}

		public void ItShouldThrowProperExceptionWhenExportFails()
		{

		}

		public void ItShouldThrowProperExceptionWhenNativeFileQueryFails()
		{

		}

		public void ItShouldThrowProperExceptionWhenFolderPathQueryFails()
		{

		}

		public void ItShouldReadDestinationTags()
		{

		}

		public void ItShouldHandleDestinationFolderStructureBehavior()
		{

		}

		private void SetupExportResultsBlocks(int numFields, int totalItemCount, int batchSize, int startingIndex = 0)
		{
			for (int i = startingIndex; i < totalItemCount; i += batchSize)
			{
				int exportIndexId = i;
				int resultsBlockSize = Math.Min(batchSize, totalItemCount - i);
				RelativityObjectSlim[] block = GenerateBatch(resultsBlockSize, numFields);
				_objectManager.Setup(x => MatchingRetrieveResultsBlockFromExportAsync(x, resultsBlockSize, exportIndexId))
					.ReturnsAsync(block);
			}
		}

		private void SetupBatches(int totalItemCount, int batchSize, int startingIndex = 0)
		{
			List<QueryResult> results = new List<QueryResult>();
			for (int i = startingIndex; i < totalItemCount; i += batchSize)
			{
				int totalItemsInBatch = Math.Min(batchSize, totalItemCount - i);
				var result = new QueryResult
				{
					TotalCount = 1,
					Objects = new List<RelativityObject>
					{
						BatchObject(totalItemsInBatch, i, "New")
					}
				};
				results.Add(result);
			}

			ISetupSequentialResult<Task<QueryResult>> setupAssertion = _objectManager.SetupSequence(x => AnyQueryAsync(x));
			foreach (QueryResult result in results)
			{
				setupAssertion.ReturnsAsync(result);
			}
		}

		private void SetupNatives()
		{

		}

		private static FieldValuePair FieldValue(Guid guid, object value)
		{
			return new FieldValuePair
			{
				Field = new Field { Guids = new List<Guid> { guid } },
				Value = value
			};
		}

		private static Task<QueryResult> AnyQueryAsync(IObjectManager objectManager)
		{
			return objectManager.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>());
		}

		private static Task<RelativityObjectSlim[]> MatchingRetrieveResultsBlockFromExportAsync(IObjectManager x, int resultsBlockSize, int exportIndexID)
		{
			return x.RetrieveResultsBlockFromExportAsync(It.IsAny<int>(), It.IsAny<Guid>(), resultsBlockSize, exportIndexID);
		}

		private static RelativityObject BatchObject(int totalItemCount, int startingIndex, string status)
		{
			return new RelativityObject
			{
				FieldValues = new List<FieldValuePair>
				{
					FieldValue(TotalItemsCountGuid, totalItemCount),
					FieldValue(StartingIndexGuid, startingIndex),
					FieldValue(StatusGuid, status)
				}
			};
		}

		private static RelativityObjectSlim[] GenerateBatch(int size, int numValues = 1)
		{
			return Enumerable.Range(0, size)
				.Select(_ => GenerateObject(numValues))
				.ToArray();
		}

		private static RelativityObjectSlim GenerateObject(int numValues)
		{
			var obj = new RelativityObjectSlim
			{
				ArtifactID = Guid.NewGuid().GetHashCode(),
				Values = Enumerable.Range(0, numValues).Select(_ => new object()).ToList()
			};
			return obj;
		}
	}
}
