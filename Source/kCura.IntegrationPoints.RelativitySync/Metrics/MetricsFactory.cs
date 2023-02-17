using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;
using static kCura.IntegrationPoints.Core.Contracts.Configuration.SourceConfiguration;

namespace kCura.IntegrationPoints.RelativitySync.Metrics
{
    public class MetricsFactory : IMetricsFactory
    {
        private readonly ISerializer _serializer;
        private readonly IIntegrationPointService _integrationPointService;
        private readonly IScheduleRuleFactory _scheduleRuleFactory;
        private readonly IServicesMgr _servicesMgr;

        public MetricsFactory(ISerializer serializer, IScheduleRuleFactory scheduleRuleFactory,
            IIntegrationPointService integrationPointService, IServicesMgr servicesMgr)
        {
            _serializer = serializer;
            _scheduleRuleFactory = scheduleRuleFactory;
            _integrationPointService = integrationPointService;
            _servicesMgr = servicesMgr;
        }

        public IMetric CreateScheduleJobStartedMetric(Job job)
        {
            IScheduleRule scheduleRule = _scheduleRuleFactory.Deserialize(job);
            if (scheduleRule == null)
            {
                return new EmptyMetric();
            }

            IntegrationPointDto integrationPoint = _integrationPointService.Read(job.RelatedObjectArtifactID);
            ExportType type = _serializer.Deserialize<SourceConfiguration>(integrationPoint.SourceConfiguration).TypeOfExport;

            return ScheduleMetric.CreateScheduleJobStarted(_servicesMgr, integrationPoint.ArtifactId, job.JobId, type, scheduleRule);
        }

        public IMetric CreateScheduleJobCompletedMetric(Job job)
        {
            IScheduleRule scheduleRule = _scheduleRuleFactory.Deserialize(job);
            if (scheduleRule == null)
            {
                return new EmptyMetric();
            }

            IntegrationPointDto integrationPoint = _integrationPointService.Read(job.RelatedObjectArtifactID);
            ExportType type = _serializer.Deserialize<SourceConfiguration>(integrationPoint.SourceConfiguration).TypeOfExport;

            return ScheduleMetric.CreateScheduleJobCompleted(_servicesMgr, integrationPoint.ArtifactId, job.JobId, type);
        }

        public IMetric CreateScheduleJobFailedMetric(Job job)
        {
            IScheduleRule scheduleRule = _scheduleRuleFactory.Deserialize(job);
            if (scheduleRule == null)
            {
                return new EmptyMetric();
            }

            IntegrationPointDto integrationPoint = _integrationPointService.Read(job.RelatedObjectArtifactID);
            ExportType type = _serializer.Deserialize<SourceConfiguration>(integrationPoint.SourceConfiguration).TypeOfExport;

            return ScheduleMetric.CreateScheduleJobFailed(_servicesMgr, integrationPoint.ArtifactId, job.JobId, type);
        }
    }
}
