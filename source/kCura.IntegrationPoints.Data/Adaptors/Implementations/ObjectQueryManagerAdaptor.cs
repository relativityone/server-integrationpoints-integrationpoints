using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.RDO;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoints.Data.Managers.Implementations
{
	public class ObjectQueryManagerAdaptor : IObjectQueryManagerAdaptor
	{
		private readonly IObjectQueryManager _objectQueryManager;

		public int WorkspaceId { set; get; }
		public int ArtifactTypeId { set; get; }

		public ObjectQueryManagerAdaptor(IObjectQueryManager objectQueryManager)
		{
			_objectQueryManager = objectQueryManager;
		}

		public ObjectQueryManagerAdaptor(IObjectQueryManager objectQueryManager, int workspaceId, int artifactTypeId)
		{
			_objectQueryManager = objectQueryManager;
			WorkspaceId = workspaceId;
			ArtifactTypeId = artifactTypeId;
		}

		public async Task<ObjectQueryResultSet> RetrieveAsync(Query query, string queryToken, int startIndex = 1, int pageSize = 1000)
		{
			return await _objectQueryManager.QueryAsync(
				WorkspaceId,
				ArtifactTypeId, 
				query,
				startIndex,
				pageSize,
				new int[] {(int) ObjectQueryPermissions.View},
				queryToken);
		}

		private enum ObjectQueryPermissions
		{
			View = 1,
			Edit = 2,
			Delete = 3,
			Secure = 4,
			Manager = 5,
			Add = 6
		}
	}
}