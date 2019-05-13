using System;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.InternalMetricsCollection;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Unit.Telemetry
{
	public class TelemetryManagerTests
	{
		private Mock<IServicesMgr> _servicesManager;
		private Mock<ISyncLog> _logger;

		[SetUp]
		public void SetUp()
		{
			_servicesManager = new Mock<IServicesMgr>();
			_logger = new Mock<ISyncLog>();
		}

		private TelemetryManager CreateInstance()
		{
			return new TelemetryManager(_servicesManager.Object, _logger.Object);
		}

		[Test]
		public void ItShouldNotThrowExceptionOnAddingNullMetricsProvider()
		{
			// ARRANGE
			ITelemetryManager telemetryManager = CreateInstance();

			// ACT
			Assert.DoesNotThrow(() => telemetryManager.AddMetricProviders(null));

			// ASSERT
			_logger.Verify(x => x.LogDebug(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void ItShouldNotThrowExceptionOnErrorDuringMetricsInstall()
		{
			// ARRANGE
			ITelemetryManager telemetryManager = CreateInstance();

			Mock<ITelemetryMetricProvider> telemetryProvider = new Mock<ITelemetryMetricProvider>();

			_servicesManager.Setup(x => x.CreateProxy<IInternalMetricsCollectionManager>(It.IsAny<ExecutionIdentity>()))
				.Throws<Exception>();

			telemetryManager.AddMetricProviders(telemetryProvider.Object);

			// ACT
			Assert.DoesNotThrow(() => telemetryManager.InstallMetrics());

			// ASSERT
			_logger.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
		}
	}
}