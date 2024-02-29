using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.InternalMetricsCollection;

namespace Relativity.Sync.Telemetry
{
    internal abstract class TelemetryMetricsProviderBase : ITelemetryMetricProvider
    {
        private readonly IAPILog _logger;

        protected abstract string ProviderName { get; }

        public abstract string CategoryName { get; }

        protected TelemetryMetricsProviderBase(IAPILog logger)
        {
            _logger = logger;
        }

        public async Task AddMetricsForCategory(IInternalMetricsCollectionManager metricsCollectionManager, CategoryRef category)
        {
            try
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

                    await metricsCollectionManager.CreateMetricIdentifierAsync(metricIdentifier, false).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception occurred when adding metrics to category {categoryName} by provider {providerName}", category.Name, ProviderName);
            }
        }

        protected abstract IEnumerable<MetricIdentifier> GetMetricIdentifiers();
    }
}
