using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Services.Keywords
{
	public class RipNameKeyword : IKeyword
	{
		public string KeywordName { get { return "\\[RIP.NAME]"; } }

		private readonly IIntegrationPointRepository _integrationPointRepository;
		private readonly Job _job;

		public RipNameKeyword(Job job, IIntegrationPointRepository integrationPointRepository)
		{
			_job = job;
			_integrationPointRepository = integrationPointRepository;
		}
		
		public string Convert()
		{
			return _integrationPointRepository.GetName(_job.RelatedObjectArtifactID);
		}
	}
}
