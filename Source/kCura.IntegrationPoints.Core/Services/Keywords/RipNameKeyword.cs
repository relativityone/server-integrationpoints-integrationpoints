using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Services.Keywords
{
	public class RipNameKeyword : IKeyword
	{
		public string KeywordName { get { return "\\[RIP.NAME]"; } }

		private readonly Job _job;
		private readonly IRSAPIService _service;
		public RipNameKeyword(Job job, IRSAPIService service)
		{
			_job = job;
			_service = service;
		}
		
		public string Convert()
		{
			return _service.RelativityObjectManager.Read<Data.IntegrationPoint>(_job.RelatedObjectArtifactID).Name;
		}
	}
}
