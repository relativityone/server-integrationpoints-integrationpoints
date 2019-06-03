using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	internal sealed class DestinationWorkspaceTagsCreationConfigurationTests
	{
		private Mock<Relativity.Sync.Storage.IConfiguration> _cache;
		private ImportSettingsDto _importSettings;
		private SyncJobParameters _syncJobParameters;
		private DestinationWorkspaceTagsCreationConfiguration _config;

		private const int _JOB_ID = 1;
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 2;

		private static readonly Guid JobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");
		private static readonly Guid DestinationWorkspaceArtifactIdGuid = new Guid("15B88438-6CF7-47AB-B630-424633159C69");
		private static readonly Guid SourceJobTagArtifactIdGuid = new Guid("C0A63A29-ABAE-4BF4-A3F4-59E5BD87A33E");
		private static readonly Guid SourceJobTagNameGuid = new Guid("DA0E1931-9460-4A61-9033-A8035697C1A4");
		private static readonly Guid SourceWorkspaceTagArtifactIdGuid = new Guid("FEAB129B-AEEF-4AA4-BC91-9EAE9A4C35F6");
		private static readonly Guid SourceWorkspaceTagNameGuid = new Guid("D828B69E-AAAE-4639-91E2-416E35C163B1");

		[SetUp]
		public void SetUp()
		{
			_cache = new Mock<Relativity.Sync.Storage.IConfiguration>();
			_importSettings = new ImportSettingsDto();
			_syncJobParameters = new SyncJobParameters(_JOB_ID, _SOURCE_WORKSPACE_ARTIFACT_ID, _importSettings);
			_config = new DestinationWorkspaceTagsCreationConfiguration(_cache.Object, _syncJobParameters);
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
			const string name = "source job";

			// act
			await _config.SetSourceJobTagAsync(artifactId, name).ConfigureAwait(false);

			// assert
			_cache.Verify(x => x.UpdateFieldValueAsync(SourceJobTagArtifactIdGuid, artifactId));
			_cache.Verify(x => x.UpdateFieldValueAsync(SourceJobTagNameGuid, name));
			_config.IsSourceJobTagSet.Should().BeTrue();
		}

		[Test]
		public async Task ItShouldSetSourceWorkspaceTag()
		{
			const int artifactId = 6;
			const string name = "source workspace";

			// act
			await _config.SetSourceWorkspaceTagAsync(artifactId, name).ConfigureAwait(false);

			// assert
			_cache.Verify(x => x.UpdateFieldValueAsync(SourceWorkspaceTagArtifactIdGuid, artifactId));
			_cache.Verify(x => x.UpdateFieldValueAsync(SourceWorkspaceTagNameGuid, name));
			_config.IsSourceWorkspaceTagSet.Should().BeTrue();
		}
	}
}