using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.RDOs;
using Relativity.Sync.Storage;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class BatchTests
	{
		private BatchRepository _batchRepository;
		private Mock<IObjectManager> _objectManager;
		private Mock<IDateTime> _dateTime;

		private const int _WORKSPACE_ID = 433;
		private const int _ARTIFACT_ID = 416;
		private const string _NAME_FIELD_NAME = "Name";
		private const string _PARENT_OBJECT_FIELD_NAME = "SyncConfiguration";

		private static readonly Guid BatchObjectTypeGuid = new Guid("18C766EB-EB71-49E4-983E-FFDE29B1A44E");
		
		private static readonly Guid TotalDocumentsCountGuid = new Guid("C30CE15E-45D6-49E6-8F62-7C5AA45A4694");
		private static readonly Guid TransferredDocumentsCountGuid = new Guid("A5618A97-48C5-441C-86DF-2867481D30AB");
		private static readonly Guid FailedDocumentsCountGuid = new Guid("4FA1CF50-B261-4157-BD2D-50619F0347D6");
		private static readonly Guid StartingIndexGuid = new Guid("B56F4F70-CEB3-49B8-BC2B-662D481DDC8A");
		private static readonly Guid StatusGuid = new Guid("D16FAF24-BC87-486C-A0AB-6354F36AF38E");
		private static readonly Guid TaggedDocumentsCountGuid = new Guid("AF3C2398-AF49-4537-9BC3-D79AE1734A8C");
		private static readonly Guid FailedItemsCountGuid = new Guid("DC3228E4-2765-4C3B-B3B1-A0F054E280F6");
		private static readonly Guid TransferredItemsCountGuid = new Guid("B2D112CA-E81E-42C7-A6B2-C0E89F32F567");
		private static readonly Guid MetadataBytesTransferredGuid = new Guid("2BE1C011-0A0C-4A10-B77A-74BB9405A68A");
		private static readonly Guid FilesBytesTransferredGuid = new Guid("4A21D596-6961-4389-8210-8D2D8C56DBAD");
		private static readonly Guid TotalBytesTransferredGuid = new Guid("511C4B49-2E05-4DFB-BB3E-771EC0BDEFED");

		[SetUp]
		public void SetUp()
		{
			var serviceFactoryMock = new Mock<ISourceServiceFactoryForAdmin>();
			_dateTime = new Mock<IDateTime>();
			_batchRepository = new BatchRepository(serviceFactoryMock.Object, _dateTime.Object);

			_objectManager = new Mock<IObjectManager>();
			serviceFactoryMock.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
		}

		[Test]
		public async Task CreateAsync_ShouldCreateBatch()
		{
			const int syncConfigurationArtifactId = 634;
			const int totalDocumentsCount = 10000;
			const int startingIndex = 5000;
			string defaultStatus = BatchStatus.New.GetDescription();

			CreateResult result = new CreateResult
			{
				Object = new RelativityObject
				{
					ArtifactID = _ARTIFACT_ID
				}
			};
			_objectManager.Setup(x => x.CreateAsync(_WORKSPACE_ID, It.IsAny<CreateRequest>())).ReturnsAsync(result);

			// ACT
			IBatch batch = await _batchRepository.CreateAsync(_WORKSPACE_ID, syncConfigurationArtifactId, totalDocumentsCount, startingIndex).ConfigureAwait(false);

			// ASSERT
			batch.TotalDocumentsCount.Should().Be(totalDocumentsCount);
			batch.StartingIndex.Should().Be(startingIndex);
			batch.ArtifactId.Should().Be(_ARTIFACT_ID);

			_objectManager.Verify(x => x.CreateAsync(_WORKSPACE_ID, It.Is<CreateRequest>(cr => AssertCreateRequest(cr, totalDocumentsCount, startingIndex, syncConfigurationArtifactId, defaultStatus))), Times.Once);
		}

		private bool AssertCreateRequest(CreateRequest createRequest, int totalItemsCount, int startingIndex, int syncConfigurationArtifactId, string batchStatus)
		{
			createRequest.ObjectType.Guid.Should().Be(BatchObjectTypeGuid);
			createRequest.ParentObject.ArtifactID.Should().Be(syncConfigurationArtifactId);
			const int expectedNumberOfFields = 4;
			createRequest.FieldValues.Count().Should().Be(expectedNumberOfFields);
			createRequest.FieldValues.Should().Contain(x => x.Field.Name == _NAME_FIELD_NAME);
			createRequest.FieldValues.First(x => x.Field.Name == _NAME_FIELD_NAME).Value.ToString().Should().NotBeNullOrWhiteSpace();
			createRequest.FieldValues.Should().Contain(x => x.Field.Guid == TotalDocumentsCountGuid);
			createRequest.FieldValues.First(x => x.Field.Guid == TotalDocumentsCountGuid).Value.Should().Be(totalItemsCount);
			createRequest.FieldValues.Should().Contain(x => x.Field.Guid == StartingIndexGuid);
			createRequest.FieldValues.First(x => x.Field.Guid == StartingIndexGuid).Value.Should().Be(startingIndex);
			createRequest.FieldValues.Should().Contain(x => x.Field.Guid == StatusGuid);
			createRequest.FieldValues.First(x => x.Field.Guid == StatusGuid).Value.Should().Be(batchStatus);
			return true;
		}

		[Test]
		public async Task GetAsync_ShouldGetBatch()
		{
			const int totalDocumentsCount = 1123;
			const int startingIndex = 532;
			const string statusDescription = "Completed With Errors";
			const BatchStatus status = BatchStatus.CompletedWithErrors;
			const int failedItemsCount = 111;
			const int transferredItemsCount = 222;
			const int failedDocumentsCount = 333;
			const int transferredDocumentsCount = 444;
			const int taggedDocumentsCount = 555;
			const int metadataBytesTransferred = 1024;
			const int filesBytesTransferred = 5120;
			const int totalBytesTransferred = 6144;
			
			QueryResult queryResult = PrepareQueryResult(
				totalDocumentsCount: totalDocumentsCount, 
				startingIndex: startingIndex,
				status: statusDescription, 
				failedItemsCount: failedItemsCount, 
				transferredItemsCount: transferredItemsCount,
				failedDocumentsCount: failedDocumentsCount,
				transferredDocumentsCount: transferredDocumentsCount,
				taggedDocumentsCount: taggedDocumentsCount, 
				metadataBytesTransferred: metadataBytesTransferred,
				filesBytesTransferred: filesBytesTransferred,
				totalBytesTransferred: totalBytesTransferred);

			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(queryResult);

			// ACT
			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// ASSERT
			batch.ArtifactId.Should().Be(_ARTIFACT_ID);
			batch.TotalDocumentsCount.Should().Be(totalDocumentsCount);
			batch.StartingIndex.Should().Be(startingIndex);
			batch.Status.Should().Be(status);
			batch.FailedItemsCount.Should().Be(failedItemsCount);
			batch.TransferredItemsCount.Should().Be(transferredItemsCount);
			batch.FailedDocumentsCount.Should().Be(failedDocumentsCount);
			batch.TransferredDocumentsCount.Should().Be(transferredDocumentsCount);
			batch.TaggedDocumentsCount.Should().Be(taggedDocumentsCount);
			batch.MetadataBytesTransferred.Should().Be(metadataBytesTransferred);
			batch.FilesBytesTransferred.Should().Be(filesBytesTransferred);
			batch.TotalBytesTransferred.Should().Be(totalBytesTransferred);

			_objectManager.Verify(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(queryRequest => AssertQueryRequest(queryRequest)), 0, 1), Times.Once);
		}

		[Test]
		public void GetAsync_ShouldThrow_WhenBatchNotFound()
		{
			QueryResult queryResult = new QueryResult()
			{
				Objects = new List<RelativityObject>()
			};
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, 1)).ReturnsAsync(queryResult);

			// ACT
			Func<Task> action = () => _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID);

			// ASSERT
			action.Should().Throw<SyncException>().Which.Message.Should().Be($"Batch ArtifactID: {_ARTIFACT_ID} not found.");
		}
		
		[Test]
		public async Task GetAsync_ShouldHandleNullValues()
		{
			// total items count and starting index are set during creation and cannot be modified
			const BatchStatus status = BatchStatus.Started;
			int? failedItemsCount = null;
			int? transferredItemsCount = null;
			int? failedDocumentsCount = null;
			int? transferredDocumentsCount = null;
			int? taggedItemsCount = null;
			int? metadataBytesTransferred = null;
			int? filesBytesTransferred = null;
			int? totalBytesTransferred = null;

			QueryResult queryResult = PrepareQueryResult(
				failedItemsCount: failedItemsCount, 
				transferredItemsCount: transferredItemsCount,
				failedDocumentsCount: failedDocumentsCount,
				transferredDocumentsCount: transferredDocumentsCount,
				taggedDocumentsCount: taggedItemsCount,
				metadataBytesTransferred: metadataBytesTransferred,
				filesBytesTransferred: filesBytesTransferred,
				totalBytesTransferred: totalBytesTransferred);
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, 1)).ReturnsAsync(queryResult);

			// ACT
			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// ASSERT
			batch.Status.Should().Be(status);
			batch.FailedItemsCount.Should().Be(0);
			batch.TransferredItemsCount.Should().Be(0);
			batch.FailedDocumentsCount.Should().Be(0);
			batch.TransferredDocumentsCount.Should().Be(0);
			batch.TaggedDocumentsCount.Should().Be(0);
			batch.MetadataBytesTransferred.Should().Be(0);
			batch.FilesBytesTransferred.Should().Be(0);
			batch.TotalBytesTransferred.Should().Be(0);
		}

