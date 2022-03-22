using System.Collections;
using System.Linq;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Common.Toggles;
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
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Core.Tests.Validation.RelativityProviderValidator
{
    [TestFixture, Category("Unit")]
    public class RelativityProviderConfigurationValidatorTests
    {
        private ArtifactValidator _destinationFolderValidatorMock;
        private FieldsMappingValidator _fieldMappingValidatorMock;

        private IAPILog _logger;
        private IArtifactService _artifactServiceMock;
        private IFieldManager _sourceFieldManager;
        private IFieldManager _targetFieldManager;
        private ImportProductionValidator _importProductionValidatorMock;
        private IPermissionManager _permissionManager;
        private IProductionManager _productionManagerMock;
        private IRelativityObjectManager _objectManagerMock;
        private IRelativityProviderValidatorsFactory _validatorsFactoryMock;
        private ISavedSearchQueryRepository _savedSearchRepositoryMock;
        private IToggleProvider _toggleProvider;
        private IWorkspaceManager _workspaceManagerMock;
        private JSONSerializer _serializerMock;
        private RelativityProviderWorkspaceNameValidator _workspaceValidatorMock;
        private SavedSearchValidator _savedSearchValidatorMock;
        private ViewValidator _viewValidatorMock;
        private const int _DESTINATION_WORKSPACE_ARTIFACT_ID = 4;
        private const int _PRODUCTION_ARTIFACT_ID = 4;
        private const int _SAVED_SEARCH_ARTIFACT_ID = 1038052;
        private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 1074540;
        private const int _TARGET_WORKSPACE_ARTIFACT_ID = 1075642;
        private const int _VIEW_ARTIFACT_ID = 10235456;

        private readonly string _sourceConfiguration =
            "{\"SavedSearchArtifactId\":" + _SAVED_SEARCH_ARTIFACT_ID + ",\"SourceWorkspaceArtifactId\":\"" + _SOURCE_WORKSPACE_ARTIFACT_ID + "\",\"TargetWorkspaceArtifactId\":" + _TARGET_WORKSPACE_ARTIFACT_ID + ",\"FolderArtifactId\":\"1039185\",\"FolderArtifactName\":\"Test Folder\",\"TypeOfExport\":\"3\"}";

        private static IEnumerable ConfigurationTestsData()
        {
            yield return new TestCaseData(true, 0, "{\"artifactTypeID\":10,\"destinationProviderType\":\"74A863B9-00EC-4BB7-9B3E-1E22323010C6\",\"CaseArtifactId\":1075642, \"ProductionArtifactId\":\"" + _PRODUCTION_ARTIFACT_ID + "\"}");
            yield return new TestCaseData(true, 0, "{\"artifactTypeID\":10,\"destinationProviderType\":\"74A863B9-00EC-4BB7-9B3E-1E22323010C6\",\"CaseArtifactId\":1075642, \"DestinationFolderArtifactId\":\"" + _DESTINATION_WORKSPACE_ARTIFACT_ID + "\"}");
            yield return new TestCaseData(false, 1, "{\"artifactTypeID\":10,\"destinationProviderType\":\"74A863B9-00EC-4BB7-9B3E-1E22323010C6\",\"CaseArtifactId\":1075642}");
        }

        [SetUp]
        public void SetUp()
        {
            _logger = Substitute.For<IAPILog>();
            _serializerMock = new JSONSerializer();
            _toggleProvider = Substitute.For<IToggleProvider>();
            _validatorsFactoryMock = Substitute.For<IRelativityProviderValidatorsFactory>();
            _workspaceManagerMock = Substitute.For<IWorkspaceManager>();
            _workspaceValidatorMock = Substitute.For<RelativityProviderWorkspaceNameValidator>(_workspaceManagerMock, string.Empty);
            _savedSearchRepositoryMock = Substitute.For<ISavedSearchQueryRepository>();
            _savedSearchValidatorMock = Substitute.For<SavedSearchValidator>(_logger, _savedSearchRepositoryMock);
            _objectManagerMock = Substitute.For<IRelativityObjectManager>();
            _viewValidatorMock = Substitute.For<ViewValidator>(_objectManagerMock, _logger);
            _artifactServiceMock = Substitute.For<IArtifactService>();
            _destinationFolderValidatorMock = Substitute.For<ArtifactValidator>(_artifactServiceMock, Arg.Any<int>(), Arg.Any<string>());
            _sourceFieldManager = Substitute.For<IFieldManager>();
            _targetFieldManager = Substitute.For<IFieldManager>();
            _fieldMappingValidatorMock = Substitute.For<FieldsMappingValidator>(_logger, _serializerMock, _sourceFieldManager, _targetFieldManager);
            _productionManagerMock = Substitute.For<IProductionManager>();
            _permissionManager = Substitute.For<IPermissionManager>();
            _importProductionValidatorMock = Substitute.For<ImportProductionValidator>(Arg.Any<int>(), _productionManagerMock, _permissionManager, Arg.Any<int?>(), Arg.Any<string>());
        }

        [TestCaseSource(typeof(RelativityProviderConfigurationValidatorTests), nameof(ConfigurationTestsData))]
        public void ItShouldValidateConfiguration(bool expectedValidationResult, int numberOfErrorMessages, string destinationConfiguration)
        {
            // arrange            
            _workspaceValidatorMock.Validate(Arg.Any<int>())
                .Returns(new ValidationResult());
            _validatorsFactoryMock.CreateWorkspaceNameValidator(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string>())
                .Returns(_workspaceValidatorMock);
            _validatorsFactoryMock.CreateWorkspaceNameValidator(Arg.Any<string>())
                .Returns(_workspaceValidatorMock);

            _savedSearchValidatorMock.Validate(Arg.Any<int>())
                .Returns(new ValidationResult());
            _validatorsFactoryMock.CreateSavedSearchValidator(Arg.Any<int>())
                .Returns(_savedSearchValidatorMock);

            _viewValidatorMock.Validate(_VIEW_ARTIFACT_ID)
                .Returns(new ValidationResult());
            _validatorsFactoryMock.CreateViewValidator(Arg.Any<int>())
                .Returns(_viewValidatorMock);

            _destinationFolderValidatorMock.Validate(Arg.Any<int>())
                .Returns(new ValidationResult());
            _validatorsFactoryMock.CreateArtifactValidator(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string>())
                .Returns(_destinationFolderValidatorMock);

            _fieldMappingValidatorMock.Validate(Arg.Any<IntegrationPointProviderValidationModel>())
                .Returns(new ValidationResult());
            _validatorsFactoryMock.CreateFieldsMappingValidator(Arg.Any<int?>(), Arg.Any<string>())
                .Returns(_fieldMappingValidatorMock);

            _importProductionValidatorMock.Validate(Arg.Any<int>())
                .Returns(new ValidationResult());
            _validatorsFactoryMock.CreateImportProductionValidator(Arg.Any<int>(), Arg.Any<int?>(), Arg.Any<string>())
                .Returns(_importProductionValidatorMock);

            RelativityProviderConfigurationValidator sut = new RelativityProviderConfigurationValidator(_logger, _serializerMock, _validatorsFactoryMock, _toggleProvider);

            IntegrationPointProviderValidationModel model = new IntegrationPointProviderValidationModel
            {
                FieldsMap = string.Empty,
                SourceProviderIdentifier = Domain.Constants.RELATIVITY_PROVIDER_GUID,
                DestinationProviderIdentifier = Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID.ToString(),
                SourceConfiguration = _sourceConfiguration,
                DestinationConfiguration = destinationConfiguration
            };

            // act
            ValidationResult actual = sut.Validate(model);

            // assert
            Assert.That(actual.IsValid, Is.EqualTo(expectedValidationResult));
            Assert.That(actual.MessageTexts.Count(), Is.EqualTo(numberOfErrorMessages));
        }
    }
}