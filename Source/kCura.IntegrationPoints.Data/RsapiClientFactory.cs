using System;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Data
{
	public class RsapiClientFactory : IRsapiClientFactory
	{
		protected readonly IHelper Helper;
		private readonly IServicesMgr _servicesMgr;
		private readonly IAPILog _logger;


		public RsapiClientFactory(IHelper helper, IServicesMgr servicesMgr)
		{
			Helper = helper;
			_servicesMgr = servicesMgr;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<RsapiClientFactory>();
		}

		public RsapiClientFactory(IHelper helper) : this(helper, helper.GetServicesManager())
		{
		}

		public virtual IRSAPIClient CreateAdminClient(int workspaceArtifactId = -1)
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
				client = _servicesMgr.CreateProxy<IRSAPIClient>(identity);
			}
			catch (NullReferenceException e)
			{
				LogCreatingRsapiClientError(e);
				client = _servicesMgr.CreateProxy<IRSAPIClient>(ExecutionIdentity.System);
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