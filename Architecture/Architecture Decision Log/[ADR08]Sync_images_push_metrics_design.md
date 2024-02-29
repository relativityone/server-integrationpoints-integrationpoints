# Images Sync - Metrics

## Status

Approved

## Context

With new flow we need to introduce metrics which will allow us to measure its usage.

## Decision

Following new metrics will be introduced.

| Metric Name                                   | Value                                                    | Description                                                                 |
|:----------------------------------------------|:--------------------------------------------------------:|:----------------------------------------------------------------------------|
| Relativity.Sync.Data.Bytes.ImagesRequested    | bytes                                                    | The total number of bytes of images files that were requested to transfer.  |
| Relativity.Sync.Data.Bytes.ImagesTransferred  | bytes                                                    | The total number of bytes that were successfully transferred, ONLY Images.  |
| Relativity.Sync.Data.Bytes.NativesTransferred | bytes                                                    | The total number of bytes that were successfully transferred, ONLY Natives. |
| Relativity.Sync.FlowType                      | [SavedSearched.Images; SavedSearched.NativesAndMetadata] | The type of Sync job flow.|

The `FlowType` metric can be added to job start metrics and send based on current pipeline. Remaining metrics needs to be send as part of job end metrics. As there is a significant difference between job end metrics send by documents and images jobs (mostly trasnferred bytes and field mappings metrics), two separate job end metrics services will be introduced together with factory to build the proper one based on current pipeline.

## Consequences

The new metrics will allow us to clearly see the usage of new flow and there will be no ambiguity for our bytes trasnferred metrics.

The implementation approach allows for clear separation of concerns and appropiate code reusage.