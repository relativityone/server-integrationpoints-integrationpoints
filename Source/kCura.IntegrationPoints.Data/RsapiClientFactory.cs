using System;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Data
{
	public class RsapiClientFactory : IRsapiClientFactory
	{
		protected readonly IHelper Helper;
		private readonly IAPILog _logger;

		public RsapiClientFactory(IHelper helper)
		{
			Helper = helper;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<RsapiClientFactory>();
		}

		public virtual IRSAPIClient CreateAdminClient(int workspaceArtifactId)
		{
			return CreateClientForWorkspace(workspaceArtifactId, ExecutionIdentity.System);
		}

		public virtual IRSAPIClient CreateUserClient(int workspaceArtifactId)
		{
			return CreateClientForWorkspace(workspaceArtifactId);
		}

		private IRSAPIClient CreateClientForWorkspace(int workspaceID, ExecutionIdentity identity = ExecutionIdentity.CurrentUser)
		{
			IRSAPIClient client;
			try
			{
				client = Helper.GetServicesManager().CreateProxy<IRSAPIClient>(identity);
			}
			catch (NullReferenceException e)
			{
				LogCreatingRsapiClientError(e);
				client = Helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System);
			}
			client.APIOptions.WorkspaceID = workspaceID;
			return client;
		}

		#region Logging

		private void LogCreatingRsapiClientError(NullReferenceException e)
		{
			_logger.LogError(e, "Failed to create RSAPI Client with given identity. Attempting to create RSAPI Client using System identity.");
		}

		#endregion
	}
}