using System;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class RsapiRdoRepository : IRdoRepository
	{
		private readonly IHelper _helper;
		private readonly int _workspaceArtifactId;

		public RsapiRdoRepository(IHelper helper, int workspaceArtifactId)
		{
			_helper = helper;
			_workspaceArtifactId = workspaceArtifactId;
		}

		public QueryResultSet<RDO> Query(Query<RDO> query)
		{
			QueryResultSet<RDO> queryResultSet = null;
			using (IRSAPIClient rsapiClient = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser))
			{
				rsapiClient.APIOptions.WorkspaceID = _workspaceArtifactId;

				try
				{
					queryResultSet = rsapiClient.Repositories.RDO.Query(query);
				}
				catch (Exception e)
				{
					throw new Exception($"Unable to retrieve RDO: {e.Message}", e);
				}
			}
			
			return queryResultSet;
		}
	}
}
