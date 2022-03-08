using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	public sealed class DestinationWorkspaceTagRepositoryTests
	{
		private Mock<IFederatedInstance> _federatedInstance;
		private Mock<ITagNameFormatter> _tagNameFormatter;
		private Mock<IObjectManager> _objectManager;
		private Mock<ISyncMetrics> _syncMetrics;
		private Mock<IStopwatch> _stopWatch;

		private ISyncLog _syncLog;
		private CancellationToken _token;

		private DestinationWorkspaceTagRepository _sut;

		private readonly Guid _destinationInstanceNameGuid = new Guid("909adc7c-2bb9-46ca-9f85-da32901d6554");
		private readonly Guid _destinationWorkspaceArtifactIdGuid = new Guid("207e6836-2961-466b-a0d2-29974a4fad36");
		private readonly Guid _destinationWorkspaceNameGuid = new Guid("348d7394-2658-4da4-87d0-8183824adf98");
		private readonly Guid _nameGuid = new Guid("155649c0-db15-4ee7-b449-bfdf2a54b7b5");

		private static readonly Guid _DESTINATION_INSTANCE_ARTIFACT_ID_GUID = new Guid("323458db-8a06-464b-9402-af2516cf47e0");

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_syncLog = new EmptyLogger();
			_token = CancellationToken.None;
		}

		[SetUp]
		public void SetUp()
		{
			var serviceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();
			_federatedInstance = new Mock<IFederatedInstance>();
			_objectManager = new Mock<IObjectManager>();
			_tagNameFormatter = new Mock<ITagNameFormatter>();
			_tagNameFormatter.Setup(x => x.FormatWorkspaceDestinationTagName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns("foo bar");
			serviceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
			_syncMetrics = new Mock<ISyncMetrics>();
			_stopWatch = new Mock<IStopwatch>();

			_sut = new DestinationWorkspaceTagRepository(serviceFactoryForUser.Object, _federatedInstance.Object,
				_tagNameFormatter.Object, new ConfigurationStub(), _syncLog, _syncMetrics.Object, () => _stopWatch.Object);
		}

		[Test]
		public async Task ItShouldReadExistingDestinationWorkspaceTag()
		{
			const int destinationWorkspaceId = 2;
			const string destinationInstanceName = "destination instance";
			const string destinationWorkspaceName = "destination workspace";

			var queryResult = new QueryResult();
			var relativityObject = new RelativityObject
			{
				FieldValues = new List<FieldValuePair>
				{
					new FieldValuePair
					{
						Field = new Field {Guids = new List<Guid> {_destinationInstanceNameGuid}},
						Value = destinationInstanceName
					},
					new FieldValuePair
					{
						Field = new Field {Guids = new List<Guid> {_destinationWorkspaceNameGuid}},
						Value = destinationWorkspaceName
					},
					new FieldValuePair
					{
						Field = new Field {Guids = new List<Guid> {_destinationWorkspaceArtifactIdGuid}},
						Value = destinationWorkspaceId
					}
				}
			};

			queryResult.Objects.Add(relativityObject);
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(),
				_token, It.IsAny<IProgress<ProgressReport>>())).ReturnsAsync(queryResult);

			// act
			DestinationWorkspaceTag tag = await _sut.ReadAsync(0, 0, _token).ConfigureAwait(false);

			// assert
			Assert.AreEqual(relativityObject.ArtifactID, tag.ArtifactId);
			Assert.AreEqual(destinationWorkspaceId, tag.DestinationWorkspaceArtifactId);
			Assert.AreEqual(destinationWorkspaceName, tag.DestinationWorkspaceName);
			Assert.AreEqual(destinationInstanceName, tag.DestinationInstanceName);
		}

		[Test]
		public async Task ItShouldReturnNullWhenReadingNotExistingTag()
		{
			var queryResult = new QueryResult();
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(),
				_token, It.IsAny<IProgress<ProgressReport>>())).ReturnsAsync(queryResult);

			// act
			DestinationWorkspaceTag tag = await _sut.ReadAsync(0, 0, _token).ConfigureAwait(false);

			// assert
			Assert.IsNull(tag);
		}

		[Test]
		public void ItShouldThrowRepositoryExceptionWhenReadServiceCallFails()
		{
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(),
				_token, It.IsAny<IProgress<ProgressReport>>())).Throws<ServiceException>();

			// act
			Func<Task> action = async () => await _sut.ReadAsync(0, 0, _token).ConfigureAwait(false);

			// assert
			action.Should().Throw<SyncKeplerException>().WithInnerException<ServiceException>();
		}

		[Test]
		public void ItShouldThrowRepositoryExceptionWhenReadFails()
		{
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(),
				_token, It.IsAny<IProgress<ProgressReport>>())).Throws<InvalidOperationException>();

			// act
			Func<Task> action = async () => await _sut.ReadAsync(0, 0, _token).ConfigureAwait(false);

			// assert
			action.Should().Throw<SyncKeplerException>().WithInnerException<InvalidOperationException>();
		}

		[Test]
		public async Task ItShouldCreateDestinationWorkspaceTag()
		{
			const int tagArtifactId = 1;
			const int sourceWorkspaceArtifactId = 2;
			const int destinationWorkspaceArtifactId = 3;
			const string destinationWorkspaceName = "workspace";
			const string destinationInstanceName = "instance";

			var createResult = new CreateResult
			{
				Object = new RelativityObject
				{
					ArtifactID = tagArtifactId,
				}
			};
			_objectManager.Setup(x => x.CreateAsync(sourceWorkspaceArtifactId, It.IsAny<CreateRequest>())).ReturnsAsync(createResult);
			_federatedInstance.Setup(x => x.GetInstanceNameAsync()).ReturnsAsync(destinationInstanceName);

			// act
			DestinationWorkspaceTag createdTag = await _sut.CreateAsync(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, destinationWorkspaceName).ConfigureAwait(false);

			// assert
			Assert.AreEqual(tagArtifactId, createdTag.ArtifactId);
			Assert.AreEqual(destinationWorkspaceName, createdTag.DestinationWorkspaceName);
			Assert.AreEqual(destinationInstanceName, createdTag.DestinationInstanceName);
			Assert.AreEqual(destinationWorkspaceArtifactId, createdTag.DestinationWorkspaceArtifactId);
		}

		[Test]
		public void ItShouldThrowRepositoryExceptionWhenCreatingTagServiceCallFails()
		{
			_objectManager.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<CreateRequest>())).Throws<ServiceException>();

			// act
			Func<Task> action = async () => await _sut.CreateAsync(0, 0, string.Empty).ConfigureAwait(false);

			// assert
			action.Should().Throw<SyncKeplerException>().WithInnerException<ServiceException>();
		}

		[Test]
		public void ItShouldThrowRepositoryExceptionWhenCreatingTagFails()
		{
			_objectManager.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<CreateRequest>())).Throws<InvalidOperationException>();

			// act
			Func<Task> action = async () => await _sut.CreateAsync(0, 0, string.Empty).ConfigureAwait(false);

			// assert
			action.Should().Throw<SyncKeplerException>().WithInnerException<InvalidOperationException>();
		}

		[Test]
		public async Task ItShouldUpdateTag()
		{

			const int tagArtifactId = 1;
			const int sourceWorkspaceArtifactId = 2;
			const int destinationWorkspaceArtifactId = 3;
			const int destinationInstanceArtifactId = 4;
			const string destinationInstanceName = "instance";
			const string destinationWorkspaceName = "workspace";
			string destinationTagName = $"{destinationInstanceName} - {destinationWorkspaceName} - {destinationWorkspaceArtifactId}";
			_tagNameFormatter.Setup(x => x.FormatWorkspaceDestinationTagName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(destinationTagName);

			var destinationWorkspaceTag = new DestinationWorkspaceTag()
			{
				ArtifactId = tagArtifactId,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId,
				DestinationWorkspaceName = destinationWorkspaceName,
				DestinationInstanceName = destinationInstanceName,
			};

			_federatedInstance.Setup(x => x.GetInstanceNameAsync()).ReturnsAsync(destinationInstanceName);
			_federatedInstance.Setup(x => x.GetInstanceIdAsync()).ReturnsAsync(destinationInstanceArtifactId);

			// act
			await _sut.UpdateAsync(sourceWorkspaceArtifactId, destinationWorkspaceTag).ConfigureAwait(false);

			// assert

			_objectManager.Verify(x => x.UpdateAsync(sourceWorkspaceArtifactId, It.Is<UpdateRequest>(request =>
				VerifyUpdateRequest(request, tagArtifactId,
					f => _nameGuid.Equals(f.Field.Guid) && string.Equals(f.Value, destinationTagName),
					f => _destinationWorkspaceNameGuid.Equals(f.Field.Guid) && string.Equals(f.Value, destinationWorkspaceName),
					f => _destinationWorkspaceArtifactIdGuid.Equals(f.Field.Guid) && Equals(f.Value, destinationWorkspaceArtifactId),
					f => _destinationInstanceNameGuid.Equals(f.Field.Guid) && string.Equals(f.Value, destinationInstanceName),
					f => _DESTINATION_INSTANCE_ARTIFACT_ID_GUID.Equals(f.Field.Guid) && Equals(f.Value, destinationInstanceArtifactId)))));
		}

		private bool VerifyUpdateRequest(UpdateRequest request, int tagArtifactId, params Predicate<FieldRefValuePair>[] predicates)
		{
			List<FieldRefValuePair> fields = request.FieldValues.ToList();
			bool checkPredicates = true;
			foreach (Predicate<FieldRefValuePair> predicate in predicates)
			{
				checkPredicates = checkPredicates && fields.Exists(predicate);
			}
			return request.Object.ArtifactID == tagArtifactId &&
					fields.Count == predicates.Length &&
					checkPredicates;
		}

		[Test]
		public void ItShouldThrowRepositoryExceptionWhenUpdatingTagServiceCallFails()
		{
			_objectManager.Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateRequest>())).Throws<ServiceException>();

			// act
			Func<Task> action = async () => await _sut.UpdateAsync(0, new DestinationWorkspaceTag()).ConfigureAwait(false);

			// assert
			action.Should().Throw<SyncKeplerException>().WithInnerException<ServiceException>();
		}

		[Test]
		public void ItShouldThrowRepositoryExceptionWhenUpdatingTagFails()
		{
			_objectManager.Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateRequest>())).Throws<InvalidOperationException>();

			// act
			Func<Task> action = async () => await _sut.UpdateAsync(0, new DestinationWorkspaceTag()).ConfigureAwait(false);

			// assert
			action.Should().Throw<SyncKeplerException>().WithInnerException<InvalidOperationException>();
		}

		[TestCaseSource(nameof(QueryTestCases))]
		public async Task ItShouldBuildProperQueryForInstances(int testInstanceId, string expectedQueryFragment)
		{
			// ARRANGE
			const int sourceWorkspaceId = 123;
			const int destinationWorkspaceId = 234;

			_federatedInstance.Setup(fi => fi.GetInstanceIdAsync()).ReturnsAsync(testInstanceId);
			var queryResult = new QueryResult();
			_objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(),
				_token, It.IsAny<IProgress<ProgressReport>>())).ReturnsAsync(queryResult);

			// ACT
			await _sut.ReadAsync(sourceWorkspaceId, destinationWorkspaceId, _token).ConfigureAwait(false);

			// ASSERT
			_objectManager.Verify(
				om => om.QueryAsync(sourceWorkspaceId, It.Is<QueryRequest>(q => q.Condition.Contains(expectedQueryFragment)), 0, 1, _token, It.IsAny<IProgress<ProgressReport>>()),
				Times.Once);
		}

		private static IEnumerable<TestCaseData> QueryTestCases => new[]
		{
			new TestCaseData(-1, $"'{_DESTINATION_INSTANCE_ARTIFACT_ID_GUID}' == -1").SetName($"{nameof(ItShouldBuildProperQueryForInstances)}_Local"),
			new TestCaseData(456, $"'{_DESTINATION_INSTANCE_ARTIFACT_ID_GUID}' == {456}").SetName($"{nameof(ItShouldBuildProperQueryForInstances)}_Federated"),
		};

		[Test]
		public async Task ItShouldReportFailureWhenExceptionThrownTryingToTagDocuments()
		{
			// Arrange
			double expectedElapsedTime = 1000;
			_stopWatch.Setup(x => x.Elapsed).Returns(TimeSpan.FromMilliseconds(expectedElapsedTime));

			const int expectedTotalObjectsUpdated = 0;
			var synchronizationConfiguration = new Mock<ISynchronizationConfiguration>(MockBehavior.Loose);
			int[] testArtifactIds = { 1 };

			_objectManager.Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<MassUpdateByObjectIdentifiersRequest>(), It.IsAny<MassUpdateOptions>(), It.IsAny<CancellationToken>()))
				.Throws<NotAuthorizedException>();

			// Act
			IList<TagDocumentsResult<int>> actualResult = await _sut.TagDocumentsAsync(synchronizationConfiguration.Object, testArtifactIds, _token).ConfigureAwait(false);

			// Assert
			CollectionAssert.IsNotEmpty(actualResult);
			Assert.IsFalse(actualResult[0].Success);
			Assert.AreEqual(expectedTotalObjectsUpdated, actualResult[0].TotalObjectsUpdated);
			CollectionAssert.AreEqual(testArtifactIds, actualResult[0].FailedDocuments);

			VerifySentMetric(m => 
				m.SourceUpdateCount == expectedTotalObjectsUpdated &&
				m.SourceUpdateTime == expectedElapsedTime);
		}

		[Test]
		public async Task ItShouldReportFailureWhenSomeDocumentsAreTagged()
		{
			// Arrange
			double expectedElapsedTime = 1000;
			_stopWatch.Setup(x => x.Elapsed).Returns(TimeSpan.FromMilliseconds(expectedElapsedTime));

			const int expectedTotalObjectsUpdated = 1;
			var synchronizationConfiguration = new Mock<ISynchronizationConfiguration>(MockBehavior.Loose);
			int[] testArtifactIds = { 1, 0 };
			var testMassUpdateResult = new MassUpdateResult
			{
				Message = "Failed completing update query.",
				Success = false,
				TotalObjectsUpdated = expectedTotalObjectsUpdated
			};

			_objectManager.Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<MassUpdateByObjectIdentifiersRequest>(), It.IsAny<MassUpdateOptions>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(testMassUpdateResult);

			// Act
			IList<TagDocumentsResult<int>> actualResult = await _sut.TagDocumentsAsync(synchronizationConfiguration.Object, testArtifactIds, _token).ConfigureAwait(false);

			// Assert
			CollectionAssert.IsNotEmpty(actualResult);
			Assert.IsFalse(actualResult[0].Success);
			Assert.AreEqual(expectedTotalObjectsUpdated, actualResult[0].TotalObjectsUpdated);
			CollectionAssert.AreEqual(new[] { 0 }, actualResult[0].FailedDocuments);

			VerifySentMetric(m => 
				m.SourceUpdateCount == expectedTotalObjectsUpdated && 
				m.SourceUpdateTime == expectedElapsedTime);
		}

		[Test]
		public async Task ItShouldReturnSuccessWhenThereAreNoDocumentsToTag()
		{
			// Arrange
			var synchronizationConfiguration = new Mock<ISynchronizationConfiguration>(MockBehavior.Loose);
			int[] testArtifactIds = Array.Empty<int>();

			// Act
			IList<TagDocumentsResult<int>> actualResult = await _sut.TagDocumentsAsync(synchronizationConfiguration.Object, testArtifactIds, _token).ConfigureAwait(false);

			// Assert
			CollectionAssert.IsNotEmpty(actualResult);
			Assert.IsTrue(actualResult[0].Success);
			Assert.AreEqual(testArtifactIds.Length, actualResult[0].TotalObjectsUpdated);
			Assert.AreEqual("A call to the Mass Update API was not made as there are no objects to update.", actualResult[0].Message);
			CollectionAssert.IsEmpty(actualResult[0].FailedDocuments);

			_syncMetrics.Verify(x => x.Send(It.IsAny<IMetric>()), Times.Never);

			_objectManager.Verify(x => x.UpdateAsync(
				It.IsAny<int>(),
				It.IsAny<MassUpdateByObjectIdentifiersRequest>(),
				It.IsAny<MassUpdateOptions>(),
				It.IsAny<CancellationToken>()),
				Times.Never);
		}

		[Test]
		public async Task ItShouldCreateTwoBatchesAndTagDocuments()
		{
			// Arrange
			double expectedElapsedTime1 = 1000;
			double expectedElapsedTime2 = 2000;
			_stopWatch.SetupSequence(x => x.Elapsed)
				.Returns(TimeSpan.FromMilliseconds(expectedElapsedTime1))
				.Returns(TimeSpan.FromMilliseconds(expectedElapsedTime2));

			const int maxBatchSize = 10000;
			const int expectedNumberOfBatches = 2;
			int firstBatchSize = maxBatchSize;
			int secondBatchSize = maxBatchSize / expectedNumberOfBatches - 1;
			int testBatchSize = firstBatchSize + secondBatchSize;
			int[] testArtifactIds = Enumerable.Repeat(0, testBatchSize).ToArray();

			var synchronizationConfiguration = new Mock<ISynchronizationConfiguration>(MockBehavior.Loose);

			_objectManager.Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<MassUpdateByObjectIdentifiersRequest>(), It.IsAny<MassUpdateOptions>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((int workspace, MassUpdateByObjectIdentifiersRequest request, MassUpdateOptions options, CancellationToken token) =>
			{
				var massUpdateResult = new MassUpdateResult
				{
					TotalObjectsUpdated = request.Objects.Count,
					Success = true
				};
				return massUpdateResult;
			});

			// Act
			IList<TagDocumentsResult<int>> actualResult = await _sut.TagDocumentsAsync(synchronizationConfiguration.Object, testArtifactIds, _token).ConfigureAwait(false);

			// Assert
			Assert.IsNotNull(actualResult);
			Assert.AreEqual(expectedNumberOfBatches, actualResult.Count);

			Assert.IsTrue(actualResult[0].Success);
			Assert.AreEqual(firstBatchSize, actualResult[0].TotalObjectsUpdated);
			CollectionAssert.IsEmpty(actualResult[0].FailedDocuments);

			Assert.IsTrue(actualResult[1].Success);
			Assert.AreEqual(secondBatchSize, actualResult[1].TotalObjectsUpdated);
			CollectionAssert.IsEmpty(actualResult[1].FailedDocuments);

			_objectManager.Verify(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<MassUpdateByObjectIdentifiersRequest>(), It.IsAny<MassUpdateOptions>(), It.IsAny<CancellationToken>()),
				Times.Exactly(expectedNumberOfBatches));

			VerifySentMetric(m => 
				m.SourceUpdateCount == firstBatchSize && 
				m.SourceUpdateTime == expectedElapsedTime1);

			VerifySentMetric(m => 
				m.SourceUpdateCount == secondBatchSize && 
				m.SourceUpdateTime == expectedElapsedTime2);
		}

		[Test]
		public async Task ItShouldTagDocumentsWithCorrectFields()
		{
			// Arrange
			double expectedElapsedTime = 1000;
			_stopWatch.Setup(x => x.Elapsed).Returns(TimeSpan.FromMilliseconds(expectedElapsedTime));

			const int testDestinationWorkspaceTagArtifactId = 102678;
			const int testJobHistoryArtifactId = 101789;
			const int testSourceWorkspaceArtifactId = 101456;
			int[] testArtifactIds = { 0, 1 };

			var synchronizationConfiguration = new Mock<ISynchronizationConfiguration>();
			synchronizationConfiguration.SetupGet(x => x.DestinationWorkspaceTagArtifactId).Returns(testDestinationWorkspaceTagArtifactId);
			synchronizationConfiguration.SetupGet(x => x.JobHistoryArtifactId).Returns(testJobHistoryArtifactId);
			synchronizationConfiguration.SetupGet(x => x.SourceWorkspaceArtifactId).Returns(testSourceWorkspaceArtifactId);

			_objectManager.Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<MassUpdateByObjectIdentifiersRequest>(), It.IsAny<MassUpdateOptions>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((int workspace, MassUpdateByObjectIdentifiersRequest request, MassUpdateOptions options, CancellationToken token) =>
			{
				var massUpdateResult = new MassUpdateResult
				{
					TotalObjectsUpdated = request.Objects.Count,
					Success = true
				};
				return massUpdateResult;
			});

			// Act
			IList<TagDocumentsResult<int>> actualResult = await _sut.TagDocumentsAsync(synchronizationConfiguration.Object, testArtifactIds, _token).ConfigureAwait(false);

			// Assert
			Assert.IsNotNull(actualResult);
			Assert.IsTrue(actualResult[0].Success);
			Assert.AreEqual(testArtifactIds.Length, actualResult[0].TotalObjectsUpdated);
			CollectionAssert.IsEmpty(actualResult[0].FailedDocuments);

			VerifySentMetric(m =>
				m.SourceUpdateCount == testArtifactIds.Length &&
				m.SourceUpdateTime == expectedElapsedTime);

			_objectManager.Verify(x => x.UpdateAsync(
				It.Is<int>(w => w == testSourceWorkspaceArtifactId),
				It.Is<MassUpdateByObjectIdentifiersRequest>(m => VerifyTagUpdateRequest(m, testArtifactIds, testDestinationWorkspaceTagArtifactId, testJobHistoryArtifactId)),
				It.Is<MassUpdateOptions>(u => u.UpdateBehavior == FieldUpdateBehavior.Merge),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		private bool VerifyTagUpdateRequest(MassUpdateByObjectIdentifiersRequest actualUpdateRequest,
			int[] expectedArtifactIds, int expectedDestinationWorkspaceTagArtifactId, int expectedJobHistoryArtifactId)
		{
			const int expectedNumberOfFields = 2;
			var expectedDestinationWorkspaceFieldMultiObject = new Guid("8980C2FA-0D33-4686-9A97-EA9D6F0B4196");
			var expectedJobHistoryFieldMultiObject = new Guid("97BC12FA-509B-4C75-8413-6889387D8EF6");

			Assert.IsNotNull(actualUpdateRequest);

			IReadOnlyList<RelativityObjectRef> actualIdentifiers = actualUpdateRequest.Objects;
			CollectionAssert.IsNotEmpty(actualIdentifiers);
			Assert.AreEqual(expectedArtifactIds.Length, actualIdentifiers.Count);

			for (int i = 0; i < expectedArtifactIds.Length; i++)
			{
				Assert.AreEqual(expectedArtifactIds[i], actualIdentifiers[i].ArtifactID);
			}

			IList<FieldRefValuePair> actualFields = actualUpdateRequest.FieldValues.ToList();
			CollectionAssert.IsNotEmpty(actualFields);
			Assert.AreEqual(expectedNumberOfFields, actualFields.Count);

			FieldRefValuePair actualDestinationTagField = actualFields[0];
			Assert.AreEqual(expectedDestinationWorkspaceFieldMultiObject, actualDestinationTagField.Field.Guid);
			AssertMultiObjectValueContainsId(expectedDestinationWorkspaceTagArtifactId, actualDestinationTagField.Value);

			FieldRefValuePair actualJobHistoryTagField = actualFields[1];
			Assert.AreEqual(expectedJobHistoryFieldMultiObject, actualJobHistoryTagField.Field.Guid);
			AssertMultiObjectValueContainsId(expectedJobHistoryArtifactId, actualJobHistoryTagField.Value);

			return true;
		}

		private static void AssertMultiObjectValueContainsId(int expectedId, object value)
		{
			Assert.IsInstanceOf<IEnumerable<RelativityObjectRef>>(value);
			List<int> valueList = ((IEnumerable<RelativityObjectRef>)value).Select(x => x.ArtifactID).ToList();
			Assert.Contains(expectedId, valueList);
		}

		private void VerifySentMetric(Expression<Func<DestinationWorkspaceTagMetric, bool>> validationFunc)
		{
			_syncMetrics.Verify(x => x.Send(It.Is(validationFunc)));
		}
	}
}