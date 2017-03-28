using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class ServiceFactory : IServiceFactory
	{
		private readonly ICaseServiceContext _caseServiceContext;
		private readonly IContextContainerFactory _contextContainerFactory;
		private readonly IIntegrationPointSerializer _serializer;
		private readonly IChoiceQuery _choiceQuery;
		private readonly IJobManager _jobService;
		private readonly IManagerFactory _managerFactory;
		private readonly IIntegrationPointProviderValidator _ipValidator;
		private readonly IIntegrationPointPermissionValidator _permissionValidator;

		public ServiceFactory(ICaseServiceContext caseServiceContext, IContextContainerFactory contextContainerFactory,
			IIntegrationPointSerializer serializer, IChoiceQuery choiceQuery,
			IJobManager jobService, IManagerFactory managerFactory, IIntegrationPointProviderValidator ipValidator,
			IIntegrationPointPermissionValidator permissionValidator)
		{
			_permissionValidator = permissionValidator;
			_ipValidator = ipValidator;
			_managerFactory = managerFactory;
			_jobService = jobService;
			_choiceQuery = choiceQuery;
			_serializer = serializer;
			_contextContainerFactory = contextContainerFactory;
			_caseServiceContext = caseServiceContext;
		}

		public IIntegrationPointService CreateIntegrationPointService(IHelper helper, IHelper targetHelper)
		{
			IJobHistoryService jobHistoryService = CreateJobHistoryService(helper, targetHelper);

			return new IntegrationPointService(
				helper,
				_caseServiceContext, 
				_contextContainerFactory, 
				_serializer, 
				_choiceQuery, 
				_jobService, 
				jobHistoryService, 
				_managerFactory,
				_ipValidator,
				_permissionValidator);
		}

		public IFieldCatalogService CreateFieldCatalogService(IHelper targetHelper)
		{
			return new FieldCatalogService(targetHelper);
		}

		public IJobHistoryService CreateJobHistoryService(IHelper helper, IHelper targetHelper)
		{
			IContextContainer sourceContextContainer = _contextContainerFactory.CreateContextContainer(helper);
			IContextContainer targetContextContainer = _contextContainerFactory.CreateContextContainer(helper, targetHelper.GetServicesManager());

			IJobHistoryService jobHistoryService = new JobHistoryService(_caseServiceContext, _managerFactory.CreateFederatedInstanceManager(sourceContextContainer),
				_managerFactory.CreateWorkspaceManager(targetContextContainer), helper, _serializer);

			return jobHistoryService;
		}
	}
}