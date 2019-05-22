using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.Config;
using Moq;
using NUnit.Framework;
using Relativity.Services.Folder;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	[TestFixture]
	public class FolderPathRetrieverTests
	{
		private Mock<ISourceServiceFactoryForUser> _serviceFactory;
		private Mock<ISyncLog> _logger;
		private FolderPathRetriever _instance;
		private Mock<IObjectManager> _objectManager;
		private Mock<IFolderManager> _folderManager;
		private static string _conditionPattern = "\"ArtifactID\" IN \\[([\\d,]+)\\]";
		private const int _WORKSPACE_ARTIFACT_ID = 123456;

		[SetUp]
		public void SetUp()
		{
			_objectManager = new Mock<IObjectManager>();
			_folderManager = new Mock<IFolderManager>();

			_serviceFactory = new Mock<ISourceServiceFactoryForUser>();
			_serviceFactory
				.Setup(x => x.CreateProxyAsync<IObjectManager>())
				.ReturnsAsync(_objectManager.Object);
			_serviceFactory
				.Setup(x => x.CreateProxyAsync<IFolderManager>())
				.ReturnsAsync(_folderManager.Object);

			_logger = new Mock<ISyncLog>();

			_instance = new FolderPathRetriever(_serviceFactory.Object, _logger.Object);
		}

		[TestCase(1, 11_232)]
		[TestCase(2, 200_000)]
		[TestCase(5, 423_499)]
		[Test]
		public async Task ItShouldGetDocumentIdToFolderIdMapInBatches(int batchCount, int documentCount)
		{
			// ARRANGE
			const int rangeStart = 1000000;
			ICollection<int> documentArtifactIds = Enumerable.Range(rangeStart, documentCount).ToArray();

			_objectManager
				.Setup(x => x.QueryAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync<int, QueryRequest, int, int, IObjectManager, QueryResult>((workspaceArtifactId, queryRequest, start, length) => BuildQueryResult(queryRequest));

			_folderManager
				.Setup(x => x.GetFullPathListAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<List<int>>()))
				.ReturnsAsync<int, List<int>, IFolderManager, List<FolderPath>>((workspaceArtifactId, folderIds) => BuildFolderPathList(folderIds));

			// ACT
			await _instance.GetFolderPathsAsync(_WORKSPACE_ARTIFACT_ID, documentArtifactIds).ConfigureAwait(false);

			// ASSERT
			_objectManager.Verify(x => x.QueryAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(batchCount));
		}

		private List<FolderPath> BuildFolderPathList(List<int> folderIds)
		{
			List<FolderPath> folderPathList = folderIds
				.Select(x => new FolderPath { ArtifactID = x, FullPath = Guid.NewGuid().ToString() })
				.ToList();

			return folderPathList;
		}

		private static QueryResult BuildQueryResult(QueryRequest queryRequest)
		{
			string queryRequestCondition = queryRequest.Condition;
			List<RelativityObject> resultObjects = ExtractArtifactIdsFromCondition(queryRequestCondition)
				.Select(x => new RelativityObject { ArtifactID = x, ParentObject = new RelativityObjectRef { ArtifactID = x + 1 } })
				.ToList();

			QueryResult queryResult = new QueryResult
			{
				Objects = resultObjects
			};

			return queryResult;
		}

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

		private static List<int> ExtractArtifactIdsFromCondition(string condition)
		{
			MatchCollection matches = Regex.Matches(condition, _conditionPattern);
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
	}
}