﻿using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit.Storage
{
	internal sealed class SynchronizationConfigurationTests : ConfigurationTestBase
	{
		private SynchronizationConfiguration _syncConfig;

		private const int _JOB_ID = 2;
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 3;


		[SetUp]
		public void SetUp()
		{
			SyncJobParameters syncJobParameters = new SyncJobParameters(_JOB_ID, _SOURCE_WORKSPACE_ARTIFACT_ID, Guid.NewGuid());
			_syncConfig = new SynchronizationConfiguration(_configuration.Object, syncJobParameters, new JSONSerializer(), new EmptyLogger());
		}

		[Test]
		public void SourceWorkspaceArtifactId_ShouldReturnSourceWorkspaceArtifactId()
		{
			// act
			int srcWorkspaceArtifactId = _syncConfig.SourceWorkspaceArtifactId;

			// assert
			srcWorkspaceArtifactId.Should().Be(_SOURCE_WORKSPACE_ARTIFACT_ID);
		}

		[Test]
		public void DestinationWorkspaceArtifactId_ShouldReturnDestinationWorkspaceArtifactId()
		{
			const int destinationWorkspaceArtifactId = 1040589;
			_configurationRdo.DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId;

			// act
			int actualDestinationWorkspaceArtifactId = _syncConfig.DestinationWorkspaceArtifactId;

			// assert
			actualDestinationWorkspaceArtifactId.Should().Be(destinationWorkspaceArtifactId);
		}

		[Test]
		public void DestinationWorkspaceTagArtifactId_ShouldReturnDestinationWorkspaceTagArtifactId()
		{
			const int destinationWorkspaceTagArtifactId = 3;
			_configurationRdo.DestinationWorkspaceTagArtifactId = destinationWorkspaceTagArtifactId;


			// act
			int actualDestinationWorkspaceTagArtifactId = _syncConfig.DestinationWorkspaceTagArtifactId;

			// assert
			actualDestinationWorkspaceTagArtifactId.Should().Be(destinationWorkspaceTagArtifactId);
		}

		[Test]
		public void JobHistoryArtifactId_ShouldReturnJobHistoryArtifactId()
		{
			const int jobHistoryArtifactId = 4;
			_configurationRdo.JobHistoryId = jobHistoryArtifactId;

			// act
			int actualJobHistoryArtifactId = _syncConfig.JobHistoryArtifactId;

			// assert
			actualJobHistoryArtifactId.Should().Be(jobHistoryArtifactId);
		}

		[Test]
		public void SourceJobTagArtifactId_ShouldReturnSourceJobTagName()
		{
			const int sourceJobTagArtifactId = 105649;
			_configurationRdo.SourceJobTagArtifactId = sourceJobTagArtifactId;
				
			// act
			int actualSourceJobTagName = _syncConfig.SourceJobTagArtifactId;

			// assert
			actualSourceJobTagName.Should().Be(sourceJobTagArtifactId);
		}

		[Test]
		public void SourceWorkspaceTagArtifactId_ShouldReturnSourceWorkspaceTagName()
		{
			const int sourceWorkspaceTagArtifactId = 105656;
			_configurationRdo.SourceWorkspaceTagArtifactId = sourceWorkspaceTagArtifactId;
			

			// act
			int actualSourceJobTagName = _syncConfig.SourceWorkspaceTagArtifactId;

			// assert
			actualSourceJobTagName.Should().Be(sourceWorkspaceTagArtifactId);
		}

		[Test]
		public void SyncConfigurationArtifactId_ShouldReturnSyncConfigurationArtifactId()
		{
			// act
			int syncConfigurationArtifactId = _syncConfig.SyncConfigurationArtifactId;

			// assert
			syncConfigurationArtifactId.Should().Be(_JOB_ID);
		}

		[Test]
		public void DestinationFolderStructureBehavior_ShouldReturnDestinationFolderStructureBehavior()
		{
			DestinationFolderStructureBehavior expected = DestinationFolderStructureBehavior.ReadFromField;
			_configurationRdo.DestinationFolderStructureBehavior = expected.ToString();

			// act
			DestinationFolderStructureBehavior actual = _syncConfig.DestinationFolderStructureBehavior;

			// assert
			actual.Should().Be(expected);
		}

		[Test]
		public void ExportRunId_ShouldReturnExportRunId()
		{
			// ARRANGE
			Guid runId = new Guid( "7B7CB209-69A5-4903-A210-3452EAB7BB34");

			_configurationRdo.SnapshotId = runId;

			// ACT
			Guid actualRunId = _syncConfig.ExportRunId;

			// ASSERT
			actualRunId.Should().Be(runId);
		}

		[Test]
		public void ImageImport_ShouldReturnValue()
		{
			// ARRANGE
			const bool imageImport = true;
			_configurationRdo.ImageImport = imageImport;

			// ACT
			bool actualImageImport = _syncConfig.ImageImport;

			// ASSERT
			actualImageImport.Should().Be(imageImport);
		}

		[Test]
		public void ImageFileCopyMode_ShouldReturnValue()
		{
			// ARRANGE
			ImportImageFileCopyMode imageCopyMode = ImportImageFileCopyMode.CopyFiles;
			_configurationRdo.ImageFileCopyMode = imageCopyMode.ToString();

			// ACT
			ImportImageFileCopyMode actualImportImageFileCopyMode = _syncConfig.ImportImageFileCopyMode;

			// ASSERT
			actualImportImageFileCopyMode.Should().Be(imageCopyMode);
		}

		[Test]
		[TestCaseSource(nameof(SnapshotCaseSource))]
		public void ExportRunId_ShouldReturnEmptyGuidForInvalidString(Guid runId)
		{
			_configurationRdo.SnapshotId = runId;

			// ACT
			Action action = () =>
			{
				Guid guid = _syncConfig.ExportRunId;
			};

			// ASSERT
			action.Should().Throw<ArgumentException>();
		}
		
		static IEnumerable<TestCaseData> SnapshotCaseSource()
		{
			yield return new TestCaseData((Guid?) null);
			yield return new TestCaseData((Guid?) Guid.Empty);
		}
	}
}