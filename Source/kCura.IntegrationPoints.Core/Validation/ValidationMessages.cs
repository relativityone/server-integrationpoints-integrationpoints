using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation
{
    internal static class ValidationMessages
    {
        #region Sync
        public static ValidationMessage DestinationWorkspaceNoAccess => new ValidationMessage(
            errorCode: $"20.001",
            shortMessage: "User does not have sufficient permissions to access destination workspace. Contact your system administrator."
        );
        public static ValidationMessage DestinationWorkspaceNotAvailable => new ValidationMessage(
            errorCode: "20.002",
            shortMessage: "Destination workspace is not available."
        );
        public static ValidationMessage SavedSearchNoAccess => new ValidationMessage(
            errorCode: "20.004",
            shortMessage: "Saved search is not available or has been secured from this user. Contact your system administrator.");
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
            shortMessage: "Verify if the user still has permission to create saved search on destination workspace.");
        #endregion
    }
}
