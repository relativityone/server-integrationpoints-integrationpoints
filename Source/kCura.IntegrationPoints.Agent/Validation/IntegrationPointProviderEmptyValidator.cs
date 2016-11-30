using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Agent.Validation
{
	internal class IntegrationPointProviderEmptyValidator : IIntegrationPointProviderValidator
	{
		public ValidationResult Validate(IntegrationPointModelBase model, SourceProvider sourceProvider, DestinationProvider destinationProvider)
		{
			return new ValidationResult();
		}
	}
}