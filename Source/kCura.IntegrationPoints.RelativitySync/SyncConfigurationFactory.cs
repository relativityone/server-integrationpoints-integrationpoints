using System;
using Castle.Windsor;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.RelativitySync
{
	internal static class SyncConfigurationFactory
	{
		public static SyncConfiguration Create(Job job, IWindsorContainer ripContainer, IAPILog logger)
		{
			IIntegrationPointService integrationPointService;
			ISerializer serializer;
			try
			{
				integrationPointService = ripContainer.Resolve<IIntegrationPointService>();
				serializer = ripContainer.Resolve<ISerializer>();
			}
			catch (Exception e)
			{
				logger.LogError(e, "Unable to resolve dependencies from container.");
				throw;
			}

			IntegrationPoint integrationPoint;
			try
			{
				integrationPoint = integrationPointService.GetRdo(job.RelatedObjectArtifactID);
			}
			catch (Exception e)
			{
				logger.LogError(e, "Unable to query for integration point {id}.", job.RelatedObjectArtifactID);
				throw;
			}

			try
			{
				SourceConfiguration sourceConfiguration = serializer.Deserialize<SourceConfiguration>(integrationPoint.SourceConfiguration);
				ImportSettings destinationConfiguration = serializer.Deserialize<ImportSettings>(integrationPoint.DestinationConfiguration);

				return new SyncConfiguration((int) job.JobId, sourceConfiguration, destinationConfiguration);
			}
			catch (Exception e)
			{
				logger.LogError(e, "Unable to deserialize integration point configuration.");
				throw;
			}
		}
	}
}