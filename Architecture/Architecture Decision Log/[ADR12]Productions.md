# Productions pushing in Sync

## Status

Proposed

## Context

Next step to fully decouple Sync from RIP, is to implement pushing of productions. This should allow our customers to use new Sync to perform:

- push a production to another production in destination
- push a production to folder in destination
- push images (original and produced) from saved search to production in destination

## Validation step

### **Production as source**

Validator should check if the user has access to the source production. We can use Kepler method `IProductionManager.ReadSingleAsync`. Validator class in RIP for reference: `ProductionValidator`

### **Production as destination**

Validator class in RIP is `ImportProductionValidator`. It uses ImportAPI's `IProductionManager` WebAPI service, but this service will be deprecated in the future. More information available here: https://einstein.kcura.com/pages/viewpage.action?pageId=227148384. However it looks like most of the functionalities of this validator can be implemented using Keplers `IProductionManager` and `IPermissionManager`. A few checks should be done here:

- Verify if production is available in destination workspace and if user has access to it. Currently RIP uses `WinEDDS.Service.Export.IProductionManager.Read` method, but this can be easily replaced with Kepler `IProductionManager.ReadSingleAsync`

- Verify if the user has permissions to create production data source in destination production set. This step is straightforward and we should use `IPermissionManager` Kepler, same way as in RIP (`ValidateCreatePermissionForProductionSource`).

- Verify if production is eligible for import. This is most problematic part. In RIP, method `ImportProductionValidator.ValidateProductionState` does this check, and under the hood it invokes `WinEDDS.Service.Export.IProductionManager.RetrieveImportEligibleByContextArtifactID`. If that method does not return requested production set, our validation fails with the message: `Verify if a Production Set used as the location in destination workspace is in New status.`. However, this error message is not entirely true, because Import API performs the following checks to determine if it can import data to particular production:
`(Production.Status = New AND Production.DataSource.Empty) OR Production.Imported`. We can use that condition to implement our own method for retrieving import eligible productions. There also exists Kepler method - `IProductionManager.GetProductionsEligibleForReproductionAsync` as suggested in mentioned Einstein article, but this probably lacks some checks (this information comes from Import API team) that `RetrieveImportEligibleByContextArtifactID` does, so we cannot use it.

## Production as source - data source snapshot creation

This step is very similar to the existing `ImageDataSourceSnapshotExecutor`. In the new `ProductionDataSourceSnapshotExecutor` we can still use Object Manager's export API to create a snapshot with documents that belong to specified production. The only difference is the query condition, which should be:

`'Production' SUBQUERY ('Production::ProductionSet' == OBJECT {sourceProductionArtifactID}) AND 'Production::Image Count' > 0`

Retries mechanism is also very similar to the one from images flow, and require changing condition to:

`(NOT 'Job History' SUBQUERY ('Job History' INTERSECTS MULTIOBJECT [{configuration.JobHistoryToRetryId}])) AND ('Production' SUBQUERY ('Production::ProductionSet' == OBJECT {sourceProductionArtifactID})) AND 'Production::Image Count' > 0`

All the rest remains the same. We already have implementation for retrieving images from documents (both original and produced), so that should work out of the box.

## Production set as destination - setting up IAPI

To import data into production in destination workspace, it is required to properly configure IAPI job. This should be done in `ImportJobFactory` where we are setting up import job. We should add another method, for example `CreateProductionImportJobAsync` which will be almost the same as the method that creates import job for images, but should additionally set:

- `ForProduction` - should be set to `true`
- `ProductionArtifactID` - artifact ID of the production set in destination workspace
- `BatesNumberField` - currently in RIP this is set to Control Number which is incorrect. More details are below.

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

## Data destination initialization and finalization

- Currently in Sync there is existing node `DataDestinationInitialization` step which was supposed to create production in destination. However now it makes no sense, because we are assuming that the production in destination is already created and ready to receive documents.
- Destination finalization has to be done to make production readonly. Currently, production is finalized after import job completes. In new approach each batch will be separate import job. It could require changes in import process, so we can manually finalize production. This should be further investigated and confirmed with ImportAPI team if it is possible to import into production without marking a production as read-only.

## Populating Production fields with Bates Number

 Currently in RIP, fields `Production::Begin Bates` and `Production::End Bates` are populated with the Control Number field value, which is incorrect. There is a story to change that behavior (REL-162726). Solution to this is to add another special field for images - `BatesNumber` and map this field to `ImageSettings.BatesNumberField` in `ImportJobFactory`. For each image, this field should contain value, taken from different source, depending on the flow:

 1. When source of the image is a production - each image has `BatesNumber` which is stored in `[ProductionDocumentFile_{ProductionID}]` table in database. This value can be easily extracted using `ISearchManager.RetrieveImagesForProductionDocuments` - it is another column in returned `DataRow` and is called `BatesNumber`
 2. When source of the image is saved search - then we should take the value from column `Identifier` in `DataRow` returned from `ISearchManager.RetrieveImagesForDocuments`. This solution is suggested by Import API team, but this should be validated with PM.

 Mass Import re-calculates Begin Bates and End Bates fields automatically:

- `Production::Begin Bates = MIN(bates number of document's images)`
- `Production::End Bates = MAX(bates number of document's images)`

However there is known defect in sorting mechanism for Bates Number. There is already a Jira ticket created to correct this defect - REL-315286.
