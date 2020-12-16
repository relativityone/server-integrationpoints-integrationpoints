using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;
using Relativity.Sync.RDOs;
using Relativity.Sync.Storage;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit.Storage
{
	[TestFixture]
	internal sealed class SynchronizationConfigurationTests
	{
		private Mock<Sync.Storage.IConfiguration> _cache;
		private SynchronizationConfiguration _syncConfig;

		private const int _JOB_ID = 2;
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 3;

		private static readonly Guid DestinationWorkspaceTagArtifactIdGuid = new Guid("E2100C10-B53B-43FA-BB1B-51E43DCE8208");
		private static readonly Guid JobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");
		private static readonly Guid SnapshotIdGuid = new Guid("D1210A1B-C461-46CB-9B73-9D22D05880C5");
		private static readonly Guid SourceJobTagArtifactIdGuid = new Guid("C0A63A29-ABAE-4BF4-A3F4-59E5BD87A33E");
		private static readonly Guid SourceWorkspaceTagArtifactIdGuid = new Guid("FEAB129B-AEEF-4AA4-BC91-9EAE9A4C35F6");

		[SetUp]
		public void SetUp()
		{
			_cache = new Mock<Sync.Storage.IConfiguration>();
			SyncJobParameters syncJobParameters = new SyncJobParameters(_JOB_ID, _SOURCE_WORKSPACE_ARTIFACT_ID, 1);
			_syncConfig = new SynchronizationConfiguration(_cache.Object, syncJobParameters, new JSONSerializer(), new EmptyLogger());
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
			_cache.Setup(x => x.GetFieldValue<int>(SyncConfigurationRdo.DestinationWorkspaceArtifactIdGuid)).Returns(destinationWorkspaceArtifactId);

			// act
			int actualDestinationWorkspaceArtifactId = _syncConfig.DestinationWorkspaceArtifactId;

			// assert
			actualDestinationWorkspaceArtifactId.Should().Be(destinationWorkspaceArtifactId);
		}

		[Test]
		public void DestinationWorkspaceTagArtifactId_ShouldReturnDestinationWorkspaceTagArtifactId()
		{
			const int destinationWorkspaceTagArtifactId = 3;
			_cache.Setup(x => x.GetFieldValue<int>(DestinationWorkspaceTagArtifactIdGuid)).Returns(destinationWorkspaceTagArtifactId);

			// act
			int actualDestinationWorkspaceTagArtifactId = _syncConfig.DestinationWorkspaceTagArtifactId;

			// assert
			actualDestinationWorkspaceTagArtifactId.Should().Be(destinationWorkspaceTagArtifactId);
		}

		[Test]
		public void JobHistoryArtifactId_ShouldReturnJobHistoryArtifactId()
		{
			const int jobHistoryArtifactId = 4;
			_cache.Setup(x => x.GetFieldValue<RelativityObjectValue>(JobHistoryGuid)).Returns(new RelativityObjectValue(){ArtifactID = jobHistoryArtifactId});

			// act
			int actualJobHistoryArtifactId = _syncConfig.JobHistoryArtifactId;

			// assert
			actualJobHistoryArtifactId.Should().Be(jobHistoryArtifactId);
		}

		[Test]
		public void SourceJobTagArtifactId_ShouldReturnSourceJobTagName()
		{
			const int sourceJobTagArtifactId = 105649;
			_cache.Setup(x => x.GetFieldValue<int>(SourceJobTagArtifactIdGuid)).Returns(sourceJobTagArtifactId);

			// act
			int actualSourceJobTagName = _syncConfig.SourceJobTagArtifactId;

			// assert
			actualSourceJobTagName.Should().Be(sourceJobTagArtifactId);
		}

		[Test]
		public void SourceWorkspaceTagArtifactId_ShouldReturnSourceWorkspaceTagName()
		{
			const int sourceWorkspaceTagArtifactId = 105656;
			_cache.Setup(x => x.GetFieldValue<int>(SourceWorkspaceTagArtifactIdGuid)).Returns(sourceWorkspaceTagArtifactId);

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
			_cache.Setup(x => x.GetFieldValue<string>(SyncConfigurationRdo.DestinationFolderStructureBehaviorGuid)).Returns(expected.ToString());

			// act
			DestinationFolderStructureBehavior actual = _syncConfig.DestinationFolderStructureBehavior;

			// assert
			actual.Should().Be(expected);
		}

		[Test]
		public void ExportRunId_ShouldReturnExportRunId()
		{
			// ARRANGE
			const string runId = "7B7CB209-69A5-4903-A210-3452EAB7BB34";

			_cache.Setup(x => x.GetFieldValue<string>(SnapshotIdGuid)).Returns(runId);

			// ACT
			Guid actualRunId = _syncConfig.ExportRunId;

			// ASSERT
			actualRunId.Should().Be(Guid.Parse(runId));
		}

		[Test]
		public void ImageImport_ShouldReturnValue()
		{
			// ARRANGE
			const bool imageImport = true;
			_cache.Setup(x => x.GetFieldValue<bool>(SyncConfigurationRdo.ImageImportGuid)).Returns(imageImport);

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
			_cache.Setup(x => x.GetFieldValue<string>(SyncConfigurationRdo.ImageFileCopyModeGuid)).Returns(imageCopyMode.ToString());

			// ACT
			ImportImageFileCopyMode actualImportImageFileCopyMode = _syncConfig.ImportImageFileCopyMode;

			// ASSERT
			actualImportImageFileCopyMode.Should().Be(imageCopyMode);
		}

		[Test]
		[TestCase("")]
		[TestCase("ABC")]
		[TestCase("7B7CB209-69A5-4903-A210-3452EAB7BB3", Description = "Missing one character")]
		public void ExportRunId_ShouldReturnEmptyGuidForInvalidString(string runId)
		{
			_cache.Setup(x => x.GetFieldValue<string>(SnapshotIdGuid)).Returns(runId);

			// ACT
			Action action = () =>
			{
				Guid guid = _syncConfig.ExportRunId;
			};

			// ASSERT
			action.Should().Throw<ArgumentException>();
		}
	}
}