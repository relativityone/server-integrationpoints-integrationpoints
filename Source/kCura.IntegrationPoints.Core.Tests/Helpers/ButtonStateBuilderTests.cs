using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Checkers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Statistics;
using kCura.IntegrationPoints.Domain.Models;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using static kCura.IntegrationPoints.Core.Contracts.Configuration.SourceConfiguration;

namespace kCura.IntegrationPoints.Core.Tests.Helpers
{
    [TestFixture]
    [Category("Unit")]
    public class ButtonStateBuilderTests : TestBase
    {
        private const int _WORKSPACE_ID = 100;
        private const int _INTEGRATION_POINT_ID = 200;
        private IViewErrorsPermissionValidator _permissionValidator;
        private IIntegrationPointService _integrationPointService;
        private IPermissionRepository _permissionRepository;
        private IJobHistoryRepository _jobHistoryRepository;
        private IProviderTypeService _providerTypeService;
        private IStateManager _stateManager;
        private IRepositoryFactory _repositoryFactory;
        private IManagerFactory _managerFactory;
        private ICustomProviderFlowCheck _customProviderFlowCheck;

        [SetUp]
        public override void SetUp()
        {
            _providerTypeService = Substitute.For<IProviderTypeService>();
            _stateManager = Substitute.For<IStateManager>();
            _permissionRepository = Substitute.For<IPermissionRepository>();
            _jobHistoryRepository = Substitute.For<IJobHistoryRepository>();
            _permissionValidator = Substitute.For<IViewErrorsPermissionValidator>();
            _integrationPointService = Substitute.For<IIntegrationPointService>();

            _repositoryFactory = Substitute.For<IRepositoryFactory>();
            _repositoryFactory.GetPermissionRepository(_WORKSPACE_ID).Returns(_permissionRepository);
            _repositoryFactory.GetJobHistoryRepository(_WORKSPACE_ID).Returns(_jobHistoryRepository);

            _managerFactory = Substitute.For<IManagerFactory>();
            _managerFactory.CreateStateManager().Returns(_stateManager);

            _customProviderFlowCheck = Substitute.For<ICustomProviderFlowCheck>();
        }

        [TestCase(ExportType.SavedSearch, ProviderType.Relativity, true, true, "Pending")]
        [TestCase(ExportType.SavedSearch, ProviderType.Other, true, true, "Pending")]
        [TestCase(ExportType.SavedSearch, ProviderType.FTP, true, true, "Pending")]
        [TestCase(ExportType.SavedSearch, ProviderType.LDAP, true, true, "Pending")]
        [TestCase(ExportType.SavedSearch, ProviderType.LoadFile, true, true, "Pending")]
        [TestCase(ExportType.SavedSearch, ProviderType.Relativity, false, true, "Pending")]
        [TestCase(ExportType.SavedSearch, ProviderType.Relativity, true, true, "Completed with errors")]
        [TestCase(ExportType.SavedSearch, ProviderType.Relativity, true, true, "Pending")]
        [TestCase(ExportType.SavedSearch, ProviderType.Relativity, true, true, "Pending")]
        [TestCase(ExportType.ProductionSet, ProviderType.Relativity, true, false, "Pending")]
        [TestCase(ExportType.View, ProviderType.Relativity, true, false, "Pending")]
        public void BuildButtonState_GoldWorkflow(
            ExportType exportType,
            ProviderType providerType,
            bool hasErrorViewPermission,
            bool hasAddProfilePermission,
            string jobHistoryStatus)
        {
            // Arrange
            SetupIntegrationPoint(providerType, exportType);

            _permissionValidator.Validate(_WORKSPACE_ID).Returns(
                hasErrorViewPermission ? new ValidationResult() : new ValidationResult(new[] { "error" }));

            _jobHistoryRepository.GetLastJobHistoryStatus(_INTEGRATION_POINT_ID)
                .Returns(jobHistoryStatus);

            _permissionRepository.UserHasArtifactTypePermission(Arg.Any<Guid>(), ArtifactPermission.Create)
                .Returns(hasAddProfilePermission);

            ButtonStateBuilder sut = GetSut();

            // Act
            sut.CreateButtonState(_WORKSPACE_ID, _INTEGRATION_POINT_ID);

            // Assert
            _stateManager.Received()
                .GetButtonState(
                    Arg.Is(exportType),
                    Arg.Is(providerType),
                    Arg.Is(hasErrorViewPermission),
                    Arg.Is(hasAddProfilePermission),
                    Arg.Any<bool>(),
                    Arg.Is(jobHistoryStatus));
        }

