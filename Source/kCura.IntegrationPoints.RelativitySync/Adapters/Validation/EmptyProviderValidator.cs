using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.RelativitySync.Adapters
{
	internal class EmptyProviderValidator : IIntegrationPointProviderValidator
	{
		public ValidationResult Validate(IntegrationPointModelBase model, SourceProvider sourceProvider, DestinationProvider destinationProvider, IntegrationPointType integrationPointType, string objectTypeGuid)
		{
			return new ValidationResult(true);
		}
	}
}