#pragma warning disable RG2011 // Method Argument Count Analyzer
		private static QueryResult PrepareQueryResult(int totalDocumentsCount = 1, int startingIndex = 1, string status = "Started", int? failedItemsCount = 1, int? transferredItemsCount = 1,
			int? failedDocumentsCount = 1, int? transferredDocumentsCount = 1, int? taggedDocumentsCount = 1, int? metadataBytesTransferred = 1024, int? filesBytesTransferred = 0, int? totalBytesTransferred = 1024, int? artifactId = null)
		{
			QueryResult readResult = new QueryResult
			{
				Objects = new List<RelativityObject>()
				{
					PrepareObject(totalDocumentsCount, startingIndex, status, failedItemsCount, transferredItemsCount, failedDocumentsCount, transferredDocumentsCount, taggedDocumentsCount,
						metadataBytesTransferred, filesBytesTransferred, totalBytesTransferred, artifactId)
				}
			};

			readResult.TotalCount = readResult.Objects.Count();
			return readResult;
		}
		
		private static QueryResultSlim PrepareQueryResultSlim()
		{
			return new QueryResultSlim
			{
				Objects = new List<RelativityObjectSlim>
				{
					PrepareObjectSlim(),
					PrepareObjectSlim()
				}
			};
		}

		private static RelativityObject PrepareObject(int totalDocumentsCount = 1, int startingIndex = 1, string status = "New", int? failedItemsCount = 1, int? transferredItemsCount = 1,
			int? failedDocumentsCount = 1, int? transferredDocumentsCount = 1, int? taggedItemsCount = 1,
			int? metadataBytesTransferred = 1024, int? filesBytesTransferred = 0, int? totalBytesTransferred = 1024, int? artifactId = null)
