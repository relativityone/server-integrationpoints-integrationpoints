﻿using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.RDOs;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
	using RdoExpression = Expression<Func<SyncConfigurationRdo, object>>;

	[TestFixture]
	internal sealed class SourceWorkspaceTagsCreationConfigurationTests : ConfigurationTestBase
	{
		private SourceWorkspaceTagsCreationConfiguration _config;

		private const int _JOB_ID = 1;
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 2;

		[SetUp]
		public void SetUp()
		{
			SyncJobParameters syncJobParameters = new SyncJobParameters(_JOB_ID, _SOURCE_WORKSPACE_ARTIFACT_ID, 1);
			_config = new SourceWorkspaceTagsCreationConfiguration(_configuration.Object, syncJobParameters);
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

			// act
			await _config.SetDestinationWorkspaceTagArtifactIdAsync(artifactId).ConfigureAwait(false);

			// assert
			_configuration.Verify(x => x.UpdateFieldValueAsync(It.Is<RdoExpression>(e => MatchMemberName(e, nameof(SyncConfigurationRdo.DestinationWorkspaceTagArtifactId))), artifactId));
			_config.IsDestinationWorkspaceTagArtifactIdSet.Should().BeTrue();
		}

	}
}