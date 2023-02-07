using System;
using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Common.RelativitySync;
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
        private readonly IRelativitySyncConstrainsChecker _syncConstrainsChecker;
        private readonly IViewErrorsPermissionValidator _viewErrorsPermissionValidator;
        private readonly IJobHistoryManager _jobHistoryManager;
        private readonly IQueueManager _queueManager;
        private readonly IStateManager _stateManager;

        public ButtonStateBuilder(
            ISerializer serializer,
            IProviderTypeService providerTypeService,
            IRepositoryFactory repositoryFactory,
            IIntegrationPointService integrationPointService,
            IRelativitySyncConstrainsChecker syncConstrainsChecker,
            IViewErrorsPermissionValidator viewErrorsPermissionValidator,
            IManagerFactory managerFactory)
        {
            _serializer = serializer;
            _providerTypeService = providerTypeService;
            _repositoryFactory = repositoryFactory;
            _integrationPointService = integrationPointService;
            _syncConstrainsChecker = syncConstrainsChecker;
            _viewErrorsPermissionValidator = viewErrorsPermissionValidator;

            _queueManager = managerFactory.CreateQueueManager();
            _jobHistoryManager = managerFactory.CreateJobHistoryManager();
            _stateManager = managerFactory.CreateStateManager();
        }

        public ButtonStateDTO CreateButtonState(int workspaceArtifactId, int integrationPointArtifactId)
        {
            IntegrationPointSlimDto integrationPoint = _integrationPointService.ReadSlim(integrationPointArtifactId);

            SourceConfiguration.ExportType exportType = GetExportType(integrationPoint.ArtifactId);

            ProviderType providerType = _providerTypeService.GetProviderType(integrationPoint.SourceProvider, integrationPoint.DestinationProvider);

            bool useSyncApp = _syncConstrainsChecker.ShouldUseRelativitySyncApp(integrationPointArtifactId);

            bool hasJobsExecutingOrInQueue = HasJobsExecutingOrInQueue(workspaceArtifactId, integrationPointArtifactId, useSyncApp);

            bool integrationPointIsStoppable = IntegrationPointIsStoppable(
                providerType,
                workspaceArtifactId,
                integrationPointArtifactId,
                exportType,
                useSyncApp);

            CalculationState calculationState = _integrationPointService.GetCalculationState(integrationPoint.ArtifactId);
            bool calculationInProgress = calculationState?.Status == CalculationStatus.InProgress;

            ButtonStateDTO buttonState = _stateManager.GetButtonState(
                exportType,
                providerType,
                hasJobsExecutingOrInQueue,
                integrationPoint.HasErrors.GetValueOrDefault(false),
                HasErrorViewPermissions(workspaceArtifactId),
                integrationPointIsStoppable,
                HasProfileAddPermission(workspaceArtifactId),
                calculationInProgress);

            return buttonState;
        }

        private SourceConfiguration.ExportType GetExportType(int integrationPointId)
        {
            string sourceConfigurationString = _integrationPointService.GetSourceConfiguration(integrationPointId);
            Dictionary<string, object> sourceConfiguration = _serializer.Deserialize<Dictionary<string, object>>(sourceConfigurationString ?? string.Empty);

            object typeOfExport = 0;
            sourceConfiguration?.TryGetValue(nameof(SourceConfiguration.TypeOfExport), out typeOfExport);
            return (SourceConfiguration.ExportType)Convert.ToInt32(typeOfExport);
        }

        private bool HasErrorViewPermissions(int workspaceArtifactId)
        {
            ValidationResult jobHistoryErrorViewPermissionCheck = _viewErrorsPermissionValidator.Validate(workspaceArtifactId);
            return jobHistoryErrorViewPermissionCheck.IsValid;
        }

        private bool HasProfileAddPermission(int workspaceArtifactId)
        {
            IPermissionRepository permissionRepository = _repositoryFactory.GetPermissionRepository(workspaceArtifactId);
            return permissionRepository.UserHasArtifactTypePermission(
                ObjectTypeGuids.IntegrationPointProfileGuid,
                ArtifactPermission.Create);
        }

        private bool HasJobsExecutingOrInQueue(int workspaceArtifactId, int integrationPointArtifactId, bool useSyncApp)
        {
            if (useSyncApp)
            {
                StoppableJobHistoryCollection stoppableJobs = _jobHistoryManager.GetStoppableJobHistory(workspaceArtifactId, integrationPointArtifactId);
                return stoppableJobs.HasStoppableJobHistory;
            }

            return _queueManager.HasJobsExecutingOrInQueue(workspaceArtifactId, integrationPointArtifactId);
        }

        private bool IntegrationPointIsStoppable(ProviderType providerType, int workspaceArtifactId, int integrationPointArtifactId, SourceConfiguration.ExportType exportType, bool useSyncApp)
        {
            StoppableJobHistoryCollection stoppableJobCollection = _jobHistoryManager.GetStoppableJobHistory(workspaceArtifactId, integrationPointArtifactId);

            bool hasExecutingJobs = !useSyncApp && _queueManager.HasJobsExecuting(workspaceArtifactId, integrationPointArtifactId);

            if (stoppableJobCollection.HasOnlyPendingJobHistory && !hasExecutingJobs)
            {
                return true;
            }

            if (IsNonStoppableBasedOnProviderType(providerType, exportType))
            {
                return false;
            }

            return stoppableJobCollection.HasStoppableJobHistory;
        }

        private static bool IsNonStoppableBasedOnProviderType(ProviderType providerType, SourceConfiguration.ExportType exportType)
        {
            return
                (providerType != ProviderType.Relativity && providerType != ProviderType.LoadFile) ||
                (providerType == ProviderType.Relativity && exportType == SourceConfiguration.ExportType.ProductionSet);
        }
    }
}
