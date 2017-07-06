using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client.DTOs;
using kCura.WinEDDS;
using Relativity;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator
{
	public class RelativityProviderConfigurationValidator : IValidator
	{
		private readonly ISerializer _serializer;
		private readonly IRelativityProviderValidatorsFactory _validatorsFactory;

		public RelativityProviderConfigurationValidator(ISerializer serializer, IRelativityProviderValidatorsFactory validatorsFactory)
		{
			_serializer = serializer;
			_validatorsFactory = validatorsFactory;
		}

		public string Key => IntegrationPointProviderValidator.GetProviderValidatorKey(IntegrationPoints.Domain.Constants.RELATIVITY_PROVIDER_GUID, Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID.ToString());

		public ValidationResult Validate(object value)
		{
			var integrationModel = value as IntegrationPointProviderValidationModel;

			var result = new ValidationResult();

			var sourceConfiguration = _serializer.Deserialize<SourceConfiguration>(integrationModel.SourceConfiguration);
			var destinationConfiguration = _serializer.Deserialize<ImportSettings>(integrationModel.DestinationConfiguration);

			var sourceWorkspaceValidator = _validatorsFactory.CreateWorkspaceValidator("Source");
			result.Add(sourceWorkspaceValidator.Validate(sourceConfiguration.SourceWorkspaceArtifactId));

			if (!result.IsValid)
				return result;

			if (sourceConfiguration.TypeOfExport == SourceConfiguration.ExportType.SavedSearch)
			{
				var savedSearchValidator =
					_validatorsFactory.CreateSavedSearchValidator(sourceConfiguration.SourceWorkspaceArtifactId,
						sourceConfiguration.SavedSearchArtifactId);
				result.Add(savedSearchValidator.Validate(sourceConfiguration.SavedSearchArtifactId));
			}
			else
			{
				var productionValidator =
					_validatorsFactory.CreateProductionValidator(sourceConfiguration.SourceWorkspaceArtifactId);
				result.Add(productionValidator.Validate(sourceConfiguration.SourceProductionId));
			}
			
			var destinationWorkspaceValidator = _validatorsFactory.CreateWorkspaceValidator("Destination", sourceConfiguration.FederatedInstanceArtifactId,
				integrationModel.SecuredConfiguration);
			result.Add(destinationWorkspaceValidator.Validate(sourceConfiguration.TargetWorkspaceArtifactId));

		    if (destinationConfiguration.DestinationFolderArtifactId > 0 && destinationConfiguration.ProductionArtifactId == 0)
		    {
		        ArtifactValidator destinationFolderValidator =
		            _validatorsFactory.CreateArtifactValidator(destinationConfiguration.CaseArtifactId,
		                ArtifactTypeNames.Folder,
		                sourceConfiguration.FederatedInstanceArtifactId, 
                        integrationModel.SecuredConfiguration);
		        result.Add(destinationFolderValidator.Validate(destinationConfiguration.DestinationFolderArtifactId));
		    }
            else if (destinationConfiguration.DestinationFolderArtifactId == 0 && destinationConfiguration.ProductionArtifactId > 0)
		    {
                ImportProductionValidator importProductionValidator =
                    _validatorsFactory.CreateImportProductionValidator(sourceConfiguration.TargetWorkspaceArtifactId, 
                    destinationConfiguration.FederatedInstanceArtifactId, 
                    integrationModel.SecuredConfiguration);
                result.Add(importProductionValidator.Validate(destinationConfiguration.ProductionArtifactId));
            }
            else if (destinationConfiguration.DestinationFolderArtifactId == 0 && destinationConfiguration.ProductionArtifactId == 0)
		    {
               result.Add(IntegrationPointProviderValidationMessages.ERROR_DESTINATON_LOCATION_EMPTY);
		    }

		    var fieldMappingValidator = _validatorsFactory.CreateFieldsMappingValidator(sourceConfiguration.FederatedInstanceArtifactId, integrationModel.SecuredConfiguration);
			result.Add(fieldMappingValidator.Validate(integrationModel));

			var transferredObjectValidator = _validatorsFactory.CreateTransferredObjectValidator();
			result.Add(transferredObjectValidator.Validate(destinationConfiguration.ArtifactTypeId));

			return result;
		}
	}
}