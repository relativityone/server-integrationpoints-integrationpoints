using System;

namespace kCura.IntegrationPoints.Data.Models
{
    public class SecretPath
    {
        public int? WorkspaceID { get; }
        public int? IntegrationPointID { get; }
        public string SecretID { get; }

        private SecretPath(
            int? workspaceID = null,
            int? integrationPointID = null,
            string secretID = null)
        {
            WorkspaceID = workspaceID;
            IntegrationPointID = integrationPointID;
            SecretID = secretID;
            ValidatePath();
        }

        public static SecretPath ForIntegrationPointSecret(
            int workspaceID, 
            int integrationPointID, 
            string secretID)
        {
            if (string.IsNullOrWhiteSpace(secretID))
            {
                ThrowInvalidSecretPathException("SecretID cannot be null or whitespace.");
            }

            return new SecretPath(
                workspaceID,
                integrationPointID,
                secretID
            );
        }

        public static SecretPath ForAllSecretsInIntegrationPoint(int workspaceID, int integrationPointID)
        {
            return new SecretPath(workspaceID, integrationPointID);
        }

        public static SecretPath ForAllSecretsInWorkspace(int workspaceID)
        {
            return new SecretPath(workspaceID);
        }

        public static SecretPath ForAllSecretsInAllWorkspaces()
        {
            return new SecretPath();
        }

        public override string ToString()
        {
            string secretPath = string.Empty;

            if (WorkspaceID != null)
            {
                secretPath = $"{secretPath}/{WorkspaceID}";
            }

            if (IntegrationPointID != null)
            {
                secretPath = $"{secretPath}/{IntegrationPointID}";
            }

            if (SecretID != null)
            {
                secretPath = $"{secretPath}/{SecretID}";
            }

            return secretPath;
        }

        private void ValidatePath()
        {
            bool isWorkspaceIDNegative = WorkspaceID < 0;
            bool isIntegrationPointIDNegative = IntegrationPointID < 0;
            bool isIntegrationPointPathSegmentInvalid = WorkspaceID == null && IntegrationPointID != null;
            bool isSecretPathSegmentInvalid = (WorkspaceID == null || IntegrationPointID == null) && SecretID != null;

            Guid secretID;
            bool secretIdIsNotGuid = SecretID != null && !Guid.TryParse(SecretID, out secretID);

            bool isPathInvalid = isIntegrationPointPathSegmentInvalid
                || isSecretPathSegmentInvalid
                || isWorkspaceIDNegative
                || isIntegrationPointIDNegative
                || secretIdIsNotGuid;

            if (isPathInvalid)
            {
                string validationDetails =
                    $"{nameof(WorkspaceID)}: {WorkspaceID}, "
                    + $"{nameof(IntegrationPointID)}: {IntegrationPointID}, "
                    + $"{nameof(SecretID)}: {SecretID}";
                ThrowInvalidSecretPathException(validationDetails);
            }
        }

        private static void ThrowInvalidSecretPathException(string message)
        {
            throw new ArgumentException($"Invalid secret path. {message}");
        }
    }
}