        [TestCase(CalculationStatus.New, false)]
        [TestCase(CalculationStatus.InProgress, true)]
        [TestCase(CalculationStatus.Error, false)]
        [TestCase(CalculationStatus.Completed, false)]
        [TestCase(CalculationStatus.Canceled, false)]
        public void CreateButtonStateAsync_ShouldCreateBasedOnCalculationState(
            CalculationStatus status,
            bool expectedInProgressFlagValue)
        {
            // Arrange
            SetupIntegrationPoint(ProviderType.Relativity, ExportType.SavedSearch,
                new CalculationState { Status = status });

            _permissionValidator.Validate(_WORKSPACE_ID)
                .Returns(new ValidationResult());

            _jobHistoryRepository.GetLastJobHistoryStatus(_INTEGRATION_POINT_ID)
                .Returns("Validating");

            _permissionRepository.UserHasArtifactTypePermission(Arg.Any<Guid>(), ArtifactPermission.Create)
                .Returns(true);

            ButtonStateBuilder sut = GetSut();

            // Act
            sut.CreateButtonState(_WORKSPACE_ID, _INTEGRATION_POINT_ID);

            // Assert
            _stateManager.Received()
                .GetButtonState(
                    Arg.Any<ExportType>(),
                    Arg.Any<ProviderType>(),
                    Arg.Any<bool>(),
                    Arg.Any<bool>(),
                    Arg.Is(expectedInProgressFlagValue),
                    Arg.Any<string>());
        }

        [Test]
        public void CreateButtonStateAsync_ShouldSetInProgressFlagToFalse_WhenCalculationStateIsEmpty()
        {
            // Arrange
            bool expectedInProgressFlagValue = false;
            SetupIntegrationPoint(ProviderType.Relativity, ExportType.SavedSearch,
                new CalculationState());

            _permissionValidator.Validate(_WORKSPACE_ID)
                .Returns(new ValidationResult());

            _jobHistoryRepository.GetLastJobHistoryStatus(_INTEGRATION_POINT_ID)
                .Returns((string)null);

            _permissionRepository.UserHasArtifactTypePermission(Arg.Any<Guid>(), ArtifactPermission.Create)
                .Returns(true);

            ButtonStateBuilder sut = GetSut();

            // Act
            sut.CreateButtonState(_WORKSPACE_ID, _INTEGRATION_POINT_ID);

            // Assert
            _stateManager.Received()
                .GetButtonState(
                    Arg.Any<ExportType>(),
                    Arg.Any<ProviderType>(),
                    Arg.Any<bool>(),
                    Arg.Any<bool>(),
                    Arg.Is(expectedInProgressFlagValue),
                    Arg.Any<string>());
        }

        private void SetupIntegrationPoint(
            ProviderType providerType,
            ExportType exportType,
            CalculationState calculationState = null)
        {
            const int sourceProviderArtifactId = 210;
            const int destinationProviderArtifactId = 220;

            IntegrationPointSlimDto integrationPoint = new IntegrationPointSlimDto
            {
                SourceProvider = sourceProviderArtifactId,
                DestinationProvider = destinationProviderArtifactId,
            };

            _integrationPointService.ReadSlim(_INTEGRATION_POINT_ID)
                .Returns(integrationPoint);
            _integrationPointService.GetSourceConfiguration(integrationPoint.ArtifactId)
                .Returns(JsonConvert.SerializeObject(new { TypeOfExport = exportType }));
            _integrationPointService.GetCalculationState(integrationPoint.ArtifactId)
                .Returns(calculationState);

            _providerTypeService.GetProviderType(integrationPoint.SourceProvider, integrationPoint.DestinationProvider)
                .Returns(providerType);
        }

        private ButtonStateBuilder GetSut()
        {
            return new ButtonStateBuilder(
                new RipJsonSerializer(null),
                _providerTypeService,
                _repositoryFactory,
                _integrationPointService,
                _permissionValidator,
                _customProviderFlowCheck,
                _managerFactory);
        }
    }
}
