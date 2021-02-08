# RIP drain stop for Export to LoadFile

## Status

Proposed

## Context

Due to new requirement for **Near Zero Downtime** we need to be able to pause the RIP job in under 5 minutes, upgrade RIP and resume the job.

Overall Drain-Stop approach in RIP was covered very well in all other Drain-Stop ADRs. In this particular one we should cover Export to LoadFile flow which differs from rest of flows. The main inconvienience is that RIP don't own the export job and has not control over it. All what is done on RIP side is configure the job and run IAPI.

This sheet cover whole RIP Export flow and propose some approaches which could achieve Drain-Stop with lowest possible cost.

## RIP Export to LoadFile overview

RIP Export job consist of two separate jobs:

- ExportManager
- ExportWorker

### Export Manager

- Establish relation between JobHistory, Job, IntegrationPoint
- Run Job validation
- Batch Job
- Get statistics

In case of Export Manager there is no batching. So in general from one ExportManager is created only one ExportWorker job which is queued into Agent Queue

Drain-Stop shouldn't be applicable to ExportManager. There is one possible bottle-neck - Calculate total count exported document/RDOs statistics which are showed in the UI. I don't think this query can take more than 5 mins, but it should be taken under consideration if it's possible. The query is built with ObjectManager. Only one record is read, and we are reading `TotalCount` property.

### Export Worker

Export Worker is core point in Export flow. It's responsible for configure and run IAPI export job. Following choosen core design for Drain-Stopping jobs for other flows we introduce no job state `DrainStopped`. We can save required paused details for drain-stopped job values in `JobDetails` column, since we don't remove paused job from the queue.

Sample `JobDetails` for ExportWorker job:

```{json}
{
  "BatchInstance": "0e8dd0d6-bed6-4ff8-9039-2940058b94c1",
  "BatchParameters": []
}
```

`BatchParameters` can be object which we would serialize/deserialize depends on use.

#### Steps for Export Worker

1. Prepare Destination Location - It creates destination locatin in the fileshare. After analysis this step is idempotent.

2. Prepare `ExtendentExportFile`:

    - Export Settings
    - Start Document Number (StartExportAtRecord form the UI - 1)

3. Create export destination folder if applicable - in this folder *.dat file with all files will be placed

4. Create `ExtendedExporter` - we don't know how CancellationToken is handled in IAPI. For know we are passing `CancellationToken.None` in the constructor

5. Create `StoppableExporter` - it's RIP wrapper for `ExtendendExporter` with `JobStopManager` instance, which handle job Stop on his own.

6. Run `StoppableExporter.ExportSearch()`:

```{csharp}
public class StoppableExporter : IExporter
{
  private readonly ProcessContext _context;
  private readonly WinEDDS.IExporter _exporter;
  private readonly IJobStopManager _jobStopManager;

  ...
  
  public bool ExportSearch()
  {
    try
    {
      _jobStopManager.StopRequestedEvent += OnStopRequested;

      var exportJobStats = new ExportJobStats
      {
        StartTime = DateTime.UtcNow
      };

      _exporter.ExportSearch();

      exportJobStats.EndTime = DateTime.UtcNow;
      exportJobStats.ExportedItemsCount = _exporter.DocumentsExported;
      DocumentsExported = _exporter.DocumentsExported;

      CompleteExportJob(exportJobStats);
      _jobStopManager.ThrowIfStopRequested();
    }
    finally
    {
      _jobStopManager.StopRequestedEvent -= OnStopRequested;
    }

    return true;
  }

  ...
  
  private void OnStopRequested(object sender, EventArgs eventArgs)
  {
    _context.PublishCancellationRequest(Guid.Empty);
  }
}
```

Looks like RIP handle stop by `PublishCancellationRequest` to ProcessContext which is IAPI object.

## Limitations and Caveats

1. RIP as a client only have access to total exported documents by property `DocumentsExported`

2. RIP doesn't own DataSource, there are just passed  _Data Source Id_ (SavedSearch/Production/Folders) so RIP doesn't know which records has been exported and which ones left to run them after resuming.

3. IAPI Export job doesn't enable to append to existing Export folder. We can export to the same folder as previously, but only with _Overwrite files_ set to True. Anyway if we set this option it's clean up whole folder and start to export from scratch.

## Proposed Design

There is two possible approaches. But if we would like to support Pause-Resuming it's impossible without changes in IAPI.

1. **Don't support resuming**

We could investigate how many jobs are _Export to LoadFile_ type and if the usage is low we could just stop the job as it's right now and when resuming start the job once again from the scratch. This approach would satisfy us since we don't support Pause by User on demand.

There is one risk we should take under consideration - Data Source can change between two runs (if record start to meet condition it would be exported as well even if during first run it wouldn't).

It could be done by passing _Export Destination Folder_ to job details to be sure that the location is the same (in some cases location is created based on job start time).

We should also consider to clean up _Export Destination Location_. Only risk is that if client export many jobs to single folder we should remove only records which are applicable for this particular job. It could be achieved by changing _Overwirte files_ to True when resuming.

2. **IAPI-RIP cooperation**

First and most important feature is to handle append to existing folder and *.dat file. After resuming the job we should be able to decide if rows should be appended. Right now it's end up with job failed with following error:

"_Overwrite not selected and file '\\emttest\DefaultFileRepository\EDDS1019364\DataTransfer\Export\JWOLFE Saved Search_export.dat' exists._"

IAPI build _temp table_ to freeze records to send and prevent situations when data source change during the job. We assume that this table is deleted after job finish/stop.

**2.1** We should also track which records left to export. Based on current implementation RIP can save `DocumentExported` value in _JobDetails_ and shift `StartExportAtRecord` by this value. Potential risk is that we can miss some data, because Data Source can changed between those two runs. E.g:

    Run 1) - 2 docs has been exported
      DOC1
      DOC2
      DOC3
      DOC4
    Run 2)
      DOC1
      DOC1.1
      DOC2
      DOC3
      DOC4

    When we shift starting index by 2, it's endup with DOC1.1 unexported and DOC2 exported twice

**2.2** RIP should be able to specify which records should be exported not only based on _Data Source Id_ but also based on _Artifact IDs_. Maybe IAPI could build _temp table_ based on input _Artifact Ids_ and take all other configuration settings as they were. Based on this approach RIP would need to get information about left records to save them in _JobDetails_. This approach can cause some problems if number of left records is large, because we need to store it in column as a JSON.

**2.3** When the job would be stop IAPI could not delete _temp table_ and expose it's ID for RIP and during resume RIP could point that _temp table ID_ in parameters with number of records needed to be skipped based on `DocumentsExported` from previous run.

**2.4** We could also combine _2.2_ and _2.3_ - IAPI could expose _temp table ID_ to RIP and on resume RIP would take the values from the _temp table_ and pass _Artifact IDs_ to Export Settings. Caveat of this approach is that old temp table wouldn't be cleanup (Or RIP could do it, but it's not owner of this table so it would be confusing)

**Note:** RIP statistics should be easy in those cases, because we could just increment the value written after Pause

## Consequences

All of the approaches have it's own limitations and based on RIP/IAPI expectations and Web Import/Export roadmap we should consider least invading solution.

1. Don't support resuming

    (+) No changes in IAPI

    (+) No complex changes in RIP code

    (-) Possible data loss (anyway i don't think we can prevent it in 100%)

    (-) Performance degradation (we run job twice, anyway if it's transparent for the client - we can confuse him when statistics go down to 0 and start to growing up)

2. IAPI-RIP cooperation

    It's opposite to "_Don't support resuming_" approach
