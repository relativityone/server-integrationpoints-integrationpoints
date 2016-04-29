﻿using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.RDO;
using Relativity.API;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoints.Data.Adaptors.Implementations
{
	public class ObjectQueryManagerAdaptor : IObjectQueryManagerAdaptor
	{
		private readonly IHelper _helper;

		public int WorkspaceId { set; get; }
		public int ArtifactTypeId { set; get; }

		public ObjectQueryManagerAdaptor(IHelper helper, int workspaceId, int artifactTypeId)
		{
			_helper = helper;
			WorkspaceId = workspaceId;
			ArtifactTypeId = artifactTypeId;
		}

		public async Task<ObjectQueryResultSet> RetrieveAsync(Query query, string queryToken, int startIndex = 1, int pageSize = 1000)
		{
			using (IObjectQueryManager objectQueryManager = _helper.GetServicesManager().CreateProxy<IObjectQueryManager>(ExecutionIdentity.CurrentUser))
			{
				var result = await objectQueryManager.QueryAsync(
					WorkspaceId,
					ArtifactTypeId,
					query,
					startIndex,
					pageSize,
					new int[] { (int)ObjectQueryPermissions.View },
					queryToken).ConfigureAwait(false);

				return result;
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