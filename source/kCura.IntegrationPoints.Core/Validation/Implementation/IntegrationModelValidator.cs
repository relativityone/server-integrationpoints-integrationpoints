using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Validation.Implementation
{
	public class IntegrationModelValidator : IIntegrationModelValidator
	{
		private readonly ILookup<string, IValidator> _validatorsMap;
		private readonly ISerializer _serializer;

		public IntegrationModelValidator(IEnumerable<IValidator> validators, ISerializer serializer)
		{
			_validatorsMap = validators.ToLookup(x => x.Key);
			_serializer = serializer;
		}

		public ValidationResult Validate(IntegrationPointModel model, SourceProvider sourceProvider, DestinationProvider destinationProvider)
		{
			var result = new ValidationResult();

			var destinationConfiguration = _serializer.Deserialize<ImportSettings>(model.Destination);

			var integrationModelValidation = new IntegrationModelValidation(model, sourceProvider.Identifier, destinationProvider.Identifier);
			integrationModelValidation.ArtifactTypeId = destinationConfiguration.ArtifactTypeId;

			if (model.Scheduler.EnableScheduler)
			{
				foreach (var validator in _validatorsMap[Constants.IntegrationPointProfiles.Validation.SCHEDULE])
				{
					result.Add(validator.Validate(model.Scheduler));
				}
			}

			foreach (var validator in _validatorsMap[Constants.IntegrationPointProfiles.Validation.EMAIL])
			{
				result.Add(validator.Validate(model.NotificationEmails));
			}

			foreach (var validator in _validatorsMap[Constants.IntegrationPointProfiles.Validation.NAME])
			{
				result.Add(validator.Validate(model.Name));
			}

			foreach (var validator in _validatorsMap[Constants.IntegrationPointProfiles.Validation.FIELD_MAP])
			{
				result.Add(validator.Validate(integrationModelValidation));
			}

			foreach (
				var validator in
					_validatorsMap[GetProviderValidatorKey(sourceProvider.Identifier, destinationProvider.Identifier)])
			{
				result.Add(validator.Validate(integrationModelValidation));
			}

			return result;
		}

		public static string GetProviderValidatorKey(string sourceProviderId, string destinationProviderId)
		{
			sourceProviderId = destinationProviderId.ToUpper();
			destinationProviderId = destinationProviderId.ToUpper();

			return $"{sourceProviderId}+{destinationProviderId}";
		}

		public static string GetDestinationProviderValidatorKey(string sourceProviderId, string destinationProviderId)
		{
			return $"{destinationProviderId}+{sourceProviderId}";
		}
	}
}