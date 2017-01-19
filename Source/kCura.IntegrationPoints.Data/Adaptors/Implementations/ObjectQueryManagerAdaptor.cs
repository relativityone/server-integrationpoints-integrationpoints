using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.RDO;
using Relativity.API;
using Relativity.Services.ObjectQuery;
using Query = Relativity.Services.ObjectQuery.Query;

namespace kCura.IntegrationPoints.Data.Adaptors.Implementations
{
	public class ObjectQueryManagerAdaptor : IObjectQueryManagerAdaptor
	{
		private readonly IServicesMgr _servicesMgr;

		public int WorkspaceId { set; get; }
		public int ArtifactTypeId { set; get; }

		public ObjectQueryManagerAdaptor(IServicesMgr servicesMgr, int workspaceId, int artifactTypeId)
		{
			_servicesMgr = servicesMgr;
			WorkspaceId = workspaceId;
			ArtifactTypeId = artifactTypeId;
		}

		public async Task<ObjectQueryResultSet> RetrieveAsync(Query query, string queryToken, int startIndex = 1, int pageSize = 1000)
		{
			using (IObjectQueryManager objectQueryManager = _servicesMgr.CreateProxy<IObjectQueryManager>(ExecutionIdentity.CurrentUser))
			{
				try
				{
					ObjectQueryResultSet result = await objectQueryManager.QueryAsync(
						WorkspaceId,
						ArtifactTypeId,
						query,
						startIndex,
						pageSize,
						new int[] {(int) ObjectQueryPermissions.View},
						queryToken).ConfigureAwait(false);

					return result;
				}
				catch (System.Exception ex)
				{
					throw ex;
				}
			}
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