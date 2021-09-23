using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Services.Keywords
{
	public class RipNameKeyword : IKeyword
	{
		private readonly IIntegrationPointRepository _integrationPointRepository;
		private readonly Job _job;

		public string KeywordName => "\\[RIP.NAME]";

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