# Sync.Plugin for RDOs Rescue

## Status

Proposed

## Context

At the end of Q1 we released **SYNC Non-Documents Flow** ([Non-Doc ADR](./%5BADR13%5D%20Import_non_document_objects.md)). It enable customers to push RDOs between workspaces. The biggest drawback is that the objects are pushed blindly - there is no data correctness checks. Due to that the flow is unusable with RDOs on which clients care most (Analytics, STRs, Productions, Integration Points Profiles) - In general all RDOs which have dependencies to ArtifactID won't work in Destination Workspace

The second issue we noticed is related to Single/Multi Objects push between workspaces - [Single/Multi Object Problem Statement](./%5BADR17%5D%20Single_Multi_Objects_Problem_Statement.md). The objects re-created in Destination Workspace are blown egss, which cannot be used - sometimes they cannot be even opened/edited (details in above ADR).

Based on that we plan to introduce the mechanism which shift the "RDO re-creation" during SYNC job to the RDO-Owner who has most detailed knowledge about RDO structure and have best tools to re-create fully operational objects in Destination Workspace

## Current Design

Problem statement and current architecture can be divided into two separate problems which will be described below

### SYNC Single/Multi Object Architecture

Architecture is relied on IAPI Import job where we get Document IDs from Saved Search and build our own `IDataReader` implementation in which we handle Non-Standard fields types manually e.g. [MultiObjectFieldSanitizer.cs](https://git.kcura.com/projects/DTX/repos/relativitysync/browse/Source/Relativity.Sync/Transfer/MultipleObjectFieldSanitizer.cs).

Linking is based on **Name** property, so if in Destination Workspace the RDO with this Name already exists it won't be created and Document in Destination Workspace will be linked with this RDO.

As mentioned above the main problems are:

* RDOs which have Parent Object different than Workspace cannot be created by Object Manager, because following Parent Object doesn't exist in Destination Workspace
* RDOs are created as blown eggs - **ArtifactID<->Name** pair which satisfy Document View, but those created objects are completely unusable

_Diagram 1. Single/Multi Object Architecture_
![Single_Multi_Object_Architecture](imgs/019_single_multi_object_architecture.jpeg)

### SYNC Non-Document Object Architecture

By Non-Document Objects we mean all RDOs except Document. This flow is quite similar standard Documents push between workspace. What differs both flows is Data Source, where for RDOs we take the IDs from View. Under the hood we still use NonDocument Import API Job.

_Diagram 2. Non-Document SYNC Architecture_
![Non_Document_Sync_Architecture](imgs/019_non_document_sync_architecture.jpeg)

## Decision



## Consequences

What becomes easier or more difficult to do because of this change?