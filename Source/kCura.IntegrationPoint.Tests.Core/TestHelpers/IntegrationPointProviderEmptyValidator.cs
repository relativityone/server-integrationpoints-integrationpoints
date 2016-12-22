using System;
using System.Linq;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	internal class IntegrationPointProviderEmptyValidator : IIntegrationPointProviderValidator
	{
		public ValidationResult Validate(IntegrationPointModelBase model, SourceProvider sourceProvider, DestinationProvider destinationProvider, IntegrationPointType integrationPointType)
		{
			return new ValidationResult();
		}
	}
}