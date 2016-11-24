using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Validation.Implementation;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator
{
	public class ProviderConfigurationValidator : IValidator
	{
		private readonly ISerializer _serializer;
		private readonly IRepositoryFactory _repositoryFactory;

		public string Key => IntegrationModelValidator.GetProviderValidatorKey(IntegrationPoints.Domain.Constants.RELATIVITY_PROVIDER_GUID, Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID.ToString());

		public const string ERROR_INTEGRATION_MODEL_VALIDATION_NOT_INITIALIZED = "Integration model validation object not initialized";
		public const string ERROR_DESTINATION_WORKSPACE_NOT_EXIST = "Destination workspace does not exist.";
		public const string ERROR_SOURCE_WORKSPACE_NOT_EXIST = "Source workspace does not exist.";
		public const string ERROR_SAVED_SEARCH_NOT_EXIST = "Saved Search does not exist.";
		public const string ERROR_DESTINATION_WORKSPACE_INVALID_NAME = "Destination workspace name contains an invalid character.";
		public const string ERROR_SOURCE_WORKSPACE_INVALID_NAME = "Source workspace name contains an invalid character.";

		public ProviderConfigurationValidator(ISerializer serializer, IRepositoryFactory repositoryFactory)
		{
			_serializer = serializer;
			_repositoryFactory = repositoryFactory;
		}

		public ValidationResult Validate(object value)
		{
			var integrationModel = value as IntegrationModelValidation;
			if (integrationModel == null) { throw new Exception(ERROR_INTEGRATION_MODEL_VALIDATION_NOT_INITIALIZED); }
			var result = new ValidationResult();

			var sourceConfiguration = _serializer.Deserialize<SourceConfiguration>(integrationModel.SourceConfiguration);
			var destinationConfiguration = _serializer.Deserialize<ImportSettings>(integrationModel.DestinationConfiguration);

			result.Add(ValidateSourceWorkspace(sourceConfiguration.SourceWorkspaceArtifactId));

			if (!result.IsValid) { return result; }

			result.Add(ValidateSavedSearchExists(sourceConfiguration.SourceWorkspaceArtifactId, sourceConfiguration.SavedSearchArtifactId));
			result.Add(ValidateDestinationWorkspace(sourceConfiguration.SourceWorkspaceArtifactId, sourceConfiguration.TargetWorkspaceArtifactId));
			result.Add(ValidateDestinationFolderExists(destinationConfiguration.CaseArtifactId, destinationConfiguration.DestinationFolderArtifactId));

			return result;
		}

		private ValidationResult ValidateSourceWorkspace(int sourceWorkspaceArtifactId)
		{
			var result = new ValidationResult();

			IWorkspaceRepository workspaceRepository = _repositoryFactory.GetWorkspaceRepository();
			WorkspaceDTO workspaceDto = workspaceRepository.Retrieve(sourceWorkspaceArtifactId);

			if (workspaceDto == null)
			{
				result.Add(ERROR_SOURCE_WORKSPACE_NOT_EXIST);
			}
			else if (workspaceDto.Name.Contains(";"))
			{
				result.Add(ERROR_SOURCE_WORKSPACE_INVALID_NAME);
			}

			return result;
		}

		private ValidationResult ValidateDestinationWorkspace(int sourceWorkspaceArtifactId, int targetWorkspaceArtifactId)
		{
			var result = new ValidationResult();

			IDestinationWorkspaceRepository destinationWorkspaceRepository =
				_repositoryFactory.GetDestinationWorkspaceRepository(sourceWorkspaceArtifactId); ;
			DestinationWorkspaceDTO destinationWorkspace = destinationWorkspaceRepository.Query(targetWorkspaceArtifactId);

			if (destinationWorkspace == null)
			{
				result.Add(ERROR_DESTINATION_WORKSPACE_NOT_EXIST);
			}
			else if (destinationWorkspace.WorkspaceName.Contains(";"))
			{
				result.Add(ERROR_DESTINATION_WORKSPACE_INVALID_NAME);
			}

			return result;
		}

		private ValidationResult ValidateSavedSearchExists(int workspaceArtifactId, int savedSearchArtifactId)
		{
			var result = new ValidationResult();

			ISavedSearchRepository savedSearchRepository = _repositoryFactory.GetSavedSearchRepository(workspaceArtifactId, savedSearchArtifactId);
			SavedSearchDTO savedSearch = savedSearchRepository.RetrieveSavedSearch();

			if (savedSearch == null)
			{
				result.Add(ERROR_SAVED_SEARCH_NOT_EXIST);
			}

			return result;
		}

		private ValidationResult ValidateDestinationFolderExists(int destinationWorkspaceArtifactId, int destinationFolderArtifactId)
		{
			var result = new ValidationResult();

			//TODO Implement this!

			return result;
		}
	}
}
