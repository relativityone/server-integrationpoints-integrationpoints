# Sync Images - DataSourceSnapshotExecutor and Images size calculation

## Status

Approved

## Context

existing DataSourceSnapshotExecutors are same for Images Sync:

+ DataSourceSnapshotExecutor
+ RetryDataSourceSnapshotNode

Problem is with statistics for documents size which are calculated in the end of this step. Right now we calculate there only Natives size. It would work anyway (statstic would be 0 for images), but in the future we would like to gather job size for Images

## Decision

```csharp
...
ExportInitializationResults results;
try
{
    using (IObjectManager objectManager = await _serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
    {
        results = await objectManager.InitializeExportAsync(configuration.SourceWorkspaceArtifactId, queryRequest, 1).ConfigureAwait(false);
        _logger.LogInformation("Retrieved {documentsCount} documents from saved search.", results.RecordCount);

        // Natives calculation
        Task<long> calculateNativesTotalSizeTask = Task.Run(() => _nativeFileRepository.CalculateNativesTotalSizeAsync(configuration.SourceWorkspaceArtifactId, queryRequest), token);
        _jobStatisticsContainer.NativesBytesRequested = calculateNativesTotalSizeTask;
    }
}
...
```

Proposed solutions:

+ Provide generic solution for data calculation and call it in the same place and decide inside which statistics should be calculated
+ Move this step outside of this _SnapshotExecutor_

## Consequences

+ (+) Enable to reuse _RetryDataSourceSnapshotExecutor_ and _DataSourceSnapshotExecutor_
+ (-) Branching has to be done
