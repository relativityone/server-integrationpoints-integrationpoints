# Productions pushing in Sync

## Status

Proposed

## Context

What is the issue that we're seeing that is motivating this decision or change?

## Validation step

### **Production as source**

Validator should check if the user has access to the source production. We can use Kepler method `IProductionManager.ReadSingleAsync`. Validator class in RIP: `ProductionValidator`

### **Production as destination**

Validator class in RIP is `ImportProductionValidator`. It uses ImportAPI's `IProductionManager` WebAPI service, but the future of this service is uncertain at the moment. More information available here: https://einstein.kcura.com/pages/viewpage.action?pageId=227148384. However it looks like all of the functionalities of this validator can be implemented using Keplers `IProductionManager` and `IPermissionManager`. A few checks should be done here:

- Verify if production is available in destination workspace and if user has access to it. Currently RIP uses `WinEDDS.Service.Export.IProductionManager.Read` method, but this can be easily replaced with Kepler `IProductionManager.ReadSingle`

- Verify if production is eligible for import, which probably means it should be in the "New" state. In RIP, method `ImportProductionValidator.ValidateProductionState` does this check, and under the hood it invokes `WinEDDS.Service.Export.IProductionManager.RetrieveImportEligibleByContextArtifactID`. If that method does not return requested production set, our validation fails with the message: `Verify if a Production Set used as the location in destination workspace is in New status.`. If it's true that the production state is the only condition we should check here, then we can probably use Kepler method `IProductionManager.ReadSingleAsync` because its' return value has this information (`Production.ProductionMetadata.Status`). If not, we should take a closer look to `IProductionManager.GetProductionsEligibleForReproductionAsync` as suggested in mentioned Einstein article, but this should be investigated further and confirmed with IAPI team.

- Verify if the user has permissions to create production data source in destination production set. This step is straightforward and we should use `IPermissionManager` Kepler, same way as in RIP (`ValidateCreatePermissionForProductionSource`).

## Production as source - data source snapshot creation

This step is very similar to the existing `ImageDataSourceSnapshotExecutor`. In the new `ProductionDataSourceSnapshotExecutor` we can still use Object Manager's export API to create a snapshot with documents that belong to specified production. The only difference is the query condition, which should be:

`'Production' SUBQUERY ('Production::ProductionSet' == OBJECT {sourceProductionArtifactID})`

Retries mechanism is also very similar to the one from images flow, and require changing condition to:

`(NOT 'Job History' SUBQUERY ('Job History' INTERSECTS MULTIOBJECT [{configuration.JobHistoryToRetryId}])) AND ('Production' SUBQUERY ('Production::ProductionSet' == OBJECT {sourceProductionArtifactID})) AND 'Production::Image Count' > 0`

All the rest remains the same. We already have implementation for retrieving images from documents (both original and produced), so that should work out of the box.

## Production set as destination - setting up IAPI

To import data into production in destination workspace, it is required to properly configure IAPI job. This should be done in `ImportJobFactory` where we are setting up import job. We should add another method, for example `CreateProductionImportJobAsync` which will be almost the same as the method that creates import job for images, but should additionally set:

- `ForProduction` - should be set to `true`
- `ProductionArtifactID` - artifact ID of the production set in destination workspace
- `BatesNumberField` - currently in RIP this is set to Control Number field name and it looks like IAPI job fails if it's not set, or set to different value

## Sync configuration

- Data Source Type and Data Destination Type - those fields already exist in Sync Configuration, however they are hardcoded strings (`SavedSearch` and `Folder`) in `IntegrationPointToSyncConverter`. We should set the values depending on the flow. Also we need to create enum types for them and make sure they are deserialized correctly in Sync `*Configuration` classes.
- Data Source Artifact ID - this is currently set to saved search Artifact ID, and should be set to either saved search or production Artifact ID, depending on the flow
- Data Destination Artifact ID - currently set to destination folder Artifact ID, and should be set to either folder or production Artifact ID, depending on the flow

## Sync pipeline

In context of pushing images from/to production, there are 3 additional possible flows.
Choosing the right flow should be based on the Data Source Type and Data Destination Type fields from Sync Configuration.

### 1. Production to production

- besides common validators, also execute validator to verify production in source, and all validators to verify production set in destination
- use new `ProductionDataSourceSnapshotExecutor` (or `ProductionRetryDataSourceSnapshotExecutor` for retries flow)
- use new method `IImportJobFactory.CreateProductionImportJobAsync` for creating import job

### 2. Production to folder

- besides common validators, also execute validator to verify production in source
- use new `ProductionDataSourceSnapshotExecutor` (or `ProductionRetryDataSourceSnapshotExecutor` for retries flow)
- use existing `IImportJobFactory.CreateImageImportJobAsync` for creating import job

### 3. Saved search to production

- besides common validators, also execute all validators to verify production set in destination
- use existing `ImageDataSourceSnapshotExecutor` (or `ImageRetryDataSourceSnapshotExecutor` for retries flow)
- use new method `IImportJobFactory.CreateProductionImportJobAsync` for creating import job

## Known issues with Production fields

 Currently in RIP, fields `Production::Begin Bates` and `Production::End Bates` are populated with the Control Number field value, which is incorrect. There is a story to change that behavior (REL-162726). However, at the moment it's still not clear if ImportAPI supports populating those fields.
