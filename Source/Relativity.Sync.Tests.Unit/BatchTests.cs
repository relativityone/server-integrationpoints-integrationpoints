using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class BatchTests
	{
		private BatchRepository _batchRepository;
		private Mock<IObjectManager> _objectManager;

		private const int _WORKSPACE_ID = 433;
		private const int _ARTIFACT_ID = 416;

		private static readonly Guid BatchObjectTypeGuid = new Guid("18C766EB-EB71-49E4-983E-FFDE29B1A44E");

		private static readonly Guid TotalItemsCountGuid = new Guid("F84589FE-A583-4EB3-BA8A-4A2EEE085C81");
		private static readonly Guid StartingIndexGuid = new Guid("B56F4F70-CEB3-49B8-BC2B-662D481DDC8A");
		private static readonly Guid StatusGuid = new Guid("D16FAF24-BC87-486C-A0AB-6354F36AF38E");
		private static readonly Guid FailedItemsCountGuid = new Guid("DC3228E4-2765-4C3B-B3B1-A0F054E280F6");
		private static readonly Guid TransferredItemsCountGuid = new Guid("B2D112CA-E81E-42C7-A6B2-C0E89F32F567");
		private static readonly Guid ProgressGuid = new Guid("8C6DAF67-9428-4F5F-98D7-3C71A1FF3AE8");
		private static readonly Guid LockedByGuid = new Guid("BEFC75D3-5825-4479-B499-58C6EF719DDB");

		[SetUp]
		public void SetUp()
		{
			Mock<ISourceServiceFactoryForAdmin> serviceFactoryMock = new Mock<ISourceServiceFactoryForAdmin>();
			_batchRepository = new BatchRepository(serviceFactoryMock.Object);

			_objectManager = new Mock<IObjectManager>();
			serviceFactoryMock.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
		}

		[Test]
		public void ItShouldPreventCreatingRecordWithStartingIndexGreaterThanTotal()
		{
			const int startingIndex = 200;
			const int totalRecordCount = 100;

			// ACT
			Func<Task> action = async () => await _batchRepository.CreateAsync(_WORKSPACE_ID, 0, totalRecordCount, startingIndex).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<ArgumentException>();
		}

		[Test]
		public async Task ItShouldCreateBatch()
		{
			const int syncConfigurationArtifactId = 634;
			const int totalItemsCount = 10000;
			const int startingIndex = 5000;

			CreateResult result = new CreateResult
			{
				Object = new RelativityObject
				{
					ArtifactID = _ARTIFACT_ID
				}
			};
			_objectManager.Setup(x => x.CreateAsync(_WORKSPACE_ID, It.IsAny<CreateRequest>())).ReturnsAsync(result);

			// ACT
			IBatch batch = await _batchRepository.CreateAsync(_WORKSPACE_ID, syncConfigurationArtifactId, totalItemsCount, startingIndex).ConfigureAwait(false);

			// ASSERT
			batch.TotalItemsCount.Should().Be(totalItemsCount);
			batch.StartingIndex.Should().Be(startingIndex);
			batch.ArtifactId.Should().Be(_ARTIFACT_ID);

			_objectManager.Verify(x => x.CreateAsync(_WORKSPACE_ID, It.Is<CreateRequest>(cr => AssertCreateRequest(cr, totalItemsCount, startingIndex))), Times.Once);
		}

		private bool AssertCreateRequest(CreateRequest createRequest, int totalItemsCount, int startingIndex)
		{
			createRequest.ObjectType.Guid.Should().Be(BatchObjectTypeGuid);
			const int two = 2;
			createRequest.FieldValues.Count().Should().Be(two);
			createRequest.FieldValues.Should().Contain(x => x.Field.Guid == TotalItemsCountGuid);
			createRequest.FieldValues.First(x => x.Field.Guid == TotalItemsCountGuid).Value.Should().Be(totalItemsCount);
			createRequest.FieldValues.Should().Contain(x => x.Field.Guid == StartingIndexGuid);
			createRequest.FieldValues.First(x => x.Field.Guid == StartingIndexGuid).Value.Should().Be(startingIndex);
			return true;
		}

		[Test]
		public async Task ItShouldReadBatch()
		{
			const int totalItemsCount = 1123;
			const int startingIndex = 532;
			const string status = "status";
			const int failedItemsCount = 111;
			const int transferredItemsCount = 222;
			const double progress = 3.1;
			const string lockedBy = "id";

			ReadResult readResult = PrepareReadResult(totalItemsCount, startingIndex, status, failedItemsCount, transferredItemsCount, progress, lockedBy);

			_objectManager.Setup(x => x.ReadAsync(_WORKSPACE_ID, It.IsAny<ReadRequest>())).ReturnsAsync(readResult);

			// ACT
			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			batch.ArtifactId.Should().Be(_ARTIFACT_ID);
			batch.TotalItemsCount.Should().Be(totalItemsCount);
			batch.StartingIndex.Should().Be(startingIndex);
			batch.Status.Should().Be(status);
			batch.FailedItemsCount.Should().Be(failedItemsCount);
			batch.TransferredItemsCount.Should().Be(transferredItemsCount);
			batch.Progress.Should().Be(progress);
			batch.LockedBy.Should().Be(lockedBy);

			_objectManager.Verify(x => x.ReadAsync(_WORKSPACE_ID, It.Is<ReadRequest>(rr => AssertReadRequest(rr))), Times.Once);
		}

#pragma warning disable RG2011 // Method Argument Count Analyzer
		private static ReadResult PrepareReadResult(int totalItemsCount, int startingIndex, string status, int failedItemsCount, int transferredItemsCount, double progress,
			string lockedBy)
#pragma warning restore RG2011 // Method Argument Count Analyzer
		{
			ReadResult readResult = new ReadResult
			{
				Object = new RelativityObject
				{
					ArtifactID = _ARTIFACT_ID,
					FieldValues = new List<FieldValuePair>
					{
						new FieldValuePair
						{
							Field = new Field
							{
								Guids = new List<Guid> {TotalItemsCountGuid}
							},
							Value = totalItemsCount
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
								Guids = new List<Guid> {ProgressGuid}
							},
							Value = progress
						},
						new FieldValuePair
						{
							Field = new Field
							{
								Guids = new List<Guid> {LockedByGuid}
							},
							Value = lockedBy
						}
					}
				}
			};
			return readResult;
		}

		private bool AssertReadRequest(ReadRequest readRequest)
		{
			readRequest.Object.ArtifactID.Should().Be(_ARTIFACT_ID);
			const int seven = 7;
			readRequest.Fields.Count().Should().Be(seven);
			readRequest.Fields.Should().Contain(x => x.Guid == TotalItemsCountGuid);
			readRequest.Fields.Should().Contain(x => x.Guid == StartingIndexGuid);
			readRequest.Fields.Should().Contain(x => x.Guid == StatusGuid);
			readRequest.Fields.Should().Contain(x => x.Guid == FailedItemsCountGuid);
			readRequest.Fields.Should().Contain(x => x.Guid == TransferredItemsCountGuid);
			readRequest.Fields.Should().Contain(x => x.Guid == ProgressGuid);
			readRequest.Fields.Should().Contain(x => x.Guid == LockedByGuid);
			return true;
		}

		[Test]
		public async Task ItShouldUpdateFailedItemsCount()
		{
			const int failedItemsCount = 9876;

			ReadResult readResult = PrepareReadResult();
			_objectManager.Setup(x => x.ReadAsync(_WORKSPACE_ID, It.IsAny<ReadRequest>())).ReturnsAsync(readResult);

			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// ACT
			await batch.SetFailedItemsCountAsync(failedItemsCount).ConfigureAwait(false);

			// ASSERT
			batch.FailedItemsCount.Should().Be(failedItemsCount);

			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => AssertUpdateRequest(up, FailedItemsCountGuid, failedItemsCount))));
		}

		[Test]
		public async Task ItShouldUpdateTransferredItemsCount()
		{
			const int transferredItemsCount = 849170;

			ReadResult readResult = PrepareReadResult();
			_objectManager.Setup(x => x.ReadAsync(_WORKSPACE_ID, It.IsAny<ReadRequest>())).ReturnsAsync(readResult);

			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// ACT
			await batch.SetTransferredItemsCountAsync(transferredItemsCount).ConfigureAwait(false);

			// ASSERT
			batch.TransferredItemsCount.Should().Be(transferredItemsCount);

			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => AssertUpdateRequest(up, TransferredItemsCountGuid, transferredItemsCount))));
		}

		[Test]
		public async Task ItShouldUpdateLockedBy()
		{
			const string lockedBy = "worker 1";

			ReadResult readResult = PrepareReadResult();
			_objectManager.Setup(x => x.ReadAsync(_WORKSPACE_ID, It.IsAny<ReadRequest>())).ReturnsAsync(readResult);

			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// ACT
			await batch.SetLockedByAsync(lockedBy).ConfigureAwait(false);

			// ASSERT
			batch.LockedBy.Should().Be(lockedBy);

			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => AssertUpdateRequest(up, LockedByGuid, lockedBy))));
		}

		[Test]
		public async Task ItShouldUpdateProgress()
		{
			const double progress = 55.5;

			ReadResult readResult = PrepareReadResult();
			_objectManager.Setup(x => x.ReadAsync(_WORKSPACE_ID, It.IsAny<ReadRequest>())).ReturnsAsync(readResult);

			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// ACT
			await batch.SetProgressAsync(progress).ConfigureAwait(false);

			// ASSERT
			batch.Progress.Should().Be(progress);

			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => AssertUpdateRequest(up, ProgressGuid, progress))));
		}

		[Test]
		public async Task ItShouldUpdateStatus()
		{
			const string status = "in progress";

			ReadResult readResult = PrepareReadResult();
			_objectManager.Setup(x => x.ReadAsync(_WORKSPACE_ID, It.IsAny<ReadRequest>())).ReturnsAsync(readResult);

			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			// ACT
			await batch.SetStatusAsync(status).ConfigureAwait(false);

			// ASSERT
			batch.Status.Should().Be(status);

			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => AssertUpdateRequest(up, StatusGuid, status))));
		}

		[Test]
		public async Task ItShouldNotSetFailedItemsCountWhenUpdateFails()
		{
			const int newValue = 876536;

			ReadResult readResult = PrepareReadResult();
			_objectManager.Setup(x => x.ReadAsync(_WORKSPACE_ID, It.IsAny<ReadRequest>())).ReturnsAsync(readResult);
			_objectManager.Setup(x => x.UpdateAsync(_WORKSPACE_ID, It.IsAny<UpdateRequest>())).Throws<ArgumentNullException>();

			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			int oldValue = batch.FailedItemsCount;

			// ACT
			Func<Task> action = async () => await batch.SetFailedItemsCountAsync(newValue).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<ArgumentNullException>();
			batch.FailedItemsCount.Should().Be(oldValue);
			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => up.FieldValues.Any(f => f.Field.Guid == FailedItemsCountGuid))));
		}

		[Test]
		public async Task ItShouldNotSetTransferredItemsCountWhenUpdateFails()
		{
			const int newValue = 85743;

			ReadResult readResult = PrepareReadResult();
			_objectManager.Setup(x => x.ReadAsync(_WORKSPACE_ID, It.IsAny<ReadRequest>())).ReturnsAsync(readResult);
			_objectManager.Setup(x => x.UpdateAsync(_WORKSPACE_ID, It.IsAny<UpdateRequest>())).Throws<ArgumentNullException>();

			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			int oldValue = batch.TransferredItemsCount;

			// ACT
			Func<Task> action = async () => await batch.SetTransferredItemsCountAsync(newValue).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<ArgumentNullException>();
			batch.TransferredItemsCount.Should().Be(oldValue);
			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => up.FieldValues.Any(f => f.Field.Guid == TransferredItemsCountGuid))));
		}

		[Test]
		public async Task ItShouldNotSetLockedByWhenUpdateFails()
		{
			const string newValue = "worker 2";

			ReadResult readResult = PrepareReadResult();
			_objectManager.Setup(x => x.ReadAsync(_WORKSPACE_ID, It.IsAny<ReadRequest>())).ReturnsAsync(readResult);
			_objectManager.Setup(x => x.UpdateAsync(_WORKSPACE_ID, It.IsAny<UpdateRequest>())).Throws<ArgumentNullException>();

			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			string oldValue = batch.LockedBy;

			// ACT
			Func<Task> action = async () => await batch.SetLockedByAsync(newValue).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<ArgumentNullException>();
			batch.LockedBy.Should().Be(oldValue);
			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => up.FieldValues.Any(f => f.Field.Guid == LockedByGuid))));
		}

		[Test]
		public async Task ItShouldNotSetStatusWhenUpdateFails()
		{
			const string newValue = "completed";

			ReadResult readResult = PrepareReadResult();
			_objectManager.Setup(x => x.ReadAsync(_WORKSPACE_ID, It.IsAny<ReadRequest>())).ReturnsAsync(readResult);
			_objectManager.Setup(x => x.UpdateAsync(_WORKSPACE_ID, It.IsAny<UpdateRequest>())).Throws<ArgumentNullException>();

			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			string oldValue = batch.Status;

			// ACT
			Func<Task> action = async () => await batch.SetStatusAsync(newValue).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<ArgumentNullException>();
			batch.Status.Should().Be(oldValue);
			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => up.FieldValues.Any(f => f.Field.Guid == StatusGuid))));
		}

		[Test]
		public async Task ItShouldNotSetProgressWhenUpdateFails()
		{
			const double newValue = 99.9;

			ReadResult readResult = PrepareReadResult();
			_objectManager.Setup(x => x.ReadAsync(_WORKSPACE_ID, It.IsAny<ReadRequest>())).ReturnsAsync(readResult);
			_objectManager.Setup(x => x.UpdateAsync(_WORKSPACE_ID, It.IsAny<UpdateRequest>())).Throws<ArgumentNullException>();

			IBatch batch = await _batchRepository.GetAsync(_WORKSPACE_ID, _ARTIFACT_ID).ConfigureAwait(false);

			double oldValue = batch.Progress;

			// ACT
			Func<Task> action = async () => await batch.SetProgressAsync(newValue).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<ArgumentNullException>();
			batch.Progress.Should().Be(oldValue);
			_objectManager.Verify(x => x.UpdateAsync(_WORKSPACE_ID, It.Is<UpdateRequest>(up => up.FieldValues.Any(f => f.Field.Guid == ProgressGuid))));
		}

		private bool AssertUpdateRequest<T>(UpdateRequest updateRequest, Guid fieldGuid, T value)
		{
			updateRequest.Object.ArtifactID.Should().Be(_ARTIFACT_ID);
			updateRequest.FieldValues.Count().Should().Be(1);
			updateRequest.FieldValues.Should().Contain(x => x.Field.Guid == fieldGuid);
			updateRequest.FieldValues.Should().Contain(x => ((T) x.Value).Equals(value));
			return true;
		}

		private static ReadResult PrepareReadResult()
		{
			const int totalItemsCount = 1111;
			const int startingIndex = 222;
			const string status = "status";
			const int failedItemsCount = 333;
			const int transferredItemsCount = 444;
			const double progress = 53.1;
			const string lockedBy = "id";

			return PrepareReadResult(totalItemsCount, startingIndex, status, failedItemsCount, transferredItemsCount, progress, lockedBy);
		}
	}
}