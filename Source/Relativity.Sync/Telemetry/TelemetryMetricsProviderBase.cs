using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.InternalMetricsCollection;

namespace Relativity.Sync.Telemetry
{
	internal abstract class TelemetryMetricsProviderBase : ITelemetryMetricProvider
	{
		private readonly IServicesMgr _servicesManager;
		private readonly ISyncLog _logger;

		protected abstract string ProviderName { get; }

		protected TelemetryMetricsProviderBase(IServicesMgr servicesManager, ISyncLog logger)
		{
			_servicesManager = servicesManager;
			_logger = logger;
		}

		public async Task AddMetricsForCategory(CategoryRef category)
		{
			try
			{
				using (var manager = _servicesManager.CreateProxy<IInternalMetricsCollectionManager>(ExecutionIdentity.System))
				{
					foreach (MetricIdentifier metricIdentifier in GetMetricIdentifiers())
					{
						if (metricIdentifier.Categories != null)
						{
							metricIdentifier.Categories.Add(category);
						}
						else
						{
							metricIdentifier.Categories = new List<CategoryRef> { category };
						}

						await manager.CreateMetricIdentifierAsync(metricIdentifier, false).ConfigureAwait(false);
					}
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occurred when adding metrics to category {categoryName} by provider {providerName}", category.Name, ProviderName);
			}
		}

		protected abstract List<MetricIdentifier> GetMetricIdentifiers();
	}
}