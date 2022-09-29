using kCura.IntegrationPoints.Common.RelativitySync;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Facades.SecretStore.Implementation;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Relativity.API;
using Relativity.DataTransfer.MessageService;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
    public class ServiceFactory : IServiceFactory
    {
        private readonly ICaseServiceContext _caseServiceContext;
        private readonly IIntegrationPointSerializer _serializer;
        private readonly IChoiceQuery _choiceQuery;
        private readonly IJobManager _jobService;
        private readonly IManagerFactory _managerFactory;
        private readonly IValidationExecutor _validationExecutor;
        private readonly IProviderTypeService _providerTypeService;
        private readonly IMessageService _messageService;
        private readonly ITaskParametersBuilder _taskParametersBuilder;
        private readonly IRelativitySyncConstrainsChecker _relativitySyncConstrainsChecker;
        private readonly IRelativitySyncAppIntegration _relativitySyncAppIntegration;

        public ServiceFactory(ICaseServiceContext caseServiceContext,
            IIntegrationPointSerializer serializer,
            IChoiceQuery choiceQuery,
            IJobManager jobService,
            IManagerFactory managerFactory,
            IValidationExecutor validationExecutor,
            IProviderTypeService providerTypeService,
            IMessageService messageService,
            ITaskParametersBuilder taskParametersBuilder,
            IRelativitySyncConstrainsChecker relativitySyncConstrainsChecker,
            IRelativitySyncAppIntegration relativitySyncAppIntegration
            )
        {
            _managerFactory = managerFactory;
            _validationExecutor = validationExecutor;
            _providerTypeService = providerTypeService;
            _messageService = messageService;
            _jobService = jobService;
            _choiceQuery = choiceQuery;
            _serializer = serializer;
            _caseServiceContext = caseServiceContext;
            _taskParametersBuilder = taskParametersBuilder;
            _relativitySyncConstrainsChecker = relativitySyncConstrainsChecker;
            _relativitySyncAppIntegration = relativitySyncAppIntegration;
        }

        public IIntegrationPointService CreateIntegrationPointService(IHelper helper)
        {
            IAPILog logger = helper.GetLoggerFactory().GetLogger();
            IJobHistoryService jobHistoryService = CreateJobHistoryService(logger);
            ISecretsRepository secretsRepository = new SecretsRepository(
                SecretStoreFacadeFactory_Deprecated.Create(helper.GetSecretStore, logger),
                logger
            );
            IIntegrationPointRepository integrationPointRepository = new IntegrationPointRepository(
                _caseServiceContext.RelativityObjectManagerService.RelativityObjectManager,
                _serializer,
                secretsRepository,
                logger
            );
            IJobHistoryErrorService jobHistoryErrorService = new JobHistoryErrorService(
                _caseServiceContext.RelativityObjectManagerService.RelativityObjectManager,
                helper,
                integrationPointRepository
            );
            return new IntegrationPointService(
                helper,
                _caseServiceContext,
                _serializer,
                _choiceQuery,
                _jobService,
                jobHistoryService,
                jobHistoryErrorService,
                _managerFactory,
                _validationExecutor,
                _providerTypeService,
                _messageService,
                integrationPointRepository,
                _caseServiceContext.RelativityObjectManagerService.RelativityObjectManager,
                _taskParametersBuilder,
                _relativitySyncConstrainsChecker,
                _relativitySyncAppIntegration);
        }

        public IJobHistoryService CreateJobHistoryService(IAPILog logger)
        {
            IJobHistoryService jobHistoryService = new JobHistoryService(
                _caseServiceContext.RelativityObjectManagerService.RelativityObjectManager,
                _managerFactory.CreateFederatedInstanceManager(),
                _managerFactory.CreateWorkspaceManager(),
                logger,
                _serializer);

            return jobHistoryService;
        }
    }
}
