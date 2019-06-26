using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	internal sealed class SynchronizationConfigurationTests
	{
		private Mock<Sync.Storage.IConfiguration> _cache;
		private ImportSettingsDto _importSettings;
		private SynchronizationConfiguration _syncConfig;

		private const int _JOB_ID = 2;
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 3;

		private static readonly Guid DestinationWorkspaceArtifactIdGuid = new Guid("15B88438-6CF7-47AB-B630-424633159C69");
		private static readonly Guid DestinationWorkspaceTagArtifactIdGuid = new Guid("E2100C10-B53B-43FA-BB1B-51E43DCE8208");
		private static readonly Guid JobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");
		private static readonly Guid SnapshotIdGuid = new Guid("D1210A1B-C461-46CB-9B73-9D22D05880C5");
		private static readonly Guid SourceJobTagArtifactIdGuid = new Guid("C0A63A29-ABAE-4BF4-A3F4-59E5BD87A33E");
		private static readonly Guid SourceWorkspaceTagArtifactIdGuid = new Guid("FEAB129B-AEEF-4AA4-BC91-9EAE9A4C35F6");
		private static readonly Guid DestinationFolderStructureBehaviorGuid = new Guid("A1593105-BD99-4A15-A51A-3AA8D4195908");

		[SetUp]
		public void SetUp()
		{
			_cache = new Mock<Sync.Storage.IConfiguration>();
			_importSettings = new ImportSettingsDto();
			SyncJobParameters syncJobParameters = new SyncJobParameters(_JOB_ID, _SOURCE_WORKSPACE_ARTIFACT_ID, Guid.NewGuid(), _importSettings);
			_syncConfig = new SynchronizationConfiguration(_cache.Object, syncJobParameters, new EmptyLogger());
		}

		[Test]
		public void ItShouldReturnImportSettings()
		{
			// act
			ImportSettingsDto importSettings = _syncConfig.ImportSettings;

			// assert
			importSettings.Should().Be(_importSettings);
		}

		[Test]
		public void ItShouldReturnSourceWorkspaceArtifactId()
		{
			// act
			int srcWorkspaceArtifactId = _syncConfig.SourceWorkspaceArtifactId;

			// assert
			srcWorkspaceArtifactId.Should().Be(_SOURCE_WORKSPACE_ARTIFACT_ID);
		}

		[Test]
		public void ItShouldReturnDestinationWorkspaceArtifactId()
		{
			const int destinationWorkspaceArtifactId = 1040589;
			_cache.Setup(x => x.GetFieldValue<int>(DestinationWorkspaceArtifactIdGuid)).Returns(destinationWorkspaceArtifactId);

			// act
			int actualDestinationWorkspaceArtifactId = _syncConfig.DestinationWorkspaceArtifactId;

			// assert
			actualDestinationWorkspaceArtifactId.Should().Be(destinationWorkspaceArtifactId);
		}

		[Test]
		public void ItShouldReturnDestinationWorkspaceTagArtifactId()
		{
			const int destinationWorkspaceTagArtifactId = 3;
			_cache.Setup(x => x.GetFieldValue<int>(DestinationWorkspaceTagArtifactIdGuid)).Returns(destinationWorkspaceTagArtifactId);

			// act
			int actualDestinationWorkspaceTagArtifactId = _syncConfig.DestinationWorkspaceTagArtifactId;

			// assert
			actualDestinationWorkspaceTagArtifactId.Should().Be(destinationWorkspaceTagArtifactId);
		}

		[Test]
		public void ItShouldReturnJobHistoryArtifactId()
		{
			const int jobHistoryArtifactId = 4;
			_cache.Setup(x => x.GetFieldValue<RelativityObjectValue>(JobHistoryGuid)).Returns(new RelativityObjectValue(){ArtifactID = jobHistoryArtifactId});

			// act
			int actualJobHistoryArtifactId = _syncConfig.JobHistoryArtifactId;

			// assert
			actualJobHistoryArtifactId.Should().Be(jobHistoryArtifactId);
		}

		[Test]
		public void ItShouldReturnSourceJobTagName()
		{
			const int sourceJobTagArtifactId = 105649;
			_cache.Setup(x => x.GetFieldValue<int>(SourceJobTagArtifactIdGuid)).Returns(sourceJobTagArtifactId);

			// act
			int actualSourceJobTagName = _syncConfig.SourceJobTagArtifactId;

			// assert
			actualSourceJobTagName.Should().Be(sourceJobTagArtifactId);
		}

		[Test]
		public void ItShouldReturnSourceWorkspaceTagName()
		{
			const int sourceWorkspaceTagArtifactId = 105656;
			_cache.Setup(x => x.GetFieldValue<int>(SourceWorkspaceTagArtifactIdGuid)).Returns(sourceWorkspaceTagArtifactId);

			// act
			int actualSourceJobTagName = _syncConfig.SourceWorkspaceTagArtifactId;

			// assert
			actualSourceJobTagName.Should().Be(sourceWorkspaceTagArtifactId);
		}

		[Test]
		public void ItShouldReturnSyncConfigurationArtifactId()
		{
			// act
			int syncConfigurationArtifactId = _syncConfig.SyncConfigurationArtifactId;

			// assert
			syncConfigurationArtifactId.Should().Be(_JOB_ID);
		}

		[Test]
		public void ItShouldReturnDestinationFolderStructureBehavior()
		{
			DestinationFolderStructureBehavior expected = DestinationFolderStructureBehavior.ReadFromField;
			_cache.Setup(x => x.GetFieldValue<string>(DestinationFolderStructureBehaviorGuid)).Returns(expected.ToString());

			// act
			DestinationFolderStructureBehavior actual = _syncConfig.DestinationFolderStructureBehavior;

			// assert
			actual.Should().Be(expected);
		}

		[Test]
		public void ItShouldReturnExportRunId()
		{
			const string runId = "7B7CB209-69A5-4903-A210-3452EAB7BB34";

			_cache.Setup(x => x.GetFieldValue<string>(SnapshotIdGuid)).Returns(runId);

			// ACT
			Guid actualRunId = _syncConfig.ExportRunId;

			// ASSERT
			actualRunId.Should().Be(Guid.Parse(runId));
		}

		[Test]
		[TestCase("")]
		[TestCase("ABC")]
		[TestCase("7B7CB209-69A5-4903-A210-3452EAB7BB3", Description = "Missing one character")]
		public void ItShouldReturnEmptyGuidForInvalidString(string runId)
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