using System;
using System.Collections.Generic;
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
		private ITelemetryManager _telemetryManager;

		[SetUp]
		public void SetUp()
		{
			_servicesManager = new Mock<IServicesMgr>();
			_logger = new Mock<ISyncLog>();
			_telemetryManager = new TelemetryManager(_servicesManager.Object, _logger.Object);
		}

		[Test]
		public void ItShouldNotThrowExceptionOnAddingNullMetricsProvider()
		{
			// ACT
			Assert.DoesNotThrow(() => _telemetryManager.AddMetricProviders(null));

			// ASSERT
			_logger.Verify(x => x.LogDebug(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void ItShouldNotAddNullMetricsProvider()
		{
			// ARRANGE
			var metricsCollectionManager = new Mock<IInternalMetricsCollectionManager>();
			var categoryTargetList = new List<CategoryTarget> { new CategoryTarget { Category = new CategoryRef { Name = TelemetryConstants.TELEMETRY_CATEGORY } } };

			metricsCollectionManager.Setup(x => x.GetCategoryTargetsAsync())
				.ReturnsAsync(categoryTargetList);

			_servicesManager.Setup(x => x.CreateProxy<IInternalMetricsCollectionManager>(It.IsAny<ExecutionIdentity>()))
				.Returns(metricsCollectionManager.Object);

			// ACT
			_telemetryManager.AddMetricProviders(null);
			_telemetryManager.InstallMetrics();

			// ASSERT
			_logger.Verify(x => x.LogError(It.IsAny<NullReferenceException>(), It.IsAny<string>()), Times.Never);
		}

		[Test]
		public void ItShouldCreateAndEnableCategoryIfItIsNotRegistered()
		{
			// ARRANGE
			var metricsCollectionManager = new Mock<IInternalMetricsCollectionManager>();
			var categoryTargetList = new List<CategoryTarget>();

			metricsCollectionManager.Setup(x => x.GetCategoryTargetsAsync())
				.ReturnsAsync(categoryTargetList);

			metricsCollectionManager.Setup(x => x.CreateCategoryAsync(It.IsAny<Category>(), It.IsAny<bool>()))
				.ReturnsAsync(0);

			_servicesManager.Setup(x => x.CreateProxy<IInternalMetricsCollectionManager>(It.IsAny<ExecutionIdentity>()))
				.Returns(metricsCollectionManager.Object);

			// ACT
			_telemetryManager.InstallMetrics();

			// ASSERT
			metricsCollectionManager.Verify(x => x.GetCategoryTargetsAsync(), Times.Once);
			metricsCollectionManager.Verify(x => x.CreateCategoryAsync(It.IsAny<Category>(), It.IsAny<bool>()), Times.Once);
			metricsCollectionManager.Verify(x => x.UpdateCategoryTargetSingleAsync(It.IsAny<CategoryTarget>()), Times.Once);
		}

		[Test]
		public void ItShouldNotThrowExceptionOnErrorDuringMetricsInstall()
		{
			// ARRANGE
			Mock<ITelemetryMetricProvider> telemetryProvider = new Mock<ITelemetryMetricProvider>();

			_servicesManager.Setup(x => x.CreateProxy<IInternalMetricsCollectionManager>(It.IsAny<ExecutionIdentity>()))
				.Throws<Exception>();

			_telemetryManager.AddMetricProviders(telemetryProvider.Object);

			// ACT
			Assert.DoesNotThrow(() => _telemetryManager.InstallMetrics());

			// ASSERT
			_logger.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
		}
	}
}