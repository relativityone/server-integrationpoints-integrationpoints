using System;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.RSAPIClient
{
	public class RsapiClientFactory : IRsapiClientFactory
	{
		public virtual IRSAPIClient CreateAdminClient(IHelper helper)
		{
			return CreateClient(helper, ExecutionIdentity.System);
		}

		public IRSAPIClient CreateAdminClient(IServicesMgr servicesMgr, IAPILog logger)
		{
			return CreateClient(servicesMgr, logger, ExecutionIdentity.System);
		}

		public virtual IRSAPIClient CreateUserClient(IHelper helper)
		{
			return CreateClient(helper, ExecutionIdentity.CurrentUser);
		}

		public IRSAPIClient CreateUserClient(IServicesMgr servicesMgr, IAPILog logger)
		{
			return CreateClient(servicesMgr, logger, ExecutionIdentity.CurrentUser);
		}

		public IRSAPIClient CreateClient(IHelper helper, ExecutionIdentity identity)
		{
			IAPILog logger = helper.GetLoggerFactory().GetLogger();

			try
			{
				IRSAPIClient client = helper.GetServicesManager().CreateProxy<IRSAPIClient>(identity);
				return new RsapiClientWrapperWithLogging(client, logger);
			}
			catch (Exception e)
			{
				logger.LogError(e, "Failed to create RSAPI Client with given identity: {identity}.", identity);
				throw new IntegrationPointsException($"Failed to create RSAPI Client with given identity: {identity}.", e)
				{
					ExceptionSource = IntegrationPointsExceptionSource.RSAPI,
					ShouldAddToErrorsTab = true
				};
			}
		}

		public IRSAPIClient CreateClient(IServicesMgr serviceManager, IAPILog logger, ExecutionIdentity identity)
		{
			try
			{
				IRSAPIClient client = serviceManager.CreateProxy<IRSAPIClient>(identity);
				return new RsapiClientWrapperWithLogging(client, logger);
			}
			catch (Exception e)
			{
				logger.LogError(e, "Failed to create RSAPI Client with given identity: {identity}.", identity);
				throw new IntegrationPointsException($"Failed to create RSAPI Client with given identity: {identity}.", e)
				{
					ExceptionSource = IntegrationPointsExceptionSource.RSAPI,
					ShouldAddToErrorsTab = true
				};
			}
		}
	}
}
