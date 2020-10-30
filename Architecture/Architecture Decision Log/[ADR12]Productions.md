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

- Verify if production is eligible for import, which probably means it should be in the "New" state. In RIP, the method `ImportProductionValidator.ValidateProductionState` does this check, and under the hood it invokes `WinEDDS.Service.Export.IProductionManager.RetrieveImportEligibleByContextArtifactID`. If that method does not return requested production set, our validation fails with the message: `Verify if a Production Set used as the location in destination workspace is in New status.`. If it's true that the production state is the only condition we should check here, then we can probably use Kepler method `IProductionManager.ReadSingleAsync` because its' return value has this information (`Production.ProductionMetadata.Status`). If not, we should take a closer look to `IProductionManager.GetProductionsEligibleForReproductionAsync` as suggested in mentioned Einstein article, but this should be investigated further and confirmed with IAPI team.

- Verify if the user has permissions to create production data source in destination production set. This step is straightforward and we should use `IPermissionManager` Kepler, same way as in RIP (`ValidateCreatePermissionForProductionSource`).

## Production as source - data source snapshot creation

This step is very similar to the existing `ImageDataSourceSnapshotExecutor`. We can still use Object Manager's export API to create a snapshot with documents that belong to specified production. The only difference is the query condition, which should be:

`'Production' SUBQUERY ('Production::ProductionSet' == OBJECT {sourceProductionArtifactID})`

All the rest remains the same. We already have implementation for retrieving images from documents, so that should work out of the box.

## Production set as destination - setting up IAPI

To import data into production in destination workspace, it is required to properly configure IAPI job. This should be done in `ImportJobFactory` where we are setting up import job. We should add following settings:

- `ForProduction` - should be set to `true`
- `ProductionArtifactID` - artifact ID of the production set in destination workspace
- `BatesNumberField` - Currently in RIP this points to identifier column, however there is a story to change that behavior (REL-162726) so for productions the bates fields are populated with the bates numbers from the source case.

## Sync configuration

