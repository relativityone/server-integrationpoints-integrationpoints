using System;
using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Checkers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Factories;
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

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
    public class ButtonStateBuilder : IButtonStateBuilder
    {
        private readonly ISerializer _serializer;
        private readonly IProviderTypeService _providerTypeService;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IIntegrationPointService _integrationPointService;
        private readonly IViewErrorsPermissionValidator _viewErrorsPermissionValidator;
        private readonly ICustomProviderFlowCheck _customProviderFlowCheck;
        private readonly IStateManager _stateManager;

        public ButtonStateBuilder(
            ISerializer serializer,
            IProviderTypeService providerTypeService,
            IRepositoryFactory repositoryFactory,
            IIntegrationPointService integrationPointService,
            IViewErrorsPermissionValidator viewErrorsPermissionValidator,
            ICustomProviderFlowCheck customProviderFlowCheck,
            IManagerFactory managerFactory)
        {
            _serializer = serializer;
            _providerTypeService = providerTypeService;
            _repositoryFactory = repositoryFactory;
            _integrationPointService = integrationPointService;
            _viewErrorsPermissionValidator = viewErrorsPermissionValidator;
            _customProviderFlowCheck = customProviderFlowCheck;
            _stateManager = managerFactory.CreateStateManager();
        }

        public ButtonStateDTO CreateButtonState(int workspaceArtifactId, int integrationPointArtifactId)
        {
            IntegrationPointDto integrationPoint = _integrationPointService.Read(integrationPointArtifactId);

            SourceConfiguration.ExportType exportType = GetExportType(integrationPoint.ArtifactId);

            ProviderType providerType = _providerTypeService.GetProviderType(
                integrationPoint.SourceProvider,
                integrationPoint.DestinationProvider);

            CalculationState calculationState = _integrationPointService.GetCalculationState(integrationPoint.ArtifactId);
            bool calculationInProgress = calculationState?.Status == CalculationStatus.InProgress;

            var jobHistoryRepository = _repositoryFactory.GetJobHistoryRepository(workspaceArtifactId);
            string lastJobHistoryStatus = jobHistoryRepository.GetLastJobHistoryStatus(integrationPointArtifactId);

            ButtonStateDTO buttonState = _stateManager.GetButtonState(
                exportType,
                providerType,
                HasErrorViewPermissions(workspaceArtifactId),
                HasProfileAddPermission(workspaceArtifactId),
                calculationInProgress,
                lastJobHistoryStatus);

            return buttonState;
        }

        private SourceConfiguration.ExportType GetExportType(int integrationPointId)
        {
            string sourceConfigurationString = _integrationPointService.GetSourceConfiguration(integrationPointId);
            Dictionary<string, object> sourceConfiguration =
                _serializer.Deserialize<Dictionary<string, object>>(sourceConfigurationString ?? string.Empty);

            object typeOfExport = 0;
            sourceConfiguration?.TryGetValue(nameof(SourceConfiguration.TypeOfExport), out typeOfExport);
            return (SourceConfiguration.ExportType)Convert.ToInt32(typeOfExport);
        }

        private bool HasErrorViewPermissions(int workspaceArtifactId)
        {
            ValidationResult jobHistoryErrorViewPermissionCheck =
                _viewErrorsPermissionValidator.Validate(workspaceArtifactId);
            return jobHistoryErrorViewPermissionCheck.IsValid;
        }

        private bool HasProfileAddPermission(int workspaceArtifactId)
        {
            IPermissionRepository permissionRepository =
                _repositoryFactory.GetPermissionRepository(workspaceArtifactId);
            return permissionRepository.UserHasArtifactTypePermission(
                ObjectTypeGuids.IntegrationPointProfileGuid,
                ArtifactPermission.Create);
        }
    }
}
