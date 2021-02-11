﻿using System.Collections.Generic;
using Moq;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Telemetry.Services.Metrics;

namespace Relativity.Sync.Tests.Unit.Telemetry.Metrics
{
	internal class DocumentJobEndMetricsTests : MetricsTestsBase<DocumentJobEndMetric>
	{
		private DocumentJobEndMetric _sut = new DocumentJobEndMetric
		{
			RetryJobEndStatus = "Completed",
			TotalRecordsTransferred = 11,
			TotalRecordsTagged = 22,
			TotalRecordsFailed = 33,
			TotalRecordsRequested = 44,
			BytesTransferred = 55,
			JobEndStatus = "Completed with Errors",
			BytesNativesRequested = 66,
			BytesMetadataTransferred = 77,
			BytesNativesTransferred = 88,
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
				_EXPECTED_WORKSPACE_GUID, _sut.WorkflowId, _sut.RetryJobEndStatus));
			metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TRANSFERRED,
				_EXPECTED_WORKSPACE_GUID, _sut.WorkflowId, _sut.TotalRecordsTransferred.Value));
			metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TAGGED,
				_EXPECTED_WORKSPACE_GUID, _sut.WorkflowId, _sut.TotalRecordsTagged.Value));
			metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_FAILED,
				_EXPECTED_WORKSPACE_GUID, _sut.WorkflowId, _sut.TotalRecordsFailed.Value));
			metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync(TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TOTAL_REQUESTED,
				_EXPECTED_WORKSPACE_GUID, _sut.WorkflowId, _sut.TotalRecordsRequested.Value));
			metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync(TelemetryConstants.MetricIdentifiers.DATA_BYTES_TOTAL_TRANSFERRED,
				_EXPECTED_WORKSPACE_GUID, _sut.WorkflowId, _sut.BytesTransferred.Value));
			metricsManagerMock.Verify(x => x.LogPointInTimeStringAsync(TelemetryConstants.MetricIdentifiers.JOB_END_STATUS_NATIVES_AND_METADATA,
				_EXPECTED_WORKSPACE_GUID, _sut.WorkflowId, _sut.JobEndStatus));
			metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync(TelemetryConstants.MetricIdentifiers.DATA_BYTES_NATIVES_REQUESTED,
				_EXPECTED_WORKSPACE_GUID, _sut.WorkflowId, _sut.BytesNativesRequested.Value));
			metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync(TelemetryConstants.MetricIdentifiers.DATA_BYTES_METADATA_TRANSFERRED,
				_EXPECTED_WORKSPACE_GUID, _sut.WorkflowId, _sut.BytesMetadataTransferred.Value));
			metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync(TelemetryConstants.MetricIdentifiers.DATA_BYTES_NATIVES_TRANSFERRED,
				_EXPECTED_WORKSPACE_GUID, _sut.WorkflowId, _sut.BytesNativesTransferred.Value));
			metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync(TelemetryConstants.MetricIdentifiers.DATA_FIELDS_MAPPED,
				_EXPECTED_WORKSPACE_GUID, _sut.WorkflowId, _sut.TotalMappedFields.Value));

			metricsManagerMock.Verify(x => x.Dispose());
			metricsManagerMock.VerifyNoOtherCalls();
		}

		protected override void VerifyApmSink(Mock<IAPMClient> apmMock)
		{
			apmMock.Verify(x => x.Log(_APPLICATION_NAME, It.Is<Dictionary<string, object>>(d =>
				d["RetryJobEndStatus"].Equals(_sut.RetryJobEndStatus) &&
				d["TotalRecordsTransferred"].Equals(_sut.TotalRecordsTransferred) &&
				d["TotalRecordsTagged"].Equals(_sut.TotalRecordsTagged) &&
				d["TotalRecordsFailed"].Equals(_sut.TotalRecordsFailed) &&
				d["TotalRecordsRequested"].Equals(_sut.TotalRecordsRequested) &&
				d["BytesTransferred"].Equals(_sut.BytesTransferred) &&
				d["JobEndStatus"].Equals(_sut.JobEndStatus) &&
				d["BytesNativesRequested"].Equals(_sut.BytesNativesRequested) &&
				d["BytesMetadataTransferred"].Equals(_sut.BytesMetadataTransferred) &&
				d["BytesNativesTransferred"].Equals(_sut.BytesNativesTransferred) &&
				d["TotalMappedFields"].Equals(_sut.TotalMappedFields))));
		}
	}
}