using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class DestinationWorkspaceSavedSearchCreationConfigurationTests
	{
		private DestinationWorkspaceSavedSearchCreationConfiguration _instance;

		private Mock<IConfiguration> _cache;
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 1;

		private static readonly Guid DestinationWorkspaceArtifactIdGuid = new Guid("15B88438-6CF7-47AB-B630-424633159C69");
		private static readonly Guid SourceJobTagNameGuid = new Guid("DA0E1931-9460-4A61-9033-A8035697C1A4");
		private static readonly Guid SourceJobTagArtifactIdGuid = new Guid("C0A63A29-ABAE-4BF4-A3F4-59E5BD87A33E");
		private static readonly Guid SourceWorkspaceTagNameGuid = new Guid("D828B69E-AAAE-4639-91E2-416E35C163B1");
		private static readonly Guid SourceWorkspaceTagArtifactIdGuid = new Guid("FEAB129B-AEEF-4AA4-BC91-9EAE9A4C35F6");
		private static readonly Guid CreateSavedSearchInDestinationGuid = new Guid("BFAB4AF6-4704-4A12-A8CA-C96A1FBCB77D");
		private static readonly Guid SavedSearchInDestinationArtifactIdGuid = new Guid("83F4DD7A-2231-4C54-BAAA-D1D5B0FE6E31");

		[SetUp]
		public void SetUp()
		{
			_cache = new Mock<IConfiguration>();

			_instance = new DestinationWorkspaceSavedSearchCreationConfiguration(_cache.Object, _SOURCE_WORKSPACE_ARTIFACT_ID);
		}

		[Test]
		public void ItShouldRetrieveSourceWorkspaceArtifactId()
		{
			_instance.SourceWorkspaceArtifactId.Should().Be(_SOURCE_WORKSPACE_ARTIFACT_ID);
		}

		[Test]
		public void ItShouldRetrieveDestinationWorkspaceArtifactId()
		{
			const int expectedValue = 852147;

			_cache.Setup(x => x.GetFieldValue<int>(DestinationWorkspaceArtifactIdGuid)).Returns(expectedValue);

			_instance.DestinationWorkspaceArtifactId.Should().Be(expectedValue);
		}

		[Test]
		public void ItShouldRetrieveSourceJobTagName()
		{
			const string expectedValue = "tag name";

			_cache.Setup(x => x.GetFieldValue<string>(SourceJobTagNameGuid)).Returns(expectedValue);

			_instance.SourceJobTagName.Should().Be(expectedValue);
		}

		[Test]
		public void ItShouldRetrieveSourceJobTagArtifactId()
		{
			const int expectedValue = 789456;

			_cache.Setup(x => x.GetFieldValue<int>(SourceJobTagArtifactIdGuid)).Returns(expectedValue);

			_instance.SourceJobTagArtifactId.Should().Be(expectedValue);
		}

		[Test]
		public void ItShouldRetrieveSourceWorkspaceTagName()
		{
			const string expectedValue = "tag name";

			_cache.Setup(x => x.GetFieldValue<string>(SourceWorkspaceTagNameGuid)).Returns(expectedValue);

			_instance.SourceWorkspaceTagName.Should().Be(expectedValue);
		}

		[Test]
		public void ItShouldRetrieveSourceWorkspaceTagArtifactId()
		{
			const int expectedValue = 258963;

			_cache.Setup(x => x.GetFieldValue<int>(SourceWorkspaceTagArtifactIdGuid)).Returns(expectedValue);

			_instance.SourceWorkspaceTagArtifactId.Should().Be(expectedValue);
		}

		[Test]
		public void ItShouldRetrieveCreateSavedSearchInDestination()
		{
			const bool expectedValue = true;

			_cache.Setup(x => x.GetFieldValue<bool>(CreateSavedSearchInDestinationGuid)).Returns(expectedValue);

			_instance.CreateSavedSearchForTags.Should().Be(expectedValue);
		}

		[Test]
		[TestCase(0, false)]
		[TestCase(789123, true)]
		public void ItShouldRetrieveIsSavedSearchArtifactId(int artifactId, bool expectedValue)
		{
			_cache.Setup(x => x.GetFieldValue<int>(SavedSearchInDestinationArtifactIdGuid)).Returns(artifactId);

			_instance.IsSavedSearchArtifactIdSet.Should().Be(expectedValue);
		}

		[Test]
		public async Task ItShouldUpdateSavedSearchArtifactId()
		{
			const int artifactId = 589632;

			await _instance.SetSavedSearchInDestinationArtifactIdAsync(artifactId).ConfigureAwait(false);

			_cache.Verify(x => x.UpdateFieldValueAsync(SavedSearchInDestinationArtifactIdGuid, artifactId), Times.Once);
		}
	}
}