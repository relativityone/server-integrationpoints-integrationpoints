using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Agent.Validation
{
	public class IntegrationPointPermissionEmptyValidator : IIntegrationPointPermissionValidator
	{
		public ValidationResult Validate(IntegrationPointModelBase model, SourceProvider sourceProvider, DestinationProvider destinationProvider,
			IntegrationPointType integrationPointType)
		{
			return new ValidationResult();
		}

		public ValidationResult ValidateSave(IntegrationPointModelBase model, SourceProvider sourceProvider, DestinationProvider destinationProvider,
			IntegrationPointType integrationPointType)
		{
			return new ValidationResult();
		}

		public ValidationResult ValidateViewErrors(int workspaceArtifactId)
		{
			return new ValidationResult();
		}

		public ValidationResult ValidateStop(IntegrationPointModelBase model, SourceProvider sourceProvider, DestinationProvider destinationProvider,
			IntegrationPointType integrationPointType)
		{
			return new ValidationResult();
		}
	}
}