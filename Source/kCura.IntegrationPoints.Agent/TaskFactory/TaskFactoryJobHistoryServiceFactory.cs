using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.TaskFactory
{
    internal class TaskFactoryJobHistoryServiceFactory : ITaskFactoryJobHistoryServiceFactory
    {
        private readonly IAPILog _logger;
        private readonly IIntegrationPointSerializer _serializer;
        private readonly IServiceFactory _serviceFactory;
        private readonly IJobHistoryErrorService _jobHistoryErrorService;
        private readonly IIntegrationPointService _integrationPointService;

        public TaskFactoryJobHistoryServiceFactory(
            IAPILog logger,
            IIntegrationPointSerializer serializer,
            IServiceFactory serviceFactory,
            IJobHistoryErrorService jobHistoryErrorService,
            IIntegrationPointService integrationPointService)
        {
            _logger = logger;
            _serializer = serializer;
            _serviceFactory = serviceFactory;
            _jobHistoryErrorService = jobHistoryErrorService;
            _integrationPointService = integrationPointService;
        }

        public ITaskFactoryJobHistoryService CreateJobHistoryService(IntegrationPointDto integrationPoint)
        {
            return new TaskFactoryJobHistoryService(
                _logger,
                _serializer,
                _serviceFactory,
                _jobHistoryErrorService,
                _integrationPointService,
                integrationPoint);
        }
    }
}
