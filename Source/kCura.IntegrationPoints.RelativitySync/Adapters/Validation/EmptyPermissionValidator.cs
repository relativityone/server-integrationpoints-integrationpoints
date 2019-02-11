using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.RelativitySync.Adapters
{
	internal sealed class EmptyPermissionValidator : IIntegrationPointPermissionValidator
	{
		public ValidationResult Validate(IntegrationPointModelBase model, SourceProvider sourceProvider, DestinationProvider destinationProvider, IntegrationPointType integrationPointType, string objectTypeGuid)
		{
			return new ValidationResult(true);
		}

		public ValidationResult ValidateSave(IntegrationPointModelBase model, SourceProvider sourceProvider, DestinationProvider destinationProvider, IntegrationPointType integrationPointType, string objectTypeGuid)
		{
			return new ValidationResult(true);
		}

		public ValidationResult ValidateViewErrors(int workspaceArtifactId)
		{
			return new ValidationResult(true);
		}

		public ValidationResult ValidateStop(IntegrationPointModelBase model, SourceProvider sourceProvider, DestinationProvider destinationProvider, IntegrationPointType integrationPointType, string objectTypeGuid)
		{
			return new ValidationResult(true);
		}
	}
}