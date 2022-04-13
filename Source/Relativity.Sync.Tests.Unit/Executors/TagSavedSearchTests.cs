using Relativity.API;
using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.Services.Search;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	public class TagSavedSearchTests
	{

		private CancellationToken _token;

		private Mock<IDestinationServiceFactoryForUser> _destinationServiceFactoryForUser;
		private Mock<IAPILog> _syncLog;
		private Mock<IKeywordSearchManager> _keywordSearchManager;

		private Mock<IDestinationWorkspaceSavedSearchCreationConfiguration> _destinationWorkspaceSavedSearchCreationConfiguration;

		private TagSavedSearch _instance;

		private const int _TEST_DEST_WORKSPACE_ARTIFACT_ID = 101987;
		private const int _TEST_SAVED_SEARCH_FOLDER_ARTIFACT_ID = 101876;
		private const int _TEST_SOURCE_JOB_TAG_ARTIFACT_ID = 102456;
		private const string _TEST_SOURCE_JOB_TAG_NAME = "Source Workspace Push";

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_token = CancellationToken.None;
		}

		[SetUp]
		public void SetUp()
		{
			_destinationServiceFactoryForUser = new Mock<IDestinationServiceFactoryForUser>();
			_syncLog = new Mock<IAPILog>();
			_keywordSearchManager = new Mock<IKeywordSearchManager>();

			_destinationWorkspaceSavedSearchCreationConfiguration = new Mock<IDestinationWorkspaceSavedSearchCreationConfiguration>();
			_destinationWorkspaceSavedSearchCreationConfiguration.SetupGet(x => x.DestinationWorkspaceArtifactId).Returns(_TEST_DEST_WORKSPACE_ARTIFACT_ID);
			_destinationWorkspaceSavedSearchCreationConfiguration.Setup(x => x.GetSourceJobTagName()).Returns(_TEST_SOURCE_JOB_TAG_NAME);
			_destinationWorkspaceSavedSearchCreationConfiguration.SetupGet(x => x.SourceJobTagArtifactId).Returns(_TEST_SOURCE_JOB_TAG_ARTIFACT_ID);

			_instance = new TagSavedSearch(_destinationServiceFactoryForUser.Object, _syncLog.Object);
		}

		[Test]
		public async Task CreateTagSavedSearchAsyncGoldFlowTest()
		{
			// Arrange
			const int expectedKeywordSearchId = 101555;

			_destinationServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IKeywordSearchManager>()).ReturnsAsync(_keywordSearchManager.Object);
			_keywordSearchManager.Setup(x => x.CreateSingleAsync(It.IsAny<int>(), It.IsAny<KeywordSearch>())).ReturnsAsync(expectedKeywordSearchId);

			// Act
			int actualKeywordSearchId = await _instance.CreateTagSavedSearchAsync(_destinationWorkspaceSavedSearchCreationConfiguration.Object, _TEST_SAVED_SEARCH_FOLDER_ARTIFACT_ID, _token)
				.ConfigureAwait(false);

			// Assert
			Assert.AreEqual(expectedKeywordSearchId, actualKeywordSearchId);

			_keywordSearchManager.Verify(x => x.CreateSingleAsync(It.Is<int>(y => y == _TEST_DEST_WORKSPACE_ARTIFACT_ID), It.Is<KeywordSearch>(y => AssertKeywordSearchDto(y))));
		}

		private bool AssertKeywordSearchDto(KeywordSearch actualKeywordSearch)
		{
			var expectedJobHistoryFieldOnDocumentGuid = new Guid("7cc3faaf-cbb8-4315-a79f-3aa882f1997f");

			Assert.AreEqual(_TEST_SOURCE_JOB_TAG_NAME, actualKeywordSearch.Name);
			Assert.AreEqual((int)ArtifactType.Document, actualKeywordSearch.ArtifactTypeID);
			Assert.AreEqual(_TEST_SAVED_SEARCH_FOLDER_ARTIFACT_ID, actualKeywordSearch.SearchContainer.ArtifactID);

			const int expectedNumberOfMultiObjectConditions = 1;
			CriteriaCollection actualCriteria = actualKeywordSearch.SearchCriteria;
			Assert.IsNotEmpty(actualCriteria.Conditions);
			Assert.AreEqual(expectedNumberOfMultiObjectConditions, actualCriteria.Conditions.Count);

			AssertKeywordSearchCriteria(actualCriteria.Conditions[0], expectedJobHistoryFieldOnDocumentGuid, _TEST_SOURCE_JOB_TAG_ARTIFACT_ID);

			return true;
		}

		private static void AssertKeywordSearchCriteria(CriteriaBase criteria, Guid expectedFieldIdentifier, int expectedFieldArtifactId)
		{
			Assert.IsInstanceOf<Criteria>(criteria);
			var parentCriteria = criteria as Criteria;
			Assert.IsNotNull(parentCriteria);
			CollectionAssert.Contains(parentCriteria.Condition.FieldIdentifier.Guids, expectedFieldIdentifier);

			var parentConditions = parentCriteria.Condition.Value as CriteriaCollection;
			Assert.IsNotNull(parentConditions);
			CollectionAssert.IsNotEmpty(parentConditions.Conditions);

			Assert.IsInstanceOf<Criteria>(parentConditions.Conditions[0]);
			var innerCriteria = parentConditions.Conditions[0] as Criteria;
			Assert.IsNotNull(innerCriteria);

			CollectionAssert.Contains(innerCriteria.Condition.FieldIdentifier.Guids, expectedFieldIdentifier);
			CollectionAssert.Contains(innerCriteria.Condition.Value as IEnumerable, expectedFieldArtifactId);
			Assert.AreEqual(BooleanOperatorEnum.And, innerCriteria.BooleanOperator);
		}

		[Test]
		public void CreateTagSavedSearchAsyncThrowsExceptionWhenFailingToCreateProxyTest()
		{
			// Arrange
			_destinationServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IKeywordSearchManager>()).Throws<IOException>();

			// Act & Assert
			Assert.ThrowsAsync<SyncException>(
				async () => await _instance.CreateTagSavedSearchAsync(_destinationWorkspaceSavedSearchCreationConfiguration.Object, _TEST_SAVED_SEARCH_FOLDER_ARTIFACT_ID, _token).ConfigureAwait(false),
				$"Failed to create Saved Search for promoted documents in destination workspace {_TEST_DEST_WORKSPACE_ARTIFACT_ID}.");

			_syncLog.Verify(x => x.LogError(It.IsAny<IOException>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
		}

		[Test]
		public void CreateTagSavedSearchAsyncThrowsExceptionWhenTryingToCreateTest()
		{
			// Arrange
			_destinationServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IKeywordSearchManager>()).ReturnsAsync(_keywordSearchManager.Object).Verifiable();
			_keywordSearchManager.Setup(x => x.CreateSingleAsync(It.IsAny<int>(), It.IsAny<KeywordSearch>())).Throws<IOException>().Verifiable();

			// Act & Assert
			Assert.ThrowsAsync<SyncException>(
				async () => await _instance.CreateTagSavedSearchAsync(_destinationWorkspaceSavedSearchCreationConfiguration.Object, _TEST_SAVED_SEARCH_FOLDER_ARTIFACT_ID, _token).ConfigureAwait(false),
				$"Failed to create Saved Search for promoted documents in destination workspace {_TEST_DEST_WORKSPACE_ARTIFACT_ID}.");

			_syncLog.Verify(x => x.LogError(It.IsAny<IOException>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);

			Mock.Verify(_destinationServiceFactoryForUser, _keywordSearchManager);
		}
	}
}
