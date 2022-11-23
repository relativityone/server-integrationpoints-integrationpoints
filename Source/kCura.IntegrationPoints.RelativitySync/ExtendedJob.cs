using System;
using Castle.Core;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.RelativitySync
{
    public sealed class ExtendedJob : IExtendedJob
    {
        private Guid? _identifier;
        private int? _jobHistoryId;
        private IntegrationPoint _integrationPoint;

        private readonly IJobHistoryService _jobHistoryService;
        private readonly IIntegrationPointService _integrationPointService;
        private readonly ISerializer _serializer;
        private readonly IAPILog _logger;

        public ExtendedJob(Job job, IJobHistoryService jobHistoryService, IIntegrationPointService integrationPointService, ISerializer serializer, IAPILog logger)
        {
            Job = job;
            _jobHistoryService = jobHistoryService;
            _integrationPointService = integrationPointService;
            _serializer = serializer;
            _logger = logger;
        }

        [DoNotSelect]
        public ExtendedJob(Job job, IJobHistoryService jobHistoryService, IntegrationPoint integrationPoint, ISerializer serializer, IAPILog logger)
        {
            Job = job;
            _jobHistoryService = jobHistoryService;
            _integrationPoint = integrationPoint;
            _serializer = serializer;
            _logger = logger;
        }

        public Job Job { get; }

        public long JobId => Job.JobId;

        public int WorkspaceId => Job.WorkspaceID;

        public int SubmittedById => Job.SubmittedBy;

        public int IntegrationPointId => Job.RelatedObjectArtifactID;

        public IntegrationPoint IntegrationPointModel
        {
            get
            {
                if (_integrationPoint == null)
                {
                    try
                    {
                        _integrationPoint = _integrationPointService.ReadIntegrationPoint(IntegrationPointId);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Unable to query for integration point {IntegrationPointId}.", IntegrationPointId);
                        throw;
                    }
                }

                return _integrationPoint;
            }
        }

        public Guid JobIdentifier
        {
            get
            {
                if (!_identifier.HasValue)
                {
                    if (string.IsNullOrWhiteSpace(Job.JobDetails))
                    {
                        _identifier = Guid.NewGuid();
                    }
                    else
                    {
                        TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(Job.JobDetails);
                        _identifier = taskParameters.BatchInstance;
                    }
                }

                return _identifier.Value;
            }
        }

        public int JobHistoryId
        {
            get
            {
                if (!_jobHistoryId.HasValue)
                {
                    _jobHistoryId = _jobHistoryService.GetOrCreateScheduledRunHistoryRdo(IntegrationPointModel, JobIdentifier, DateTime.UtcNow).ArtifactId;
                }

                return _jobHistoryId.Value;
            }
        }
    }
}