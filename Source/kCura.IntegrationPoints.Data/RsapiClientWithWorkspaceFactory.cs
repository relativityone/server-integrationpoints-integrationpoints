using System;
using kCura.IntegrationPoints.Data.RSAPIClient;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Data
{
	public class RsapiClientWithWorkspaceFactory : IRsapiClientWithWorkspaceFactory
	{
		protected readonly IHelper Helper;
		private readonly IServicesMgr _servicesMgr;
		private readonly IAPILog _logger;


		public RsapiClientWithWorkspaceFactory(IHelper helper, IServicesMgr servicesMgr)
		{
			Helper = helper;
			_servicesMgr = servicesMgr;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<RsapiClientWithWorkspaceFactory>();
		}

		public RsapiClientWithWorkspaceFactory(IHelper helper) : this(helper, helper.GetServicesManager())
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
				LogCreatingRsapiClientError(e, identity);
				if (identity == ExecutionIdentity.System)
				{
					throw CreateRsapiClientCreationException(e, identity);
				}
				LogRetryWithSystemIdentity();
				return CreateClientForWorkspace(workspaceID, ExecutionIdentity.System);
			}
			catch (Exception e)
			{
				LogCreatingRsapiClientError(e, identity);
				throw CreateRsapiClientCreationException(e, identity);
			}
			client.APIOptions.WorkspaceID = workspaceID;
			return new RsapiClientWrapperWithLogging(client, _logger);
		}

		private IntegrationPointsException CreateRsapiClientCreationException(Exception e, ExecutionIdentity identity)
		{
			throw new IntegrationPointsException($"An error occured creating RSAPI Client with given identity: {identity}", e)
			{
				ExceptionSource = IntegrationPointsExceptionSource.RSAPI
			};
		}

		private void LogCreatingRsapiClientError(Exception e, ExecutionIdentity identity)
		{
			_logger.LogError(e, "Failed to create RSAPI Client with given identity: {identity}. ", identity);
		}

		private void LogRetryWithSystemIdentity()
		{
			_logger.LogWarning("Attempting to create RSAPI Client using System identity.");
		}
	}
}