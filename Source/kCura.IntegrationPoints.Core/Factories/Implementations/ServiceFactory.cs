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

		public ServiceFactory(ICaseServiceContext caseServiceContext,
			IIntegrationPointSerializer serializer,
			IChoiceQuery choiceQuery,
			IJobManager jobService,
			IManagerFactory managerFactory,
			IValidationExecutor validationExecutor,
			IProviderTypeService providerTypeService,
			IMessageService messageService)
		{
			_managerFactory = managerFactory;
			_validationExecutor = validationExecutor;
			_providerTypeService = providerTypeService;
			_messageService = messageService;
			_jobService = jobService;
			_choiceQuery = choiceQuery;
			_serializer = serializer;
			_caseServiceContext = caseServiceContext;
		}

		public IIntegrationPointService CreateIntegrationPointService(IHelper helper)
		{
			IAPILog logger = helper.GetLoggerFactory().GetLogger();
			IJobHistoryService jobHistoryService = CreateJobHistoryService(helper);
			ISecretsRepository secretsRepository = new SecretsRepository(
				SecretStoreFacadeFactory_Deprecated.Create(helper.GetSecretStore, logger),
				logger
			);
			IIntegrationPointRepository integrationPointRepository = new IntegrationPointRepository(
				_caseServiceContext.RsapiService.RelativityObjectManager,
				_serializer,
				secretsRepository,
				logger
			);
			IJobHistoryErrorService jobHistoryErrorService = new JobHistoryErrorService(
				_caseServiceContext,
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
				_caseServiceContext.RsapiService.RelativityObjectManager);
		}

		public IFieldCatalogService CreateFieldCatalogService(IHelper targetHelper)
		{
			return new FieldCatalogService(targetHelper);
		}

		public IJobHistoryService CreateJobHistoryService(IHelper helper)
		{
			IJobHistoryService jobHistoryService = new JobHistoryService(
				_caseServiceContext.RsapiService.RelativityObjectManager,
				_managerFactory.CreateFederatedInstanceManager(),
				_managerFactory.CreateWorkspaceManager(),
				helper,
				_serializer,
				_providerTypeService,
				_messageService);

			return jobHistoryService;
		}
	}
}