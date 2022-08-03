using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.TaskFactory
{
    internal class TaskFactoryJobHistoryServiceFactory : ITaskFactoryJobHistoryServiceFactory
    {
        private readonly IAPILog _logger;
        private readonly IIntegrationPointSerializer _serializer;
        private readonly IServiceFactory _serviceFactory;
        private readonly IJobHistoryErrorService _jobHistoryErrorService;
        private readonly IIntegrationPointRepository _integrationPointRepository;

        public TaskFactoryJobHistoryServiceFactory(
            IAPILog logger,
            IIntegrationPointSerializer serializer,
            IServiceFactory serviceFactory,
            IJobHistoryErrorService jobHistoryErrorService,
            IIntegrationPointRepository integrationPointRepository)
        {
            _logger = logger;
            _serializer = serializer;
            _serviceFactory = serviceFactory;
            _jobHistoryErrorService = jobHistoryErrorService;
            _integrationPointRepository = integrationPointRepository;
        }

        public ITaskFactoryJobHistoryService CreateJobHistoryService(IntegrationPoint integrationPoint)
        {
            return new TaskFactoryJobHistoryService(
                _logger,
                _serializer,
                _serviceFactory,
                _jobHistoryErrorService,
                _integrationPointRepository,
                integrationPoint);
        }
    }
}