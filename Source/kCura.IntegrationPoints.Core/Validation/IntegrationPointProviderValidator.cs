﻿using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Validation
{
	public class IntegrationPointProviderValidator : BaseIntegrationPointValidator<IValidator>, IIntegrationPointProviderValidator
	{
		public IntegrationPointProviderValidator(IEnumerable<IValidator> validators, ISerializer serializer)
			: base(validators, serializer)
		{			
		}

		public override ValidationResult Validate(IntegrationPointModelBase model, SourceProvider sourceProvider, DestinationProvider destinationProvider, IntegrationPointType integrationPointType)
		{
			var result = new ValidationResult();

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

			var validationModel = CreateValidationModel(model, sourceProvider, destinationProvider, integrationPointType);

			foreach (var validator in _validatorsMap[Constants.IntegrationPointProfiles.Validation.INTEGRATION_POINT_TYPE])
			{
				result.Add(validator.Validate(validationModel));
			}

			foreach (var validator in _validatorsMap[GetProviderValidatorKey(sourceProvider.Identifier, destinationProvider.Identifier)])
			{
				result.Add(validator.Validate(validationModel));
			}

			return result;
		}
	}
}