using System;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Abstract
{
    public interface IIntegrationPointPermissionValidator : IIntegrationPointValidator
    {
        ValidationResult ValidateSave(
            IntegrationPointModelBase model,
            SourceProvider sourceProvider,
            DestinationProvider destinationProvider,
            IntegrationPointType integrationPointType,
            Guid objectTypeGuid,
            int userId);

        ValidationResult ValidateViewErrors(int workspaceArtifactId);

        ValidationResult ValidateStop(
            IntegrationPointModelBase model,
            SourceProvider sourceProvider,
            DestinationProvider destinationProvider,
            IntegrationPointType integrationPointType,
            Guid objectTypeGuid,
            int UserId);
    }
}
