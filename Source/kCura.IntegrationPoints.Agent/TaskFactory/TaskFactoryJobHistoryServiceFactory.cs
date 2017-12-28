using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.TaskFactory
{
	internal class TaskFactoryJobHistoryServiceFactory : ITaskFactoryJobHistoryServiceFactory
	{
		private readonly IHelper _helper;
		private readonly IHelperFactory _helperFactory;
		private readonly IIntegrationPointSerializer _serializer;
		private readonly IServiceFactory _serviceFactory;
		private readonly IJobHistoryErrorService _jobHistoryErrorService;
		private readonly ICaseServiceContext _caseServiceContext;

		public TaskFactoryJobHistoryServiceFactory(IHelper helper, IHelperFactory helperFactory,
			IIntegrationPointSerializer serializer, IServiceFactory serviceFactory, IJobHistoryErrorService jobHistoryErrorService,
			ICaseServiceContext caseServiceContext)
		{
			_helper = helper;
			_helperFactory = helperFactory;
			_serializer = serializer;
			_serviceFactory = serviceFactory;
			_jobHistoryErrorService = jobHistoryErrorService;
			_caseServiceContext = caseServiceContext;
		}

		public ITaskFactoryJobHistoryService CreateJobHistoryService(IntegrationPoint integrationPoint)
		{
			return new TaskFactoryJobHistoryService(_helper, _helperFactory, _serializer, _serviceFactory, _jobHistoryErrorService, _caseServiceContext, integrationPoint);
		}
	}
}