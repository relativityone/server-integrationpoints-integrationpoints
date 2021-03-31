using System.Collections.Generic;
using Moq;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Telemetry.Services.Metrics;

namespace Relativity.Sync.Tests.Unit.Telemetry.Metrics
{
	internal class DocumentBatchEndMetricTests : MetricTestsBase<DocumentBatchEndMetric>
	{
		private readonly DocumentBatchEndMetric _sut = new DocumentBatchEndMetric
		{
			AvgSizeLessThan1MB = 1.1,
			AvgTimeLessThan1MB = 2.2,
			AvgSizeLessBetween1and10MB = 3.3,
			AvgTimeLessBetween1and10MB = 4.4,
			AvgSizeLessBetween10and20MB = 5.5,
			AvgTimeLessBetween10and20MB = 6.6,
			AvgSizeOver20MB = 7.7,
			AvgTimeOver20MB = 8.8,
			TotalRecordsRequested = 101,
			TotalRecordsTransferred = 102,
			TotalRecordsFailed = 103,
			TotalRecordsTagged = 104,
			BytesMetadataTransferred = 105,
			BytesNativesTransferred = 106,
			BytesTransferred = 107,
			BatchTotalTime = 108,
			BatchImportAPITime = 109
		};

		protected override IMetric ArrangeTestMetric()
		{
			return _sut;
		}

		protected override IMetric EmptyTestMetric()
		{
			return new DocumentBatchEndMetric();
		}

		protected override void VerifySumSink(Mock<IMetricsManager> metricsManagerMock)
		{
			metricsManagerMock.Verify(x => x.LogPointInTimeDoubleAsync(TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_AVERAGE_SIZE_LESSTHAN1MB,
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.AvgSizeLessThan1MB.Value));
			metricsManagerMock.Verify(x => x.LogPointInTimeDoubleAsync(TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_AVERAGE_TIME_LESSTHAN1MB,
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.AvgTimeLessThan1MB.Value));
			metricsManagerMock.Verify(x => x.LogPointInTimeDoubleAsync(TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_AVERAGE_SIZE_BETWEEN1AND10MB,
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.AvgSizeLessBetween1and10MB.Value));
			metricsManagerMock.Verify(x => x.LogPointInTimeDoubleAsync(TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_AVERAGE_TIME_BETWEEN1AND10MB,
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.AvgTimeLessBetween1and10MB.Value));
			metricsManagerMock.Verify(x => x.LogPointInTimeDoubleAsync(TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_AVERAGE_SIZE_BETWEEN10AND20MB,
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.AvgSizeLessBetween10and20MB.Value));
			metricsManagerMock.Verify(x => x.LogPointInTimeDoubleAsync(TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_AVERAGE_TIME_BETWEEN10AND20MB,
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.AvgTimeLessBetween10and20MB.Value));
			metricsManagerMock.Verify(x => x.LogPointInTimeDoubleAsync(TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_AVERAGE_SIZE_OVER20MB,
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.AvgSizeOver20MB.Value));
			metricsManagerMock.Verify(x => x.LogPointInTimeDoubleAsync(TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_AVERAGE_TIME_OVER20MB,
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.AvgTimeOver20MB.Value));

			metricsManagerMock.Verify(x => x.Dispose());
			metricsManagerMock.VerifyNoOtherCalls();
		}

		protected override void VerifyApmSink(Mock<IAPMClient> apmMock)
		{
			apmMock.Verify(x => x.Count(_APPLICATION_NAME, It.Is<Dictionary<string, object>>(d =>
				d["AvgSizeLessThan1MB"].Equals(_sut.AvgSizeLessThan1MB) &&
				d["AvgTimeLessThan1MB"].Equals(_sut.AvgTimeLessThan1MB) &&
				d["AvgSizeLessBetween1and10MB"].Equals(_sut.AvgSizeLessBetween1and10MB) &&
				d["AvgTimeLessBetween1and10MB"].Equals(_sut.AvgTimeLessBetween1and10MB) &&
				d["AvgSizeLessBetween10and20MB"].Equals(_sut.AvgSizeLessBetween10and20MB) &&
				d["AvgTimeLessBetween10and20MB"].Equals(_sut.AvgTimeLessBetween10and20MB) &&
				d["AvgSizeOver20MB"].Equals(_sut.AvgSizeOver20MB) &&
				d["AvgTimeOver20MB"].Equals(_sut.AvgTimeOver20MB) &&
				d["TotalRecordsRequested"].Equals(_sut.TotalRecordsRequested) &&
				d["TotalRecordsTransferred"].Equals(_sut.TotalRecordsTransferred) &&
				d["TotalRecordsFailed"].Equals(_sut.TotalRecordsFailed) &&
				d["TotalRecordsTagged"].Equals(_sut.TotalRecordsTagged) &&
				d["BytesMetadataTransferred"].Equals(_sut.BytesMetadataTransferred) &&
				d["BytesNativesTransferred"].Equals(_sut.BytesNativesTransferred) &&
				d["BytesTransferred"].Equals(_sut.BytesTransferred) &&
				d["BatchTotalTime"].Equals(_sut.BatchTotalTime) &&
				d["BatchImportAPITime"].Equals(_sut.BatchImportAPITime) &&
				d.ContainsKey("LongTextStreamStatistics") == false
				)));
		}
	}
}
