using System;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Validation.RelativityProviderValidator
{
	[TestFixture]
	public class RelativityProviderConfigurationValidatorTests
	{
		private const int SavedSearchArtifactId = 1038052;
		private const int SourceWorkspaceArtifactId = 1074540;
		private const int TargetWorkspaceArtifactId = 1075642;

		private readonly string SourceConfiguration =
			"{\"SavedSearchArtifactId\":" + SavedSearchArtifactId + ",\"SourceWorkspaceArtifactId\":\"" + SourceWorkspaceArtifactId + "\",\"TargetWorkspaceArtifactId\":" + TargetWorkspaceArtifactId + ",\"FolderArtifactId\":\"1039185\",\"FolderArtifactName\":\"Test Folder\"}";

		private readonly string DestinationConfiguration =
			"{\"artifactTypeID\":10,\"destinationProviderType\":\"74A863B9-00EC-4BB7-9B3E-1E22323010C6\",\"CaseArtifactId\":1075642}";

		[Test]
		public void ItShouldValidateConfiguration()
		{
			// arrange
			var serializerMock = new JSONSerializer();
			var validatorsFactoryMock = Substitute.For<IRelativityProviderValidatorsFactory>();

			var workspaceManagerMock = Substitute.For<IWorkspaceManager>();
			var workspaceValidatorMock = Substitute.For<RelativityProviderWorkspaceValidator>(workspaceManagerMock, String.Empty);
			workspaceValidatorMock.Validate(Arg.Any<int>())
				.Returns(new ValidationResult());
			validatorsFactoryMock.CreateWorkspaceValidator(Arg.Any<string>())
				.Returns(workspaceValidatorMock);

			var savedSearchRepositoryMock = Substitute.For<ISavedSearchRepository>();
			var savedSearchValidatorMock = Substitute.For<SavedSearchValidator>(savedSearchRepositoryMock);
			savedSearchValidatorMock.Validate(Arg.Any<int>())
				.Returns(new ValidationResult());
			validatorsFactoryMock.CreateSavedSearchValidator(Arg.Any<int>(), Arg.Any<int>())
				.Returns(savedSearchValidatorMock);

			var artifactServiceMock = Substitute.For<IArtifactService>();
			var destinationFolderValidatorMock = Substitute.For<ArtifactValidator>(artifactServiceMock, Arg.Any<int>(), Arg.Any<string>());
			destinationFolderValidatorMock.Validate(Arg.Any<int>())
				.Returns(new ValidationResult());
			validatorsFactoryMock.CreateArtifactValidator(Arg.Any<int>(), Arg.Any<string>())
				.Returns(destinationFolderValidatorMock);

			var sourceFieldManager = Substitute.For<IFieldManager>();
			var targetFieldManager = Substitute.For<IFieldManager>();
			var fieldMappingValidatorMock = Substitute.For<FieldsMappingValidator>(serializerMock, sourceFieldManager, targetFieldManager);
			fieldMappingValidatorMock.Validate(Arg.Any<IntegrationPointProviderValidationModel>())
				.Returns(new ValidationResult());
			validatorsFactoryMock.CreateFieldsMappingValidator()
				.Returns(fieldMappingValidatorMock);

			var transferredObjectValidatorMock = Substitute.For<TransferredObjectValidator>();
			transferredObjectValidatorMock.Validate(Arg.Any<int>())
				.Returns(new ValidationResult());
			validatorsFactoryMock.CreateTransferredObjectValidator()
				.Returns(transferredObjectValidatorMock);

			var validator = new RelativityProviderConfigurationValidator(serializerMock, validatorsFactoryMock);

			var model = new IntegrationPointProviderValidationModel
			{
				FieldsMap = string.Empty,
				SourceProviderIdentifier = IntegrationPoints.Domain.Constants.RELATIVITY_PROVIDER_GUID,
				DestinationProviderIdentifier = Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID.ToString(),
				SourceConfiguration = SourceConfiguration,
				DestinationConfiguration = DestinationConfiguration
			};

			// act
			var actual = validator.Validate(model);

			// assert
			Assert.IsTrue(actual.IsValid);
			Assert.That(actual.Messages.Count(), Is.EqualTo(0));
		}
	}
}