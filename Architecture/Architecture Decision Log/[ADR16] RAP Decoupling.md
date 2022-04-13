# Sync - RAP Decoupling

## Status

Proposed

## Context

Currently, Relativity.Sync is just a DLL library which is installed as package reference in RIP and is added to RAP file. Sync job is created and executed by RIP Agent. Therefore, Sync is tightly coupled to RIP and cannot be run separately in Relativity environment. Every single change, bugfix or new feature in Sync requires updating Sync package in RIP, running several Trident pipelines, and only then we can deploy new Integration Points RAP to production. Second problem is that updating packages in Sync and then in RIP is very problematic, leading to dependency hell which is sometimes very hard to resolve.

## Idea

To mitigate described problems, reasonable idea would be to decouple Sync from RIP and convert Sync to completely separate application, with its own Agent, APIs (Keplers), RDOs, Event Handlers, and perhaps its own UI in the future. This will significantly improve development experience and will allow Sync to evolve without being limited by RIP ecosystem.

## Required architecture and code changes

- Sync Repository - we can use existing one and just add needed projects to solution, along with Trident scripting and anything that is needed for RAP-CD. While creating new RAP application, we must still publish `Relativity.Sync` nuget package which must be backwards compatible with RIP - we don't want to break existing workflow. Cuncurrently we can develop and publish new RAP which will reference Relativity.Sync DLL and can be run and tested independently.
- New Sync should expose Kepler endpoints for creating, running and checking status of the jobs
- Agent - we are going to implement it from scratch, so it will be much easier to maintain than RIP Agent. We just have to pay attention that it must be fully K8s-compatible.
- RIP - best approach would be to introduce feature toggle, and depending on its value we can choose job execution path - either use existing DLL-driven Sync workflow, or switch to new Sync RAP and integrate using Keplers
- There will be need to implement job queueing - we can use Relativity Service Bus for this purpose, or maybe even Azure Storage Queue. It may work like this: when user runs a RIP (Sync) job, RIP enqueues Sync job by calling Kepler endpoint. Sync's Agent picks up the job and process it. In the meantime, RIP Agent is released and can pick up another job. Sync will update Job History and Job History errors as needed, so that won't be a problem. The problem is that we would have to change the bahavior of `Run` button in RIP, which should be grayed-out until Sync job completes and that might be a little bit tricky to implement (`ButtonStateBuilder` could poll Sync job status and return appropriate result, but we must store Sync Job ID somewhere to make it possible).
- We need to preserve backwards compatibility with RIP RDOs, i.e. Job History, Job History Errors and Destination Workspace. When building Sync configuration, the RDO Framework requires to pass RDO configuration (GUIDs) via `ISyncConfigurationBuilder.ConfigureRdos`. We can simply leave it as it is for RIP and it should work out of the box - job status table will be refreshed independently of any running Agent, and item-level errors will be added for Job History.

## Consequences

Decoupling Sync to separate RAP is going to have following benefits:

- **faster deployment of Sync into production**, i.e. we don't bump Sync package in RIP and deploy whole RIP anymore. Approximate calculations: updating package in RIP along with other dependencies sometimes takes up to 4 hours if there are package conflicts that are hard to resolve. Running RIP Trident pipeline takes about 1 hour and this must be multiplied by 3 (build on PR, build on develop, build on master). This gives us up to 7 hours only for producing RIP package with updated Sync.
- another consequence of above is that **Sync RAP deployment will not affect or possibly break any running RIP jobs** (for example Import From Load File which is extensively used by Trace)
- development of new features such as Single/Multi Objects or ADF **can be done simultanously with decoupling**
- easier separation of R1 and Server implementations
- easier integration with Web Import-Export application via Keplers
- Sync and RIP could be maintanted by two completely separate Teams and on-boarding new joiners for Sync will be much easier

If we don't decouple Sync before other initiatives (Single/Multi Objects support and ADF) development and deployment would be costly, and will take additional time to deploy into production. Decoupling is going to take some time (hard to estimate at this point, but my gut feeling says it might be something between 20-40 MD), however we can do it simultanously with implementing Single/Multi Objects and ADF.

## Concerns and open questions

- decoupled Sync will require installing it into workspace separately, otherwise RIP workspace-to-workspace flow is not going to work, however we can install Sync by default to every new workspace
- when new Sync job is run, RIP Agent should be released and ready to take next RIP job. However, Integration Point should have `Run` button disabled until Sync job completes and that might be tricky to implement
- Company may not allow us to log on information level for new application - however we have one strong argument, that the amount of logs won't change, we just move them from one application to another
