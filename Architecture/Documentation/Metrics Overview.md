# SYNC Metrics

## SUM

Sum Metric Types:

+ PointInTimeString
+ PointInTimeLong
+ PointInTimeDouble
+ TimedOperation
+ Counter
+ GaugeOperation

### Job Start Metric

| Metric | Type | Description |
|-|-|-|
| Relativity.Sync.Job.Start.Type | PointInTimeString | The name of the Integration Points provider for this job |
| Relativity.Sync.Retry.Job.Start.Type | PointInTimeString | The name of the Integration Points provider for this retry job |
| Relativity.Sync.FlowType | PointInTimeString | The type of Sync job flow |

### Document Job End Metric

| Metric | Type | Description |
|-|-|-|
| Relativity.Sync.Job.End.Status.SavedSearched.NativesAndMetadata | PointInTimeString | The end status of the Integration Points job for natives and metadata flow |
| Relativity.Sync.Retry.Job.End.Status | PointInTimeString | The end status of the Integration Points retry job |
| Relativity.Sync.Data.Records.Transferred | PointInTimeLong | The number of records that were successfully transferred during the Integration Points job |
| Relativity.Sync.Data.Records.Tagged | PointInTimeLong | The number of records that were successfully tagged during the Integration Points job |
| Relativity.Sync.Data.Records.Failed | PointInTimeLong | The number of records that failed to transfer during the Integration Points job |
| Relativity.Sync.Data.Records.TotalRequested | PointInTimeLong | The total number of records that were included to be transferred in the Integration Points job |
| Relativity.Sync.Data.Bytes.TotalTransferred | PointInTimeLong | The number of records that were successfully transferred during the Integration Points job |
| Relativity.Sync.Data.Bytes.NativesRequested | PointInTimeLong | The total number of bytes of native files that were requested to transfer |
| Relativity.Sync.Data.Bytes.MetadataTransferred | PointInTimeLong | The total number of bytes of metadata that were successfully transferred |
| Relativity.Sync.Data.Bytes.NativesTransferred | PointInTimeLong | The total number of bytes of natives that were successfully transferred |
| Relativity.Sync.Data.Fields.Mapped | PointInTimeLong | The number of fields mapped for the Integration Points job |

### Image Job End Metric

| Metric | Type | Description |
|-|-|-|
| Relativity.Sync.Job.End.Status.SavedSearched.Images | PointInTimeString | The end status of the Integration Points job for images flow |
| Relativity.Sync.Retry.Job.End.Status | PointInTimeString | The end status of the Integration Points retry job |
| Relativity.Sync.Data.Records.Transferred | PointInTimeLong | The number of records that were successfully transferred during the Integration Points job |
| Relativity.Sync.Data.Records.Tagged | PointInTimeLong | The number of records that were successfully tagged during the Integration Points job |
| Relativity.Sync.Data.Records.Failed | PointInTimeLong | The number of records that failed to transfer during the Integration Points job |
| Relativity.Sync.Data.Records.TotalRequested | PointInTimeLong | The total number of records that were included to be transferred in the Integration Points job |
| Relativity.Sync.Data.Bytes.TotalTransferred | PointInTimeLong | The number of records that were successfully transferred during the Integration Points job |
| Relativity.Sync.Data.Bytes.ImagesRequested | PointInTimeLong | The total number of bytes of images that were requested to transfer |
| Relativity.Sync.Data.Bytes.ImagesTransferred | PointInTimeLong | The total number of bytes of images that were successfully transferred |

### Object Manager Kepler Metric

| Metric | Type | Description |
|-|-|-|
| Relativity.Sync.KeplerServiceInterceptor.IObjectManager.Success | PointInTimeLong | The count of retries needed for IObjectManager Kepler Service to succeed |
| Relativity.Sync.KeplerServiceInterceptor.IObjectManager.Failed | PointInTimeLong | The count of retries despite which IObjectManager Kepler Service failed |
| Relativity.Sync.KeplerServiceInterceptor.IObjectManager.AuthRefresh | PointInTimeLong | The count of auth token refreshes for IObjectManager Kepler Service |

## APM / Splunk

