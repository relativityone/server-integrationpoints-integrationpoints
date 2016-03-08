using System.Threading.Tasks;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoints.Contracts.RDO
{
	public class RDORepository : IRDORepository
	{
		private readonly IObjectQueryManager _objectQueryManager;
		private readonly int _workspaceId;
		private readonly int _artifactTypeId;

		public RDORepository(IObjectQueryManager objectQueryManager, int workspaceId, int artifactTypeId)
		{
			_objectQueryManager = objectQueryManager;
			_workspaceId = workspaceId;
			_artifactTypeId = artifactTypeId;
		}

		public async Task<ObjectQueryResutSet> RetrieveAsync(Query query, string queryToken, int startIndex = 1, int pageSize = 1000)
		{
			Task<ObjectQueryResutSet> resultSet = _objectQueryManager.QueryAsync(
				_workspaceId, 
				_artifactTypeId, 
				query, 
				startIndex, 
				pageSize, 
				new int[] { (int)ObjectQueryPermissions.View }, 
				queryToken);


			return await resultSet;
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