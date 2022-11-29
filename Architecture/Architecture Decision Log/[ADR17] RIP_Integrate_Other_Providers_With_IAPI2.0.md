# IAPI 2.0 Integration in Other Providers

## Status

Proposed

## Context

The biggest trouble we currently have with Other Providers in R1 is job failing on Kubernetes due to containers crash or lost connectivity to services. We're unable to recover from such failures and job is temporarily left in unknown status, then after certain period of time (30 minutes currently)marked as failed and user must manually run the job again. Second problem in my opinion is how Other Providers are designed - RIP creates child jobs for each 1000 items. So if user tries to run job for 1M records, we create 1k child jobs and each of those jobs has to be processed picked up and processed by the agent separately. This approach is problematic because of several reasons. First, it adds unncecessary complexity and is error prone. Second, jobs is progressing slower.

## Solution



## References

