﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Synchronizer;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Conversion;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Conversion;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class ExportWorker : SyncWorker
	{
		public ExportWorker(ICaseServiceContext caseServiceContext, IHelper helper, IDataProviderFactory dataProviderFactory, ISerializer serializer, ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory, JobHistoryService jobHistoryService, JobHistoryErrorService jobHistoryErrorService, IJobManager jobManager, IEnumerable<IBatchStatus> statuses, JobStatisticsService statisticsService) 
			: base(caseServiceContext, helper, dataProviderFactory, serializer, appDomainRdoSynchronizerFactoryFactory, jobHistoryService, jobHistoryErrorService, jobManager, statuses, statisticsService)
		{
		}
		internal override IDataSynchronizer GetDestinationProvider(DestinationProvider destinationProviderRdo, string configuration, Job job)
		{
			Guid providerGuid = new Guid(destinationProviderRdo.Identifier);
			
			IDataSynchronizer sourceProvider = _appDomainRdoSynchronizerFactoryFactory.CreateSynchronizer(providerGuid, configuration);
			return sourceProvider;
		}
    }
}
