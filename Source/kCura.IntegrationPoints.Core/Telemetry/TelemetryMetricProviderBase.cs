using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.InternalMetricsCollection;

namespace kCura.IntegrationPoints.Core.Telemetry
{
	public abstract class TelemetryMetricProviderBase : ITelemetryMetricProvider
	{
		private readonly IAPILog _logger;

		protected TelemetryMetricProviderBase(IHelper helper)
		{
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<TelemetryMetricProviderBase>();
		}

		#region Members to override

		protected abstract List<MetricIdentifier> GetMetricIdentifiers();
		protected abstract string ProviderName { get; }

		#endregion //Members to override

		#region Methods

		public void Run(Category integrationPointCategory, IHelper helper)
		{
			try
			{
				using (IInternalMetricsCollectionManager internalMetricsCollectionManager =
					helper.GetServicesManager().CreateProxy<IInternalMetricsCollectionManager>(ExecutionIdentity.System))
				{
					AddMetricsForCategoryAsync(GetMetricIdentifiers(), integrationPointCategory, internalMetricsCollectionManager).GetAwaiter().GetResult();
				}
			}
			catch (AggregateException ex)
			{
				LogAddingMetricsError(ex);
				throw new Exception($"Failed to add telemetry metric identifiers for {ProviderName}!", ex.InnerException);
			}
		}

		private static async Task AddMetricsForCategoryAsync(List<MetricIdentifier> metricIdentifiers, Category category,
			IInternalMetricsCollectionManager internalMetricsCollectionManager)
		{
			// Requests to IInternalMetricsCollectionManager have to be sent sequentially REL-307015
			foreach (MetricIdentifier metricIdentifier in metricIdentifiers)
			{
				await AddMetricForCategoryAsync(
					category,
					internalMetricsCollectionManager,
					metricIdentifier).ConfigureAwait(false);
			}
		}

		private static Task AddMetricForCategoryAsync(Category category,
			IInternalMetricsCollectionManager internalMetricsCollectionManager, MetricIdentifier metricIdentifier)
		{
			metricIdentifier.Categories = new List<CategoryRef> { category };
			return internalMetricsCollectionManager.CreateMetricIdentifierAsync(metricIdentifier, false);
		}

		#endregion //Methods

		#region Logging

		private void LogAddingMetricsError(AggregateException ex)
		{
			_logger.LogError(ex, "Failed to add telemetry metric identifiers for {ProviderName}.", ProviderName);
		}

		#endregion
	}
}