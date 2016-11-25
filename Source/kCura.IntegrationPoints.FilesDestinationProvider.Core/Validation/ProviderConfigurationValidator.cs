using System;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation.Implementation;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Validation
{
    public class ProviderConfigurationValidator : IValidator, IIntegrationPointValidationService
    {
		private readonly ISerializer _serializer;

		private readonly IValidatorsFactory _validatorsFactory;

		public ProviderConfigurationValidator(ISerializer serializer, IValidatorsFactory validatorsFactory)
		{
			_serializer = serializer;
			_validatorsFactory = validatorsFactory;
		}

		public string Key => IntegrationModelValidator.GetSourceProviderValidatorKey(
			IntegrationPoints.Domain.Constants.RELATIVITY_PROVIDER_GUID,
			IntegrationPoints.Core.Constants.IntegrationPoints.LOAD_FILE_DESTINATION_PROVIDER_GUID
		);

		public ValidationResult PreValidate(IntegrationModel model)
		{
			var result = new ValidationResult();

			var exportFileValidator = _validatorsFactory.CreateExportFileValidator();
			result.Add(exportFileValidator.Validate(model));

			return result;
		}

		public ValidationResult Validate(object value)
		{
			return Validate(value as IntegrationModelValidation);
		}

		public ValidationResult Validate(IntegrationModel model)
		{
			var result = new ValidationResult();

			return result;
		}
	}
}