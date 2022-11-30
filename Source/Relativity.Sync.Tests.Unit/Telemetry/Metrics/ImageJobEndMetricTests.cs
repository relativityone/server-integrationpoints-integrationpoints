using System.Collections.Generic;
using Moq;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Telemetry.Services.Metrics;

namespace Relativity.Sync.Tests.Unit.Telemetry.Metrics
{
    internal class ImageJobEndMetricTests : MetricTestsBase<ImageJobEndMetric>
    {
        private ImageJobEndMetric _sut = new ImageJobEndMetric
        {
            RetryJobEndStatus = "Completed",
            TotalRecordsTransferred = 11,
            TotalRecordsTagged = 22,
            TotalRecordsFailed = 33,
            TotalRecordsRequested = 44,
            BytesTransferred = 55,
            JobEndStatus = "Completed with Errors",
            BytesImagesRequested = 66,
            BytesImagesTransferred = 77
        };

        protected override IMetric ArrangeTestMetric()
        {
            return _sut;
        }

        protected override IMetric EmptyTestMetric()
        {
            return new ImageJobEndMetric();
        }

        protected override void VerifySumSink(Mock<IMetricsManager> metricsManagerMock)
        {
            metricsManagerMock.Verify(x => x.LogPointInTimeStringAsync(
                TelemetryConstants.MetricIdentifiers.RETRY_JOB_END_STATUS,
                _EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.RetryJobEndStatus));
            metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync(
                TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TRANSFERRED,
                _EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.TotalRecordsTransferred.Value));
            metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync(
                TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TAGGED,
                _EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.TotalRecordsTagged.Value));
            metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync(
                TelemetryConstants.MetricIdentifiers.DATA_RECORDS_FAILED,
                _EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.TotalRecordsFailed.Value));
            metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync(
                TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TOTAL_REQUESTED,
                _EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.TotalRecordsRequested.Value));
            metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync(
                TelemetryConstants.MetricIdentifiers.DATA_BYTES_TOTAL_TRANSFERRED,
                _EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.BytesTransferred.Value));
            metricsManagerMock.Verify(x => x.LogPointInTimeStringAsync(
                TelemetryConstants.MetricIdentifiers.JOB_END_STATUS_IMAGES,
                _EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.JobEndStatus));
            metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync(
                TelemetryConstants.MetricIdentifiers.DATA_BYTES_IMAGES_REQUESTED,
                _EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.BytesImagesRequested.Value));
            metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync(
                TelemetryConstants.MetricIdentifiers.DATA_BYTES_IMAGES_TRANSFERRED,
                _EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.BytesImagesTransferred.Value));

            metricsManagerMock.Verify(x => x.Dispose());
        }

        protected override void VerifyApmSink(Mock<IAPMClient> apmMock)
        {
            apmMock.Verify(x => x.Count(_APPLICATION_NAME, It.Is<Dictionary<string, object>>(d =>
                d["RetryJobEndStatus"].Equals(_sut.RetryJobEndStatus) &&
                d["TotalRecordsTransferred"].Equals(_sut.TotalRecordsTransferred) &&
                d["TotalRecordsTagged"].Equals(_sut.TotalRecordsTagged) &&
                d["TotalRecordsFailed"].Equals(_sut.TotalRecordsFailed) &&
                d["TotalRecordsRequested"].Equals(_sut.TotalRecordsRequested) &&
                d["BytesTransferred"].Equals(_sut.BytesTransferred) &&
                d["JobEndStatus"].Equals(_sut.JobEndStatus) &&
                d["BytesImagesRequested"].Equals(_sut.BytesImagesRequested) &&
                d["BytesImagesTransferred"].Equals(_sut.BytesImagesTransferred))));
        }
    }
}
