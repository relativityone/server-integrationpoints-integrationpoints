using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts.Interfaces;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Validation.RelativityProviderValidator
{
    [TestFixture, Category("Unit")]
    public class RelativityProviderPermissionValidatorTests : PermissionValidatorTestsBase
    {
        private IRelativityProviderValidatorsFactory _validatorsFactory;
        private IRelativityProviderDestinationWorkspaceExistenceValidator _destinationWorkspaceExistenceValidator;
        private IRelativityProviderDestinationWorkspacePermissionValidator _destinationWorkspacePermissionValidator;
        private IRelativityProviderSourceWorkspacePermissionValidator _sourceWorkspacePermissionValidator;
        private IRelativityProviderSourceProductionPermissionValidator _sourceWorkspaceSourceProductionPermissionValidator;
        private IRelativityProviderDestinationFolderPermissionValidator _destinationFolderPermissionValidator;
        private const int _FEDERATED_INSTANCE_ID = 5;
        private const int _SOURCE_PRODUCTION_ID = 7;
        private const int _DESTINATION_FOLDER_ID = 986454;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _validatorsFactory = Substitute.For<IRelativityProviderValidatorsFactory>();

            _destinationWorkspaceExistenceValidator = Substitute.For<IRelativityProviderDestinationWorkspaceExistenceValidator>();
            _destinationWorkspacePermissionValidator = Substitute.For<IRelativityProviderDestinationWorkspacePermissionValidator>();
            _sourceWorkspacePermissionValidator = Substitute.For<IRelativityProviderSourceWorkspacePermissionValidator>();
            _sourceWorkspaceSourceProductionPermissionValidator = Substitute.For<IRelativityProviderSourceProductionPermissionValidator>();
            _destinationFolderPermissionValidator = Substitute.For<IRelativityProviderDestinationFolderPermissionValidator>();

            _validatorsFactory.CreateDestinationWorkspaceExistenceValidator(Arg.Any<int?>(), Arg.Any<string>())
                .Returns(_destinationWorkspaceExistenceValidator);

            _validatorsFactory.CreateDestinationWorkspacePermissionValidator(Arg.Any<int?>(), Arg.Any<string>())
                .Returns(_destinationWorkspacePermissionValidator);

            _validatorsFactory.CreateSourceWorkspacePermissionValidator().Returns(_sourceWorkspacePermissionValidator);
            _validatorsFactory.CreateSourceProductionPermissionValidator(Arg.Any<int>())
                .Returns(_sourceWorkspaceSourceProductionPermissionValidator);

            _validatorsFactory.CreateDestinationFolderPermissionValidator(Arg.Any<int>(), Arg.Any<int?>(), Arg.Any<string>()).Returns(_destinationFolderPermissionValidator);

            _serializer.Deserialize<ImportSettings>(_validationModel.DestinationConfiguration).Returns(
                new ImportSettings
                {
                    FederatedInstanceArtifactId = null
                });
        }

        [Test]
        public void ItShouldValidateDestinationWorkspacePermissions_WhenDestinationWorkspaceExists()
        {
            // arrange
            _serializer.Deserialize<SourceConfiguration>(_validationModel.SourceConfiguration)
                .Returns(new SourceConfiguration
                {
                    SavedSearchArtifactId = _SAVED_SEARCH_ID,
                    SourceWorkspaceArtifactId = _SOURCE_WORKSPACE_ID,
                    TargetWorkspaceArtifactId = _DESTINATION_WORKSPACE_ID,
                    FederatedInstanceArtifactId = _FEDERATED_INSTANCE_ID,
                });

            var relativityProviderPermissionValidator = new RelativityProviderPermissionValidator(_serializer, ServiceContextHelper, _validatorsFactory);

            // act
            relativityProviderPermissionValidator.Validate(_validationModel);

            // assert
            _destinationWorkspacePermissionValidator.Received().Validate(_DESTINATION_WORKSPACE_ID, _DESTINATION_ARTIFACT_TYPE_ID, Arg.Any<bool>());
        }

        [Test]
        public void ItShouldNotValidate_WhenFederatedInstanceIsSetUp()
        {
            // arrange
            _serializer.Deserialize<SourceConfiguration>(_validationModel.SourceConfiguration)
                .Returns(new SourceConfiguration
                {
                    SavedSearchArtifactId = _SAVED_SEARCH_ID,
                    SourceWorkspaceArtifactId = _SOURCE_WORKSPACE_ID,
                    TargetWorkspaceArtifactId = _DESTINATION_WORKSPACE_ID,
                    FederatedInstanceArtifactId = _FEDERATED_INSTANCE_ID,
                });
            _serializer.Deserialize<ImportSettings>(_validationModel.DestinationConfiguration).Returns(
                new ImportSettings
                {
                    FederatedInstanceArtifactId = _FEDERATED_INSTANCE_ID
                });

            var relativityProviderPermissionValidator = new RelativityProviderPermissionValidator(_serializer, ServiceContextHelper, _validatorsFactory);

            // act
            ValidationResult validationResult = relativityProviderPermissionValidator.Validate(_validationModel);

            // assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Messages.First().ShortMessage.Should().Be(
                "Federated instance transfers are currently not supported. Please update the Integration Point job to use a destination workspace within the same instance.");
            validationResult.Messages.First().ErrorCode.Should().Be("20.014");
        }

        [Test]
        public void ItShouldNotValidateDestinationWorkspacePermissions_WhenDestinationWorkspaceDoesNotExist()
        {
            // arrange
            _serializer.Deserialize<SourceConfiguration>(_validationModel.SourceConfiguration)
                .Returns(new SourceConfiguration
                {
                    SavedSearchArtifactId = _SAVED_SEARCH_ID,
                    SourceWorkspaceArtifactId = _SOURCE_WORKSPACE_ID,
                    TargetWorkspaceArtifactId = _DESTINATION_WORKSPACE_ID,
                    FederatedInstanceArtifactId = _FEDERATED_INSTANCE_ID
                });

            _destinationWorkspaceExistenceValidator
                .Validate(Arg.Is<SourceConfiguration>(sc => sc.TargetWorkspaceArtifactId == _DESTINATION_WORKSPACE_ID))
                .Returns(CreateValidationMessage(false));

            var relativityProviderPermissionValidator = new RelativityProviderPermissionValidator(_serializer, ServiceContextHelper, _validatorsFactory);

            // act
            relativityProviderPermissionValidator.Validate(_validationModel);

            // assert
            _destinationWorkspacePermissionValidator.DidNotReceiveWithAnyArgs().Validate(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<bool>());
        }

        [Test]
        public void ItShouldValidateSourceProductionPermissions()
        {
            // arrange
            _serializer.Deserialize<SourceConfiguration>(_validationModel.SourceConfiguration)
                .Returns(new SourceConfiguration
                {
                    SavedSearchArtifactId = _SAVED_SEARCH_ID,
                    SourceWorkspaceArtifactId = _SOURCE_WORKSPACE_ID,
                    TargetWorkspaceArtifactId = _DESTINATION_WORKSPACE_ID,
                    SourceProductionId = _SOURCE_PRODUCTION_ID
                });

            var relativityProviderPermissionValidator = new RelativityProviderPermissionValidator(_serializer, ServiceContextHelper, _validatorsFactory);

            // act
            relativityProviderPermissionValidator.Validate(_validationModel);

            // assert
            _sourceWorkspaceSourceProductionPermissionValidator.Received().Validate(_SOURCE_WORKSPACE_ID, _SOURCE_PRODUCTION_ID);
        }

        [Test]
        public void ItShouldValidateDestinationFolderPermission_WhenDestinationWorkspaceIsValid_AndDestinationFolderIsSet()
        {
            // arrange
            const bool useFolderPath = true;
            const bool moveExistingDocuments = false;

            var validValidationResult = new ValidationResult(true);
            _destinationWorkspaceExistenceValidator.Validate(Arg.Any<SourceConfiguration>()).Returns(validValidationResult);
            _destinationWorkspacePermissionValidator.Validate(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<bool>()).Returns(validValidationResult);

            _serializer.Deserialize<DestinationConfigurationPermissionValidationModel>(_validationModel.DestinationConfiguration)
                .Returns(new DestinationConfigurationPermissionValidationModel
                {
                    ArtifactTypeId = _ARTIFACT_TYPE_ID,
                    DestinationFolderArtifactId = _DESTINATION_FOLDER_ID,
                    MoveExistingDocuments = moveExistingDocuments,
                    UseFolderPathInformation = useFolderPath
                });

            var relativityProviderPermissionValidator = new RelativityProviderPermissionValidator(_serializer, ServiceContextHelper, _validatorsFactory);

            // act
            relativityProviderPermissionValidator.Validate(_validationModel);

            // assert
            _destinationFolderPermissionValidator.Received().Validate(_DESTINATION_FOLDER_ID, useFolderPath, moveExistingDocuments);
        }

        [Test]
        public void ItShouldNotValidateDestinationFolderPermission_WhenDestinationWorkspaceIsValid_AndDestinationFolderNotSet()
        {
            // arrange
            bool useFolderPath = true;
            bool moveExistingDocuments = false;

            var validValidationResult = new ValidationResult(true);
            _destinationWorkspaceExistenceValidator.Validate(Arg.Any<SourceConfiguration>()).Returns(validValidationResult);
            _destinationWorkspacePermissionValidator.Validate(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<bool>()).Returns(validValidationResult);

            _serializer.Deserialize<DestinationConfigurationPermissionValidationModel>(_validationModel.DestinationConfiguration)
                .Returns(new DestinationConfigurationPermissionValidationModel
                {
                    ArtifactTypeId = _ARTIFACT_TYPE_ID,
                    MoveExistingDocuments = moveExistingDocuments,
                    UseFolderPathInformation = useFolderPath
                });

            var relativityProviderPermissionValidator = new RelativityProviderPermissionValidator(_serializer, ServiceContextHelper, _validatorsFactory);

            // act
            relativityProviderPermissionValidator.Validate(_validationModel);

            // assert
            _destinationFolderPermissionValidator.DidNotReceiveWithAnyArgs().Validate(Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<bool>());
        }

        [Test]
        public void ItShouldNotValidateDestinationFolderPermission_WhenDestinationWorkspaceIsInvalid()
        {
            // arrange
            bool useFolderPath = true;
            bool moveExistingDocuments = false;

            var validValidationResult = new ValidationResult(true);
            var invalidValidationResult = new ValidationResult(false);
            _destinationWorkspaceExistenceValidator.Validate(Arg.Any<SourceConfiguration>()).Returns(validValidationResult);
            _destinationWorkspacePermissionValidator.Validate(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<bool>()).Returns(invalidValidationResult);

            _serializer.Deserialize<DestinationConfigurationPermissionValidationModel>(_validationModel.DestinationConfiguration)
                .Returns(new DestinationConfigurationPermissionValidationModel
                {
                    ArtifactTypeId = _ARTIFACT_TYPE_ID,
                    DestinationFolderArtifactId = _DESTINATION_FOLDER_ID,
                    MoveExistingDocuments = moveExistingDocuments,
                    UseFolderPathInformation = useFolderPath
                });

            var relativityProviderPermissionValidator = new RelativityProviderPermissionValidator(_serializer, ServiceContextHelper, _validatorsFactory);

            // act
            relativityProviderPermissionValidator.Validate(_validationModel);

            // assert
            _destinationFolderPermissionValidator.DidNotReceiveWithAnyArgs().Validate(Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<bool>());
        }

        private static ValidationResult CreateValidationMessage(bool success)
        {
            var failedValidationResult = new ValidationResult();
            if (!success)
            {
                failedValidationResult.Add(new ValidationMessage("Failed"));
            }
            return failedValidationResult;
        }
    }
}
