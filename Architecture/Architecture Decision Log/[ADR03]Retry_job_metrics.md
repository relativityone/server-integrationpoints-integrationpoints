# Title

## Status

Proposed

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
1. We have this metric for free. Right now in Sync we are logging in bucket _Relativity.IntegrationPoints.Job.End.Status_ value _Completed with Errors_ which tells us how many jobs require to fix errors
2. There are two approaches:
2.1 Define two buckets: _Relativity.Sync.Retry.Job.Start_ and _Relativity.Sync.Retry.Job.End.Status_. Percentage result would be calculated as _Relativity.Sync.Retry.Job.End.Status (which statuses we would like to) / Relativity.Sync.Retry.Job.Start_. Downside of this approach is that we have to register two new buckets every time Sync job is run (based on current implementation). On other side we just adding new metrics on top of all others which let us avoid changing existing buckets/metrics, we are 100% that we don't break anything on dashboards. But we also duplicate existing metrics (creating the same, but for retry)
Proposed Implementation:
        
Job Start:
Modify ``JobStartMetricsExecutor.ExecuteAsync``:
```csharp
public Task<ExecutionResult> ExecuteAsync(ISumReporterConfiguration configuration, CancellationToken token)
{
    _syncMetrics.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.JOB_START_TYPE, TelemetryConstants.PROVIDER_NAME);

    if(configuration.RetryJobHistoryArtifactId != 0)
    {
        _syncMetrics.LogPointInTimeString("Relativity.Sync.Retry.Job.Start", TelemetryConstants.PROVIDER_NAME);
    }

    return Task.FromResult(ExecutionResult.Success());
}
```

Extend ``ISumReporterConfiguration``
```csharp
internal interface ISumReporterConfiguration : IConfiguration
{
    int RetryJobHistoryArtifactId { get; }
}
```

Job End:
``IJobEndMetricsConfiguration`` should have new property _RetryJobHistoryArtifactId_ which enable to determine if job has been retried:
```csharp
interface IJobEndMetricsConfiguration : ISumReporterConfiguration
{
    int SourceWorkspaceArtifactId { get; }

    int SyncConfigurationArtifactId { get; }

    int RetryJobHistoryArtifactId { get; }
}
```

In ``JobEndMetricsService.ExecuteAsync`` we should append following if-statement
```csharp
if (_configuration.RetryJobHistoryArtifactId != 0)
{
    _syncMetrics.LogPointInTimeString(TelemetryConstants.MetricIdentifiers.JOB_END_STATUS, jobExecutionStatus.GetDescription());
}
```

2.2 Modify workflowId based on job type (New, Retry). Right now we send only one const workflowId - Sync_<jobHistoryArtifactId>. Proposed workflowIds:
    - New - Sync_<jobHistoryArtifactId>
    - Retry - Sync_Retry_<jobHistoryArtifactId>
It would give us all benefits from currently gathered metrics. Downside of this approach are existing dashboards, becuase if they are configured only by <Sync>_<JobHistoryArtifactId> we don't get results from Retries so it would require by us to validate all of them to be sure they are configured to follow "Sync_*" pattern.

Proposed Implementation:

In ``SyncJobParameters`` there is no easy way to provide retryJobHistoryArtifactId parameter which could deteremine if job is retry, we could pass something like JobType. From technical point of view this is bad because there shouldn't be any difference for the Sync Client which type of job has been created - it should be create sync job -> run
```csharp
JobType: Normal, Retry

public SyncJobParameters(int syncConfigurationArtifactId, int workspaceId, int jobHistoryArtifactId, JobType type = Normal)
{
    SyncConfigurationArtifactId = syncConfigurationArtifactId;
    WorkspaceId = workspaceId;
    SyncBuildVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
    WorkflowId = new Lazy<string>(() => $"{TelemetryConstants.PROVIDER_NAME}_{Type}_{jobHistoryArtifactId}"); (I know about "__" ;) )
}
```

Potential solution could be WorkflowId creation directly in ``SyncMetrics``. We could pass configuration there, but one more time it could change existing business behavior because we would send another workflowId to different sinks (not only SUM, but Splunk, New Relic) 

I thought also about passing ``IConfiguration`` to ``SyncJobParameters`` but it looks like circular dependency (we use SyncJobParameters during IConfiguration creation).

3. We get it for free from point 2

Could you share your thoughts or ideas right now i don't see proper one.

## Consequences

Sync metrics would be the same as they are right now. This resolution won't affect them and doesn't make future redesigning harder. For MVP it's easiest solution.

As negative this approach doesn't make them better.