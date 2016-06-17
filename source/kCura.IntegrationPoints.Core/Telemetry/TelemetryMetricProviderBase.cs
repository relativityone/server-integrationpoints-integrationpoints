﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.InternalMetricsCollection;

namespace kCura.IntegrationPoints.Core.Telemetry
{
	public abstract class TelemetryMetricProviderBase : ITelemetryMetricProvider
	{
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
					AddMetricsForCategory(GetMetricIdentifiers(), integrationPointCategory, internalMetricsCollectionManager);
				}
			}
			catch (AggregateException ex)
			{
				throw new Exception($"Failed to add telemetry metric identifiers for {ProviderName}!", ex.InnerException);
			}
		}

		private static void AddMetricsForCategory(List<MetricIdentifier> metricIdentifiers, Category category, IInternalMetricsCollectionManager internalMetricsCollectionManager)
		{
			foreach (MetricIdentifier metricIdentifier in metricIdentifiers)
			{
				metricIdentifier.Categories = new List<CategoryRef> { category };

				MetricIdentifier identifier = metricIdentifier;
				Task.Run(async () => await internalMetricsCollectionManager.CreateMetricIdentifierAsync(identifier, false)).ConfigureAwait(false).GetAwaiter().GetResult();
			}
		}

		#endregion //Methods
	}
}
