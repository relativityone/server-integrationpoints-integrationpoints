using System;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Core
{
	public class RsapiClientFactory
	{
		private readonly IHelper _helper;
		private readonly IAPILog _logger;

		public RsapiClientFactory(IHelper helper)
		{
			_helper = helper;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<RsapiClientFactory>();
		}

		public IRSAPIClient CreateClientForWorkspace(int workspaceID, ExecutionIdentity identity = ExecutionIdentity.CurrentUser)
		{
			IRSAPIClient client;
			try
			{
				client = _helper.GetServicesManager().CreateProxy<IRSAPIClient>(identity);
			}
			catch (NullReferenceException e)
			{
				LogCreatingRsapiClientError(e);
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

		public IServicesMgr CreateServicesMgr()
		{
			IServicesMgr servicesMgr = _helper.GetServicesManager();

			return servicesMgr;
		}

		#region Logging

		private void LogCreatingRsapiClientError(NullReferenceException e)
		{
			_logger.LogError(e, "Failed to create RSAPI Client with given identity. Attempting to create RSAPI Client using System identity.");
		}

		#endregion
	}
}