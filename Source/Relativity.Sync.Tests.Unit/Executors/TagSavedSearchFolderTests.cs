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
		[Test]
		public async Task ItShouldReturnExistingFolder()
		{
			// ARRANGE
			const int searchContainerArtifactId = 123456;
			const int workspaceArtifactId = 345678;

			var syncLogMock = new Mock<ISyncLog>();
			var serviceFactoryForUser = new Mock<IDestinationServiceFactoryForUser>();
			var searchContainerManager = new Mock<ISearchContainerManager>();

			var searchContainer = new SearchContainer()
			{
				ArtifactID = searchContainerArtifactId
			};
			var result = new SearchContainerQueryResultSet()
			{
				Success = true,
				Results = new List<Result<SearchContainer>>() { new Result<SearchContainer>() { Artifact = searchContainer } }
			};

			searchContainerManager
				.Setup(x => x.QueryAsync(It.Is<int>(y => y == workspaceArtifactId), It.IsAny<Services.Query>()))
				.ReturnsAsync(result);

			serviceFactoryForUser
				.Setup(x => x.CreateProxyAsync<ISearchContainerManager>())
				.ReturnsAsync(searchContainerManager.Object);

			var tagSavedSearchFolder = new TagSavedSearchFolder(serviceFactoryForUser.Object, syncLogMock.Object);

			// ACT
			int folderId = await tagSavedSearchFolder.GetFolderId(workspaceArtifactId).ConfigureAwait(false);

			// ASSERT
			Assert.AreEqual(searchContainerArtifactId, folderId);
			searchContainerManager.Verify(x => x.CreateSingleAsync(It.IsAny<int>(), It.IsAny<SearchContainer>()), Times.Never);
		}

		[Test]
		public async Task ItShouldCreateFolderIfNotFound()
		{
			// ARRANGE
			const int newlyCreatedSearchContainerArtifactId = 123456;
			const int workspaceArtifactId = 345678;

			var syncLogMock = new Mock<ISyncLog>();
			var serviceFactoryForUser = new Mock<IDestinationServiceFactoryForUser>();
			var searchContainerManager = new Mock<ISearchContainerManager>();

			var result = new SearchContainerQueryResultSet()
			{
				Success = true,
				Results = new List<Result<SearchContainer>>()
			};

			searchContainerManager
				.Setup(x => x.QueryAsync(It.Is<int>(y => y == workspaceArtifactId), It.IsAny<Services.Query>()))
				.ReturnsAsync(result);

			searchContainerManager
				.Setup(x => x.CreateSingleAsync(It.Is<int>(y => y == workspaceArtifactId), It.IsAny<SearchContainer>()))
				.ReturnsAsync(newlyCreatedSearchContainerArtifactId);

			serviceFactoryForUser
				.Setup(x => x.CreateProxyAsync<ISearchContainerManager>())
				.ReturnsAsync(searchContainerManager.Object);

			var tagSavedSearchFolder = new TagSavedSearchFolder(serviceFactoryForUser.Object, syncLogMock.Object);

			// ACT
			int folderId = await tagSavedSearchFolder.GetFolderId(workspaceArtifactId).ConfigureAwait(false);

			// ASSERT
			Assert.AreEqual(newlyCreatedSearchContainerArtifactId, folderId);
		}

		[Test]
		public void ItShouldThrowExceptionOnNonSuccessfulQuery()
		{
			// ARRANGE
			const int workspaceArtifactId = 345678;

			var syncLogMock = new Mock<ISyncLog>();
			var serviceFactoryForUser = new Mock<IDestinationServiceFactoryForUser>();
			var searchContainerManager = new Mock<ISearchContainerManager>();
			
			var result = new SearchContainerQueryResultSet()
			{
				Success = false
			};

			searchContainerManager
				.Setup(x => x.QueryAsync(It.Is<int>(y => y == workspaceArtifactId), It.IsAny<Services.Query>()))
				.ReturnsAsync(result);

			serviceFactoryForUser
				.Setup(x => x.CreateProxyAsync<ISearchContainerManager>())
				.ReturnsAsync(searchContainerManager.Object);

			var tagSavedSearchFolder = new TagSavedSearchFolder(serviceFactoryForUser.Object, syncLogMock.Object);

			// ACT
			// ASSERT
			Assert.ThrowsAsync<SyncException>(async () => await tagSavedSearchFolder.GetFolderId(workspaceArtifactId).ConfigureAwait(false));

			searchContainerManager.Verify(x => x.CreateSingleAsync(It.IsAny<int>(), It.IsAny<SearchContainer>()), Times.Never);
		}

		[Test]
		public void ItShouldThrowExceptionOnCreationFailure()
		{
			// ARRANGE
			const int workspaceArtifactId = 345678;

			var syncLogMock = new Mock<ISyncLog>();
			var serviceFactoryForUser = new Mock<IDestinationServiceFactoryForUser>();
			var searchContainerManager = new Mock<ISearchContainerManager>();

			var result = new SearchContainerQueryResultSet()
			{
				Success = true,
				Results = new List<Result<SearchContainer>>()
			};

			searchContainerManager
				.Setup(x => x.QueryAsync(It.Is<int>(y => y == workspaceArtifactId), It.IsAny<Services.Query>()))
				.ReturnsAsync(result);

			searchContainerManager
				.Setup(x => x.CreateSingleAsync(It.Is<int>(y => y == workspaceArtifactId), It.IsAny<SearchContainer>()))
				.Throws<Exception>();

			serviceFactoryForUser
				.Setup(x => x.CreateProxyAsync<ISearchContainerManager>())
				.ReturnsAsync(searchContainerManager.Object);

			var tagSavedSearchFolder = new TagSavedSearchFolder(serviceFactoryForUser.Object, syncLogMock.Object);

			// ACT
			// ASSERT
			Assert.ThrowsAsync<SyncException>(async () => await tagSavedSearchFolder.GetFolderId(workspaceArtifactId).ConfigureAwait(false));
		}

		[Test]
		public void ItShouldThrowExceptionOnQueryFailure()
		{
			// ARRANGE
			const int workspaceArtifactId = 345678;

			var syncLogMock = new Mock<ISyncLog>();
			var serviceFactoryForUser = new Mock<IDestinationServiceFactoryForUser>();
			var searchContainerManager = new Mock<ISearchContainerManager>();

			searchContainerManager
				.Setup(x => x.QueryAsync(It.Is<int>(y => y == workspaceArtifactId), It.IsAny<Services.Query>()))
				.Throws<Exception>();

			serviceFactoryForUser
				.Setup(x => x.CreateProxyAsync<ISearchContainerManager>())
				.ReturnsAsync(searchContainerManager.Object);

			var tagSavedSearchFolder = new TagSavedSearchFolder(serviceFactoryForUser.Object, syncLogMock.Object);

			// ACT
			// ASSERT
			Assert.ThrowsAsync<Exception>(async () => await tagSavedSearchFolder.GetFolderId(workspaceArtifactId).ConfigureAwait(false));

			searchContainerManager.Verify(x => x.CreateSingleAsync(It.IsAny<int>(), It.IsAny<SearchContainer>()), Times.Never);
		}

		[Test]
		public void ItShouldThrowExceptionOnCreateProxyFailure()
		{
			// ARRANGE
			const int workspaceArtifactId = 345678;

			var syncLogMock = new Mock<ISyncLog>();
			var serviceFactoryForUser = new Mock<IDestinationServiceFactoryForUser>();

			serviceFactoryForUser
				.Setup(x => x.CreateProxyAsync<ISearchContainerManager>())
				.Throws<Exception>();

			var tagSavedSearchFolder = new TagSavedSearchFolder(serviceFactoryForUser.Object, syncLogMock.Object);

			// ACT
			// ASSERT
			Assert.ThrowsAsync<Exception>(async () => await tagSavedSearchFolder.GetFolderId(workspaceArtifactId).ConfigureAwait(false));
		}
	}
}