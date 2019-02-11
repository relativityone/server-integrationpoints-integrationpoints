using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;

namespace kCura.IntegrationPoints.RelativitySync
{
	public sealed class ExtendedJob : IExtendedJob
	{
		private Guid? _identifier;
		private int? _jobHistoryId;

		private readonly Job _job;
		private readonly IJobHistoryService _jobHistoryService;
		private readonly ISerializer _serializer;

		public ExtendedJob(Job job, IJobHistoryService jobHistoryService, ISerializer serializer)
		{
			_job = job;
			_jobHistoryService = jobHistoryService;
			_serializer = serializer;
		}

		public long JobId => _job.JobId;

		public int WorkspaceId => _job.WorkspaceID;

		public int IntegrationPointId => _job.RelatedObjectArtifactID;

		public IntegrationPoint IntegrationPointModel { get; }

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