#pragma warning disable CS0618 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning disable CS0612 // Type or member is obsolete (IRSAPI deprecation)
using System;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.RSAPIClient
{
	public class RsapiClientFactory : IRsapiClientFactory
	{
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
	}
}
#pragma warning restore CS0612 // Type or member is obsolete (IRSAPI deprecation)
#pragma warning restore CS0618 // Type or member is obsolete (IRSAPI deprecation)
