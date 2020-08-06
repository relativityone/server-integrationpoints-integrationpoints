# Sync Images - ImagesSynchronizationNode

## Status

Proposed

## Context

Current implementation of _SynchronizationExecutor is thightly coupled with Natives Push.

## Decision

Code in _ImagesSynchronizationExecutor_ should be almost the same as in SynchronizationExecutor. Only difference is in ImportJobFactory which should create Import Job for images

_IImportJobFactory_ should provide new method _CreateImportImagesJobAsync_ which would enable to create import job for images.

```csharp
...
using (IImportJob importJob = await _importJobFactory.CreateImportJobAsync(configuration, batch, token).ConfigureAwait(false))
...
```

Also before creating Import Job _SynchronizationConfiguration_ is modified for creation needs. This method probably needs to be removed/modified.

```csharp
public async Task<ExecutionResult> ExecuteAsync(ISynchronizationConfiguration configuration, CancellationToken token)
{
    _logger.LogInformation("Creating settings for ImportAPI.");
    UpdateImportSettings(configuration); //Configuration Update

    ExecutionResult importAndTagResult = await ExecuteSynchronizationAsync(configuration, token).ConfigureAwait(false);

    _jobCleanupConfiguration.SynchronizationExecutionResult = importAndTagResult;
    _automatedWorkflowTriggerConfiguration.SynchronizationExecutionResult = importAndTagResult;
    return importAndTagResult;
}
```

TBD:

+ Think about adding new method to IIMportJobFactory
+ Decide about code duplication or abstraction for tagging and utility code (status handling)
+ Do we need new _SynchronizationConfiguration_ or we do it over existing one

## Consequences

It's part which is mandatory to successfully run Images Sync job.