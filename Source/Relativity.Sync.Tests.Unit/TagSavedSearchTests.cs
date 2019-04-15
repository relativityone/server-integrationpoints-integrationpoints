using System.IO;
using System.Threading;
using Moq;
using NUnit.Framework;
using Relativity.Services.Search;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public class TagSavedSearchTests
	{

		private CancellationToken _token;

		private Mock<IDestinationServiceFactoryForUser> _destinationServiceFactoryForUser;

		private Mock<IDestinationWorkspaceSavedSearchCreationConfiguration> _destinationWorkspaceSavedSearchCreationConfiguration;
		private Mock<ISyncLog> _syncLog;

		private TagSavedSearch _instance;

		private const int _TEST_DEST_WORKSPACE_ARTIFACT_ID = 101987;
		private const int _TEST_SAVED_SEARCH_FOLDER_ARTIFACT_ID = 101987;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_token = CancellationToken.None;
			_syncLog = new Mock<ISyncLog>();
		}

		[SetUp]
		public void SetUp()
		{
			_destinationServiceFactoryForUser = new Mock<IDestinationServiceFactoryForUser>();

			_destinationWorkspaceSavedSearchCreationConfiguration = new Mock<IDestinationWorkspaceSavedSearchCreationConfiguration>();
			_destinationWorkspaceSavedSearchCreationConfiguration.SetupGet(x => x.DestinationWorkspaceArtifactId).Returns(_TEST_DEST_WORKSPACE_ARTIFACT_ID);

			_instance = new TagSavedSearch(_destinationServiceFactoryForUser.Object, _syncLog.Object);
		}

		[Test]
		public void CreateTagSavedSearchAsyncThrowsExceptionWhenFailingToCreateProxyTest()
		{
			// Arrange
			_destinationServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IKeywordSearchManager>()).Throws<IOException>();

			// Act & Assert
			Assert.ThrowsAsync<DestinationWorkspaceTagRepositoryException>(
				async () => await _instance.CreateTagSavedSearchAsync(_destinationWorkspaceSavedSearchCreationConfiguration.Object, _TEST_SAVED_SEARCH_FOLDER_ARTIFACT_ID, _token).ConfigureAwait(false),
				$"Failed to create Saved Search for promoted documents in destination workspace {_TEST_DEST_WORKSPACE_ARTIFACT_ID}.");
		}
	}
}