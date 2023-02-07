using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.TaskFactory
{
    internal class TaskFactoryJobHistoryServiceFactory : ITaskFactoryJobHistoryServiceFactory
    {
        private readonly IAPILog _logger;
        private readonly ISerializer _serializer;
        private readonly IJobHistoryService _jobHistoryService;
        private readonly IJobHistoryErrorService _jobHistoryErrorService;
        private readonly IIntegrationPointService _integrationPointService;

        public TaskFactoryJobHistoryServiceFactory(
            IAPILog logger,
            ISerializer serializer,
            IJobHistoryService jobHistoryJobHistoryService,
            IJobHistoryErrorService jobHistoryErrorService,
            IIntegrationPointService integrationPointService)
        {
            _logger = logger;
            _serializer = serializer;
            _jobHistoryService = jobHistoryJobHistoryService;
            _jobHistoryErrorService = jobHistoryErrorService;
            _integrationPointService = integrationPointService;
        }

        public ITaskFactoryJobHistoryService CreateJobHistoryService(IntegrationPointDto integrationPoint)
        {
            return new TaskFactoryJobHistoryService(
                _logger,
                _serializer,
                _jobHistoryErrorService,
                _integrationPointService,
                _jobHistoryService,
                integrationPoint);
        }
    }
}
