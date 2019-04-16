using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	internal sealed class SnapshotPartitionConfigurationTests
	{
		private SnapshotPartitionConfiguration _instance;

		private Mock<IConfiguration> _cache;

		private const int _WORKSPACE_ID = 987432;
		private const int _JOB_ID = 9687413;

		private static readonly Guid SnapshotIdGuid = new Guid("D1210A1B-C461-46CB-9B73-9D22D05880C5");
		private static readonly Guid SnapshotRecordsCountGuid = new Guid("57B93F20-2648-4ACF-973B-BCBA8A08E2BD");

		[SetUp]
		public void SetUp()
		{
			_cache = new Mock<IConfiguration>();
			SyncJobParameters syncJobParameters = new SyncJobParameters(_JOB_ID, _WORKSPACE_ID);

			_instance = new SnapshotPartitionConfiguration(_cache.Object, syncJobParameters, new EmptyLogger());
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
		public void ItShouldReturnTotalRecordsCount()
		{
			const int totalRecordsCount = 874596;

			_cache.Setup(x => x.GetFieldValue<int>(SnapshotRecordsCountGuid)).Returns(totalRecordsCount);

			// ACT & ASSERT
			_instance.TotalRecordsCount.Should().Be(totalRecordsCount);
		}

		[Test]
		public void ItShouldReturnExportRunId()
		{
			const string runId = "7B7CB209-69A5-4903-A210-3452EAB7BB34";

			_cache.Setup(x => x.GetFieldValue<string>(SnapshotIdGuid)).Returns(runId);

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
			_cache.Setup(x => x.GetFieldValue<string>(SnapshotIdGuid)).Returns(runId);

			// ACT
			Guid actualRunId = _instance.ExportRunId;

			// ASSERT
			actualRunId.Should().Be(Guid.Empty);
		}
	}
}