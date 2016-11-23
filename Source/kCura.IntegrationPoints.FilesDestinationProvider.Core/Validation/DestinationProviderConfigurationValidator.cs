using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Validation.Implementation;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation
{
	public class DestinationProviderConfigurationValidator : IValidator
	{
		private readonly ISerializer _serializer;

		public string Key => IntegrationModelValidator.GetDestinationProviderValidatorKey(Constants.RELATIVITY_PROVIDER_GUID, IntegrationPoints.Core.Constants.IntegrationPoints.LOAD_FILE_DESTINATION_PROVIDER_GUID);

		public DestinationProviderConfigurationValidator(ISerializer serializer)
		{
			_serializer = serializer;
		}

		//TODO Merge this with Source Provider
		public ValidationResult Validate(object value)
		{
			//TODO var destinationConfiguration = value as ImportSettings;
			//var settings = _serializer.Deserialize<ExportSettings>(value.ToString());

			// TODO implement validation (doh!)

			return new ValidationResult { IsValid = true };
		}
	}
}