# Sync Integration with ADF and readiness to use ADLS

## Status

Proposed

## Context

In 2022 Relativity will migrate most of its data (native files, images, Processing files, ARM archives etc.) out of current File Servers to ADLS and make it accessible through SMB (Bedrock) or directly on ADLS. All teams must prepare for it and choose the best strategy for them. The easiest way to integrate with ADLS is to go through Bedrock because it provides exactly the same interface as current File Servers. However, for majority of use cases in Data Transfer it's not performant enough. All teams accessing files through Bedrock noticed significant performance degradation.
Mainly due to implementation cost and the fact that Bedrock is a temporary solution recommendation is to use ADF as a tool for copying data. It would enable us to growth faster as it can be used to very easily connect to external cloud storages. Also, time not spend on changing products for Bedrock or developing custom solution can be used to further improve ARM and Migrate which would accelerate clients transition from Relativity Server to R1.
[Integration with ADLS](https://einstein.kcura.com/display/DV/Integration+with+ADLS)

## Current Flow

Sync is currently using IAPI to Execute jobs with `NativeFileCopyMode =CopyFiles`. We are creating DataReader and passing it along to IAPI. IAPI is executing job:
 - copy metadata according to field mapping
 - copy natives to new location
 - set new native file location path for each document

## New flow with ADF S2 service
### Idea
In order to migrate CopyFiles to use ADLS, we are going to use new S2 service that is being made as part of Data Transfer initiative: [File Movement Service](https://git.kcura.com/projects/DTX/repos/file-movement-service)

### How does File Movement Service works
It's a S2 service - Azure service that is going to be hosted per tenant. It will allow file transfers from one place to another within Relativity Instance(or at last that is starting point, maybe in the future there will be possibility to do that across Instances).
- create list of files that are going to be transffered
    - we need to provide source and destination location for each file, so FMS know what to copy
- save this list onto fileshare that is accessible from Relativity
    - we can save it using Bedrock or making our own connector to ADLS
    - more prefered way is to use Bedrock as it will be just this one file
- calculate new path for each file
    - sample old path `\\files.t025.ctus014128.r1.kcura.com\T025\Files\EDDS1029088\RV_14ead259-830e-41f1-a1be-045ce963eb1c\32bad1e5-4e09-46b7-ba99-2e23ac2ef031`
        - UNC Path `\\files.t025.ctus014128.r1.kcura.com\T025\Files\`
        - Workspace specific folder: `EDDS102988`
        - Folder created by IAPI: `RV_14ead259-830e-41f1-a1be-045ce963eb1c`
        - file name:`32bad1e5-4e09-46b7-ba99-2e23ac2ef031` GUID, to make sure that file is unique
    - **ADLS Path creation**
        - we need to CREATE folder in destination workspace by using Bedrock
        - name of this Folder should be GUID
        - UNC Path of destination fileshare in ADLS convention `\\Destination\Workspace\Files\Location\On\ADLS`
            - we need to make a call to new Kepler service that is going to be created by FAST team
            - this will result in Response with token and exact destination location
        - Final full path: `\\Destination\Workspace\Files\Location\On\ADLS\{NewFolderNameGUID}\{FileName}32bad1e5-4e09-46b7-ba99-2e23ac2ef031`

## Consequences of using FMS
Natural consequnce of FMS is modification of document flow.
 - set new native file location path - this point will be done by Sync using **ADLS Path creation**
 - create "LoadFile" for FMS and save it on ADLS
 - copy natives to new location - this point will be done by using FMS 
 - run IAPI job with CopyLinks mode and disable FileExists check - We need to supply new location bases on **ADLS Path creation**
 - copy metadata according to field mapping

## Required code changes
In order to transtion away from IAPI dependency on copying native files, we need to change couple of things:

- `SynchronizationExeecutorBase.cs`
    - `ExecuteSynchronizationAsync(..)` method needs to be change. Current flow relies on creating one IAPI job that fits flow that it used for.( Documents, Images etc.)
Proposed changes:
1. Create separate flow for CopyNatives that is different from LinksOnly/Metadata Only modes.
    - Add new parallel Banzai node for ADSL actions(new paths, start FMS copy action)
    - when implementing IAPI 2.0 this will be unchanged.
2. Change implementation of CreateNativeImportJobAsync in ImportJobFactory that will change mode to LinksOnly when in CopyFiles flow.
    - Add dependency on CopyFiles config option in method `ExecuteSynchronizationAsync` of `SynchronizationExecutorBase.cs` and modify flow to trigger FMS and start LinksOnly job for metadata and FilePath

## Concerns
1. We need to implement QC to make sure that none of involved party is properly finished.(IAPI and FMS)
2. We need to make sure that we do not delete Snapshot before ending whole job( or at least Synchronization step).