### Sync Metric Template

```{json}
Metric: {
    Name: <Metric_name>
    WorkflowId: Sync_<Job_History_Id>
    ...
    <Custom_Metric_Properties>
}
```

APM and Splunk metrics are complementary

### Comand Metric

```{json}
{
    WorkflowId: <Workflow_Id>
    Name: <Command_Name>
    ExecutionStatus
    Duration
}
```

### Destination Workspace Tag Metric

```{json}
{
    WorkflowId: <Workflow_Id>
    Name: "DestinationWorkspaceTagMetric"
    SourceUpdateTime
    SourceUpdateCount
    UnitOfMeasure: "document(s)"
    BatchSize
}
```

### Job Start Metric

```{json}
{
    WorkflowId: <Workflow_Id>
    Name: "JobStartMetric"
    Type: "Sync"
    RetryType: "Sync"
    FlowType: "SavedSearched.Images" |"SavedSearched.NativesAndMetadata"
}
```

### Document Batch End Metric

```{json}
{
    WorkflowId: <Workflow_Id>
    Name: "DocumentBatchEndMetric"
    TotalRecordsRequested
    TotalRecordsTransferred
    TotalRecordsFailed
    TotalRecordsTagged
    BytesMetadataTransferred
    BytesNativesTransferred
    BytesTransferred
    BatchTotalTime
    BatchImportAPITime
    AvgSizeLessThan1MB
    AvgTimeLessThan1MB
    AvgSizeLessBetween1and10MB
    AvgTimeLessBetween1and10MB
    AvgSizeLessBetween10and20MB
    AvgTimeLessBetween10and20MB
    AvgSizeOver20MB
    AvgTimeOver20MB
}
```

### Document Job End Metric

```{json}
{
    WorkflowId: <Workflow_Id>
    Name: "DocumentJobEndMetric"
    JobEndStatus
    RetryJobEndStatus
    TotalRecordsTransferred
    TotalRecordsTagged
    TotalRecordsFailed
    TotalRecordsRequested
    BytesTransferred
    BytesNativesRequested
    BytesMetadataTransferred
    BytesNativesTransferred
    TotalMappedFields
}
```

Additionally in Splunk, to above metric is included list of top 10 long-text streams:
```{json}
{
    TotalBytesRead
    TotalReadTime
}
```

### Image Batch End Metric

```{json}
{
    WorkflowId: <Workflow_Id>
    Name: "DocumentJobEndMetric"
    TotalRecordsRequested
    TotalRecordsTransferred
    TotalRecordsFailed
    TotalRecordsTagged
    BytesMetadataTransferred
    BytesNativesTransferred
    BytesTransferred
    BatchTotalTime
    BatchImportAPITime
}
```

### Image Job End Metric

```{json}
{
    WorkflowId: <Workflow_Id>
    Name: "DocumentJobEndMetric"
    JobEndStatus
    RetryJobEndStatus
    TotalRecordsTransferred
    TotalRecordsTagged
    TotalRecordsFailed
    TotalRecordsRequested
    BytesTransferred
    JobEndStatus
    BytesImagesRequested
    BytesImagesTransferred
}
```

### Kepler Metric

```{json}
{
    WorkflowId: <Workflow_Id>
    Name: <Kepler_Name>
    ExecutionStatus
    Duration
    NumberOfHttpRetriesForSuccess
    NumberOfHttpRetriesForFailed
    AuthTokenExpirationCount
}
```

### Source Workspace Tag Metric

```{json}
{
    WorkflowId: <Workflow_Id>
    Name: SourceWorkspaceTagMetric
    DestinationUpdateTime
    DestinationUpdateCount
    UnitOfMeasure
    BatchSize
}
```

### Stream Retry Metric

```{json}
{
    WorkflowId: <Workflow_Id>
    Name: StreamRetryMetric
    RetryCounter: 1
}
```

### Validation Metric

```{json}
{
    WorkflowId: <Workflow_Id>
    Name: <Validator_Name>
    ExecutionStatus
    Duration
    FailedCounter : 1
}
```

## Reference

https://einstein.kcura.com/display/DV/Sync+Specific+Metric