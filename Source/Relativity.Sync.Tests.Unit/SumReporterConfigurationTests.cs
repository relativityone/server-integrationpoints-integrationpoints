using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public class SumReporterConfigurationTests
	{
		[Test]
		public void WorkflowIdContainsProperFormatTest()
		{
			// Arrange
			const int integrationPointArtifactId = 102575;
			const int jobHistoryArtifactId = 102586;
			var jobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");

			var cache = new Mock<Relativity.Sync.Storage.IConfiguration>();
			cache.Setup(x => x.GetFieldValue<RelativityObjectValue>(jobHistoryGuid)).Returns(new RelativityObjectValue { ArtifactID = jobHistoryArtifactId }).Verifiable();
			var syncJobParameters = new SyncJobParameters(0, 1, Guid.NewGuid(), integrationPointArtifactId, string.Empty, new ImportSettingsDto());

			var configuration = new SumReporterConfiguration(cache.Object, syncJobParameters);

			// Act
			string actualValue = configuration.WorkflowId;

			// Assert
			Mock.Verify(cache);

			actualValue.Should().Be($"{TelemetryConstants.PROVIDER_NAME}_{integrationPointArtifactId}_{jobHistoryArtifactId}");
		}
	}
}