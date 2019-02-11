using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.RelativitySync
{
	public sealed class ExtendedJob : IExtendedJob
	{
		private Guid? _identifier;
		private int? _jobHistoryId;
		private IntegrationPoint _integrationPoint;

		private readonly Job _job;
		private readonly IJobHistoryService _jobHistoryService;
		private readonly IIntegrationPointService _integrationPointService;
		private readonly ISerializer _serializer;
		private readonly IAPILog _logger;

		public ExtendedJob(Job job, IJobHistoryService jobHistoryService, IIntegrationPointService integrationPointService, ISerializer serializer, IAPILog logger)
		{
			_job = job;
			_jobHistoryService = jobHistoryService;
			_integrationPointService = integrationPointService;
			_serializer = serializer;
			_logger = logger;
		}

		public long JobId => _job.JobId;

		public int WorkspaceId => _job.WorkspaceID;

		public int IntegrationPointId => _job.RelatedObjectArtifactID;

		public IntegrationPoint IntegrationPointModel
		{
			get
			{
				if (_integrationPoint == null)
				{
					try
					{
						_integrationPoint = _integrationPointService.GetRdo(IntegrationPointId);
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
					if (string.IsNullOrWhiteSpace(_job.JobDetails))
					{
						_identifier = Guid.NewGuid();
					}
					else
					{
						TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(_job.JobDetails);
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
					_jobHistoryId = _jobHistoryService.GetRdo(JobIdentifier).ArtifactId;
				}

				return _jobHistoryId.Value;
			}
		}
	}
}