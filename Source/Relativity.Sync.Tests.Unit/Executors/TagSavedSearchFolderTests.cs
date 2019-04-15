using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.Services;
using Relativity.Services.Search;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	public sealed class TagSavedSearchFolderTests
	{
		private Mock<ISyncLog> _syncLogMock;
		private Mock<IDestinationServiceFactoryForUser> _serviceFactoryForUser;
		private Mock<ISearchContainerManager> _searchContainerManager;
		private TagSavedSearchFolder _instance;

		private const int _SEARCH_CONTAINER_ARTIFACT_ID = 123456;
		private const int _WORKSPACE_ARTIFACT_ID = 345678;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_syncLogMock = new Mock<ISyncLog>();
		}

		[SetUp]
		public void SetUp()
		{
			_serviceFactoryForUser = new Mock<IDestinationServiceFactoryForUser>();
			_searchContainerManager = new Mock<ISearchContainerManager>();

			_serviceFactoryForUser
				.Setup(x => x.CreateProxyAsync<ISearchContainerManager>())
				.ReturnsAsync(_searchContainerManager.Object);

			_instance = new TagSavedSearchFolder(_serviceFactoryForUser.Object, _syncLogMock.Object);
		}

		[Test]
		public async Task ItShouldReturnExistingFolder()
		{
			// ARRANGE
			var searchContainer = new SearchContainer()
			{
				ArtifactID = _SEARCH_CONTAINER_ARTIFACT_ID
			};
			var result = new SearchContainerQueryResultSet()
			{
				Success = true,
				Results = new List<Result<SearchContainer>>() { new Result<SearchContainer>() { Artifact = searchContainer } }
			};

			_searchContainerManager
				.Setup(x => x.QueryAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<Services.Query>()))
				.ReturnsAsync(result);

			// ACT
			int folderId = await _instance.GetFolderId(_WORKSPACE_ARTIFACT_ID).ConfigureAwait(false);

			// ASSERT
			Assert.AreEqual(_SEARCH_CONTAINER_ARTIFACT_ID, folderId);
			_searchContainerManager.Verify(x => x.CreateSingleAsync(It.IsAny<int>(), It.IsAny<SearchContainer>()), Times.Never);
		}

		[Test]
		public async Task ItShouldCreateFolderIfNotFound()
		{
			// ARRANGE
			var result = new SearchContainerQueryResultSet()
			{
				Success = true,
				Results = new List<Result<SearchContainer>>()
			};

			_searchContainerManager
				.Setup(x => x.QueryAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<Services.Query>()))
				.ReturnsAsync(result);

			_searchContainerManager
				.Setup(x => x.CreateSingleAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<SearchContainer>()))
				.ReturnsAsync(_SEARCH_CONTAINER_ARTIFACT_ID);

			// ACT
			int folderId = await _instance.GetFolderId(_WORKSPACE_ARTIFACT_ID).ConfigureAwait(false);

			// ASSERT
			Assert.AreEqual(_SEARCH_CONTAINER_ARTIFACT_ID, folderId);
		}

		[Test]
		public void ItShouldThrowExceptionOnNonSuccessfulQuery()
		{
			// ARRANGE
			var result = new SearchContainerQueryResultSet()
			{
				Success = false
			};

			_searchContainerManager
				.Setup(x => x.QueryAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<Services.Query>()))
				.ReturnsAsync(result);

			// ACT & ASSERT
			Assert.ThrowsAsync<SyncException>(async () => await _instance.GetFolderId(_WORKSPACE_ARTIFACT_ID).ConfigureAwait(false));

			_searchContainerManager.Verify(x => x.CreateSingleAsync(It.IsAny<int>(), It.IsAny<SearchContainer>()), Times.Never);
		}

		[Test]
		public void ItShouldThrowExceptionOnCreationFailure()
		{
			// ARRANGE
			var result = new SearchContainerQueryResultSet()
			{
				Success = true,
				Results = new List<Result<SearchContainer>>()
			};

			_searchContainerManager
				.Setup(x => x.QueryAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<Services.Query>()))
				.ReturnsAsync(result);

			_searchContainerManager
				.Setup(x => x.CreateSingleAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<SearchContainer>()))
				.Throws<Exception>();

			// ACT & ASSERT
			Assert.ThrowsAsync<SyncException>(async () => await _instance.GetFolderId(_WORKSPACE_ARTIFACT_ID).ConfigureAwait(false));
		}

		[Test]
		public void ItShouldThrowExceptionOnQueryFailure()
		{
			// ARRANGE
			_searchContainerManager
				.Setup(x => x.QueryAsync(It.Is<int>(y => y == _WORKSPACE_ARTIFACT_ID), It.IsAny<Services.Query>()))
				.Throws<Exception>();

			// ACT & ASSERT
			Assert.ThrowsAsync<Exception>(async () => await _instance.GetFolderId(_WORKSPACE_ARTIFACT_ID).ConfigureAwait(false));

			_searchContainerManager.Verify(x => x.CreateSingleAsync(It.IsAny<int>(), It.IsAny<SearchContainer>()), Times.Never);
		}

		[Test]
		public void ItShouldThrowExceptionOnCreateProxyFailure()
		{
			// ARRANGE
			_serviceFactoryForUser
				.Setup(x => x.CreateProxyAsync<ISearchContainerManager>())
				.Throws<Exception>();

			// ACT & ASSERT
			Assert.ThrowsAsync<Exception>(async () => await _instance.GetFolderId(_WORKSPACE_ARTIFACT_ID).ConfigureAwait(false));
		}
	}
}