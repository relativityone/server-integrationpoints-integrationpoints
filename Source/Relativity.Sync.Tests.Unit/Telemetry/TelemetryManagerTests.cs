using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.InternalMetricsCollection;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Unit.Telemetry
{
	public class TelemetryManagerTests
	{
		private Mock<ISourceServiceFactoryForAdmin> _serviceFactoryForAdmin;
		private Mock<IAPILog> _logger;
		private ITelemetryManager _telemetryManager;

		[SetUp]
		public void SetUp()
		{
			_serviceFactoryForAdmin = new Mock<ISourceServiceFactoryForAdmin>();
			_logger = new Mock<IAPILog>();
			_telemetryManager = new TelemetryMetricsInstaller(_serviceFactoryForAdmin.Object, _logger.Object);
		}

		[Test]
		public void ItShouldNotThrowExceptionOnAddingNullMetricsProvider()
		{
			// ACT
			Assert.DoesNotThrow(() => _telemetryManager.AddMetricProvider(null));

			// ASSERT
			_logger.Verify(x => x.LogDebug(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void ItShouldNotAddNullMetricsProvider()
		{
			// ARRANGE
			var metricsCollectionManager = new Mock<IInternalMetricsCollectionManager>();
			var categoryTargetList = new List<CategoryTarget> { new CategoryTarget { Category = new CategoryRef { Name = TelemetryConstants.INTEGRATION_POINTS_TELEMETRY_CATEGORY } } };

			metricsCollectionManager.Setup(x => x.GetCategoryTargetsAsync())
				.ReturnsAsync(categoryTargetList);

			_serviceFactoryForAdmin.Setup(x => x.CreateProxyAsync<IInternalMetricsCollectionManager>())
				.Returns(Task.FromResult(metricsCollectionManager.Object));

			// ACT
			_telemetryManager.AddMetricProvider(null);
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

			_serviceFactoryForAdmin.Setup(x => x.CreateProxyAsync<IInternalMetricsCollectionManager>())
				.Returns(Task.FromResult(metricsCollectionManager.Object));

			Mock<ITelemetryMetricProvider> telemetryProvider = new Mock<ITelemetryMetricProvider>();
			telemetryProvider.SetupGet(m => m.CategoryName).Returns(nameof(TelemetryManagerTests));
			_telemetryManager.AddMetricProvider(telemetryProvider.Object);

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

			_serviceFactoryForAdmin.Setup(x => x.CreateProxyAsync<IInternalMetricsCollectionManager>())
				.Throws<Exception>();

			_telemetryManager.AddMetricProvider(telemetryProvider.Object);

			// ACT
			Assert.DoesNotThrow(() => _telemetryManager.InstallMetrics());

			// ASSERT
			_logger.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
		}
	}
}
