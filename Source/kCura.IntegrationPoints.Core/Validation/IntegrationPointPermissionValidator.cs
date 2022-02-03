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
	public class IntegrationPointPermissionValidator : BaseIntegrationPointValidator<IPermissionValidator>, IIntegrationPointPermissionValidator
	{
		public IntegrationPointPermissionValidator(IEnumerable<IPermissionValidator> validators, IIntegrationPointSerializer serializer, IServicesMgr servicesMgr)
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
			IntegrationPointProviderValidationModel validationModel = CreateValidationModel(model, sourceProvider, destinationProvider, integrationPointType, objectTypeGuid, userId);
			return Validate(validationModel, sourceProvider, destinationProvider, integrationPointType);
		}

		public ValidationResult ValidateSave(
			IntegrationPointModelBase model,
			SourceProvider sourceProvider,
			DestinationProvider destinationProvider,
			IntegrationPointType integrationPointType,
			Guid objectTypeGuid,
			int userId)
		{
			var result = new ValidationResult();

			IntegrationPointProviderValidationModel validationModel = CreateValidationModel(model, sourceProvider, destinationProvider, integrationPointType, objectTypeGuid, userId);

			foreach (var validator in _validatorsMap[Constants.IntegrationPoints.Validation.SAVE])
			{
				result.Add(validator.Validate(validationModel));
			}

			result.Add(Validate(validationModel, sourceProvider, destinationProvider, integrationPointType));

			return result;
		}

		public ValidationResult ValidateViewErrors(int workspaceArtifactId)
		{
			var result = new ValidationResult();

			foreach (var validator in _validatorsMap[Constants.IntegrationPoints.Validation.VIEW_ERRORS])
			{
				result.Add(validator.Validate(workspaceArtifactId));
			}

			return result;
		}

		public ValidationResult ValidateStop(
			IntegrationPointModelBase model,
			SourceProvider sourceProvider,
			DestinationProvider destinationProvider,
			IntegrationPointType integrationPointType,
			Guid objectTypeGuid,
			int userId)
		{
			var result = new ValidationResult();

			IntegrationPointProviderValidationModel validationModel = CreateValidationModel(model, sourceProvider, destinationProvider, integrationPointType, objectTypeGuid, userId);

			foreach (var validator in _validatorsMap[Constants.IntegrationPoints.Validation.STOP])
			{
				result.Add(validator.Validate(validationModel));
			}

			return result;
		}

		private ValidationResult Validate(
			IntegrationPointProviderValidationModel validationModel,
			SourceProvider sourceProvider,
			DestinationProvider destinationProvider,
			IntegrationPointType integrationPointType)
		{
			var result = new ValidationResult();

			foreach (IPermissionValidator validator in _validatorsMap[Constants.IntegrationPoints.Validation.INTEGRATION_POINT])
			{
				result.Add(validator.Validate(validationModel));
			}

			//workaround for import providers
			if (integrationPointType.Identifier.Equals(Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid.ToString()))
			{
				foreach (IPermissionValidator validator in _validatorsMap[Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid.ToString()])
				{
					result.Add(validator.Validate(validationModel));
				}
			}

			// provider-specific validation
			foreach (IPermissionValidator validator in _validatorsMap[GetProviderValidatorKey(sourceProvider.Identifier, destinationProvider.Identifier)])
			{
				result.Add(validator.Validate(validationModel));
			}

			foreach (IPermissionValidator validator in _validatorsMap[Constants.IntegrationPoints.Validation.NATIVE_COPY_LINKS_MODE])
			{
				result.Add(validator.Validate(validationModel));
			}

			return result;
		}

	}
}
