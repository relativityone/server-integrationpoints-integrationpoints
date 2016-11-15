using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain;

namespace kCura.IntegrationPoints.Core.Validation
{
	public interface IValidatorFactory
	{
		List<IProviderValidator> CreateIntegrationModelValidators(IntegrationModel model);
	}
}
