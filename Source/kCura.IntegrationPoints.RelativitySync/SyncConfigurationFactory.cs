﻿using System;
using System.Collections.Generic;
using Castle.Windsor;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Agent;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
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
				SourceConfiguration sourceConfiguration = serializer.Deserialize<SourceConfiguration>(job.IntegrationPointModel.SourceConfiguration);
				ImportSettings destinationConfiguration = serializer.Deserialize<ImportSettings>(job.IntegrationPointModel.DestinationConfiguration);
				List<string> emailRecipients = IntegrationPointTaskBase.GetRecipientEmails(job.IntegrationPointModel, logger);

				return new SyncConfiguration(job.JobHistoryId, job.SubmittedById, sourceConfiguration, destinationConfiguration, emailRecipients);
			}
			catch (Exception e)
			{
				logger.LogError(e, "Unable to deserialize integration point configuration.");
				throw;
			}
		}
	}
}