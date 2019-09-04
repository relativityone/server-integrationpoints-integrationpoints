using System;
using Castle.Windsor;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;

namespace kCura.IntegrationPoints.RelativitySync
{
	internal static class SyncConfigurationFactory
	{
		public static SyncConfiguration Create(IExtendedJob job, IWindsorContainer ripContainer, IAPILog logger)
		{
			ISerializer serializer;
			try
			{
				serializer = ripContainer.Resolve<ISerializer>();
			}
			catch (Exception e)
			{
				logger.LogError(e, "Unable to resolve dependencies from container.");
				throw;
			}

			try
			{
				ImportSettings destinationConfiguration = serializer.Deserialize<ImportSettings>(job.IntegrationPointModel.DestinationConfiguration);

				return new SyncConfiguration(job.SubmittedById, destinationConfiguration);
			}
			catch (Exception e)
			{
				logger.LogError(e, "Unable to deserialize integration point configuration.");
				throw;
			}
		}
	}
}