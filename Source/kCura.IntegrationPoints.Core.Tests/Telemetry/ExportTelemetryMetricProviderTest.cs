using System;
using kCura.IntegrationPoints.Core.Telemetry;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.InternalMetricsCollection;

namespace kCura.IntegrationPoints.Core.Tests.Telemetry
{
	public class ExportTelemetryMetricProviderTest
	{
		#region Fields

		private ExportTelemetryMetricProvider _instanceToTest;

		private IHelper _mockHelper;
		private IServicesMgr _mockServicesMgr;
		private IInternalMetricsCollectionManager _mockInternalMetricsCollectionManager;

		private Category _category;

		#endregion  //Fields

		[SetUp]
		public void Init()
		{
			_instanceToTest = new ExportTelemetryMetricProvider();

			_mockHelper = Substitute.For<IHelper>();
			_mockServicesMgr = Substitute.For<IServicesMgr>();
			_mockInternalMetricsCollectionManager = Substitute.For<IInternalMetricsCollectionManager>();

			_mockHelper.GetServicesManager().Returns(_mockServicesMgr);
			_mockServicesMgr.CreateProxy<IInternalMetricsCollectionManager>(ExecutionIdentity.System)
				.Returns(_mockInternalMetricsCollectionManager);


			_category = new Category
			{
				ID = 1,
				Name = Constants.IntegrationPoints.Telemetry.TELEMETRY_CATEGORY
			};
		}

		#region Tests

		[Test]
		public void ItShouldInstallExportMetricIdentifiers()
		{
			// Act
			_instanceToTest.Run(_category, _mockHelper);

			// Assert
			_mockInternalMetricsCollectionManager
				.Received(ExportTelemetryMetricProvider.ExportMetricIdentifiers.Count)
				.CreateMetricIdentifierAsync(Arg.Is<MetricIdentifier>(item => item.Categories.Contains(_category)), false);
		}

		[Test]
		public void ItShouldThrowExceptionOnInstallExportMetricIdentifiers()
		{
			// Arrange
			_mockInternalMetricsCollectionManager
				.CreateMetricIdentifierAsync(Arg.Any<MetricIdentifier>(), Arg.Any<bool>())
				.Throws<AggregateException>();

			// Act/Assert
			Assert.That(() => _instanceToTest.Run(_category, _mockHelper), Throws.TypeOf<Exception>());
		}

		#endregion //Tests
	}
}
