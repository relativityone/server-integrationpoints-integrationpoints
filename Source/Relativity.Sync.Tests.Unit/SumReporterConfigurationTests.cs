using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
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
			var integrationPointGuid = new Guid("03D4F67E-22C9-488C-BEE6-411F05C52E01");
			var jobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");

			var cache = new Mock<IConfiguration>();
			cache.Setup(x => x.GetFieldValue<RelativityObjectValue>(integrationPointGuid)).Returns(new RelativityObjectValue { ArtifactID = integrationPointArtifactId }).Verifiable();
			cache.Setup(x => x.GetFieldValue<RelativityObjectValue>(jobHistoryGuid)).Returns(new RelativityObjectValue { ArtifactID = jobHistoryArtifactId }).Verifiable();

			var configuration = new SumReporterConfiguration(cache.Object);

			// Act
			string actualValue = configuration.WorkflowId;

			// Assert
			Mock.Verify(cache);

			actualValue.Should().Be($"{TelemetryConstants.PROVIDER_NAME}_{integrationPointArtifactId}_{jobHistoryArtifactId}");
		}
	}
}