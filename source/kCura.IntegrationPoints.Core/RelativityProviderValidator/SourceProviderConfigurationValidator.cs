using System;
using kCura.IntegrationPoints.Core.Validation.Implementation;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.RelativityProviderValidator
{
	class SourceProviderConfigurationValidator : IValidator
	{
		public string Key => IntegrationModelValidator.GetSourceProviderValidatorKey(IntegrationPoints.Domain.Constants.RELATIVITY_PROVIDER_GUID, Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID.ToString());

		public ValidationResult Validate(object value)
		{
			throw new NotImplementedException();
		}
	}
}
