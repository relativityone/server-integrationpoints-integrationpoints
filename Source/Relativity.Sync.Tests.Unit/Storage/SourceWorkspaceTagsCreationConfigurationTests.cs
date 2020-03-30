using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
	[TestFixture]
	internal sealed class SourceWorkspaceTagsCreationConfigurationTests
	{
		private Mock<Sync.Storage.IConfiguration> _cache;
		private SourceWorkspaceTagsCreationConfiguration _config;

		private const int _JOB_ID = 1;
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 2;

		private static readonly Guid JobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");
		private static readonly Guid DestinationWorkspaceArtifactIdGuid = new Guid("15B88438-6CF7-47AB-B630-424633159C69");
		private static readonly Guid DestinationWorkspaceTagArtifactIdGuid = new Guid("E2100C10-B53B-43FA-BB1B-51E43DCE8208");

		[SetUp]
		public void SetUp()
		{
			_cache = new Mock<Sync.Storage.IConfiguration>();
			SyncJobParameters syncJobParameters = new SyncJobParameters(_JOB_ID, _SOURCE_WORKSPACE_ARTIFACT_ID, 1);
			_config = new SourceWorkspaceTagsCreationConfiguration(_cache.Object, syncJobParameters);
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
			_cache.Setup(x => x.GetFieldValue<int>(DestinationWorkspaceArtifactIdGuid)).Returns(destinationWorkspaceArtifactId);

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

			// act
			await _config.SetDestinationWorkspaceTagArtifactIdAsync(artifactId).ConfigureAwait(false);

			// assert
			_cache.Verify(x => x.UpdateFieldValueAsync(DestinationWorkspaceTagArtifactIdGuid, artifactId));
			_config.IsDestinationWorkspaceTagArtifactIdSet.Should().BeTrue();
		}

	}
}