﻿using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.Abstract
{
	public interface IIntegrationPointProviderValidator
	{
		ValidationResult Validate(IntegrationPointModel model, SourceProvider sourceProvider, DestinationProvider destinationProvider);
	}
}