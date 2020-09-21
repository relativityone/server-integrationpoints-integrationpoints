﻿using System.Collections.Generic;
using Relativity.Services.InternalMetricsCollection;

namespace Relativity.Sync.Telemetry
{
	internal sealed class MainTelemetryMetricsProvider : TelemetryMetricsProviderBase
	{
		private readonly MetricIdentifier[] _metricIdentifiers =
		{
			new MetricIdentifier
			{
				Name = TelemetryConstants.MetricIdentifiers.JOB_START_TYPE,
				Description = "The name of the Integration Points provider for this job."
			},
			new MetricIdentifier
			{
				Name = TelemetryConstants.MetricIdentifiers.JOB_END_STATUS,
				Description = "The end status of the Integration Points job."
			},
			new MetricIdentifier
			{
				Name = TelemetryConstants.MetricIdentifiers.RETRY_JOB_START_TYPE,
				Description = "The name of the Integration Points provider for this retry job."
			},
			new MetricIdentifier
			{
				Name = TelemetryConstants.MetricIdentifiers.RETRY_JOB_END_STATUS,
				Description = "The end status of the Integration Points retry job."
			},
			new MetricIdentifier
			{
				Name = TelemetryConstants.MetricIdentifiers.DATA_FIELDS_MAPPED,
				Description = "The number of fields mapped for the Integration Points job."
			},
			new MetricIdentifier
			{
				Name = TelemetryConstants.MetricIdentifiers.DATA_RECORDS_FAILED,
				Description = "The number of records that failed to transfer during the Integration Points job."
			},
			new MetricIdentifier
			{
				Name = TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TOTAL_REQUESTED,
				Description = "The total number of records that were included to be transferred in the Integration Points job."
			},
			new MetricIdentifier
			{
				Name = TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TRANSFERRED,
				Description = "The number of records that were successfully transferred during the Integration Points job."
			},
			new MetricIdentifier
			{
				Name = TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TAGGED,
				Description = "The number of records that were successfully tagged during the Integration Points job."
			},
			new MetricIdentifier
			{
				Name = TelemetryConstants.MetricIdentifiers.DATA_BYTES_METADATA_TRANSFERRED,
				Description = "The total number of bytes of metadata that were successfully transferred."
			},
			new MetricIdentifier
			{
				Name = TelemetryConstants.MetricIdentifiers.DATA_BYTES_NATIVES_REQUESTED,
				Description = "The total number of bytes of native files that were requested to transfer."
			},
			new MetricIdentifier
			{
				Name = TelemetryConstants.MetricIdentifiers.DATA_BYTES_TOTAL_TRANSFERRED,
				Description = "The total number of bytes that were successfully transferred, including files and metadata."
			}
		};

		public MainTelemetryMetricsProvider(ISyncLog logger) : base(logger)
		{
		}

		public override string CategoryName { get; } = TelemetryConstants.INTEGRATION_POINTS_TELEMETRY_CATEGORY;

		protected override string ProviderName { get; } = nameof(MainTelemetryMetricsProvider);

		protected override IEnumerable<MetricIdentifier> GetMetricIdentifiers()
		{
			return _metricIdentifiers;
		}
	}
}