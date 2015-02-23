using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Castle.Core.Internal;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Contracts.Synchronizer;
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
		internal ICaseServiceContext _caseServiceContext;
		internal IDataSyncronizerFactory _dataSyncronizerFactory;
		internal IDataProviderFactory _dataProviderFactory;
		internal kCura.Apps.Common.Utils.Serializers.ISerializer _serializer;
		internal GeneralWithCustodianRdoSynchronizerFactory _appDomainRdoSynchronizerFactoryFactory;
		internal IJobManager _jobManager;
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
			ExecuteTask(job);
		}

		internal virtual void ExecuteTask(Job job)
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
				List<string> entryIDs = GetEntryIDs(job);
				Data.SourceProvider sourceProviderRdo = _caseServiceContext.RsapiService.SourceProviderLibrary.Read(rdoIntegrationPoint.SourceProvider.Value);
				Data.DestinationProvider destinationProvider = _caseServiceContext.RsapiService.DestinationProviderLibrary.Read(rdoIntegrationPoint.DestinationProvider.Value);
				IEnumerable<FieldMap> fieldMap = GetFieldMap(rdoIntegrationPoint.FieldMappings);
				string sourceConfiguration = GetSourceConfiguration(rdoIntegrationPoint.SourceConfiguration);
				ExecuteImport(fieldMap, sourceConfiguration, rdoIntegrationPoint.DestinationConfiguration, entryIDs,
					sourceProviderRdo, destinationProvider, job);
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

		internal virtual string GetSourceConfiguration(string originalSourceConfiguration)
		{
			return originalSourceConfiguration;
		}

		internal Guid BatchInstance { get; set; }

		internal virtual List<string> GetEntryIDs(Job job)
		{
			TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
			this.BatchInstance = taskParameters.BatchInstance;
			if (taskParameters.BatchParameters != null)
			{
				if (taskParameters.BatchParameters is Newtonsoft.Json.Linq.JArray)
				{
					return ((Newtonsoft.Json.Linq.JArray)taskParameters.BatchParameters).ToObject<List<string>>();
				}
				else if (taskParameters.BatchParameters is List<string>)
				{
					return (List<string>)taskParameters.BatchParameters;
				}
			}
			return new List<string>();
		}

		internal virtual IEnumerable<FieldMap> GetFieldMap(string serializedFieldMappings)
		{
			IEnumerable<FieldMap> fieldMap = _serializer.Deserialize<List<FieldMap>>(serializedFieldMappings);
			fieldMap.ForEach(f => f.SourceField.IsIdentifier = f.FieldMapType == FieldMapTypeEnum.Identifier);
			return fieldMap;
		}

		internal virtual void ExecuteImport(IEnumerable<FieldMap> fieldMap,
			string sourceConfiguration, string destinationConfiguration, List<string> entryIDs,
			Data.SourceProvider sourceProviderRdo, Data.DestinationProvider destinationProvider, Job job)
		{
			IDataSourceProvider sourceProvider = GetSourceProvider(sourceProviderRdo, job);

			List<FieldEntry> sourceFields = GetSourceFields(fieldMap);

			IDataReader sourceDataReader = sourceProvider.GetData(sourceFields, entryIDs, sourceConfiguration);

			IEnumerable<IDictionary<FieldEntry, object>> sourceData = GetSourceData(sourceFields, sourceDataReader);

			IDataSynchronizer dataSyncronizer = GetDestinationProvider(destinationProvider, destinationConfiguration, job);

			dataSyncronizer.SyncData(sourceData, fieldMap, destinationConfiguration);
		}

		internal virtual List<FieldEntry> GetSourceFields(IEnumerable<FieldMap> fieldMap)
		{
			return fieldMap.Select(f => f.SourceField).ToList();
		}

		internal virtual IEnumerable<IDictionary<FieldEntry, object>> GetSourceData(List<FieldEntry> sourceFields, IDataReader sourceDataReader)
		{
			var objectBuilder = new SynchronizerObjectBuilder(sourceFields);
			return new DataReaderToEnumerableService(objectBuilder).GetData<IDictionary<FieldEntry, object>>(sourceDataReader);
		}

		internal virtual IDataSourceProvider GetSourceProvider(SourceProvider sourceProviderRdo, Job job)
		{
			Guid applicationGuid = new Guid(sourceProviderRdo.ApplicationIdentifier);
			Guid providerGuid = new Guid(sourceProviderRdo.Identifier);
			IDataSourceProvider sourceProvider = _dataProviderFactory.GetDataProvider(applicationGuid, providerGuid);
			return sourceProvider;
		}

		internal virtual IDataSynchronizer GetDestinationProvider(DestinationProvider destinationProviderRdo, string configuration, Job job)
		{
			Guid providerGuid = new Guid(destinationProviderRdo.Identifier);
			if (_appDomainRdoSynchronizerFactoryFactory is GeneralWithCustodianRdoSynchronizerFactory)
			{
				((GeneralWithCustodianRdoSynchronizerFactory)_appDomainRdoSynchronizerFactoryFactory).TaskJobSubmitter =
					new TaskJobSubmitter(_jobManager, job, TaskType.SyncCustodianManagerWorker, this.BatchInstance);
			}
			Contracts.PluginBuilder.Current.SetSynchronizerFactory(_appDomainRdoSynchronizerFactoryFactory);
			IDataSynchronizer sourceProvider = _dataSyncronizerFactory.GetSyncronizer(providerGuid, configuration);
			return sourceProvider;
		}
	}
}
