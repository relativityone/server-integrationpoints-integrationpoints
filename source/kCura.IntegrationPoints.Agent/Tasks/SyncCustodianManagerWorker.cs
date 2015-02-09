using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Castle.Core.Internal;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Contracts.Syncronizer;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Conversion;
using kCura.IntegrationPoints.Core.Services.Conversion;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Syncronizer;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class SyncCustodianManagerWorker : ITask
	{
		private ICaseServiceContext _caseServiceContext;
		private IDataSyncronizerFactory _dataSyncronizerFactory;
		private IDataProviderFactory _dataProviderFactory;
		private kCura.Apps.Common.Utils.Serializers.ISerializer _serializer;
		private GeneralWithCustodianRdoSynchronizerFactory _appDomainRdoSynchronizerFactoryFactory;
		private IJobManager _jobManager;
		public SyncCustodianManagerWorker(ICaseServiceContext caseServiceContext,
											IDataSyncronizerFactory dataSyncronizerFactory,
											IDataProviderFactory dataProviderFactory,
											kCura.Apps.Common.Utils.Serializers.ISerializer serializer,
											GeneralWithCustodianRdoSynchronizerFactory appDomainRdoSynchronizerFactoryFactory,
											IJobManager jobManager)
		{
			_caseServiceContext = caseServiceContext;
			_dataSyncronizerFactory = dataSyncronizerFactory;
			_dataProviderFactory = dataProviderFactory;
			_serializer = serializer;
			_appDomainRdoSynchronizerFactoryFactory = appDomainRdoSynchronizerFactoryFactory;
			_jobManager = jobManager;
		}

		public void Execute(Job job)
		{
			IntegrationPoint rdoIntegrationPoint = null;
			try
			{

			}
			catch
			{
				throw;
			}
			finally
			{
				//rdo last run and next scheduled time will be updated in Manager job
			}
		}
	}
}
