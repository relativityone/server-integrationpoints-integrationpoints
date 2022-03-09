using System.Collections.Generic;
using Moq;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Telemetry.Services.Metrics;

namespace Relativity.Sync.Tests.Unit.Telemetry.Metrics
{
	internal class NonDocumentJobEndMetricsTests : MetricTestsBase<NonDocumentJobEndMetric>
	{
		private NonDocumentJobEndMetric _sut = new NonDocumentJobEndMetric
		{
			RetryJobEndStatus = "Completed",
			TotalRecordsTransferred = 11,
			TotalRecordsTagged = 22,
			TotalRecordsFailed = 33,
			TotalRecordsRequested = 44,
			BytesTransferred = 55,
			JobEndStatus = "Completed with Errors",
			BytesMetadataTransferred = 77,
			TotalMappedFields = 99
		};

		protected override IMetric ArrangeTestMetric()
		{
			return _sut;
		}

		protected override IMetric EmptyTestMetric()
		{
			return new DocumentJobEndMetric();
		}

		protected override void VerifySumSink(Mock<IMetricsManager> metricsManagerMock)
		{
			metricsManagerMock.Verify(x => x.LogPointInTimeStringAsync(TelemetryConstants.MetricIdentifiers.RETRY_JOB_END_STATUS,
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.RetryJobEndStatus));
			metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TRANSFERRED,
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.TotalRecordsTransferred.Value));
			metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TAGGED,
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.TotalRecordsTagged.Value));
			metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_FAILED,
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.TotalRecordsFailed.Value));
			metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TOTAL_REQUESTED,
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.TotalRecordsRequested.Value));
			metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync(TelemetryConstants.MetricIdentifiers.DATA_BYTES_TOTAL_TRANSFERRED,
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.BytesTransferred.Value));
			metricsManagerMock.Verify(x => x.LogPointInTimeStringAsync(TelemetryConstants.MetricIdentifiers.JOB_END_STATUS_NON_DOCUMENT_OBJECTS,
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.JobEndStatus));
			metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync(TelemetryConstants.MetricIdentifiers.DATA_BYTES_METADATA_TRANSFERRED,
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.BytesMetadataTransferred.Value));
			metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync(TelemetryConstants.MetricIdentifiers.DATA_FIELDS_MAPPED,
				_EXPECTED_WORKSPACE_GUID, _sut.CorrelationId, _sut.TotalMappedFields.Value));

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
				d["BytesMetadataTransferred"].Equals(_sut.BytesMetadataTransferred) &&
				d["TotalMappedFields"].Equals(_sut.TotalMappedFields))));
		}
	}
}