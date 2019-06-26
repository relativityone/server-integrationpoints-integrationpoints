using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Unit.Storage
{
	[TestFixture]
	public class JobEndMetricsConfigurationTests
	{
		private JobEndMetricsConfiguration _instance;
		
		private const int _INTEGRATION_POINT_ARTIFACT_ID = 102779;
		private const int _JOB_HISTORY_ARTIFACT_ID = 103799;

		private static readonly Guid JobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");

		[SetUp]
		public void SetUp()
		{
			const int jobId = 50;
			const int workspaceArtifactId = 101679;
			const string correlationId = "Sample_Correlation_ID";

			Mock<Sync.Storage.IConfiguration> cache = new Mock<Sync.Storage.IConfiguration>();
			SyncJobParameters syncJobParameters = new SyncJobParameters(jobId, workspaceArtifactId, Guid.NewGuid(), _INTEGRATION_POINT_ARTIFACT_ID, correlationId, new ImportSettingsDto());

			cache.Setup(x => x.GetFieldValue<RelativityObjectValue>(JobHistoryGuid)).Returns(new RelativityObjectValue { ArtifactID = _JOB_HISTORY_ARTIFACT_ID });

			_instance = new JobEndMetricsConfiguration(cache.Object, syncJobParameters);
		}

		[Test]
		public void WorkflowIdGoldFlowTest()
		{
			// Arrange
			string expectedWorkflowId = $"{TelemetryConstants.PROVIDER_NAME}_{_INTEGRATION_POINT_ARTIFACT_ID}_{_JOB_HISTORY_ARTIFACT_ID}";

			// Act
			string actualWorkflowId = _instance.WorkflowId;

			// Assert
			actualWorkflowId.Should().Be(expectedWorkflowId);
		}
	}
}