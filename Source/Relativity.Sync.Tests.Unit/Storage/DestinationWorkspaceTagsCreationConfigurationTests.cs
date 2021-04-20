using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.RDOs;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
	using RdoExpressionInt = Expression<Func<SyncConfigurationRdo, int>>;
	using RdoExpressionString = Expression<Func<SyncConfigurationRdo, string>>;

	[TestFixture]
	internal sealed class DestinationWorkspaceTagsCreationConfigurationTests : ConfigurationTestBase
	{
		private DestinationWorkspaceTagsCreationConfiguration _config;

		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 2;

		[SetUp]
		public void SetUp()
		{
			SyncJobParameters syncJobParameters = new SyncJobParameters(It.IsAny<int>(), _SOURCE_WORKSPACE_ARTIFACT_ID, It.IsAny<Guid>());
			_config = new DestinationWorkspaceTagsCreationConfiguration(_configuration.Object, syncJobParameters);
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
			_configurationRdo.DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId;

			// act
			int actualDestinationWorkspaceTagArtifactId = _config.DestinationWorkspaceArtifactId;

			// assert
			actualDestinationWorkspaceTagArtifactId.Should().Be(destinationWorkspaceArtifactId);
		}

		[Test]
		public void ItShouldReturnJobHistoryArtifactId()
		{
			const int jobHistoryArtifactId = 4;
			_configurationRdo.JobHistoryId = jobHistoryArtifactId;

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
			_configuration.Verify(x => x.UpdateFieldValueAsync(It.Is<RdoExpressionInt>(e => MatchMemberName(e, nameof(SyncConfigurationRdo.SourceJobTagArtifactId))), artifactId));
			_configuration.Verify(x => x.UpdateFieldValueAsync(It.Is<RdoExpressionString>(e => MatchMemberName(e, nameof(SyncConfigurationRdo.SourceJobTagName))), name));
		}

		[Test]
		public async Task ItShouldSetSourceWorkspaceTag()
		{
			const int artifactId = 6;
			const string name = "source workspace";

			// act
			await _config.SetSourceWorkspaceTagAsync(artifactId, name).ConfigureAwait(false);

			// assert
			_configuration.Verify(x => x.UpdateFieldValueAsync(It.Is<RdoExpressionInt>(e => MatchMemberName(e, nameof(SyncConfigurationRdo.SourceWorkspaceTagArtifactId))), artifactId));
			_configuration.Verify(x => x.UpdateFieldValueAsync(It.Is<RdoExpressionString>(e => MatchMemberName(e, nameof(SyncConfigurationRdo.SourceWorkspaceTagName))), name));
		}
	}
}