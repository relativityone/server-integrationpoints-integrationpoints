using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.Synchronizer;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent
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
			var factory = _appDomainRdoSynchronizerFactoryFactory as ExportDestinationSynchronizerFactory;
			
			// TODO
			IDataSynchronizer sourceProvider = factory.CreateSynchronizer(providerGuid, configuration);
			return sourceProvider;
		}
	}
}
