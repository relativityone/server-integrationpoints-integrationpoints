# Sync RAP - Workflow and Architecture

## Status

Proposed

## Context

As we move to decouple Sync to separate RAP application, we must figure out how the new workflow and architecture is going to look like.
In the first stage of decoupling, we should keep all RDO logic unchanged (Job History, Job History Error etc.).

## Workflow

### Run

- User runs the job
- RIP calls Sync's Kepler endpoint (`SubmitJobAsync`) and pass job configuration in parameters (for example `SyncDocumentJobParameters` DTO for document flow)
- `SubmitJobAsync` returns job ID
- RIP should not add the job to `ScheduleAgentQueue`, but instead an Integration Point should remember Sync Job ID (for example we can add new field in Job History) and RIP should check if it's completed or errored to change state of `Run`, `Stop` or `Retry Errors` buttons. This can be done via polling, or better - using Azure Event Grid
- Sync validates parameters and if they are correct stores the job configuration in SyncConfiguration RDO (just like before in `IntegrationPointToSyncConverter`)
- Sync adds `SyncJobDTO` to some sort of job list (see below)
- Sync's Agent's Workload Discovery endpoint looks into the list of jobs and returns t-shirt size of work to do
- If there is work to do, Sync's Agent picks up the job and process it by calling `ISyncJobFactory.Create(..).ExecuteAsync()`
- After job completion it should be removed from the list of jobs (the problem here is that how can we check the status of the job when it's removed from the queue? That's why much better way to check status of the job is to use Azure Event Grid instead of polling)
- Once the Sync job is completed (or errored) RIP should adjust state of `Run`, `Stop` or `Retry Errors` buttons

### Stop

- User stops the job
- RIP knows Sync Job ID and uses it to call Sync Kepler endpoint (`CancelJob`)
- Sync receives the call and sends a message to Azure Event Grid
- Sync job receives a message from Azure Event Grid, then invokes `CancellationTokenSource.Cancel()`
- Sync job gracefully stops

### Retry errors

- User runs "Retry Errors"
- RIP calls `SubmitJobAsync` and pass Job History Artifact ID to retry
- Sync runs retry flow

## Job list

Sync should store list of jobs somewhere. I would definitely not use existing `ScheduleAgentQueue` table in SQL for that purpose. Here are some of the ideas where to store this list:

- Instance level RDO (classic way)
- Cosmos DB - because we are in Azure so why not? (modern, more flexible way - preferred)

Until we figure out where to finally store list of jobs, we must at least design an interface which will be used to manage the list. Later this interface will be implemented by actual class. It can look like this:

```cs

public interface ISyncJobManager
{
    Task<bool> TryGetNextJobAsync(out SyncJobDTO job);
    Task<IEnumerable<SyncJobDTO>> GetJobsAsync();
    Task AddAsync(SyncJobDTO syncJob);
    Task RemoveAsync(int syncConfigurationArtifactID);
}

public class SyncJobDTO
{
    public int WorkspaceID { get; set; }
    public int SyncConfigurationArtifactID { get; set; }
    public SyncJobStatus Status { get; set; }
}

```

## Sync Kepler endpoints

Sync application is going to be controlled fully via Kepler endpoints. They must provide following functionalities:

- Submitting jobs for processing - I think best would be to have separate endpoint for each of the flows (document, image, non-document, etc.) because job parameters will be different
- cancelling job (and removing from the list of jobs)
- checking status of the job (optional - if we decide to use Azure Event Grid, we don't have to poll for job status)

Kepler Service interface might look like this:

```cs
[WebService("Sync Service Manager")]
[ServiceAudience(Audience.Public)]
public interface ISyncService
{
    // Returns Sync Job ID
    Task<int> SubmitJobAsync(SyncDocumentJobParameters parameters);
    Task<int> SubmitJobAsync(SyncImageJobParameters parameters);
    Task<int> SubmitJobAsync(SyncNonDocumentJobParameters parameters);

    // Returns status of the job - not needed when using Azure Event Grid
    Task<SyncJobStatus> GetJobStatus(int syncJobId);

    // Requests job cancellation
    Task CancelJobAsync(int syncJobId);
}

// Example parameter class for Document flow
public class SyncDocumentJobParameters
{
    public string CorrelationID {get;}
    public RetryOptions RetryOptions {get;}
    public RdoOptions RDOs {get;}
    public DocumentSyncOptions DocumentOptions {get;}
    public FieldsMapping FieldsMapping {get;}
    public DestinationFolderStructureOptions FolderStructure {get;}
    public EmailNotificationsOptions EmailNotifications {get;}
    public OverwriteOptions OverwriteOptions {get;}
    public CreateSavedSearchOptions SavedSearchOptions {get;}
}

public enum SyncJobStatus
{
    Pending = 0,
    Processing,

    // If we decide to use Azure Event Grid to notify clients about job status, we won't need below statuses
    Completed,
    CompletedWithErrors,
    Cancelled,
    Failed
}

```

## Summary

Hhopefully now we have overall look of how separated Sync should work. However there are still some implementation details to figure out and decisions to make:

- where to store list of jobs? (preferred option: Azure Cosmos DB)
- how to check job status / cancel the job? (preferred option: Azure Event Grid)

## Documentation

- Azure Cosmos DB: <https://docs.microsoft.com/en-us/azure/cosmos-db/introduction>
- Azure Event Grid: <https://docs.microsoft.com/en-us/azure/event-grid/overview>
