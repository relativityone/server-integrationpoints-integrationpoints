using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.InternalMetricsCollection;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Integration.Installers
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class RegisterIntegrationPointsMetricsEventHandlerTests : IntegrationTestBase
	{
		[Test]
		public async Task IntegrationPointsMetricCategory_ShouldBeRegisteredForSUMAndAPM_AfterEventHandlerRunAtLeastOnce()
		{
			// Arrange
			var expectedMetricCategoryTargets = new Dictionary<CategoryMetricTarget, bool>
			{
				{CategoryMetricTarget.APM, true},
				{CategoryMetricTarget.SUM, true},
			};

			IInternalMetricsCollectionManager metricsManager = Helper.GetServicesManager()
				.CreateProxy<IInternalMetricsCollectionManager>(ExecutionIdentity.System);

			// Act
			List<CategoryTarget> metricCategories = await metricsManager.GetCategoryTargetsAsync().ConfigureAwait(false);

			// Assert
			metricCategories.Should().Contain(x =>
				x.Category.Name == Constants.IntegrationPoints.Telemetry.TELEMETRY_CATEGORY
				&& x.IsCategoryMetricTargetEnabled.SequenceEqual(expectedMetricCategoryTargets));
		}

		[Test]
		public async Task IntegrationPointsMetrics_ShouldBeRegisteredInIntegrationPointsCategory_AfterEventHandlerRunAtLeastOnce()
		{
			// Arrange
			IInternalMetricsCollectionManager metricsManager = Helper.GetServicesManager()
				.CreateProxy<IInternalMetricsCollectionManager>(ExecutionIdentity.System);

			// Act
			List<MetricIdentifier> metricIdentifiers = await metricsManager
				.GetMetricIdentifiersByCategoryNameAsync(Constants.IntegrationPoints.Telemetry.TELEMETRY_CATEGORY)
				.ConfigureAwait(false);

			// Assert
			metricIdentifiers.Should().Contain(x => x.Name == Constants.IntegrationPoints.Telemetry.BUCKET_INTEGRATION_POINTS);
		}
	}
}
