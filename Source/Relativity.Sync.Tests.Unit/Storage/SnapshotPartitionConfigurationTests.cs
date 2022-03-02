using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit.Storage
{
	internal sealed class SnapshotPartitionConfigurationTests : ConfigurationTestBase
	{
		private SnapshotPartitionConfiguration _sut;

		private const int _WORKSPACE_ID = 987432;
		private const int _JOB_ID = 9687413;

		[SetUp]
		public void SetUp()
		{
			SyncJobParameters syncJobParameters = new SyncJobParameters(_JOB_ID, _WORKSPACE_ID, It.IsAny<Guid>());
			_sut = new SnapshotPartitionConfiguration(_configuration, syncJobParameters, new EmptyLogger());
		}

		[Test]
		public void WorkspaceIdShouldMatch()
		{
			_sut.SourceWorkspaceArtifactId.Should().Be(_WORKSPACE_ID);
		}

		[Test]
		public void SyncConfigurationIdShouldMatch()
		{
			_sut.SyncConfigurationArtifactId.Should().Be(_JOB_ID);
		}

		[Test]
		public void ItShouldReturnTotalRecordsCount()
		{
			const int totalRecordsCount = 874596;

			_configurationRdo.SnapshotRecordsCount = totalRecordsCount;

			// ACT & ASSERT
			_sut.TotalRecordsCount.Should().Be(totalRecordsCount);
		}

		[Test]
		public void ItShouldReturnExportRunId()
		{
			Guid runId = new Guid("7B7CB209-69A5-4903-A210-3452EAB7BB34");

			_configurationRdo.SnapshotId = runId;

			// ACT
			Guid actualRunId = _sut.ExportRunId;

			// ASSERT
			actualRunId.Should().Be(runId);
		}

		[Test]
		[TestCaseSource(nameof(SnapshotCaseSource))]
		public void ItShouldReturnEmptyGuidForInvalidString(Guid? runId)
		{
			_configurationRdo.SnapshotId = runId;

			// ACT
			Action action = () =>
			{
				Guid guid = _sut.ExportRunId;
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