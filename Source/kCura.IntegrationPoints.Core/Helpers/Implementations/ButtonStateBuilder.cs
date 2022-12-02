using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Common.RelativitySync;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Statistics;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
    public class ButtonStateBuilder : IButtonStateBuilder
    {
        private readonly IProviderTypeService _providerTypeService;
        private readonly IIntegrationPointRepository _integrationPointRepository;
        private readonly IJobHistoryManager _jobHistoryManager;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IQueueManager _queueManager;
        private readonly IStateManager _stateManager;
        private readonly IIntegrationPointPermissionValidator _permissionValidator;
        private readonly ICalculationChecker _calculationChecker;

        public bool IsSyncAppInUse { get; }

        public ButtonStateBuilder(
            IProviderTypeService providerTypeService,
            IQueueManager queueManager,
            IJobHistoryManager jobHistoryManager,
            IStateManager stateManager,
            IPermissionRepository permissionRepository,
            IIntegrationPointPermissionValidator permissionValidator,
            IIntegrationPointRepository integrationPointRepository,
            ICalculationChecker calculationChecker,
            bool isSyncAppInUse)
        {
            _providerTypeService = providerTypeService;
            _queueManager = queueManager;
            _jobHistoryManager = jobHistoryManager;
            _stateManager = stateManager;
            _permissionRepository = permissionRepository;
            _permissionValidator = permissionValidator;
            _integrationPointRepository = integrationPointRepository;
            _calculationChecker = calculationChecker;

            IsSyncAppInUse = isSyncAppInUse;
        }

        public static ButtonStateBuilder CreateButtonStateBuilder(
            ICPHelper helper,
            IRepositoryFactory repositoryFactory,
            IManagerFactory managerFactory,
            IIntegrationPointRepository integrationPointRepository,
            IProviderTypeService providerTypeService,
            IRelativitySyncConstrainsChecker relativitySyncConstrainsChecker,
            ICalculationChecker calculationChecker,
            int workspaceId,
            int integrationPointId)
        {
            var logger = helper.GetLoggerFactory().GetLogger();

            var queueManager = managerFactory.CreateQueueManager();
            var jobHistoryManager = managerFactory.CreateJobHistoryManager();
            var stateManager = managerFactory.CreateStateManager();
            var permissionValidator = new IntegrationPointPermissionValidator(
                new[]
                {
                    new ViewErrorsPermissionValidator(repositoryFactory)
                },
                new IntegrationPointSerializer(logger));

            IPermissionRepository permissionRepository = repositoryFactory.GetPermissionRepository(workspaceId);

            bool isSyncAppInUse = relativitySyncConstrainsChecker
                .ShouldUseRelativitySyncApp(integrationPointId);

            var buttonStateBuilder = new ButtonStateBuilder(
                providerTypeService,
                queueManager,
                jobHistoryManager,
                stateManager,
                permissionRepository,
                permissionValidator,
                integrationPointRepository,
                calculationChecker,
                isSyncAppInUse);

            return buttonStateBuilder;
        }

        public async Task<ButtonStateDTO> CreateButtonStateAsync(int workspaceArtifactId, int integrationPointArtifactId)
        {
            IntegrationPoint integrationPoint = await _integrationPointRepository.ReadAsync(integrationPointArtifactId).ConfigureAwait(false);

            ProviderType providerType = _providerTypeService.GetProviderType(integrationPoint);

            ValidationResult jobHistoryErrorViewPermissionCheck = _permissionValidator.ValidateViewErrors(workspaceArtifactId);

            ImportSettings settings = JsonConvert.DeserializeObject<ImportSettings>(integrationPoint.DestinationConfiguration);

            bool hasAddProfilePermission = _permissionRepository.UserHasArtifactTypePermission(
                ObjectTypeGuids.IntegrationPointProfileGuid,
                ArtifactPermission.Create);

            bool userCanSaveAsProfile = hasAddProfilePermission && !settings.IsFederatedInstance();

            bool canViewErrors = jobHistoryErrorViewPermissionCheck.IsValid;
            bool hasJobsExecutingOrInQueue = HasJobsExecutingOrInQueue(workspaceArtifactId, integrationPointArtifactId);

            SourceConfiguration.ExportType exportType;
            try
            {
                exportType = (SourceConfiguration.ExportType)JsonConvert
                    .DeserializeAnonymousType(integrationPoint.SourceConfiguration, new { TypeOfExport = 0 })
                    .TypeOfExport;
            }
            catch (Exception)
            {
                exportType = 0;
            }

            bool integrationPointIsStoppable = IntegrationPointIsStoppable(
                providerType,
                workspaceArtifactId,
                integrationPointArtifactId,
                exportType);

            bool integrationPointHasErrors = integrationPoint.HasErrors.GetValueOrDefault(false);

            CalculationState calculationState = string.IsNullOrWhiteSpace(integrationPoint.CalculationState) ?
                new CalculationState { Status = CalculationStatus.New }
                : JsonConvert.DeserializeObject<CalculationState>(integrationPoint.CalculationState);

            bool calculationInProgress = calculationState.Status == CalculationStatus.InProgress;

            ButtonStateDTO buttonState = _stateManager.GetButtonState(
                exportType,
                providerType,
                hasJobsExecutingOrInQueue,
                integrationPointHasErrors,
                canViewErrors,
                integrationPointIsStoppable,
                userCanSaveAsProfile,
                calculationInProgress);

            return buttonState;
        }

        private bool HasJobsExecutingOrInQueue(int workspaceArtifactId, int integrationPointArtifactId)
        {
            if (IsSyncAppInUse)
            {
                StoppableJobHistoryCollection stoppableJobs = _jobHistoryManager.GetStoppableJobHistory(workspaceArtifactId, integrationPointArtifactId);
                return stoppableJobs.HasStoppableJobHistory;
            }

            return _queueManager.HasJobsExecutingOrInQueue(workspaceArtifactId, integrationPointArtifactId);
        }

        private bool IntegrationPointIsStoppable(ProviderType providerType, int workspaceArtifactId, int integrationPointArtifactId, SourceConfiguration.ExportType exportType)
        {
            StoppableJobHistoryCollection stoppableJobCollection = _jobHistoryManager.GetStoppableJobHistory(workspaceArtifactId, integrationPointArtifactId);

            bool hasExecutingJobs = IsSyncAppInUse
                ? false
                : _queueManager.HasJobsExecuting(workspaceArtifactId, integrationPointArtifactId);

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
