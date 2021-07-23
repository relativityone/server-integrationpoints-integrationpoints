# RIP UI Statistics Calculation - Decrease Saved Search execution counts

## Status

Proposed

## Context

When client configure Integration Point - Push between workspaces, there is calculation on Summary Page which shows:

- Documents count
- Total Natives size (if applicable)
- Total Images size (if applicable)

The calculation is performed on every page refresh which can lead to SQL Server performance degradation (query can take up to ~10 mins!) due often job status monitoring by the client. Additional problem with this statistics calculation is that it's performed separatly for each of parameter (every refersh leads to 3 SQL Queries for documents ids - but with different data). It's problematic for cases where there is significant amount of documents.

To addressing following issue there are two approaches where every of them improve performance comparing to current behavior. First one is obligatory and the second one is as nice to have

1. Merge statistics calculation as single operation which returns whole statistics at once
2. Move statistics calculation to SignalR Hub and run the calculation periodically (one calculation during amount of time, but only after refresh)

## Merge Statistics Calculation

Right now the calculation is calculated in `summary-page-statistics.js`. In this script based on the integration point configuration some calculation functions are called. Possible statuses:

- _Calculating..._ - initial status, when calculation is in progress
- _Error occured_ - calculation has failed
- _713 (26.45 MB)_ - sample natives calculation

The calculation is done by Integration Point Kepler - [`IStatisticsManager`](https://git.kcura.com/projects/IN/repos/integrationpoints-keplerservicesinterfaces/browse/source/Relativity.IntegrationPoints.Services.Interfaces.Private/IStatisticsManager.cs)

Proposed bucket class:

```{csharp}
public class DocumentsStatistics
{
  public long DocumentsCount {get;set;}
  public long TotalNativesCount {get;set;}
  public long TotalNativesSizeBytes {get;set;}
  public long TotalImagesCount {get;set;}
  public long TotalImagesSizeBytes {get;set;}
}
```

We should create new methods which would be responsible for whole calculation:

- `DocumentsStatistics IStatisticsManager.GetNativesStatisticsForSavedSearchAsync(int workspaceId, int savedSearchId)`
- `DocumentsStatistics IStatisticsManager.GetImagesStatisticsForSavedSearchAsync(int workspaceId, int savedSearchId)`
- `DocumentsStatistics IStatisticsManager.GetImagesStatisticsForProductionAsync(int workspaceId, int productionId)`

Detailed implementation is not part of this elaborate, but main goal is to reduce Object Manager calls to one (if possible) with all needed data and then just perform calculation as it was done before.

Present implementation uses standard ObjectManager approach. As there could be significant amount of data needs to be retrieved we should change the code for using **Export API**.

**Note:** Based on current implementation the statistics part that couldn't be calculated should be marked as -1 which is handle later in the UI.

Calling an API and return processed result should be done in `summary-page-statistics.js` where we could leave logic as it is, but only replace the calling logic at the end of the file. Rest should work as it was.

## Move Statistics Calculation to SignalR

At this point the calls to Object Manager was drastically reduced 3 -> 1 (in best case), but client still can decrease performance via summary page refresh, which triggers calculation every time. We know that the calculation can't be done once and forget, because SavedSearch content can vary, same as Images/Natives size. Otherwise we don't want to do calculation every time the client refresh the page.

As a resolution we could move statistics calculation to SignalR Hub and make re-calculation when needed. There are two possible methods:

1. re-calculate after fixed amount of time (e.g. 30 mins), **but only after client refresh**
2. re-calculate after refresh, **but only when the job is not in progres** (when the job runs return previous values)

The implementation should be done in `IntegrationPointDataHub.cs` and `integrationPointHub.js`.

Implementation details from `IntegrationPointDataHub` class:

- `OnDisconnected` - is called after every page refresh
- `GetIntegrationPointUpdate` - called from `integrationPointHub.js` after refresh for initialization
- `UpdateTimerElapsed` - called every 5 secs to update UI

What is changed between the refreshes is `Context.ConnectionId` thanks to that the sessions can be distinguished.

### Proposed design - Timer

We could introduce new timer for every `IntegrationPointDataHubKey` which could be released after exceeding the defined interval and then after refresh we could start statistics calculation as a Task and polling the result if it was done every 5 secs in `UpdateTimerElapsed` method

```{csharp}
public class IntegrationPointDataHub
{
  ...
  private static ConcurrentDictionary<IntegrationPointDataHubKey, IntegrationPointStatisticsTask> _statsTasks;


  public void GetIntegrationPointUpdate(int workspaceId, int artifactId)
  {
    ...

    AddTask(key);
    AddStatisticsTask(key);
    ...
  }

  private void UpdateTimerElapsed(object sender, ElapsedEventArgs e)
  {
    try
    {
      ...
      foreach (var key in _tasks.Keys)
      {
        ...
        UpdateStatistics(key);
      }
    }
    catch (Exception exception)
    {
      ...
    }
    finally
    {
      ...
    }
  }

  private void AddStatisticsTask(IntegrationPointDataHubKey key)
  {
    // When key does not exist add to _statsTasks and run asynchronously statistics calculation
    // When key exists check if timer is defined and was exceeded - then run re-calculation
  }

  private void UpdateStatistics(IntegrationPointDataHubKey key)
  {
    // When the calculation is in progress update the UI as "Calculating..."
    // When the calculation has finished run timer for "break" and store the result and return to the UI
  }
}

public class IntegrationPointStatisticsTask
{
  public DocumentStatistics Result {get;set;}
  public Task<DocumentsStatistics> {get;set;}
  public Timer WaitingTimer {get;set;}
}

public class StatisticsIntegrationPointDataHubKey : IntegrationPointDataHubKey
{
  public int SourceId {get;set;} //We need this for re-calculating when SavedSearch/Production changes
}
```

### Proposed design - Job in progress

Same as above but... There would be no `WaitingTimer` property. Instead of it in `AddStatisticsTask` for existing key we would check for the result and if the job is in progress. If so we would return stored result, if not we would started re-calculation.

**Note:** Update on the UI should be done following the same rules as in `summary-page-statistics.js`

## Consequences

### Merge Statistics Calculation - **Mandatory**

(+) Reduce kepler calls

(+) Decrease SQL load during heavy jobs

(-) Require SKD changes

(-) Does not protect against often reloads

### Move Statistics Calculation to SignalR - **Nice to have**

(+) Prevent re-calculation on every reload

(-) It's complicated

#### Timer approach

(+) really prevent re-calculation on refresh

(-) implementation seems to be more complicated

#### Job in Progress

(+) Implementation is easier

(-) We prevent re-calculation only during job in progress (for Integration Point it's good enough)
