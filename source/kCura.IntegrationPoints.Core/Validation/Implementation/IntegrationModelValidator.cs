using System.Collections;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
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

		public ValidationResult Validate(IntegrationModel model, SourceProvider sourceProvider,
			DestinationProvider destinationProvider)
		{
			var result = new ValidationResult();

			//TODO Figure out if deserialize these two below here??
			//var sourceConfiguration = _serializer.Deserialize<SourceConfiguration>(model.SourceConfiguration);
			//var destinationConfiguration = _serializer.Deserialize<ImportSettings>(model.Destination);  //TODO Maybe ExportSettings or just send JSON????
			//TODO var fieldMap = _serializer.Deserialize<IEnumerable<FieldMap>>(model.Map).ToList();

			var integrationModelValidation = new IntegrationModelValidation(model, sourceProvider.Identifier, destinationProvider.Identifier);

			if (model.Scheduler.EnableScheduler)
			{
				foreach (var validator in _validatorsMap[Constants.IntegrationPoints.Validation.SCHEDULE])
				{
					result.Add(validator.Validate(model.Scheduler));
				}
			}

			foreach (var validator in _validatorsMap[Constants.IntegrationPoints.Validation.EMAIL])
			{
				result.Add(validator.Validate(model.NotificationEmails));
			}

			foreach (var validator in _validatorsMap[Constants.IntegrationPoints.Validation.FIELD_MAP])
			{
				result.Add(validator.Validate(integrationModelValidation));
			}

			foreach (
				var validator in
					_validatorsMap[GetSourceProviderValidatorKey(sourceProvider.Identifier, destinationProvider.Identifier)])
			{
				result.Add(validator.Validate(model.SourceConfiguration));
			}

			foreach (
				var validator in
					_validatorsMap[GetDestinationProviderValidatorKey(sourceProvider.Identifier, destinationProvider.Identifier)])
			{
				result.Add(validator.Validate(model.Destination));
			}


			return result;
		}

		public static string GetSourceProviderValidatorKey(string sourceProviderId, string destinationProviderId)
		{
			return $"{sourceProviderId}+{destinationProviderId}";
		}

		public static string GetDestinationProviderValidatorKey(string sourceProviderId, string destinationProviderId)
		{
			return $"{destinationProviderId}+{sourceProviderId}";
		}
	}
}