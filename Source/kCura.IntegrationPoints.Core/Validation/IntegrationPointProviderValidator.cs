using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Validation
{
	public class IntegrationPointProviderValidator : BaseIntegrationPointValidator<IValidator>, IIntegrationPointProviderValidator
	{
		public IntegrationPointProviderValidator(IEnumerable<IValidator> validators, IIntegrationPointSerializer serializer, IServicesMgr servicesMgr)
			: base(validators, serializer, servicesMgr)
		{
		}

		public override ValidationResult Validate(
			IntegrationPointModelBase model,
			SourceProvider sourceProvider,
			DestinationProvider destinationProvider,
			IntegrationPointType integrationPointType,
			Guid objectTypeGuid,
			int userId)
		{
			var result = new ValidationResult();

			if (model.Scheduler.EnableScheduler)
			{
				foreach (IValidator validator in _validatorsMap[Constants.IntegrationPointProfiles.Validation.SCHEDULE])
				{
					result.Add(validator.Validate(model.Scheduler));
				}
			}

			foreach (IValidator validator in _validatorsMap[Constants.IntegrationPointProfiles.Validation.EMAIL])
			{
				result.Add(validator.Validate(model.NotificationEmails));
			}

			foreach (IValidator validator in _validatorsMap[Constants.IntegrationPointProfiles.Validation.NAME])
			{
				result.Add(validator.Validate(model.Name));
			}

			IntegrationPointProviderValidationModel validationModel = CreateValidationModel(model, sourceProvider, destinationProvider, integrationPointType, objectTypeGuid, userId);

			foreach (IValidator validator in _validatorsMap[Constants.IntegrationPointProfiles.Validation.INTEGRATION_POINT_TYPE])
			{
				result.Add(validator.Validate(validationModel));
			}

			foreach (IValidator validator in _validatorsMap[GetProviderValidatorKey(sourceProvider.Identifier, destinationProvider.Identifier)])
			{
				result.Add(validator.Validate(validationModel));
			}
			
			foreach (IValidator validator in _validatorsMap[GetTransferredObjectObjectTypeGuid(validationModel)])
            {
            	result.Add(validator.Validate(validationModel));
            }

			return result;
		}
	}
}