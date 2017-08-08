using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.Statistics.Implementations
{
	public class RdoStatistics : IRdoStatistics
	{
		private readonly IRdoRepository _rdoRepository;

		public RdoStatistics(IRdoRepository rdoRepository)
		{
			_rdoRepository = rdoRepository;
		}

		public int ForView(int artifactTypeId, int viewId)
		{
			var query = new Query<RDO>
			{
				ArtifactTypeID = artifactTypeId,
				Fields = FieldValue.NoFields,
				Condition = new ViewCondition(viewId)
			};

			return _rdoRepository.Query(query).TotalCount;
		}
	}
}