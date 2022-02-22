using System.Collections;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Validation.RelativityProviderValidator
{
	[TestFixture, Category("Unit")]
	public class RelativityProviderConfigurationValidatorTests
	{
		private const int _SAVED_SEARCH_ARTIFACT_ID = 1038052;
		private const int _VIEW_ARTIFACT_ID = 10235456;
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 1074540;
		private const int _TARGET_WORKSPACE_ARTIFACT_ID = 1075642;
	    private const int _PRODUCTION_ARTIFACT_ID = 4;
	    private const int _DESTINATION_WORKSPACE_ARTIFACT_ID = 4;

        private readonly string _sourceConfiguration =
			"{\"SavedSearchArtifactId\":" + _SAVED_SEARCH_ARTIFACT_ID + ",\"SourceWorkspaceArtifactId\":\"" + _SOURCE_WORKSPACE_ARTIFACT_ID + "\",\"TargetWorkspaceArtifactId\":" + _TARGET_WORKSPACE_ARTIFACT_ID + ",\"FolderArtifactId\":\"1039185\",\"FolderArtifactName\":\"Test Folder\",\"TypeOfExport\":\"3\"}";

	    private static IEnumerable ConfigurationTestsData()
	    {
	        yield return new TestCaseData(true, 0, "{\"artifactTypeID\":10,\"destinationProviderType\":\"74A863B9-00EC-4BB7-9B3E-1E22323010C6\",\"CaseArtifactId\":1075642, \"ProductionArtifactId\":\"" + _PRODUCTION_ARTIFACT_ID + "\"}");
	        yield return new TestCaseData(true, 0, "{\"artifactTypeID\":10,\"destinationProviderType\":\"74A863B9-00EC-4BB7-9B3E-1E22323010C6\",\"CaseArtifactId\":1075642, \"DestinationFolderArtifactId\":\"" + _DESTINATION_WORKSPACE_ARTIFACT_ID + "\"}");
            yield return new TestCaseData(false, 1, "{\"artifactTypeID\":10,\"destinationProviderType\":\"74A863B9-00EC-4BB7-9B3E-1E22323010C6\",\"CaseArtifactId\":1075642}");
        }

        [TestCaseSource(typeof(RelativityProviderConfigurationValidatorTests), nameof(ConfigurationTestsData))]
		public void ItShouldValidateConfiguration(bool expectedValidationResult, int numberOfErrorMessages, string destinationConfiguration)
		{
			// arrange
			var logger = Substitute.For<IAPILog>();
			var serializerMock = new JSONSerializer();
			var validatorsFactoryMock = Substitute.For<IRelativityProviderValidatorsFactory>();

			var workspaceManagerMock = Substitute.For<IWorkspaceManager>();
			var workspaceValidatorMock = Substitute.For<RelativityProviderWorkspaceNameValidator>(workspaceManagerMock, string.Empty);
			workspaceValidatorMock.Validate(Arg.Any<int>())
				.Returns(new ValidationResult());
			validatorsFactoryMock.CreateWorkspaceNameValidator(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string>())
				.Returns(workspaceValidatorMock);
			validatorsFactoryMock.CreateWorkspaceNameValidator(Arg.Any<string>())
				.Returns(workspaceValidatorMock);

			var savedSearchRepositoryMock = Substitute.For<ISavedSearchQueryRepository>();
			var savedSearchValidatorMock = Substitute.For<SavedSearchValidator>(logger, savedSearchRepositoryMock);
			savedSearchValidatorMock.Validate(Arg.Any<int>())
				.Returns(new ValidationResult());
			validatorsFactoryMock.CreateSavedSearchValidator(Arg.Any<int>())
				.Returns(savedSearchValidatorMock);

			var objectManagerMock = Substitute.For<IRelativityObjectManager>();
			var viewValidatorMock = Substitute.For<ViewValidator>(objectManagerMock, logger);
			viewValidatorMock.Validate(_VIEW_ARTIFACT_ID)
				.Returns(new ValidationResult());
			validatorsFactoryMock.CreateViewValidator(Arg.Any<int>())
				.Returns(viewValidatorMock);

			var artifactServiceMock = Substitute.For<IArtifactService>();
			var destinationFolderValidatorMock = Substitute.For<ArtifactValidator>(artifactServiceMock, Arg.Any<int>(), Arg.Any<string>());
			destinationFolderValidatorMock.Validate(Arg.Any<int>())
				.Returns(new ValidationResult());
			validatorsFactoryMock.CreateArtifactValidator(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string>())
				.Returns(destinationFolderValidatorMock);

			var sourceFieldManager = Substitute.For<IFieldManager>();
			var targetFieldManager = Substitute.For<IFieldManager>();
			
			var fieldMappingValidatorMock = Substitute.For<FieldsMappingValidator>(logger, serializerMock, sourceFieldManager, targetFieldManager);
			fieldMappingValidatorMock.Validate(Arg.Any<IntegrationPointProviderValidationModel>())
				.Returns(new ValidationResult());
			validatorsFactoryMock.CreateFieldsMappingValidator(Arg.Any<int?>(), Arg.Any<string>())
				.Returns(fieldMappingValidatorMock);

		    var productionManagerMock = Substitute.For<IProductionManager>();
			var permissionManager = Substitute.For<IPermissionManager>();
		    var importProductionValidatorMock = Substitute.For<ImportProductionValidator>(Arg.Any<int>(), productionManagerMock, permissionManager, Arg.Any<int?>(), Arg.Any<string>());
		    importProductionValidatorMock.Validate(Arg.Any<int>())
		        .Returns(new ValidationResult());
		    validatorsFactoryMock.CreateImportProductionValidator(Arg.Any<int>(), Arg.Any<int?>(), Arg.Any<string>())
		        .Returns(importProductionValidatorMock);

            var validator = new RelativityProviderConfigurationValidator(logger, serializerMock, validatorsFactoryMock);

			var model = new IntegrationPointProviderValidationModel
			{
				FieldsMap = string.Empty,
				SourceProviderIdentifier = Domain.Constants.RELATIVITY_PROVIDER_GUID,
				DestinationProviderIdentifier = Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID.ToString(),
				SourceConfiguration = _sourceConfiguration,
				DestinationConfiguration = destinationConfiguration
            };

			// act
			ValidationResult actual = validator.Validate(model);

			// assert
			Assert.That(actual.IsValid, Is.EqualTo(expectedValidationResult));
			Assert.That(actual.MessageTexts.Count(), Is.EqualTo(numberOfErrorMessages));
		}
    }
}