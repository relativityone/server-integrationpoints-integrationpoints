# RIP drain stop for loadfile import

## Status

Proposed

## Context

Due to new requirement for **Near Zero Downtime** we need to be able to pause the RIP job in under 5 minutes, upgrade RIP and resume the job.

For loadfile import, the task type is `ImportService`, which resolves to `ImportServiceManager`. The manager creates a RDO synchronizer and runs a single IAPI job for the entire loadfile - very simple flow.

## Decision

The pausing and resuming technique should be in line with the approach for custom providers.

### Prerequisites

As for all stop and pause scenarios, we need a way to stop IAPI in a timely fashion. In the mean time all we can do is to stop the data reader (by returning `false` from `Read()` method). This will only prevent IAPI from reading data for the next internal batch, which does not guarantee the 5 min stop time.

### Pausing

On pause, the data reader should stop serving items and IAPI job should be cancelled. Then we wait for IAPI final report on how many items were imported.

After everything is stopped, we update the `JobDetails` column with the number of sent items. The job stays in agent queue with a `Paused` state.

```Optional: we could also save the MD5 checskum of the loadfile and check it on resume. If the MD5 changed, fail the resume job```

### Resuming

On resume, the agent would pick up the job as usual and deserialize the `JobDetails` column into a class with `ItemsSent` property (it could do that even for new jobs) and skip the amount of loadfile rows. The rest of the job could continue as before - create a IAPI job for all remaining rows in the loadfile.

## Consequences

Pros:

- jobs can be paused indefinitely
- easy to reason about method that is common for all providers
- no performance impact
- very little code to change

Cons:

- requires changes to general agent code to support jobs in paused state
