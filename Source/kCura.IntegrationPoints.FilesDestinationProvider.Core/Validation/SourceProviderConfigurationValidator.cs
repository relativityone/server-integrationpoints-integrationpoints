using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Validation.Implementation;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation
{
	public class SourceProviderConfigurationValidator : IValidator
	{
		private readonly ISerializer _serializer;

		public string Key => IntegrationModelValidator.GetSourceProviderValidatorKey(Constants.RELATIVITY_PROVIDER_GUID, IntegrationPoints.Core.Constants.IntegrationPoints.LOAD_FILE_DESTINATION_PROVIDER_GUID);

		public SourceProviderConfigurationValidator(ISerializer serializer)
		{
			_serializer = serializer;
		}

		public ValidationResult Validate(object value)
		{
			var sourceConfiguration = value as SourceConfiguration;
			//var settings = _serializer.Deserialize<ExportUsingSavedSearchSettings>(value.ToString());


			// TODO implement validation (doh!) but don't mess with Relativity provider's settings!

			return new ValidationResult { IsValid = true };
		}
	}
}