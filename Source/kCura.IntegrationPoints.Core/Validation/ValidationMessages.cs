﻿using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation
{
	internal static class ValidationMessages
	{
		#region Sync
		public static ValidationMessage DestinationWorkspaceNoAccess => new ValidationMessage(
			errorCode: "20.001",
			shortMessage: "User does not have sufficient permissions to access destination workspace. Contact your system administrator."
		);
		public static ValidationMessage DestinationWorkspaceNotAvailable => new ValidationMessage(
			errorCode: "20.002",
			shortMessage: "Destination workspace is not available."
		);
		public static ValidationMessage SavedSearchNoAccess => new ValidationMessage(
			errorCode: "20.004",
			shortMessage: "Saved search is not available or has been secured from this user. Contact your system administrator."
		);
		public static ValidationMessage FieldMapFieldNotExistInDestinationWorkspace(string fieldNames)
		{
			string shortMessage = $"Destination field(s) mapped may no longer be available or has been renamed. Review the mapping for the following field(s): {fieldNames}.";
			return new ValidationMessage(
				errorCode: "20.005",
				shortMessage: shortMessage);
		}
		public static ValidationMessage SourceProductionNoAccess => new ValidationMessage(
			errorCode: "20.007",
			shortMessage: "Verify if production, which is the data source of this Integration Point, still exist or if user has required permissions for it."
		);
		public static ValidationMessage MissingDestinationSavedSearchAddPermission => new ValidationMessage(
			errorCode: "20.008",
			shortMessage: "Verify if the user still has permission to create saved search on destination workspace."
		);
		public static ValidationMessage MissingDestinationFolderItemLevelPermissions => new ValidationMessage(
			errorCode: "20.009",
			shortMessage: "Verify if a folder in destination workspace selected in the Integration Point exists or if a user has a proper permission to access it."
		);
		public static ValidationMessage MissingDestinationProductionPermissions => new ValidationMessage(
			errorCode: "20.010",
			shortMessage: "Verify if a Production Set used as the location in destination workspace exists or if a user has a proper permission to access it."
		);

		public static ValidationMessage FederatedInstanceNotAccessible => new ValidationMessage(
			errorCode: "20.011",
			shortMessage: "Unable to connect to federated instance. Check if destination instance still exists or if a user has a proper permission to access it."
		);

		public static ValidationMessage FederatedInstanceDestinationWorkspaceNotAvailable => new ValidationMessage(
			errorCode: "20.012",
			shortMessage: "Destination workspace on federated instance is not available. Check if destination workspace still exists or if a user has a proper permission to access it."
		);
		public static ValidationMessage DestinationProductionNotEligibleForImport => new ValidationMessage(
			errorCode: "20.013",
			shortMessage: "Verify if a Production Set used as the location in destination workspace is in New status.");

		#endregion
	}
}