#pragma warning restore RG2011 // Method Argument Count Analyzer
		{
			return new RelativityObject
			{
				ArtifactID = artifactId ?? _ARTIFACT_ID,
				FieldValues = new List<FieldValuePair>
				{
					new FieldValuePair
					{
						Field = new Field
						{
							Guids = new List<Guid> {TotalDocumentsCountGuid}
						},
						Value = totalDocumentsCount
					},
					new FieldValuePair
					{
						Field = new Field
						{
							Guids = new List<Guid> {StartingIndexGuid}
						},
						Value = startingIndex
					},
					new FieldValuePair
					{
						Field = new Field
						{
							Guids = new List<Guid> {StatusGuid}
						},
						Value = status
					},
					new FieldValuePair
					{
						Field = new Field
						{
							Guids = new List<Guid> {FailedItemsCountGuid}
						},
						Value = failedItemsCount
					},
					new FieldValuePair
					{
						Field = new Field
						{
							Guids = new List<Guid> {TransferredItemsCountGuid}
						},
						Value = transferredItemsCount
					},
					new FieldValuePair
					{
						Field = new Field
						{
							Guids = new List<Guid> {FailedDocumentsCountGuid}
						},
						Value = failedDocumentsCount
					},
					new FieldValuePair
					{
						Field = new Field
						{
							Guids = new List<Guid> {TransferredDocumentsCountGuid}
						},
						Value = transferredDocumentsCount
					},
					new FieldValuePair
					{
						Field = new Field
						{
							Guids = new List<Guid> {TaggedDocumentsCountGuid}
						},
						Value = taggedItemsCount
					},
					new FieldValuePair
					{
						Field = new Field
						{
							Guids = new List<Guid> {MetadataBytesTransferredGuid}
						},
						Value = metadataBytesTransferred
					},
					new FieldValuePair
					{
						Field = new Field
						{
							Guids = new List<Guid> {FilesBytesTransferredGuid}
						},
						Value = filesBytesTransferred
					},
					new FieldValuePair
					{
						Field = new Field
						{
							Guids = new List<Guid> {TotalBytesTransferredGuid}
						},
						Value = totalBytesTransferred
					}
				}
			};
		}

		private static RelativityObjectSlim PrepareObjectSlim()
		{
			return new RelativityObjectSlim
			{
				ArtifactID = _ARTIFACT_ID
			};
		}

		private bool AssertQueryRequest(QueryRequest queryRequest)
		{
			queryRequest.Condition.Should().Be($"'ArtifactID' == {_ARTIFACT_ID}");
			IList<FieldRef> fields = queryRequest.Fields.ToList();
			AssertReadFields(fields);
			return true;
		}

		private static void AssertReadFields(IList<FieldRef> fields)
		{
			const int expectedNumberOfFields = 8;
			fields.Count().Should().Be(expectedNumberOfFields);
			fields.Should().Contain(x => x.Guid == TotalDocumentsCountGuid);
			fields.Should().Contain(x => x.Guid == StartingIndexGuid);
			fields.Should().Contain(x => x.Guid == StatusGuid);
			fields.Should().Contain(x => x.Guid == FailedItemsCountGuid);
			fields.Should().Contain(x => x.Guid == TransferredItemsCountGuid);
			fields.Should().Contain(x => x.Guid == FailedDocumentsCountGuid);
			fields.Should().Contain(x => x.Guid == TransferredDocumentsCountGuid);
			fields.Should().Contain(x => x.Guid == TaggedDocumentsCountGuid);
		}

		[Test]
		public async Task SetFailedItemsCountAsync_ShouldUpdateFailedItemsCount()
		{
			const int failedItemsCount = 9876;

			QueryResult queryResult = PrepareQueryResult();
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, 1)).ReturnsAsync(queryResult);

			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// ACT
			await batch.SetFailedItemsCountAsync(failedItemsCount).ConfigureAwait(false);

			// ASSERT
			batch.FailedItemsCount.Should().Be(failedItemsCount);

			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => AssertUpdateRequest(up, FailedItemsCountGuid, failedItemsCount))));
		}

		[Test]
		public async Task SetTransferredItemsCountAsync_ShouldUpdateTransferredItemsCount()
		{
			const int transferredItemsCount = 849170;

			QueryResult queryResult = PrepareQueryResult();
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, 1)).ReturnsAsync(queryResult);

			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// ACT
			await batch.SetTransferredItemsCountAsync(transferredItemsCount).ConfigureAwait(false);

			// ASSERT
			batch.TransferredItemsCount.Should().Be(transferredItemsCount);

			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => AssertUpdateRequest(up, TransferredItemsCountGuid, transferredItemsCount))));
		}

		[Test]
		public async Task SetMetadataBytesTransferredAsync_ShouldUpdateMetadataBytesTransferred()
		{
			const long metadataBytesTransferred = 1024;

			QueryResult queryResult = PrepareQueryResult();
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, 1)).ReturnsAsync(queryResult);

			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// ACT
			await batch.SetMetadataBytesTransferredAsync(metadataBytesTransferred).ConfigureAwait(false);

			// ASSERT
			batch.MetadataBytesTransferred.Should().Be(metadataBytesTransferred);

			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => AssertUpdateRequest(up, MetadataBytesTransferredGuid, metadataBytesTransferred))));
		}

		[Test]
		public async Task SetFilesBytesTransferredAsync_ShouldUpdateFilesBytesTransferredGuid()
		{
			const long filesBytesTransferred = 5120;

			QueryResult queryResult = PrepareQueryResult();
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, 1)).ReturnsAsync(queryResult);

			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// ACT
			await batch.SetFilesBytesTransferredAsync(filesBytesTransferred).ConfigureAwait(false);

			// ASSERT
			batch.FilesBytesTransferred.Should().Be(filesBytesTransferred);

			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => AssertUpdateRequest(up, FilesBytesTransferredGuid, filesBytesTransferred))));
		}

		[Test]
		public async Task SetTotalBytesTransferredAsync_ShouldUpdateTotalBytesTransferred()
		{
			const long totalBytesTransferred = 6144;

			QueryResult queryResult = PrepareQueryResult();
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, 1)).ReturnsAsync(queryResult);

			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// ACT
			await batch.SetTotalBytesTransferredAsync(totalBytesTransferred).ConfigureAwait(false);

			// ASSERT
			batch.TotalBytesTransferred.Should().Be(totalBytesTransferred);

			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => AssertUpdateRequest(up, TotalBytesTransferredGuid, totalBytesTransferred))));
		}

		[Test]
		public async Task SetLockedByAsync_ShouldUpdateLockedBy()
		{
			const int failedDocumentsCount = 111;

			QueryResult queryResult = PrepareQueryResult();
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, 1)).ReturnsAsync(queryResult);

			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// ACT
			await batch.SetFailedDocumentsCountAsync(failedDocumentsCount).ConfigureAwait(false);

			// ASSERT
			batch.FailedDocumentsCount.Should().Be(failedDocumentsCount);

			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => AssertUpdateRequest(up, FailedDocumentsCountGuid, failedDocumentsCount))));
		}

		[Test]
		public async Task SetTransferredDocumentsCountAsync_ShouldUpdateTransferredDocumentsCount()
		{
			const int transferredDocumentsCount = 222;

			QueryResult queryResult = PrepareQueryResult();
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, 1)).ReturnsAsync(queryResult);

			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// ACT
			await batch.SetTransferredDocumentsCountAsync(transferredDocumentsCount).ConfigureAwait(false);

			// ASSERT
			batch.TransferredDocumentsCount.Should().Be(transferredDocumentsCount);

			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => AssertUpdateRequest(up, TransferredDocumentsCountGuid, transferredDocumentsCount))));
		}

		[Test]
		public async Task SetStatusAsync_ShouldUpdateStatus()
		{
			const BatchStatus status = BatchStatus.InProgress;
			const string expectedStatusDescription = "In Progress";

			QueryResult queryResult = PrepareQueryResult();
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, 1)).ReturnsAsync(queryResult);

			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// ACT
			await batch.SetStatusAsync(status).ConfigureAwait(false);

			// ASSERT
			batch.Status.Should().Be(status);

			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => AssertUpdateRequest(up, StatusGuid, expectedStatusDescription))));
		}

		[Test]
		public async Task SetTaggedDocumentsCountAsync_ShouldUpdateTaggedDocumentsCount()
		{
			const int taggedDocumentsCount = 849170;

			QueryResult queryResult = PrepareQueryResult();
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, 1)).ReturnsAsync(queryResult);

			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// ACT
			await batch.SetTaggedDocumentsCountAsync(taggedDocumentsCount).ConfigureAwait(false);

			// ASSERT
			batch.TaggedDocumentsCount.Should().Be(taggedDocumentsCount);

			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => AssertUpdateRequest(up, TaggedDocumentsCountGuid, taggedDocumentsCount))));
		}

		[Test]
		public async Task SetFailedItemsCountAsync_ShouldNotSetFailedItemsCount_WhenUpdateFails()
		{
			const int newValue = 876536;

			SetupObjectManagerForUpdatingBatchFields();
			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			int oldValue = batch.FailedItemsCount;

			// ACT
			Func<Task> action = () => batch.SetFailedItemsCountAsync(newValue);

			// ASSERT
			action.Should().Throw<ArgumentNullException>();
			batch.FailedItemsCount.Should().Be(oldValue);
			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => up.FieldValues.Any(f => f.Field.Guid == FailedItemsCountGuid))));
		}

		[Test]
		public async Task SetTransferredItemsCountAsync_ShouldNotSetTransferredItemsCount_WhenUpdateFails()
		{
			const int newValue = 85743;

			SetupObjectManagerForUpdatingBatchFields();
			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			int oldValue = batch.TransferredItemsCount;

			// ACT
			Func<Task> action = () => batch.SetTransferredItemsCountAsync(newValue);

			// ASSERT
			action.Should().Throw<ArgumentNullException>();
			batch.TransferredItemsCount.Should().Be(oldValue);
			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => up.FieldValues.Any(f => f.Field.Guid == TransferredItemsCountGuid))));
		}

		[Test]
		public async Task SetFailedDocumentsCountAsync_ShouldNotChangeValue_WhenUpdateFails()
		{
			const int newValue = 4444;

			SetupObjectManagerForUpdatingBatchFields();
			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			int oldValue = batch.FailedDocumentsCount;

			// ACT
			Func<Task> action = () => batch.SetFailedDocumentsCountAsync(newValue);

			// ASSERT
			action.Should().Throw<ArgumentNullException>();
			batch.FailedDocumentsCount.Should().Be(oldValue);
			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => up.FieldValues.Any(f => f.Field.Guid == FailedDocumentsCountGuid))));
		}

		[Test]
		public async Task SetTransferredDocumentsCountAsync_ShouldNotChangeValue_WhenUpdateFails()
		{
			const int newValue = 5555;
			SetupObjectManagerForUpdatingBatchFields();
			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			int oldValue = batch.TransferredDocumentsCount;

			// ACT
			Func<Task> action = () => batch.SetTransferredDocumentsCountAsync(newValue);

			// ASSERT
			action.Should().Throw<ArgumentNullException>();
			batch.TransferredDocumentsCount.Should().Be(oldValue);
			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => up.FieldValues.Any(f => f.Field.Guid == TransferredDocumentsCountGuid))));
		}

		[Test]
		public async Task SetStatusAsync_ShouldNotSetStatus_WhenUpdateFails()
		{
			const BatchStatus newValue = BatchStatus.Completed;

			SetupObjectManagerForUpdatingBatchFields();
			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			BatchStatus oldValue = batch.Status;

			// ACT
			Func<Task> action = () => batch.SetStatusAsync(newValue);

			// ASSERT
			action.Should().Throw<ArgumentNullException>();
			batch.Status.Should().Be(oldValue);
			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => up.FieldValues.Any(f => f.Field.Guid == StatusGuid))));
		}

		[Test]
		public async Task SetTaggedItemsCountAsync_ShouldNotSetTaggedItemsCount_WhenUpdateFails()
		{
			const int newValue = 85743;

			SetupObjectManagerForUpdatingBatchFields();
			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			int oldValue = batch.TaggedDocumentsCount;

			// ACT
			Func<Task> action = () => batch.SetTaggedDocumentsCountAsync(newValue);

			// ASSERT
			action.Should().Throw<ArgumentNullException>();
			batch.TaggedDocumentsCount.Should().Be(oldValue);
			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => up.FieldValues.Any(f => f.Field.Guid == TaggedDocumentsCountGuid))));
		}

		private void SetupObjectManagerForUpdatingBatchFields()
		{
			QueryResult queryResult = PrepareQueryResult();
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, 1)).ReturnsAsync(queryResult);
			_objectManager.Setup(x => x.UpdateAsync(_WORKSPACE_ID, It.IsAny<UpdateRequest>())).Throws<ArgumentNullException>();
		}

		private bool AssertUpdateRequest<T>(UpdateRequest updateRequest, Guid fieldGuid, T value)
		{
			updateRequest.Object.ArtifactID.Should().Be(_ARTIFACT_ID);
			updateRequest.FieldValues.Count().Should().Be(1);
			updateRequest.FieldValues.Should().Contain(x => x.Field.Guid == fieldGuid);
			updateRequest.FieldValues.Should().Contain(x => ((T)x.Value).Equals(value));
			return true;
		}

		[Test]
		public async Task GetLastAsync_ShouldReturnNull_WhenNoBatchesFound()
		{
			const int syncConfigurationArtifactId = 845967;

			QueryResult queryResult = new QueryResult();
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(queryResult);

			// ACT
			IBatch batch = await _batchRepository.GetLastAsync(_WORKSPACE_ID, syncConfigurationArtifactId).ConfigureAwait(false);

			// ASSERT
			batch.Should().BeNull();
		}

		[Test]
		public async Task GetLastAsync_ShouldReturnLastBatch()
		{
			const int syncConfigurationArtifactId = 845967;

			QueryResult queryResult = PrepareQueryResult();
			queryResult.TotalCount = 1;

			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, 1)).ReturnsAsync(queryResult);

			// ACT
			IBatch batch = await _batchRepository.GetLastAsync(_WORKSPACE_ID, syncConfigurationArtifactId).ConfigureAwait(false);

			// ASSERT
			batch.Should().NotBeNull();
			_objectManager.Verify(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(qr => AssertQueryRequest(qr, syncConfigurationArtifactId)), 1, 1), Times.Once);
		}

		private bool AssertQueryRequest(QueryRequest queryRequest, int syncConfigurationArtifactId)
		{
			queryRequest.ObjectType.Guid.Should().Be(BatchObjectTypeGuid);
			queryRequest.Condition.Should().Be($"'{_PARENT_OBJECT_FIELD_NAME}' == OBJECT {syncConfigurationArtifactId}");
			IList<FieldRef> fields = queryRequest.Fields.ToList();
			AssertReadFields(fields);
			return true;
		}

		private bool AssertQueryRequestSlim(QueryRequest queryRequest, int syncConfigurationArtifactId)
		{
			queryRequest.ObjectType.Guid.Should().Be(BatchObjectTypeGuid);
			queryRequest.Condition.Should().Be($"'{_PARENT_OBJECT_FIELD_NAME}' == OBJECT {syncConfigurationArtifactId}");
			return true;
		}

		[Test]
		public async Task GetAllBatchesIdsToExecuteAsync_ShouldReturnPausedAndThenNewBatchIds()
		{
			// Arrange
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(r => r.Condition.Contains("New")), 1, int.MaxValue)).ReturnsAsync(PrepareQueryResult(status: BatchStatus.New.GetDescription(), artifactId: 1));
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(r => r.Condition.Contains("Paused")), 1, int.MaxValue)).ReturnsAsync(PrepareQueryResult(status: BatchStatus.Paused.GetDescription(), artifactId: 2));

			// Act
			IEnumerable<int> batchIds = await _batchRepository.GetAllBatchesIdsToExecuteAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// Assert
			batchIds.Should().NotBeNullOrEmpty();
			batchIds.Should().ContainInOrder(new[] {2, 1});

			VerifyQueryAllRequests();
		}

		[Test]
		public async Task GetAllNewBatchesIdsAsync_ShouldReturnNoBatchIds_WhenNoNewBatchesExist()
		{
			// Arrange
			var queryResult = new QueryResult();
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, int.MaxValue)).ReturnsAsync(queryResult);

			// Act
			IEnumerable<int> batchIds = await _batchRepository.GetAllBatchesIdsToExecuteAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// Assert
			batchIds.Should().NotBeNull();
			batchIds.Should().BeEmpty();
			batchIds.Any().Should().BeFalse();

			VerifyQueryAllRequests();
		}

		[Test]
		public void GetAllNewBatchesIdsAsync_ShouldThrow_WhenItFailsToQueryForNewBatches()
		{
			// Arrange
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, int.MaxValue)).Throws<NotAuthorizedException>();

			// Act & Assert
			Assert.ThrowsAsync<NotAuthorizedException>(() => _batchRepository.GetAllBatchesIdsToExecuteAsync(_WORKSPACE_ID, _ARTIFACT_ID));

			VerifyQueryAllRequests();
		}

		[Test]
		public async Task GetAllAsync_ShouldReadAllBatches()
		{
			const int syncConfigurationArtifactId = 634;

			QueryResultSlim queryResultSlim = PrepareQueryResultSlim();
			queryResultSlim.TotalCount = queryResultSlim.Objects.Count;
			_objectManager.Setup(x => x.QuerySlimAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1, int.MaxValue)).ReturnsAsync(queryResultSlim);

			QueryResult queryResult = PrepareQueryResult();
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, 1)).ReturnsAsync(queryResult);

			// ACT
			IEnumerable<IBatch> batches = await _batchRepository.GetAllAsync(_WORKSPACE_ID, syncConfigurationArtifactId).ConfigureAwait(false);

			// ASSERT
			batches.Should().NotBeNullOrEmpty();
			batches.Should().NotContainNulls();

			_objectManager.Verify(x => x.QuerySlimAsync(_WORKSPACE_ID, It.Is<QueryRequest>(rr => AssertQueryRequestSlim(rr, syncConfigurationArtifactId)), 1, int.MaxValue), Times.Once);
		}

		[Test]
		public async Task DeleteAllForConfiguration_ShouldDeleteAllBatchesForGivenConfiguration()
		{
			const int syncConfigurationArtifactId = 634;

			// ACT
			await _batchRepository.DeleteAllForConfigurationAsync(_WORKSPACE_ID, syncConfigurationArtifactId).ConfigureAwait(false);

			// ASSERT
			_objectManager.Verify(x => x.DeleteAsync(_WORKSPACE_ID, It.Is<MassDeleteByCriteriaRequest>(request => AssertMassDeleteByCriteriaRequest(request, syncConfigurationArtifactId))));
		}

		[Test]
		public async Task DeleteAllOlderThan_ShouldDeleteOnlyBatchesOlderThanSpecifiedTimespan()
		{
#pragma warning disable RG2009 // Hardcoded Numeric Value
			DateTime utcNow = new DateTime(2019, 10, 21);
			_dateTime.SetupGet(x => x.UtcNow).Returns(utcNow);

			DateTime newBatchCreationDate = new DateTime(2019, 10, 20);
			const int newConfigurationArtifactId = 222;

			DateTime oldBatchCreationDate = new DateTime(2019, 9, 9);
			const int oldConfigurationArtifactId = 111;

			TimeSpan removeOlderThan = TimeSpan.FromDays(1);

			RelativityObject CreateObject(int configurationArtifactId, DateTime date)
			{
				return new RelativityObject()
				{
					ArtifactID = configurationArtifactId,
					FieldValues = new List<FieldValuePair>()
					{
						new FieldValuePair()
						{
							Field = new Field()
							{
								Name = "System Created On"
							},
							Value = date
						}
					}
				};
			}

			QueryResult queryResult = new QueryResult()
			{
				Objects = new List<RelativityObject>()
				{
					CreateObject(oldConfigurationArtifactId, oldBatchCreationDate),
					CreateObject(newConfigurationArtifactId, newBatchCreationDate)
				}
			};
			_objectManager.Setup(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(request => request.ObjectType.Guid == new Guid(SyncRdoGuids.SyncConfigurationGuid) &&
				request.Fields.Single().Name == "System Created On"),
				It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(queryResult);

			// ACT
			await _batchRepository.DeleteAllOlderThanAsync(_WORKSPACE_ID, removeOlderThan).ConfigureAwait(false);

			// ASSERT
			_objectManager.Verify(x => x.DeleteAsync(_WORKSPACE_ID, It.Is<MassDeleteByCriteriaRequest>(request => AssertMassDeleteByCriteriaRequest(request, oldConfigurationArtifactId))),
				Times.Once);
			_objectManager.Verify(x => x.DeleteAsync(_WORKSPACE_ID, It.Is<MassDeleteByCriteriaRequest>(request => AssertMassDeleteByCriteriaRequest(request, newConfigurationArtifactId))),
				Times.Never);

#pragma warning restore RG2009 // Hardcoded Numeric Value
		}


		private bool AssertMassDeleteByCriteriaRequest(MassDeleteByCriteriaRequest request, int syncConfigurationArtifactId)
		{
			return
				request.ObjectIdentificationCriteria.ObjectType.Guid == BatchObjectTypeGuid &&
				request.ObjectIdentificationCriteria.Condition == $"'{_PARENT_OBJECT_FIELD_NAME}' == OBJECT {syncConfigurationArtifactId}";
		}
		
		private void VerifyQueryAllRequests()
		{
			void VerifyStatusWasRead(BatchStatus status)
			{
				string expectedCondition =
					$"'{_PARENT_OBJECT_FIELD_NAME}' == OBJECT {_ARTIFACT_ID} AND '{StatusGuid}' == '{status.GetDescription()}'";
				_objectManager.Verify(x => x.QueryAsync(_WORKSPACE_ID, It.Is<QueryRequest>(rr => rr.ObjectType.Guid == BatchObjectTypeGuid && rr.Condition == expectedCondition), 1, int.MaxValue), Times.Once);
			}
			
			VerifyStatusWasRead(BatchStatus.Paused);
			VerifyStatusWasRead(BatchStatus.New);
		}
	}
}