﻿using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts.Interfaces;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts
{
	public class RelativityProviderPermissionValidator : BasePermissionValidator
	{
		private readonly IRelativityProviderValidatorsFactory _validatorsFactory;

		public RelativityProviderPermissionValidator(ISerializer serializer, IServiceContextHelper contextHelper, IRelativityProviderValidatorsFactory validatorsFactory)
			: base(serializer, contextHelper)
		{
			_validatorsFactory = validatorsFactory;
		}

		public override string Key
			=>
			IntegrationPointPermissionValidator.GetProviderValidatorKey(Domain.Constants.RELATIVITY_PROVIDER_GUID,
				Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID.ToString());

		public override ValidationResult Validate(IntegrationPointProviderValidationModel model)
		{
			var result = new ValidationResult();
            result.Add(ValidateFederatedInstanceConnection(model));
		    if (result.IsValid)
		    {
		        result.Add(ValidateSourceWorkspacePermission());
		        result.Add(ValidateDestinationWorkspacePermission(model));
		        result.Add(ValidateSourceWorkspaceProductionPermission(model));
		    }
		    return result;
		}

	    private ValidationResult ValidateFederatedInstanceConnection(IntegrationPointProviderValidationModel model)
	    {
	        var result = new ValidationResult();

            SourceConfiguration sourceConfiguration = Serializer.Deserialize<SourceConfiguration>(model.SourceConfiguration);
	        if (sourceConfiguration.FederatedInstanceArtifactId.HasValue)
	        {
	            IFederatedInstanceConnectionValidator validator = _validatorsFactory.CreateFederatedInstanceConnectionValidator(
	                sourceConfiguration.FederatedInstanceArtifactId, model.SecuredConfiguration);
                result.Add(validator.Validate());
	        }
	        return result;
	    }

	    private ValidationResult ValidateSourceWorkspacePermission()
		{
			IRelativityProviderSourceWorkspacePermissionValidator sourceWorkspacePermissionValidator = _validatorsFactory.CreateSourceWorkspacePermissionValidator();
			return sourceWorkspacePermissionValidator.Validate(ContextHelper.WorkspaceID);
		}

		private ValidationResult ValidateDestinationWorkspacePermission(IntegrationPointProviderValidationModel model)
		{
		    var result = new ValidationResult();

            SourceConfiguration sourceConfiguration = Serializer.Deserialize<SourceConfiguration>(model.SourceConfiguration);
		    DestinationConfigurationPermissionValidationModel destinationConfiguration = Serializer.Deserialize<DestinationConfigurationPermissionValidationModel>(model.DestinationConfiguration);


		    IRelativityProviderDestinationWorkspaceExistenceValidator destinationWorkspaceExistenceValidator = _validatorsFactory.CreateDestinationWorkspaceExistenceValidator(
				sourceConfiguration.FederatedInstanceArtifactId, model.SecuredConfiguration);
			result.Add(destinationWorkspaceExistenceValidator.Validate(sourceConfiguration));
			if (!result.IsValid)
			{
				return result; // destination workspace doesnt exist
			}

			IRelativityProviderDestinationWorkspacePermissionValidator destinationWorkspacePermissionValidator = _validatorsFactory.CreateDestinationWorkspacePermissionValidator(
				sourceConfiguration.FederatedInstanceArtifactId, model.SecuredConfiguration);
			result.Add(destinationWorkspacePermissionValidator.Validate(sourceConfiguration.TargetWorkspaceArtifactId, destinationConfiguration.ArtifactTypeId, model.CreateSavedSearch));
			if (!result.IsValid)
			{
				return result; // no permission to destination workspace
			}

			result.Add(ValidateDestinationFolderPermissions(model, sourceConfiguration, destinationConfiguration));

			return result;
		}

		private ValidationResult ValidateDestinationFolderPermissions(IntegrationPointProviderValidationModel model, SourceConfiguration sourceConfiguration, DestinationConfigurationPermissionValidationModel destinationConfiguration)
		{
			ValidationResult result = new ValidationResult();
			if (destinationConfiguration.DestinationFolderArtifactId > 0)
			{
				IRelativityProviderDestinationFolderPermissionValidator destinationFolderPermissionValidator =
					_validatorsFactory.CreateDestinationFolderPermissionValidator(destinationConfiguration.CaseArtifactId, sourceConfiguration.FederatedInstanceArtifactId, model.SecuredConfiguration);
				result.Add(destinationFolderPermissionValidator.Validate(destinationConfiguration.DestinationFolderArtifactId, destinationConfiguration.UseFolderPath, destinationConfiguration.MoveExistingDocuments));
			}

			return result;
		}

		private ValidationResult ValidateSourceWorkspaceProductionPermission(IntegrationPointProviderValidationModel model)
		{
			SourceConfiguration sourceConfiguration = Serializer.Deserialize<SourceConfiguration>(model.SourceConfiguration);

			var result = new ValidationResult();
			if (sourceConfiguration.SourceProductionId > 0)
			{
				var validator = _validatorsFactory.CreateSourceProductionPermissionValidator(sourceConfiguration.SourceWorkspaceArtifactId);
				return validator.Validate(sourceConfiguration.SourceWorkspaceArtifactId, sourceConfiguration.SourceProductionId);
			}
			return result;
		}
	}
}