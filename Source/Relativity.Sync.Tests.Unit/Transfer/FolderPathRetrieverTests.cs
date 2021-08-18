using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Exceptions;
using Relativity.Services.Folder;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	[TestFixture]
	public class FolderPathRetrieverTests
	{
		private Mock<ISyncLog> _logger;
		private FolderPathRetriever _instance;
		private Mock<IObjectManager> _objectManager;
		private Mock<IFolderManager> _folderManager;

		private const int _WORKSPACE_ARTIFACT_ID = 123456;
		private const int _DIVISOR = 13;
		private const int _BATCH_SIZE = 100_000;

		[SetUp]
		public void SetUp()
		{
			_objectManager = new Mock<IObjectManager>();
			_folderManager = new Mock<IFolderManager>();

			var serviceFactory = new Mock<ISourceServiceFactoryForUser>();
			serviceFactory
				.Setup(x => x.CreateProxyAsync<IObjectManager>())
				.ReturnsAsync(_objectManager.Object);
			serviceFactory
				.Setup(x => x.CreateProxyAsync<IFolderManager>())
				.ReturnsAsync(_folderManager.Object);

			_logger = new Mock<ISyncLog>();

			_instance = new FolderPathRetriever(serviceFactory.Object, _logger.Object);
		}

		private static IEnumerable<ICollection<int>> EmptyAndNullDocumentArtifactIds()
		{
			return new[]
			{
				null,
				new List<int>(0)
			};
		}

		[TestCaseSource(nameof(EmptyAndNullDocumentArtifactIds))]
		public async Task ItShouldGetEmptyDocumentIdToFolderIdMapWhenDocumentCountIsZero(ICollection<int> documentArtifactIds)
		{
			// ARRANGE
			const int batchCount = 0;

			_objectManager
				.Setup(x => x.QueryAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync<int, QueryRequest, int, int, IObjectManager, QueryResult>((workspaceArtifactId, queryRequest, start, length) => BuildQueryResult(queryRequest));

			_folderManager
				.Setup(x => x.GetFullPathListAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<List<int>>()))
				.ReturnsAsync<int, List<int>, IFolderManager, List<FolderPath>>((workspaceArtifactId, folderIds) => BuildFolderPathList(folderIds));

			// ACT
			IDictionary<int, string> result = await _instance.GetFolderPathsAsync(_WORKSPACE_ARTIFACT_ID, documentArtifactIds).ConfigureAwait(false);

			// ASSERT
			_objectManager.Verify(x => x.QueryAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(batchCount));

			result.Should().BeEmpty();
		}

		[Test]
		[Parallelizable]
		public async Task ItShouldGetDocumentIdToFolderIdMapInBatches([Random(50_000, 400_000, 3, Distinct = true)] int documentCount)
		{
			// ARRANGE
			int batchCount = GetBatchCount(documentCount);
			const int rangeStart = 1000000;
			ICollection<int> documentArtifactIds = Enumerable.Range(rangeStart, documentCount).ToArray();

			_objectManager
				.Setup(x => x.QueryAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync<int, QueryRequest, int, int, IObjectManager, QueryResult>((workspaceArtifactId, queryRequest, start, length) => BuildQueryResult(queryRequest));

			_folderManager
				.Setup(x => x.GetFullPathListAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<List<int>>()))
				.ReturnsAsync<int, List<int>, IFolderManager, List<FolderPath>>((workspaceArtifactId, folderIds) => BuildFolderPathList(folderIds));

			// ACT
			IDictionary<int, string> result = await _instance.GetFolderPathsAsync(_WORKSPACE_ARTIFACT_ID, documentArtifactIds).ConfigureAwait(false);

			// ASSERT
			_objectManager.Verify(x => x.QueryAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(batchCount));

			documentArtifactIds.ForEach(x => result.Should().Contain(x, $"FolderPath-{x % _DIVISOR}"));
		}

		[Test]
		public void ItShouldThrowSyncKeplerExceptionOnObjectManagerServiceException()
		{
			// ARRANGE
			const int documentCount = 523_456;
			int batchCount = GetBatchCount(documentCount);
			const int rangeStart = 1000000;
			ICollection<int> documentArtifactIds = Enumerable.Range(rangeStart, documentCount).ToArray();

			_objectManager
				.Setup(x => x.QueryAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()))
				.Throws<ServiceException>();

			_folderManager
				.Setup(x => x.GetFullPathListAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<List<int>>()))
				.ReturnsAsync<int, List<int>, IFolderManager, List<FolderPath>>((workspaceArtifactId, folderIds) => BuildFolderPathList(folderIds));

			// ACT & ASSERT
			SyncKeplerException thrown = Assert.ThrowsAsync<SyncKeplerException>(async () =>
				await _instance.GetFolderPathsAsync(_WORKSPACE_ARTIFACT_ID, documentArtifactIds).ConfigureAwait(false)
			);

			thrown.InnerException.Should().BeOfType<ServiceException>();

			// Task.WaitAll doesn't throw immediately when any Object or Folder Manager throws. Instead it waits for ALL tasks to complete in any way.
			// But at the end it throws the first exception thrown. That is why in this test case we check for `logError` called exactly `batchCount` times
			// Task.WaitAll call is made inside our SelectAsync extension method.
			_logger.Verify(x => x.LogError(It.IsAny<ServiceException>(), It.IsAny<string>(), It.IsAny<QueryRequest>()), Times.Exactly(batchCount));
		}

		[Test]
		public void ItShouldThrowSyncKeplerExceptionOnObjectManagerException()
		{
			// ARRANGE
			const int documentCount = 123_456;
			int batchCount = GetBatchCount(documentCount);
			const int rangeStart = 1000000;
			ICollection<int> documentArtifactIds = Enumerable.Range(rangeStart, documentCount).ToArray();

			_objectManager
				.Setup(x => x.QueryAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()))
				.Throws<ServiceException>();

			_folderManager
				.Setup(x => x.GetFullPathListAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<List<int>>()))
				.ReturnsAsync<int, List<int>, IFolderManager, List<FolderPath>>((workspaceArtifactId, folderIds) => BuildFolderPathList(folderIds));

			// ACT & ASSERT
			Assert.ThrowsAsync<SyncKeplerException>(async () =>
				await _instance.GetFolderPathsAsync(_WORKSPACE_ARTIFACT_ID, documentArtifactIds).ConfigureAwait(false)
			);

			// Task.WaitAll doesn't throw immediately when any Object or Folder Manager throws. Instead it waits for ALL tasks to complete in any way.
			// But at the end it throws the first exception thrown. That is why in this test case we check for `logError` called exactly `batchCount` times
			// Task.WaitAll call is made inside our SelectAsync extension method.
			_logger.Verify(x => x.LogError(It.IsAny<ServiceException>(), It.IsAny<string>(), It.IsAny<QueryRequest>()), Times.Exactly(batchCount));
		}

		[Test]
		public void ItShouldThrowSyncKeplerExceptionOnFolderManagerServiceException()
		{
			// ARRANGE
			const int documentCount = 123_456;
			const int rangeStart = 1000000;
			ICollection<int> documentArtifactIds = Enumerable.Range(rangeStart, documentCount).ToArray();

			_objectManager
				.Setup(x => x.QueryAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync<int, QueryRequest, int, int, IObjectManager, QueryResult>((workspaceArtifactId, queryRequest, start, length) => BuildQueryResult(queryRequest));

			_folderManager
				.Setup(x => x.GetFullPathListAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<List<int>>()))
				.Throws<ServiceException>();

			// ACT & ASSERT
			SyncKeplerException thrown = Assert.ThrowsAsync<SyncKeplerException>(async () =>
				await _instance.GetFolderPathsAsync(_WORKSPACE_ARTIFACT_ID, documentArtifactIds).ConfigureAwait(false)
			);

			thrown.InnerException.Should().BeOfType<ServiceException>();

			_logger.Verify(x => x.LogError(It.IsAny<ServiceException>(), It.IsAny<string>(), It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID)), Times.Once);
		}

		[Test]
		public void ItShouldLogWarningWhenFolderManagerResultsAreMissing()
		{
			// ARRANGE
			const int countArtifactId = 10;
			const int countListId = 7;
			const int rangeStart = 1000000;
			ICollection<int> documentArtifactIds = Enumerable.Range(rangeStart, countArtifactId).ToArray();
			List<int> documentListId = Enumerable.Range(rangeStart, countListId).ToArray().ToList();
			_objectManager
				.Setup(x => x.QueryAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync<int, QueryRequest, int, int, IObjectManager, QueryResult>((workspaceArtifactId, queryRequest, start, length) => BuildQueryResult(queryRequest));

			List<FolderPath> folderPaths = BuildFolderPathList(documentListId);
			
			_folderManager
				.Setup(x => x.GetFullPathListAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<List<int>>()))
				.ReturnsAsync(folderPaths);

			// ACT
			_instance.GetFolderPathsAsync(_WORKSPACE_ARTIFACT_ID, documentArtifactIds).ConfigureAwait(false);

			// ASSERT
			string message = $"Could not find folders with IDs 1000007,1000008,1000009,1000010 in workspace {_WORKSPACE_ARTIFACT_ID}.";
			_logger.Verify(x => x.LogWarning(It.Is<string>(msg => msg == message)), Times.Once);
		}

		[Test]
		public void ItShouldThrowSyncKeplerExceptionOnFolderManagerException()
		{
			// ARRANGE
			const int documentCount = 123_456;
			const int rangeStart = 1000000;
			ICollection<int> documentArtifactIds = Enumerable.Range(rangeStart, documentCount).ToArray();

			_objectManager
				.Setup(x => x.QueryAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync<int, QueryRequest, int, int, IObjectManager, QueryResult>((workspaceArtifactId, queryRequest, start, length) => BuildQueryResult(queryRequest));

			_folderManager
				.Setup(x => x.GetFullPathListAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<List<int>>()))
				.Throws<ServiceException>();

			// ACT & ASSERT
			Assert.ThrowsAsync<SyncKeplerException>(async () =>
				await _instance.GetFolderPathsAsync(_WORKSPACE_ARTIFACT_ID, documentArtifactIds).ConfigureAwait(false)
			);

			_logger.Verify(x => x.LogError(It.IsAny<ServiceException>(), It.IsAny<string>(), It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID)), Times.Once);
		}

		#region Auxiliary methods

		private int GetBatchCount(int documentCount)
		{
			int quotient = Math.DivRem(documentCount, _BATCH_SIZE, out int reminder);
			int additionalBatch = (reminder == 0) ? 0 : 1;

			return quotient + additionalBatch;
		}

		private List<FolderPath> BuildFolderPathList(List<int> folderIds)
		{
			List<FolderPath> folderPathList = folderIds
				.Select(x => new FolderPath { ArtifactID = x, FullPath = $"FolderPath-{FolderIdToDocumentId(x) % _DIVISOR}" })
				.ToList();

			return folderPathList;
		}

		private static QueryResult BuildQueryResult(QueryRequest queryRequest)
		{
			string queryRequestCondition = queryRequest.Condition;
			List<RelativityObject> resultObjects = ExtractArtifactIdsFromCondition(queryRequestCondition)
				.Select(x => new RelativityObject { ArtifactID = x, ParentObject = new RelativityObjectRef { ArtifactID = DocumentIdToFolderId(x) } })
				.ToList();

			QueryResult queryResult = new QueryResult
			{
				Objects = resultObjects
			};

			return queryResult;
		}

		private static List<int> ExtractArtifactIdsFromCondition(string condition)
		{
			const string conditionPattern = "\"ArtifactID\" IN \\[([\\d,]+)\\]";
			MatchCollection matches = Regex.Matches(condition, conditionPattern);
			if (matches.Count > 0 && matches[0].Groups.Count > 1)
			{
				string artifactIdsSeparatedByComma = matches[0].Groups[1].Value;
				List<int> artifactIdsAsInt = artifactIdsSeparatedByComma.Split(new[] { "," }, StringSplitOptions.None)
					.Select(x => int.Parse(x, NumberStyles.None, CultureInfo.InvariantCulture))
					.ToList();
				return artifactIdsAsInt;
			}

			return new List<int>();
		}

		private static int DocumentIdToFolderId(int documentId)
		{
			return documentId + 1;
		}

		private static int FolderIdToDocumentId(int folderId)
		{
			return folderId - 1;
		}

		#region Auxiliary methods' tests

		[Test]
		public void ExtractArtifactIdsFromConditionTest()
		{
			const int rangeStart = 1000;
			const int rangeEnd = 2000;
			List<int> intList = Enumerable.Range(rangeStart, rangeEnd).ToList();

			string condition = $"\"ArtifactID\" IN [{string.Join(",", intList)}]";
			List<int> artifactIds = ExtractArtifactIdsFromCondition(condition);

			intList.ForEach(x => Assert.Contains(x, artifactIds));

			Assert.AreEqual(intList.Count, artifactIds.Count);
		}

		[Test]
		public void ConversionBetweenFolderIdAndDocumentIdTest([Random(0, 100_000, 100, Distinct = true)] int documentId)
		{
			FolderIdToDocumentId(DocumentIdToFolderId(documentId)).Should().Be(documentId);
		}

		#endregion

		#endregion
	}
}