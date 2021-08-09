# Import non-document objects with Relativity Sync using ImportAPI

## Status

Proposed

## Context

Customers are looking for a feature that allows to transfer non-document objects between workspaces using Relativity Sync. That flow has never been supported in RIP and needs to be investigated and designed from the ground up.

## Object Types

In order to import RDOs to the workspace using ImportAPI, all required Object Types must already exist in that workspace. There are two different approaches to fill this requirement.

### Assume Object Types already exist in destination

The most common scenarios for pushing non-document objects using Sync, is to push objects which are part of Relativity Applications. One example is `Entity` - it is a part of Legal Hold application. If the customer wants to push Entities from workspace A to workspace B, he must first install Legal Hold in workspace B to create required `Entity` Object Type. **This is the same workflow as in RDC today.** The problem with this approach is pushing custom Object Types, which are not a part of any Relativity Application. In such case, the customer would have to manually re-create that Object Type (and other related parent Object Types) in destination workspace, which is not so convenient and user-friendly.

### Automatically create required Object Types in destination

It is possible to automatically create non-existing Object Types in destination. However, the problem is that we obviously don't want to create aforementioned `Entity` which belongs to Legal Hold application. Such Object Types and their fields, usually have their own GUIDs defined in application schema. If we create them in destination, they might not be fully compatible with types defined in schema (e.g. missing GUIDs) thus making it impossible to install application in that workspace afterwards. Possible solution to this, might be showing a warning to the user, with information that he should make sure that the Object Type he is transferring is not a part of any Relativity Application and that all missing Object Types will be created on his own risk.

We can also consider detecting if selected Object Type belongs to any Relativity Application. This is probably possible to do with `ObjectTypeManager` Kepler, but it should be done with extra caution, to ensure we are not breaking breaking application installation in destination workspace.

With this approach, there will be also required UI changes in RIP, because of the field mapping. When Object Type doesn't exist in destination, it's impossible to map fields as usual. Instead we could for example disable panels on the right (destination), and only choose fields from left panel (source). Or - what's probably even better solution - we can add an option to create Object Types hierarchy in destination earlier on second step - see `Required UI changes - destination location` for more details about this idea.

As a side note, having that functionality implemented will be also helpful in supporting Object / Multi Object fields in Document sync flow in the future.

## Required UI changes

- Transferred Object drop-down on first step is currently set to a Document and is grayed-out, so we need to allow Object Type selection.
- Source on second step - Saved Search or Production are no longer a valid source for non-document objects. We should probably still allow user to transfer only a subset of objects, not necessarily all of them. Best option seems to be a View selection, because they support conditions, similar to Saved Searches. So on second step in RIP there should be probably only one option for Source - View. It should allow to select a View in similar way as saved search (drop-down or ellipsis button).
- Destination location on second step. Currently there are two options: Folder and Production Set. This can be hidden completely for non-document objects, or we can replace it with destination Object Type selection. This might be useful in case selected Object Type in source has different name in destination. We can even go further and implement functionality that allows user to ad-hoc create Object Type hierarchy in destination in case it's missing (similar to creating Production Set). Advantage is that Object Type will already exist when user goes to the 3rd step, so we can auto-map fields that are included in a View (using a button similar to "Map Saved Search"). Potential disadvantage is that this process will not be a part of Sync job (however still can be implemented in Relativity.Sync.dll to be fully decoupled from RIP).
- Create Saved Search on second step should be hidden for non-document objects.
- Copy Images, Copy Native Files and Use Folder Path Information on 3rd step - should be hidden.

## Sync Architectural changes

For transferring non-document objects, there will be need to create new Sync Pipeline along with a couple of new nodes/steps.

### Tagging and retries

The problem with tagging is that currently we are using `Job History` field on document to tag and later grab the documents for retries. We could probably use similar mechanism for non-document objects. But it will require creation of `Job History` field on selected Object Type in source workspace. This is subject for further discussions as it might be a breaking change for some applications.

Another question is if we want to tag trasferred objects (both in source and destination) same as we do with Documents:

- In source - `Relativity Destination Case`
- In destination - `Relativity Source Case` and `Relativity Source Job`

 Again, in that case those fields should be created in source and destination Object Types. This also needs to be discussed with the team and PM.

### RdoObjectTypesCreationExecutor

If we choose to automatically create all required Object Types in destination when running Sync job, then we need to add this new step in pipeline. It should make a 1:1 copy of selected Object Type in destination workspace, along with fields of all supported types and their properties. It should handle partially created Object Types in case something goes wrong, and user retries the job.

### ValidationExecutor (extend)

We need to validate existence and permissions for Object Types in source and destination workspaces:

- Source Workspace - check if user has `View` permission on selected Object Type
- Destination Workspace - check for existence and integrity of selected Object Type and its related Object Types and fields, by comparing whole object hierarchy with source workspace. This will require `Read` and `Edit` permissions on selected Object Type, as well as `Add` permission for Object Type.

### RdoDataSourceSnapshotExecutor and RdoRetryDataSourceSnapshotExecutor

Creating data source snapshot of non-document objects is very similar to the existing ones for documents and images, because Object Manager supports creating export snapshot for all Object Types. Difference is that now we will need to set `ObjectType.ArtifactTypeID` in `QueryRequest` to the Artifact Type ID from Sync configuration - `RdoArtifactTypeId`. Query condition for both executors will change a little bit - we must replace `IN SAVEDSEARCH` with `IN VIEW`, because a view is now the source for the objects. Example Condition for `RdoDataSourceSnapshotExecutor`:

```cs
Condition = $"'ArtifactID' IN VIEW {configuration.DataSourceArtifactId}"
```

### RdoSynchronizationExecutor

First of all, we will need to extend `ImportJobFactory` class and add new method - `CreateRdoImportJobAsync`. This method should call `IImportApi.NewObjectImportJob` and pass `RdoArtifactTypeId` as a parameter. Configuration looks straightforward and simplier than configuring natives or images. Example implementation can be found here: <https://jira.kcura.com/secure/attachment/212194/Sample%20-%20ImportRDOWithFiles.cs>
More information about configuring ImportAPI: <https://github.com/relativitydev/import-api-samples>

We should also create new implementation of `ISourceWorkspaceDataReader` along with Batch Data Reader, Field Manager and Row Values Builder for non-document objects implementation. We can re-use existing sanitizers for all field types.

### Sync Pipelines

There will be two new Pipelines:

- SyncRdoRunPipeline
- SyncRdoRetryPipeline

We should also extend `SyncConfigurationBuilder` to enable creating configuration for import non-document objects Sync job.

### Metrics

We should consider gathering the following metrics:

- Object Type being transferred - built-in (e.g. `Entity`) or custom
- Number of fields mapped
- Number of objects requested
- How many times Sync needed to create missing Object Type in destination
