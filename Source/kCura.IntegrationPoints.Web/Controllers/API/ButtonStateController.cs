using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Facades.SecretStore.Implementation;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Attributes;
using kCura.ScheduleQueue.Core.Data;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.CustomPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace kCura.IntegrationPoints.Web.Controllers.API
{
    public class ButtonStateController: ApiController
    {
        private readonly ICPHelper _helper;
        private readonly IAPILog _logger;
        private readonly IManagerFactory _managerFactory;

        private readonly IJobHistoryManager _jobHistoryManager;
        private readonly IQueueManager _queueManager;
        private readonly IStateManager _stateManager;
        private readonly IIntegrationPointPermissionValidator _permissionValidator;


        public ButtonStateController(ICPHelper helper)
        {
            _helper = helper;
            _logger = _helper.GetLoggerFactory().GetLogger();

            IQueueQueryManager queryManager = new QueueQueryManager(helper, new Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID));
            IJobServiceDataProvider jobServiceDataProvider = new JobServiceDataProvider(queryManager);
            _managerFactory = new ManagerFactory(_helper, new FakeNonRemovableAgent(), jobServiceDataProvider);
            _queueManager = _managerFactory.CreateQueueManager();
            _jobHistoryManager = _managerFactory.CreateJobHistoryManager();
            _stateManager = _managerFactory.CreateStateManager();
            IRepositoryFactory repositoryFactory = new RepositoryFactory(_helper, _helper.GetServicesManager());
            _permissionValidator = new IntegrationPointPermissionValidator(new[] { new ViewErrorsPermissionValidator(repositoryFactory) }, new IntegrationPointSerializer(_logger));
        }

        [HttpGet]
        [LogApiExceptionFilter(Message = "Unable to check permissions")]
        public IHttpActionResult GetUserPermissionsCheck(int workspaceId, int integrationPointArtifactId)
        {
            // prepare everything 
            ICPHelper helper = ConnectionHelper.Helper();
            var permissionRepository = new PermissionRepository(helper, workspaceId);
            IRelativityObjectManager objectManager = new RelativityObjectManagerFactory(helper).CreateRelativityObjectManager(workspaceId);
            IAPILog logger = ConnectionHelper.Helper().GetLoggerFactory().GetLogger();
            IIntegrationPointSerializer integrationPointSerializer = new IntegrationPointSerializer(logger);
            ISecretsRepository secretsRepository = new SecretsRepository(
                SecretStoreFacadeFactory_Deprecated.Create(helper.GetSecretStore, logger),
                logger
            );
            IIntegrationPointRepository integrationPointRepository =
                new IntegrationPointRepository(objectManager, integrationPointSerializer, secretsRepository, logger);
            var providerTypeService = new ProviderTypeService(objectManager);

            // logic from buttonstatebuilder recreated
            IntegrationPoint integrationPoint =
                integrationPointRepository.ReadWithFieldMappingAsync(integrationPointArtifactId).GetAwaiter().GetResult();
            ProviderType providerType = providerTypeService.GetProviderType(integrationPoint.SourceProvider.Value,
                integrationPoint.DestinationProvider.Value);

            ValidationResult jobHistoryErrorViewPermissionCheck = _permissionValidator.ValidateViewErrors(workspaceId);

            var settings = JsonConvert.DeserializeObject<ImportSettings>(integrationPoint.DestinationConfiguration);

            bool hasAddProfilePermission = permissionRepository.UserHasArtifactTypePermission(Guid.Parse(ObjectTypeGuids.IntegrationPointProfile),
                ArtifactPermission.Create) && !settings.IsFederatedInstance();

            bool canViewErrors = jobHistoryErrorViewPermissionCheck.IsValid;
            bool hasJobsExecutingOrInQueue = _queueManager.HasJobsExecutingOrInQueue(workspaceId, integrationPointArtifactId);
            bool integrationPointIsStoppable = IntegrationPointIsStoppable(providerType, workspaceId, integrationPointArtifactId, settings);
            bool integrationPointHasErrors = integrationPoint.HasErrors.GetValueOrDefault(false);
            ButtonStateDTO buttonState = _stateManager.GetButtonState(providerType, hasJobsExecutingOrInQueue, integrationPointHasErrors, canViewErrors,
                integrationPointIsStoppable, hasAddProfilePermission);

            return Ok(buttonState);
        }

        private bool IntegrationPointIsStoppable(ProviderType providerType, int applicationArtifactId, int integrationPointArtifactId, ImportSettings settings)
        {
            if (IsNonStoppableBasedOnProviderType(providerType, settings))
            {
                return false;
            }

            StoppableJobCollection stoppableJobCollection = _jobHistoryManager.GetStoppableJobCollection(applicationArtifactId, integrationPointArtifactId);
            bool integrationPointIsStoppable = stoppableJobCollection.HasStoppableJobs;
            return integrationPointIsStoppable;
        }

        private static bool IsNonStoppableBasedOnProviderType(ProviderType providerType, ImportSettings settings)
        {
            return (providerType != ProviderType.Relativity && providerType != ProviderType.LoadFile) ||
                (providerType == ProviderType.Relativity && settings != null && settings.ImageImport);
        }
    }
}
