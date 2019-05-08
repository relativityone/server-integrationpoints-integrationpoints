using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.InternalMetricsCollection;

namespace Relativity.Sync
{
	internal abstract class TelemetryMetricProviderBase : ITelemetryMetricProvider
	{
		private readonly IServicesMgr _servicesMgr;

		protected TelemetryMetricProviderBase(IServicesMgr servicesMgr)
		{
			_servicesMgr = servicesMgr;
		}

		public async Task AddMetricsForCategory(CategoryRef category)
		{
			try
			{
				using (var manager = _servicesMgr.CreateProxy<IInternalMetricsCollectionManager>(ExecutionIdentity.System))
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
			catch (AggregateException ex)
			{
				LogAddingMetricsError(ex);
				//throw new Exception($"Failed to add telemetry metric identifiers for {ProviderName}!", ex.InnerException);
			}
		}

		protected abstract List<MetricIdentifier> GetMetricIdentifiers();

		private void LogAddingMetricsError(AggregateException aggregateException)
		{
			throw new NotImplementedException();
		}
	}
}