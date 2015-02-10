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
	public class SyncWorker : ITask
	{
		private ICaseServiceContext _caseServiceContext;
		private IDataSyncronizerFactory _dataSyncronizerFactory;
		private IDataProviderFactory _dataProviderFactory;
		private kCura.Apps.Common.Utils.Serializers.ISerializer _serializer;
		private GeneralWithCustodianRdoSynchronizerFactory _appDomainRdoSynchronizerFactoryFactory;
		private IJobManager _jobManager;
		public SyncWorker(ICaseServiceContext caseServiceContext,
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
				int integrationPointID = job.RelatedObjectArtifactID;
				rdoIntegrationPoint = _caseServiceContext.RsapiService.IntegrationPointLibrary.Read(integrationPointID);
				if (rdoIntegrationPoint == null)
				{
					throw new ArgumentException("Failed to retrieved corresponding Integration Point.");
				}
				if (rdoIntegrationPoint.SourceProvider.GetValueOrDefault(0) == 0)
				{
					throw new ArgumentException("Cannot import source provider with unknown id.");
				}

				if (rdoIntegrationPoint.DestinationProvider.GetValueOrDefault(0) == 0)
				{
					throw new ArgumentException("Cannot import destination provider with unknown id.");
				}
				List<string> entryIDs = _serializer.Deserialize<List<string>>(job.JobDetails);
				Data.SourceProvider sourceProviderRdo = _caseServiceContext.RsapiService.SourceProviderLibrary.Read(rdoIntegrationPoint.SourceProvider.Value);
				ExecuteImport(rdoIntegrationPoint, entryIDs, sourceProviderRdo, job);
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

		private void ExecuteImport(IntegrationPoint rdoIntegrationPoint, List<string> entryIDs, Data.SourceProvider sourceProviderRdo, Job job)
		{

			IDataSourceProvider sourceProvider = GetSourceProvider(sourceProviderRdo, job);

			IEnumerable<FieldMap> fieldMap = _serializer.Deserialize<List<FieldMap>>(rdoIntegrationPoint.FieldMappings);
			fieldMap.ForEach(f => f.SourceField.IsIdentifier = f.FieldMapType == FieldMapTypeEnum.Identifier);
			List<FieldEntry> sourceFields = fieldMap.Select(f => f.SourceField).ToList();

			IDataReader sourceDataReader = sourceProvider.GetData(sourceFields, entryIDs, rdoIntegrationPoint.SourceConfiguration);
			Data.DestinationProvider destination = _caseServiceContext.RsapiService.DestinationProviderLibrary.Read(rdoIntegrationPoint.DestinationProvider.Value);

			IDataSyncronizer dataSyncronizer = GetDestinationProvider(destination, rdoIntegrationPoint.DestinationConfiguration, job);
			var objectBuilder = new SynchronizerObjectBuilder(sourceFields);
			IEnumerable<IDictionary<FieldEntry, object>> data = new DataReaderToEnumerableService(objectBuilder).GetData<IDictionary<FieldEntry, object>>(sourceDataReader);

			dataSyncronizer.SyncData(data, fieldMap, rdoIntegrationPoint.DestinationConfiguration);
		}

		private IDataSourceProvider GetSourceProvider(SourceProvider sourceProviderRdo, Job job)
		{
			Guid applicationGuid = new Guid(sourceProviderRdo.ApplicationIdentifier);
			Guid providerGuid = new Guid(sourceProviderRdo.Identifier);
			IDataSourceProvider sourceProvider = _dataProviderFactory.GetDataProvider(applicationGuid, providerGuid);
			return sourceProvider;
		}

		private IDataSyncronizer GetDestinationProvider(DestinationProvider destinationProviderRdo, string configuration, Job job)
		{
			Guid providerGuid = new Guid(destinationProviderRdo.Identifier);
			if (_appDomainRdoSynchronizerFactoryFactory is GeneralWithCustodianRdoSynchronizerFactory)
			{
				((GeneralWithCustodianRdoSynchronizerFactory)_appDomainRdoSynchronizerFactoryFactory).TaskJobSubmitter =
					new TaskJobSubmitter(_jobManager, job, TaskType.SyncCustodianManagerWorker);
			}
			Contracts.PluginBuilder.Current.SetSynchronizerFactory(_appDomainRdoSynchronizerFactoryFactory);
			IDataSyncronizer sourceProvider = _dataSyncronizerFactory.GetSyncronizer(providerGuid, configuration);
			return sourceProvider;
		}
	}
}
