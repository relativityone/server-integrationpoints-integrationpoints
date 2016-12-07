using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator
{
	public class RelativityProviderConfigurationValidator : IValidator
	{
		private readonly ISerializer _serializer;
		private readonly IRelativityProviderValidatorsFactory _validatorsFactory;

		public string Key => IntegrationPointProviderValidator.GetProviderValidatorKey(IntegrationPoints.Domain.Constants.RELATIVITY_PROVIDER_GUID, Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID.ToString());

		public RelativityProviderConfigurationValidator(ISerializer serializer, IRelativityProviderValidatorsFactory validatorsFactory)
		{
			_serializer = serializer;
			_validatorsFactory = validatorsFactory;
		}

		public ValidationResult Validate(object value)
		{
			var integrationModel = value as IntegrationPointProviderValidationModel;

			var result = new ValidationResult();

			var sourceConfiguration = _serializer.Deserialize<SourceConfiguration>(integrationModel.SourceConfiguration);
			var destinationConfiguration = _serializer.Deserialize<ImportSettings>(integrationModel.DestinationConfiguration);

			var sourceWorkspaceValidator = _validatorsFactory.CreateWorkspaceValidator("Source");
			result.Add(sourceWorkspaceValidator.Validate(sourceConfiguration.SourceWorkspaceArtifactId));

			if (!result.IsValid)
			{
				return result;
			}

			var savedSearchValidator = _validatorsFactory.CreateSavedSearchValidator(sourceConfiguration.SourceWorkspaceArtifactId, sourceConfiguration.SavedSearchArtifactId);
			result.Add(savedSearchValidator.Validate(sourceConfiguration.SavedSearchArtifactId));

			var destinationWorkspaceValidator = _validatorsFactory.CreateWorkspaceValidator("Destination");
			result.Add(destinationWorkspaceValidator.Validate(sourceConfiguration.TargetWorkspaceArtifactId));

			var destinationFolderValidator = _validatorsFactory.CreateArtifactValidator(destinationConfiguration.CaseArtifactId, ArtifactTypeNames.Folder);
			result.Add(destinationFolderValidator.Validate(destinationConfiguration.DestinationFolderArtifactId));

			var fieldMappingValidator = _validatorsFactory.CreateFieldsMappingValidator();
			result.Add(fieldMappingValidator.Validate(integrationModel));

			var transferredObjectValidator = _validatorsFactory.CreateTransferredObjectValidator();
			result.Add(transferredObjectValidator.Validate(destinationConfiguration.ArtifactTypeId));

			return result;
		}
	}
}