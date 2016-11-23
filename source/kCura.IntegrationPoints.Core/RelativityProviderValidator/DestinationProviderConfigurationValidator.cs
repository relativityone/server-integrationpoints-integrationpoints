using kCura.IntegrationPoints.Core.Validation.Implementation;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.RelativityProviderValidator
{
	public class DestinationProviderConfigurationValidator : IValidator
	{
		public string Key => IntegrationModelValidator.GetDestinationProviderValidatorKey(IntegrationPoints.Domain.Constants.RELATIVITY_PROVIDER_GUID, Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID.ToString());

		public ValidationResult Validate(object value)
		{
			//TODO Merge this with Source Provider
			throw new System.NotImplementedException();
		}
	}
}
