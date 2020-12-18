using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.RDOs;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
	[TestFixture]
	internal sealed class DestinationWorkspaceTagsCreationConfigurationTests
	{
		private Mock<Relativity.Sync.Storage.IConfiguration> _cache;

		private DestinationWorkspaceTagsCreationConfiguration _config;

		private const int _JOB_ID = 1;
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 2;

		private static readonly Guid JobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");

		[SetUp]
		public void SetUp()
		{
			_cache = new Mock<Relativity.Sync.Storage.IConfiguration>();
			SyncJobParameters syncJobParameters = new SyncJobParameters(_JOB_ID, _SOURCE_WORKSPACE_ARTIFACT_ID, 1);
			_config = new DestinationWorkspaceTagsCreationConfiguration(_cache.Object, syncJobParameters);
		}

		[Test]
		public void ItShouldReturnSourceWorkspaceArtifactId()
		{
			// act
			int srcWorkspaceArtifactId = _config.SourceWorkspaceArtifactId;

			// assert
			srcWorkspaceArtifactId.Should().Be(_SOURCE_WORKSPACE_ARTIFACT_ID);
		}

		[Test]
		public void ItShouldReturnDestinationWorkspaceArtifactId()
		{
			const int destinationWorkspaceArtifactId = 3;
			_cache.Setup(x => x.GetFieldValue<int>(SyncConfigurationRdo.DestinationWorkspaceArtifactIdGuid)).Returns(destinationWorkspaceArtifactId);

			// act
			int actualDestinationWorkspaceTagArtifactId = _config.DestinationWorkspaceArtifactId;

			// assert
			actualDestinationWorkspaceTagArtifactId.Should().Be(destinationWorkspaceArtifactId);
		}

		[Test]
		public void ItShouldReturnJobHistoryArtifactId()
		{
			const int jobHistoryArtifactId = 4;
			_cache.Setup(x => x.GetFieldValue<RelativityObjectValue>(JobHistoryGuid)).Returns(new RelativityObjectValue() { ArtifactID = jobHistoryArtifactId });

			// act
			int actualJobHistoryArtifactId = _config.JobHistoryArtifactId;

			// assert
			actualJobHistoryArtifactId.Should().Be(jobHistoryArtifactId);
		}

		[Test]
		public async Task ItShouldSetSourceJobTag()
		{
			const int artifactId = 5;
			const string name = "source job";

			// act
			await _config.SetSourceJobTagAsync(artifactId, name).ConfigureAwait(false);

			// assert
			_cache.Verify(x => x.UpdateFieldValueAsync(SyncConfigurationRdo.SourceJobTagArtifactIdGuid, artifactId));
			_cache.Verify(x => x.UpdateFieldValueAsync(SyncConfigurationRdo.SourceJobTagNameGuid, name));
		}

		[Test]
		public async Task ItShouldSetSourceWorkspaceTag()
		{
			const int artifactId = 6;
			const string name = "source workspace";

			// act
			await _config.SetSourceWorkspaceTagAsync(artifactId, name).ConfigureAwait(false);

			// assert
			_cache.Verify(x => x.UpdateFieldValueAsync(SyncConfigurationRdo.SourceWorkspaceTagArtifactIdGuid, artifactId));
			_cache.Verify(x => x.UpdateFieldValueAsync(SyncConfigurationRdo.SourceWorkspaceTagNameGuid, name));
		}
	}
}