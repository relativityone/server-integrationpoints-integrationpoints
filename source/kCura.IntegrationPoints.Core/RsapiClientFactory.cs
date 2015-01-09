using System;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Core
{
	public class RsapiClientFactory
	{
		private readonly IHelper _helper;
		public RsapiClientFactory(IHelper helper)
		{
			_helper = helper;
		}

		public IRSAPIClient CreateClientForWorkspace(int workspaceID, ExecutionIdentity identity = ExecutionIdentity.CurrentUser)
		{
			IRSAPIClient client;
			try
			{
				client = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(identity);
			}
			catch (NullReferenceException)
			{
				client = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System);
			}
			client.APIOptions.WorkspaceID = workspaceID;
			return client;
		}

		public IDBContext CreateDbContext(int workspaceID)
		{
			var context = _helper.GetDBContext(workspaceID);
			return context;
		}
		
	}
}
