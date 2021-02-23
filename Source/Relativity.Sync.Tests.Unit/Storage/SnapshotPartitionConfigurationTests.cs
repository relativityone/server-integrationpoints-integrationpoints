using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Logging;
using Relativity.Sync.RDOs;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
	[TestFixture]
	internal sealed class SnapshotPartitionConfigurationTests
	{
		private SnapshotPartitionConfiguration _instance;

		private Mock<Sync.Storage.IConfiguration> _cache;

		private const int _WORKSPACE_ID = 987432;
		private const int _JOB_ID = 9687413;

		private const int _BATCH_SIZE = 985632;

		[SetUp]
		public void SetUp()
		{
			_cache = new Mock<Sync.Storage.IConfiguration>();
			SyncJobParameters syncJobParameters = new SyncJobParameters(_JOB_ID, _WORKSPACE_ID, 1);
			SyncJobExecutionConfiguration configuration = new SyncJobExecutionConfiguration
			{
				BatchSize = _BATCH_SIZE
			};

			_instance = new SnapshotPartitionConfiguration(_cache.Object, syncJobParameters, configuration, new EmptyLogger());
		}

		[Test]
		public void WorkspaceIdShouldMatch()
		{
			_instance.SourceWorkspaceArtifactId.Should().Be(_WORKSPACE_ID);
		}

		[Test]
		public void SyncConfigurationIdShouldMatch()
		{
			_instance.SyncConfigurationArtifactId.Should().Be(_JOB_ID);
		}

		[Test]
		public void BatchSizeShouldMatch()
		{
			_instance.BatchSize.Should().Be(_BATCH_SIZE);
		}

		[Test]
		public void ItShouldReturnTotalRecordsCount()
		{
			const int totalRecordsCount = 874596;

			_cache.Setup(x => x.GetFieldValue<int>(SyncRdoGuids.SnapshotRecordsCountGuid)).Returns(totalRecordsCount);

			// ACT & ASSERT
			_instance.TotalRecordsCount.Should().Be(totalRecordsCount);
		}

		[Test]
		public void ItShouldReturnExportRunId()
		{
			const string runId = "7B7CB209-69A5-4903-A210-3452EAB7BB34";

			_cache.Setup(x => x.GetFieldValue<string>(SyncRdoGuids.SnapshotIdGuid)).Returns(runId);

			// ACT
			Guid actualRunId = _instance.ExportRunId;

			// ASSERT
			actualRunId.Should().Be(Guid.Parse(runId));
		}

		[Test]
		[TestCase("")]
		[TestCase("ABC")]
		[TestCase("7B7CB209-69A5-4903-A210-3452EAB7BB3", Description = "Missing one character")]
		public void ItShouldReturnEmptyGuidForInvalidString(string runId)
		{
			_cache.Setup(x => x.GetFieldValue<string>(SyncRdoGuids.SnapshotIdGuid)).Returns(runId);

			// ACT
			Action action = () =>
			{
				Guid guid = _instance.ExportRunId;
			};

			// ASSERT
			action.Should().Throw<ArgumentException>();
		}
	}
}