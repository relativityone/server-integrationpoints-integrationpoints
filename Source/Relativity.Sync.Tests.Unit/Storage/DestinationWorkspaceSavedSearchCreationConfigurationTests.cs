using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.RDOs;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
	[TestFixture]
	public sealed class DestinationWorkspaceSavedSearchCreationConfigurationTests
	{
		private DestinationWorkspaceSavedSearchCreationConfiguration _instance;

		private Mock<IConfiguration> _cache;

		[SetUp]
		public void SetUp()
		{
			_cache = new Mock<IConfiguration>();

			_instance = new DestinationWorkspaceSavedSearchCreationConfiguration(_cache.Object);
		}

		[Test]
		public void ItShouldRetrieveDestinationWorkspaceArtifactId()
		{
			const int expectedValue = 852147;

			_cache.Setup(x => x.GetFieldValue<int>(SyncConfigurationRdo.DestinationWorkspaceArtifactIdGuid)).Returns(expectedValue);

			_instance.DestinationWorkspaceArtifactId.Should().Be(expectedValue);
		}

		[Test]
		public void ItShouldRetrieveSourceJobTagName()
		{
			const string expectedValue = "tag name";

			_cache.Setup(x => x.GetFieldValue<string>(SyncConfigurationRdo.SourceJobTagNameGuid)).Returns(expectedValue);

			_instance.GetSourceJobTagName().Should().Be(expectedValue);
		}

		[Test]
		public void ItShouldRetrieveSourceJobTagArtifactId()
		{
			const int expectedValue = 789456;

			_cache.Setup(x => x.GetFieldValue<int>(SyncConfigurationRdo.SourceJobTagArtifactIdGuid)).Returns(expectedValue);

			_instance.SourceJobTagArtifactId.Should().Be(expectedValue);
		}

		[Test]
		public void ItShouldRetrieveSourceWorkspaceTagArtifactId()
		{
			const int expectedValue = 258963;

			_cache.Setup(x => x.GetFieldValue<int>(SyncConfigurationRdo.SourceWorkspaceTagArtifactIdGuid)).Returns(expectedValue);

			_instance.SourceWorkspaceTagArtifactId.Should().Be(expectedValue);
		}

		[Test]
		public void ItShouldRetrieveCreateSavedSearchInDestination()
		{
			const bool expectedValue = true;

			_cache.Setup(x => x.GetFieldValue<bool>(SyncConfigurationRdo.CreateSavedSearchInDestinationGuid)).Returns(expectedValue);

			_instance.CreateSavedSearchForTags.Should().Be(expectedValue);
		}

		[Test]
		[TestCase(0, false)]
		[TestCase(789123, true)]
		public void ItShouldRetrieveIsSavedSearchArtifactId(int artifactId, bool expectedValue)
		{
			_cache.Setup(x => x.GetFieldValue<int>(SyncConfigurationRdo.SavedSearchInDestinationArtifactIdGuid)).Returns(artifactId);

			_instance.IsSavedSearchArtifactIdSet.Should().Be(expectedValue);
		}

		[Test]
		public async Task ItShouldUpdateSavedSearchArtifactId()
		{
			const int artifactId = 589632;

			await _instance.SetSavedSearchInDestinationArtifactIdAsync(artifactId).ConfigureAwait(false);

			_cache.Verify(x => x.UpdateFieldValueAsync(SyncConfigurationRdo.SavedSearchInDestinationArtifactIdGuid, artifactId), Times.Once);
		}
	}
}