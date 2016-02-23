using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Authentication;
using Castle.Core.Internal;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Contracts.Synchronizer;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Conversion;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Conversion;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class SyncWorker : ITask
	{
		internal ICaseServiceContext _caseServiceContext;
		private readonly IHelper _helper;
		internal IDataProviderFactory _dataProviderFactory;
		internal kCura.Apps.Common.Utils.Serializers.ISerializer _serializer;
		internal JobHistoryService _jobHistoryService;
		internal JobHistoryErrorService _jobHistoryErrorService;
		internal kCura.IntegrationPoints.Contracts.ISynchronizerFactory _appDomainRdoSynchronizerFactoryFactory;
		internal IJobManager _jobManager;
		private JobStatisticsService _statisticsService;
		private IEnumerable<Core.IBatchStatus> _batchStatus;

		public IEnumerable<Core.IBatchStatus> BatchStatus
		{
			get { return _batchStatus ?? (_batchStatus = new List<IBatchStatus>()); }
			set { _batchStatus = value; }
		}

		public SyncWorker(
			ICaseServiceContext caseServiceContext,
			IHelper helper,
			IDataProviderFactory dataProviderFactory,
			kCura.Apps.Common.Utils.Serializers.ISerializer serializer,
			kCura.IntegrationPoints.Contracts.ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory,
			JobHistoryService jobHistoryService,
			JobHistoryErrorService jobHistoryErrorService,
			IJobManager jobManager,
			IEnumerable<IBatchStatus> statuses,
			JobStatisticsService statisticsService)
		{
			_caseServiceContext = caseServiceContext;
			_helper = helper;
			_dataProviderFactory = dataProviderFactory;
			_serializer = serializer;
			_appDomainRdoSynchronizerFactoryFactory = appDomainRdoSynchronizerFactoryFactory;
			_jobHistoryService = jobHistoryService;
			_jobHistoryErrorService = jobHistoryErrorService;
			_jobManager = jobManager;
			BatchStatus = statuses;
			_statisticsService = statisticsService;
		}

		internal Data.IntegrationPoint IntegrationPoint { get; set; }
		internal Data.JobHistory JobHistory { get; set; }
		internal Guid BatchInstance { get; set; }

		public void Execute(Job job)
		{
			foreach (var batchComplete in BatchStatus)
			{
				batchComplete.JobStarted(job);
			}
			ExecuteTask(job);
		}

		internal virtual void ExecuteTask(Job job)
		{
			try
			{
				kCura.Method.Injection.InjectionManager.Instance.Evaluate("640E9695-AB99-4763-ADC5-03E1252277F7");

				GetIntegrationPointRdo(job);

				List<string> entryIDs = GetEntryIDs(job);

				GetJobHistoryRdo();

				kCura.Method.Injection.InjectionManager.Instance.Evaluate("CB070ADB-8912-4B61-99B0-3321C0670FC6");

				if (!this.IntegrationPoint.SourceProvider.HasValue)
				{
					throw new ArgumentException("Cannot import source provider with unknown id.");
				}
				if (!this.IntegrationPoint.DestinationProvider.HasValue)
				{
					throw new ArgumentException("Cannot import destination provider with unknown id.");
				}
				Data.SourceProvider sourceProviderRdo = _caseServiceContext.RsapiService.SourceProviderLibrary.Read(this.IntegrationPoint.SourceProvider.Value);
				Data.DestinationProvider destinationProvider = _caseServiceContext.RsapiService.DestinationProviderLibrary.Read(this.IntegrationPoint.DestinationProvider.Value);
				IEnumerable<FieldMap> fieldMap = GetFieldMap(this.IntegrationPoint.FieldMappings);
				string sourceConfiguration = GetSourceConfiguration(this.IntegrationPoint.SourceConfiguration);

				ExecuteImport(fieldMap, sourceConfiguration, this.IntegrationPoint.DestinationConfiguration, entryIDs, sourceProviderRdo, destinationProvider, job);

				InjectErrors();
			}
			catch (AuthenticationException e)
			{
				_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, string.Empty, e.Message, e.StackTrace);
			}
			catch (Exception ex)
			{
				_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
			}
			finally
			{
				//rdo last run and next scheduled time will be updated in Manager job
				_jobHistoryErrorService.CommitErrors();
				PostExecute(job);
			}
		}

		internal void PostExecute(Job job)
		{
			try
			{
				TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
				var batchInstance = taskParameters.BatchInstance;
				bool isJobComplete = _jobManager.CheckBatchJobComplete(job, batchInstance.ToString());
				if (isJobComplete)
				{
					foreach (var completedItem in BatchStatus)
					{
						try
						{
							completedItem.JobComplete(job);
						}
						catch (Exception e)
						{
							_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, e);
						}
					}
				}
			}
			catch (Exception e)
			{
				_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, e);
			}
			finally
			{
				_jobHistoryErrorService.CommitErrors();
			}
		}

		internal void GetIntegrationPointRdo(Job job)
		{
			if (this.IntegrationPoint != null) return;

			int integrationPointID = job.RelatedObjectArtifactID;
			this.IntegrationPoint = _caseServiceContext.RsapiService.IntegrationPointLibrary.Read(integrationPointID);
			if (this.IntegrationPoint == null)
			{
				throw new ArgumentException("Failed to retrieved corresponding Integration Point.");
			}
		}

		internal void GetJobHistoryRdo()
		{
			if (this.JobHistory != null) return;

			this.JobHistory = _jobHistoryService.CreateRdo(this.IntegrationPoint, this.BatchInstance, DateTime.UtcNow);
			_jobHistoryErrorService.JobHistory = this.JobHistory;
			_jobHistoryErrorService.IntegrationPoint = this.IntegrationPoint;
		}

		internal virtual string GetSourceConfiguration(string originalSourceConfiguration)
		{
			return originalSourceConfiguration;
		}

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

		protected void SetupSubscriptions(IDataSynchronizer synchronizer, Job job)
		{
			_statisticsService.Subscribe(synchronizer as IBatchReporter, job);
			_jobHistoryErrorService.SubscribeToBatchReporterEvents(synchronizer);
		}

		internal virtual void ExecuteImport(IEnumerable<FieldMap> fieldMap,
			string sourceConfiguration, string destinationConfiguration, List<string> entryIDs,
			Data.SourceProvider sourceProviderRdo, Data.DestinationProvider destinationProvider, Job job)
		{
			FieldMap[] fieldMaps = fieldMap as FieldMap[] ?? fieldMap.ToArray();

			IDataSourceProvider sourceProvider = GetSourceProvider(sourceProviderRdo, job);

			List<FieldEntry> sourceFields = GetSourceFields(fieldMaps);

			IDataReader sourceDataReader = sourceProvider.GetData(sourceFields, entryIDs, sourceConfiguration);

			IDataSynchronizer dataSynchronizer = GetDestinationProvider(destinationProvider, destinationConfiguration, job);

			SetupSubscriptions(dataSynchronizer, job);

			dataSynchronizer.SyncData(sourceDataReader, fieldMaps, destinationConfiguration);
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
			IDataSourceProvider sourceProvider = _dataProviderFactory.GetDataProvider(applicationGuid, providerGuid, _helper);
			return sourceProvider;
		}

		internal virtual IDataSynchronizer GetDestinationProvider(DestinationProvider destinationProviderRdo, string configuration, Job job)
		{
			Guid providerGuid = new Guid(destinationProviderRdo.Identifier);
			var factory = _appDomainRdoSynchronizerFactoryFactory as GeneralWithCustodianRdoSynchronizerFactory;
			if (factory != null)
			{
				factory.TaskJobSubmitter = new TaskJobSubmitter(_jobManager, job, TaskType.SyncCustodianManagerWorker, this.BatchInstance);
			}
			IDataSynchronizer sourceProvider = _appDomainRdoSynchronizerFactoryFactory.CreateSynchronizer(providerGuid, configuration);
			return sourceProvider;
		}

		private void InjectErrors()
		{
			try
			{
				kCura.Method.Injection.InjectionManager.Instance.Evaluate("DFE4D63C-3A6A-49C2-A80D-25CA60F2B31C");
			}
			catch (Exception ex)
			{
				_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
			}

			try
			{
				kCura.Method.Injection.InjectionManager.Instance.Evaluate("40af620b-af2e-4b50-9f62-870654819df6");
			}
			catch (Exception ex)
			{
				_jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorItem, "MyUniqueIdentifier", ex.Message, ex.StackTrace);
			}
		}
	}
}