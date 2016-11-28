using System;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Validation.Implementation;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Validation
{
	[TestFixture]
	public class ProviderConfigurationValidatorTest
	{
		private IValidator _instance;
		private IRepositoryFactory _repositoryFactory;
		private IWorkspaceRepository _workspaceRepository;

		private const int SavedSearchArtifactId = 1038052;
		private const int SourceWorkspaceArtifactId = 1074540;
		private const int TargetWorkspaceArtifactId = 1075642;
		private readonly string SourceConfiguration =
			"{\"SavedSearchArtifactId\":" + SavedSearchArtifactId  + ",\"SourceWorkspaceArtifactId\":\"" + SourceWorkspaceArtifactId + "\",\"TargetWorkspaceArtifactId\":" + TargetWorkspaceArtifactId + ",\"FolderArtifactId\":\"1039185\",\"FolderArtifactName\":\"Test Folder\"}";
		private readonly string DestinationConfiguration =
			"{\"artifactTypeID\":10,\"destinationProviderType\":\"74A863B9-00EC-4BB7-9B3E-1E22323010C6\",\"CaseArtifactId\":1075642}";

		[SetUp]
		public void Setup()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_workspaceRepository = Substitute.For<IWorkspaceRepository>();
			_repositoryFactory.GetWorkspaceRepository().Returns(_workspaceRepository);
			_instance = new ProviderConfigurationValidator(new JSONSerializer(), _repositoryFactory);
		}

		[Test]
		public void Validate_Source_Workspace_Not_Exists()
		{
			// Arrange
			MockWorkspaceRepository(null, SourceWorkspaceArtifactId);
			IntegrationModelValidation integrationModelValidation = GetFieldMapValidationObject();

			// Act
			ValidationResult result = _instance.Validate(integrationModelValidation);

			// Assert
			Assert.IsFalse(result.IsValid);
			Assert.IsTrue(result.Messages.Contains(ProviderConfigurationValidator.ERROR_SOURCE_WORKSPACE_NOT_EXIST));
		}

		[Test]
		[TestCase(";")]
		[TestCase("Source;Workspace")]
		[TestCase("SourceWorkspace;")]
		public void Validate_Source_Workspace_Invalid_Name(string sourceWorkspaceName)
		{
			// Arrange
			WorkspaceDTO workspaceDto = new WorkspaceDTO() {Name = sourceWorkspaceName };
			MockWorkspaceRepository(workspaceDto, SourceWorkspaceArtifactId);
			IntegrationModelValidation integrationModelValidation = GetFieldMapValidationObject();

			// Act
			ValidationResult result = _instance.Validate(integrationModelValidation);

			// Assert
			Assert.IsFalse(result.IsValid);
			Assert.IsTrue(result.Messages.Contains(ProviderConfigurationValidator.ERROR_SOURCE_WORKSPACE_INVALID_NAME));
		}

		[Test]
		public void Validate_Saved_Search_Not_Exist()
		{
			// Arrange
			WorkspaceDTO workspaceDto = new WorkspaceDTO() { Name = "sourceWorkspaceName" };
			MockWorkspaceRepository(workspaceDto, SourceWorkspaceArtifactId);
			MockSavedSearchRepository(null);
			IntegrationModelValidation integrationModelValidation = GetFieldMapValidationObject();

			// Act
			ValidationResult result = _instance.Validate(integrationModelValidation);

			// Assert
			Assert.IsFalse(result.IsValid);
			Assert.IsTrue(result.Messages.Contains(ProviderConfigurationValidator.ERROR_SAVED_SEARCH_NOT_EXIST));
		}

		[Test]
		public void Validate_Destination_Workspace_Not_Exist()
		{
			// Arrange
			WorkspaceDTO sourceWorkspaceDto = new WorkspaceDTO() { Name = "sourceWorkspaceName" };
			MockWorkspaceRepository(sourceWorkspaceDto, SourceWorkspaceArtifactId);
			MockWorkspaceRepository(null, TargetWorkspaceArtifactId);
			//TODO MockDestinationWorkspaceRepository(null);
			IntegrationModelValidation integrationModelValidation = GetFieldMapValidationObject();

			// Act
			ValidationResult result = _instance.Validate(integrationModelValidation);

			// Assert
			Assert.IsFalse(result.IsValid);
			Assert.IsTrue(result.Messages.Contains(ProviderConfigurationValidator.ERROR_DESTINATION_WORKSPACE_NOT_EXIST));
		}

		[Test]
		[TestCase("Destination;Workspace")]
		[TestCase("DestinationWorkspace;")]
		[TestCase(";")]
		public void Validate_Destination_Workspace_Invalid_Name(string destinationWorkspaceName)
		{
			// Arrange
			WorkspaceDTO sourceWorkspaceDto = new WorkspaceDTO() { Name = "sourceWorkspaceName" };
			MockWorkspaceRepository(sourceWorkspaceDto, SourceWorkspaceArtifactId);
			WorkspaceDTO destinationWorkspaceDto = new WorkspaceDTO() { Name = destinationWorkspaceName};
			MockWorkspaceRepository(destinationWorkspaceDto, TargetWorkspaceArtifactId);
			//TODO MockDestinationWorkspaceRepository(destinationWorkspace);
			IntegrationModelValidation integrationModelValidation = GetFieldMapValidationObject();

			// Act
			ValidationResult result = _instance.Validate(integrationModelValidation);

			// Assert
			Assert.IsFalse(result.IsValid);
			Assert.IsTrue(result.Messages.Contains(ProviderConfigurationValidator.ERROR_DESTINATION_WORKSPACE_INVALID_NAME));
		}

		private void MockWorkspaceRepository(WorkspaceDTO workspaceDto, int workspaceArtifactId)
		{
			//TODO
			//IWorkspaceRepository workspaceRepository = Substitute.For<IWorkspaceRepository>();
			//_repositoryFactory.GetWorkspaceRepository().Returns(workspaceRepository);
			_workspaceRepository.Retrieve(workspaceArtifactId).Returns(workspaceDto);
		}

		//TODO Delete
		//private void MockDestinationWorkspaceRepository(DestinationWorkspaceDTO workspaceDto)
		//{
		//	IDestinationWorkspaceRepository workspaceRepository = Substitute.For<IDestinationWorkspaceRepository>();
		//	_repositoryFactory.GetDestinationWorkspaceRepository(SourceWorkspaceArtifactId).Returns(workspaceRepository);
		//	workspaceRepository.Query(TargetWorkspaceArtifactId).Returns(workspaceDto);
		//}

		private void MockSavedSearchRepository(SavedSearchDTO savedSearch)
		{
			ISavedSearchRepository savedSearchRepository = Substitute.For<ISavedSearchRepository>();
			_repositoryFactory.GetSavedSearchRepository(SourceWorkspaceArtifactId, SavedSearchArtifactId)
				.Returns(savedSearchRepository);
			savedSearchRepository.RetrieveSavedSearch().Returns(savedSearch);
		}

		private IntegrationModelValidation GetFieldMapValidationObject()
		{
			return new IntegrationModelValidation
			{
				FieldsMap = string.Empty,
				SourceProviderId = IntegrationPoints.Domain.Constants.RELATIVITY_PROVIDER_GUID,
				DestinationProviderId = Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID.ToString(),
				SourceConfiguration = SourceConfiguration,
				DestinationConfiguration = DestinationConfiguration
			};
		}
	}
}
