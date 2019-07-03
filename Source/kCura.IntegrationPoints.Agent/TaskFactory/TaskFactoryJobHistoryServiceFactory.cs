using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
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
		private readonly IIntegrationPointRepository _integrationPointRepository;

		public TaskFactoryJobHistoryServiceFactory(
			IHelper helper, 
			IHelperFactory helperFactory,
			IIntegrationPointSerializer serializer, 
			IServiceFactory serviceFactory, 
			IJobHistoryErrorService jobHistoryErrorService,
			IIntegrationPointRepository integrationPointRepository)
		{
			_helper = helper;
			_helperFactory = helperFactory;
			_serializer = serializer;
			_serviceFactory = serviceFactory;
			_jobHistoryErrorService = jobHistoryErrorService;
			_integrationPointRepository = integrationPointRepository;
		}

		public ITaskFactoryJobHistoryService CreateJobHistoryService(IntegrationPoint integrationPoint)
		{
			return new TaskFactoryJobHistoryService(
				_helper, 
				_helperFactory, 
				_serializer, 
				_serviceFactory, 
				_jobHistoryErrorService,
				_integrationPointRepository, 
				integrationPoint);
		}
	}
}