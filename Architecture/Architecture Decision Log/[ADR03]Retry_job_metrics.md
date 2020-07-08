# Retry job metrics

## Status

Approved

## Context

In Sync we should cover following metrics:

1. What is the percent of jobs with item level errors logged per amount of jobs ? (for example, Customer A pushed 100 jobs and 30% of them included item level errors, which means in 30% of cases the customers needed to fix errors and re-run/retry the job)
2. What is the success rate of retried jobs per time period, per account (for example: Customer A retried 30 jobs in May the success rate of those jobs is 90%, should be calculated the same as Sync jobs completed/completed+failed)
3. Every started retried Sync job should be send as metric.

Metrics in Sync are confusing and hard to extensible. Upon that we should redefine them.

## Decision

We should postpone redesigning metrics implementation in time (it should be whole initiative to do that). It shouldn't be done upon some partially requirements like in this case.

Proposition is to use current implementation and reuse all existing code:

Retry Sync metrics resolution:

1. We have this metric for free. Right now in Sync we gather point in time metric _Relativity.IntegrationPoints.Job.End.Status_ value _Completed with Errors_ which tells us how many jobs require to fix errors
2. We should define two new buckets:

    + _Relativity.Sync.Retry.Job.Start_
    + _Relativity.Sync.Retry.Job.End.Status_

    Success rate of retried jobs would be count the same as for normal Sync jobs, but for _Relativity.Sync.Retry.Job.End.Status_:

        Completed / Completed + Failed
3. This metric would be cover by _Relativity.Sync.Retry.Job.Start_

### Proposed Implementation

```csharp
internal interface ISumReporterConfiguration : IConfiguration
{
    int RetryJobHistoryArtifactId { get; }
}
```

```csharp
internal class JobStartMetricsExecutor : IExecutor<ISumReporterConfiguration>
{
    ...
    public Task<ExecutionResult> ExecuteAsync(ISumReporterConfiguration configuration, CancellationToken token)
    {
        _syncMetrics.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.JOB_START_TYPE, TelemetryConstants.PROVIDER_NAME);

        if(configuration.RetryJobHistoryArtifactId != 0)
        {
            _syncMetrics.LogPointInTimeString("Relativity.Sync.Retry.Job.Start", TelemetryConstants.PROVIDER_NAME);
        }

        return Task.FromResult(ExecutionResult.Success());
    }
    ...
}
```

```csharp
internal class JobEndMetricsService : IJobEndMetricsService
{

    ...

    public async Task<ExecutionResult> ExecuteAsync(ExecutionStatus jobExecutionStatus)
    {
        try
        {
            ...

            _syncMetrics.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.JOB_END_STATUS, jobExecutionStatus.GetDescription());

            if (_configuration.RetryJobHistoryArtifactId != 0) // _configuration implements ISumReporterConfiguration
            {
                _syncMetrics.LogPointInTimeString("Relativity.Sync.Retry.Job.End.Status", jobExecutionStatus.GetDescription());
            }

            ...
        }
        catch (Exception e)
        {
        }
    }
}
```

## Consequences

Sync metrics would be the same as they are right now. This resolution won't affect them and doesn't make future redesigning harder. For MVP it's easiest solution.

Positive:

+ It's much less invasive
+ We don't need any additional information from RIP (in general we don't touch RIP code)
+ We don't change workflowId which would be part of metrics redesign
+ It's simple
+ It won't affect existing dashboards

Negative:

+ It requires new bucket creation
+ Following current implementation this two buckets would be registered every time job is running
+ We need to add ugly if-statement in code
+ We gather almost the same metric twice (Job End, Retry Job End)
