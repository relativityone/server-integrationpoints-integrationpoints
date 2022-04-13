using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	public  sealed class SourceWorkspaceTagRepositoryTests
	{
		private Mock<IFieldMappings> _fieldMappings;
		private Mock<IObjectManager> _objectManager;
		private Mock<ISyncMetrics> _syncMetrics;
		private Mock<IStopwatch> _stopwatch;

		private IAPILog _syncLog;
		private CancellationToken _token;

		private SourceWorkspaceTagRepository _instance;

		private const string _DESTINATION_IDENTIFIER_FIELD = "Document Identifier";

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_syncLog = new EmptyLogger();
			_token = CancellationToken.None;
		}

		[SetUp]
		public void SetUp()
		{
			_fieldMappings = new Mock<IFieldMappings>();
			_objectManager = new Mock<IObjectManager>();
			_syncMetrics = new Mock<ISyncMetrics>();
			var serviceFactory = new Mock<IDestinationServiceFactoryForUser>();
			serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);

			var destinationIdentifier = new FieldMap
			{
				DestinationField = new FieldEntry
				{
					DisplayName = _DESTINATION_IDENTIFIER_FIELD,
					IsIdentifier = true
				}
			};
			_fieldMappings.Setup(x => x.GetFieldMappings()).Returns(new List<FieldMap>{ destinationIdentifier });

			_stopwatch = new Mock<IStopwatch>();

			_instance = new SourceWorkspaceTagRepository(serviceFactory.Object, _syncLog, _syncMetrics.Object, _fieldMappings.Object, () => _stopwatch.Object);
		}

		[Test]
		public async Task ItShouldReportFailureWhenExceptionThrownTryingToTagDocuments()
		{
			// Arrange
			double expectedElapsedTime = 1000;
			_stopwatch.Setup(x => x.Elapsed).Returns(TimeSpan.FromMilliseconds(expectedElapsedTime));

			const int expectedTotalObjectsUpdated = 0;
			var synchronizationConfiguration = new Mock<ISynchronizationConfiguration>(MockBehavior.Loose);
			string[] testIdentifiers = { "CONTROL_NUMBER_1" };

			_objectManager.Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<MassUpdateByCriteriaRequest>(), It.IsAny<MassUpdateOptions>(), It.IsAny<CancellationToken>()))
				.Throws<NotAuthorizedException>();

			// Act
			IList<TagDocumentsResult<string>> actualResult = await _instance.TagDocumentsAsync(synchronizationConfiguration.Object, testIdentifiers, _token).ConfigureAwait(false);

			// Assert
			CollectionAssert.IsNotEmpty(actualResult);
			Assert.IsFalse(actualResult[0].Success);
			Assert.AreEqual(expectedTotalObjectsUpdated, actualResult[0].TotalObjectsUpdated);
			CollectionAssert.AreEqual(testIdentifiers, actualResult[0].FailedDocuments);

			VerifySentMetric(m =>
				m.DestinationUpdateCount == expectedTotalObjectsUpdated &&
				m.DestinationUpdateTime == expectedElapsedTime);
		}

		[Test]
		public async Task ItShouldReportFailureWhenSomeDocumentsAreTagged()
		{
			// Arrange
			double expectedElapsedTime = 1000;
			_stopwatch.Setup(x => x.Elapsed).Returns(TimeSpan.FromMilliseconds(expectedElapsedTime));

			const int expectedTotalObjectsUpdated = 1;
			var synchronizationConfiguration = new Mock<ISynchronizationConfiguration>(MockBehavior.Loose);
			string[] testIdentifiers = { "CONTROL_NUMBER_1", "CONTROL_NUMBER_2" };
			var testMassUpdateResult = new MassUpdateResult
			{
				Message = "Failed completing update query.",
				Success = false,
				TotalObjectsUpdated = expectedTotalObjectsUpdated
			};

			_objectManager.Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<MassUpdateByCriteriaRequest>(), It.IsAny<MassUpdateOptions>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(testMassUpdateResult);

			// Act
			IList<TagDocumentsResult<string>> actualResult = await _instance.TagDocumentsAsync(synchronizationConfiguration.Object, testIdentifiers, _token).ConfigureAwait(false);

			// Assert
			CollectionAssert.IsNotEmpty(actualResult);
			Assert.IsFalse(actualResult[0].Success);
			Assert.AreEqual(expectedTotalObjectsUpdated, actualResult[0].TotalObjectsUpdated);
			CollectionAssert.AreEqual(new[] { "CONTROL_NUMBER_2" }, actualResult[0].FailedDocuments);

			VerifySentMetric(m =>
				m.DestinationUpdateCount == expectedTotalObjectsUpdated &&
				m.DestinationUpdateTime == expectedElapsedTime);
		}

		[Test]
		public async Task ItShouldReturnSuccessWhenThereAreNoDocumentsToTag()
		{
			// Arrange
			var synchronizationConfiguration = new Mock<ISynchronizationConfiguration>(MockBehavior.Loose);
			string[] testIdentifiers = Array.Empty<string>();

			// Act
			IList<TagDocumentsResult<string>> actualResult = await _instance.TagDocumentsAsync(synchronizationConfiguration.Object, testIdentifiers, _token).ConfigureAwait(false);

			// Assert
			CollectionAssert.IsNotEmpty(actualResult);
			Assert.IsTrue(actualResult[0].Success);
			Assert.AreEqual(testIdentifiers.Length, actualResult[0].TotalObjectsUpdated);
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
			_stopwatch.SetupSequence(x => x.Elapsed)
				.Returns(TimeSpan.FromMilliseconds(expectedElapsedTime1))
				.Returns(TimeSpan.FromMilliseconds(expectedElapsedTime2));

			const int maxBatchSize = 10000;
			const int expectedNumberOfBatches = 2;
			int firstBatchSize = maxBatchSize;
			int secondBatchSize = maxBatchSize / expectedNumberOfBatches - 1;
			int testBatchSize = firstBatchSize + secondBatchSize;
			string[] testIdentifiers = Enumerable.Repeat("CONTROL_NUMBER", testBatchSize).ToArray();

			var synchronizationConfiguration = new Mock<ISynchronizationConfiguration>(MockBehavior.Loose);

			_objectManager.Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<MassUpdateByCriteriaRequest>(), It.IsAny<MassUpdateOptions>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((int workspace, MassUpdateByCriteriaRequest request, MassUpdateOptions options, CancellationToken token) =>
			{
				MatchCollection quotedIdentifiers = Regex.Matches(request.ObjectIdentificationCriteria.Condition, "([\"'])(?:(?=(\\\\?))\\2.)*?\\1");
				var massUpdateResult = new MassUpdateResult
				{
					TotalObjectsUpdated = quotedIdentifiers.Count - 1,	// Remove the quoted condition identifier field
					Success = true
				};
				return massUpdateResult;
			});

			// Act
			IList<TagDocumentsResult<string>> actualResult = await _instance.TagDocumentsAsync(synchronizationConfiguration.Object, testIdentifiers, _token).ConfigureAwait(false);

			// Assert
			Assert.IsNotNull(actualResult);
			Assert.AreEqual(expectedNumberOfBatches, actualResult.Count);

			Assert.IsTrue(actualResult[0].Success);
			Assert.AreEqual(firstBatchSize, actualResult[0].TotalObjectsUpdated);
			CollectionAssert.IsEmpty(actualResult[0].FailedDocuments);

			Assert.IsTrue(actualResult[1].Success);
			Assert.AreEqual(secondBatchSize, actualResult[1].TotalObjectsUpdated);
			CollectionAssert.IsEmpty(actualResult[1].FailedDocuments);

			_objectManager.Verify(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<MassUpdateByCriteriaRequest>(), It.IsAny<MassUpdateOptions>(), It.IsAny<CancellationToken>()),
				Times.Exactly(expectedNumberOfBatches));

			VerifySentMetric(m =>
				m.DestinationUpdateCount == firstBatchSize &&
				m.DestinationUpdateTime == expectedElapsedTime1);

			VerifySentMetric(m =>
				m.DestinationUpdateCount == secondBatchSize &&
				m.DestinationUpdateTime == expectedElapsedTime2);
		}

		[Test]
		public async Task ItShouldTagDocumentsWithCorrectFields()
		{
			// Arrange
			double expectedElapsedTime = 1000;
			_stopwatch.Setup(x => x.Elapsed).Returns(TimeSpan.FromMilliseconds(expectedElapsedTime));

			const int testSourceWorkspaceTagArtifactId = 102678;
			const int testSourceJobHistoryTagArtifactId = 101789;
			const int testDestinationWorkspaceArtifactId = 101456;
			string[] testIdentifiers = { "CONTROL_NUM_3", "CONTROL_NUM_4" };

			var synchronizationConfiguration = new Mock<ISynchronizationConfiguration>();
			synchronizationConfiguration.SetupGet(x => x.SourceWorkspaceTagArtifactId).Returns(testSourceWorkspaceTagArtifactId);
			synchronizationConfiguration.SetupGet(x => x.SourceJobTagArtifactId).Returns(testSourceJobHistoryTagArtifactId);
			synchronizationConfiguration.SetupGet(x => x.DestinationWorkspaceArtifactId).Returns(testDestinationWorkspaceArtifactId);

			_objectManager.Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<MassUpdateByCriteriaRequest>(), It.IsAny<MassUpdateOptions>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((int workspace, MassUpdateByCriteriaRequest request, MassUpdateOptions options, CancellationToken token) =>
			{
				MatchCollection quotedIdentifiers = Regex.Matches(request.ObjectIdentificationCriteria.Condition, "([\"'])(?:(?=(\\\\?))\\2.)*?\\1");
				var massUpdateResult = new MassUpdateResult
				{
					TotalObjectsUpdated = quotedIdentifiers.Count - 1,   // Remove the quoted condition identifier field
					Success = true
				};
				return massUpdateResult;
			});

			// Act
			IList<TagDocumentsResult<string>> actualResult = await _instance.TagDocumentsAsync(synchronizationConfiguration.Object, testIdentifiers, _token).ConfigureAwait(false);

			// Assert
			Assert.IsNotNull(actualResult);
			Assert.IsTrue(actualResult[0].Success);
			Assert.AreEqual(testIdentifiers.Length, actualResult[0].TotalObjectsUpdated);
			CollectionAssert.IsEmpty(actualResult[0].FailedDocuments);

			VerifySentMetric(m =>
				m.DestinationUpdateCount == testIdentifiers.Length &&
				m.DestinationUpdateTime == expectedElapsedTime);

			_objectManager.Verify(x => x.UpdateAsync(
				It.Is<int>(w => w == testDestinationWorkspaceArtifactId),
				It.Is<MassUpdateByCriteriaRequest>(m => VerifyTagUpdateRequest(m, testIdentifiers, testSourceWorkspaceTagArtifactId, testSourceJobHistoryTagArtifactId)),
				It.Is<MassUpdateOptions>(u => u.UpdateBehavior == FieldUpdateBehavior.Merge),
				It.IsAny<CancellationToken>()), Times.Once);
		}

		private bool VerifyTagUpdateRequest(MassUpdateByCriteriaRequest actualUpdateRequest,
			string[] expectedIdentifiers, int expectedSourceWorkspaceTagArtifactId, int expectedSourceJobHistoryTagArtifactId)
		{
			const int expectedNumberOfFields = 2;
			Assert.IsNotNull(actualUpdateRequest);

			IEnumerable<string> quotedIdentifiers = expectedIdentifiers.Select(KeplerQueryHelpers.EscapeForSingleQuotes).Select(i => $"'{i}'");
			string joinedIdentifiers = string.Join(",", quotedIdentifiers);
			string expectedCondition = $"'{_DESTINATION_IDENTIFIER_FIELD}' IN [{joinedIdentifiers}]";
			Assert.AreEqual(expectedCondition, actualUpdateRequest.ObjectIdentificationCriteria.Condition);

			var sourceWorkspaceTagFieldMultiObject = new Guid("2FA844E3-44F0-47F9-ABB7-D6D8BE0C9B8F");
			var sourceJobTagFieldMultiObject = new Guid("7CC3FAAF-CBB8-4315-A79F-3AA882F1997F");

			IList<FieldRefValuePair> actualFields = actualUpdateRequest.FieldValues.ToList();
			CollectionAssert.IsNotEmpty(actualFields);
			Assert.AreEqual(expectedNumberOfFields, actualFields.Count);

			FieldRefValuePair actualSourceTagField = actualFields[0];
			Assert.AreEqual(sourceWorkspaceTagFieldMultiObject, actualSourceTagField.Field.Guid);
			AssertMultiObjectValueContainsId(expectedSourceWorkspaceTagArtifactId, actualSourceTagField.Value);

			FieldRefValuePair actualJobHistoryTagField = actualFields[1];
			Assert.AreEqual(sourceJobTagFieldMultiObject, actualJobHistoryTagField.Field.Guid);
			AssertMultiObjectValueContainsId(expectedSourceJobHistoryTagArtifactId, actualJobHistoryTagField.Value);

			return true;
		}

		private static void AssertMultiObjectValueContainsId(int expectedId, object value)
		{
			Assert.IsInstanceOf<IEnumerable<RelativityObjectRef>>(value);
			List<int> valueList = ((IEnumerable<RelativityObjectRef>)value).Select(x => x.ArtifactID).ToList();
			Assert.Contains(expectedId, valueList);
		}

		private void VerifySentMetric(Expression<Func<SourceWorkspaceTagMetric, bool>> validationFunc)
		{
			_syncMetrics.Verify(x => x.Send(It.Is(validationFunc)));
		}
	}
}
