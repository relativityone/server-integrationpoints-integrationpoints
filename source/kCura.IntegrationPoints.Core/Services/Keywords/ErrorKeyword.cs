using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Data.Queries;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Services.Keywords
{
	public class ErrorKeyword : IKeyword
	{

		public string KeywordName { get { return "\\[ERROR]"; } }

		private readonly Job _job;
		private readonly JobHistoryErrorQuery _query;
		private readonly JobHistoryService _historyService;
		private readonly ISerializer _serializer;
		public ErrorKeyword(Job job, JobHistoryErrorQuery query, JobHistoryService historyService, ISerializer serializer)
		{
			_job = job;
			_query = query;
			_historyService = historyService;
			_serializer = serializer;
		}
		public string Convert()
		{
			var details = _serializer.Deserialize<TaskParameters>(_job.JobDetails);
			var history = _historyService.GetRdo(details.BatchInstance);
			var error = _query.GetJobLevelError(history.ArtifactId);
			return error.Error;
		}
	}
}
