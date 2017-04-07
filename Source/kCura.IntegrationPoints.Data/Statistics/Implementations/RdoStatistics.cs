using kCura.IntegrationPoints.Data.Factories;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.Statistics.Implementations
{
	public class RdoStatistics : IRdoStatistics
	{
		private readonly IRepositoryFactory _repositoryFactory;

		public RdoStatistics(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public int ForView(int workspaceArtifactId, int artifactTypeId, int viewId)
		{
			var query = new Query<RDO>
			{
				ArtifactTypeID = artifactTypeId,
				Fields = FieldValue.NoFields,
				Condition = new ViewCondition(viewId)
			};
			var rdoRepository = _repositoryFactory.GetRdoRepository(workspaceArtifactId);
			return rdoRepository.Query(query).TotalCount;
		}
	}
}