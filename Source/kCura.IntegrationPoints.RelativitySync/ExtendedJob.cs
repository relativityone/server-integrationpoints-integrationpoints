using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;

namespace kCura.IntegrationPoints.RelativitySync
{
	public sealed class ExtendedJob : IExtendedJob
	{
		private Guid? _identifier;
		private int? _jobHistoryId;

		private readonly IJobHistoryService _jobHistoryService;
		private readonly ISerializer _serializer;

		public ExtendedJob(Job job, IJobHistoryService jobHistoryService, ISerializer serializer)
		{
			Job = job;
			_jobHistoryService = jobHistoryService;
			_serializer = serializer;
		}

		public Job Job { get; }

		public long JobId => Job.JobId;

		public int WorkspaceId => Job.WorkspaceID;

		public int IntegrationPointId => Job.RelatedObjectArtifactID;

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
					_jobHistoryId = _jobHistoryService.GetRdo(JobIdentifier).ArtifactId;
				}

				return _jobHistoryId.Value;
			}
		}
	}
}